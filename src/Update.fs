// Update.fs - Updated with product detail handling
module App.Update

open Elmish
open App.Types
open App.Router
open App.Notifications
open App.Interop
open App.UpdateCounterState
open App.UpdatePluginState
open App.UpdateProductsState
open App.UpdateUserApiState
open App.UpdateProductApiState

// アプリケーションの状態更新関数
let update msg model =
    match msg with
    | CounterMsg counterMsg ->
        let newState, counterCmd = updateCounterState counterMsg model.CounterState
        { model with CounterState = newState }, Cmd.map CounterMsg counterCmd

    // 通知メッセージはサブモジュールに委譲
    | NotificationMsg notificationMsg ->
        let newState, cmd = Notifications.update notificationMsg model.NotificationState

        if model.NotificationState = newState then
            model, Cmd.map NotificationMsg cmd
        else
            { model with
                NotificationState = newState },
            Cmd.map NotificationMsg cmd

    | PluginMsg pluginMsg ->
        let newState, cmd = updatePluginState pluginMsg model.PluginState

        { model with PluginState = newState }, cmd

    | CustomMsg(msgType, payload) ->
        printfn "Received CustomMsg: %s with payload %A" msgType payload

        try
            // カスタム更新ハンドラーを呼び出す
            let newModel = applyCustomUpdate msgType payload model
            newModel, Cmd.none
        with ex ->
            // エラーハンドリング
            printfn "Error in CustomMsg handling: %s" ex.Message
            printfn "Stack trace: %s" ex.StackTrace

            // エラー通知の作成 - メタデータを活用
            let notification =
                error (sprintf "カスタムメッセージ処理エラー: %s" ex.Message)
                |> withDetails ex.StackTrace
                |> withMetadata "messageType" msgType
                |> fromSource "CustomMsgHandler"

            model, Cmd.ofMsg (NotificationMsg(Add notification))

    // ProductsMsg処理
    | ProductsMsg productsMsg ->
        // 製品リストを取得
        let products =
            match model.ApiData.ProductData.Products with
            | Success prods -> prods
            | _ -> []

        let newState, cmd = updateProductsState productsMsg model.ProductsState products

        { model with ProductsState = newState }, cmd

    // ApiMsg処理を更新 - ドメイン別に委譲
    | ApiMsg apiMsg ->
        match apiMsg with
        | UserApi userMsg ->
            // ユーザードメインの更新処理
            let newUserData, userApiCmd = updateUserApiState userMsg model.ApiData.UserData

            { model with
                ApiData =
                    { model.ApiData with
                        UserData = newUserData } },
            userApiCmd

        | ProductApi productMsg ->
            // 製品ドメインの更新処理
            let newProductData, productApiCmd =
                updateProductApiState productMsg model.ApiData.ProductData

            { model with
                ApiData =
                    { model.ApiData with
                        ProductData = newProductData } },
            productApiCmd

    // NavigateToメッセージ処理
    | NavigateTo tab ->
        // 既存のタブハンドリングを維持しながら、ルーティングも更新
        let route = tabToRoute tab
        navigateTo route

        // Products タブをナビゲーションした時に製品データのロードを開始
        let cmd =
            match tab with
            | Tab.Products ->
                // Products タブに移動した時かつデータが未ロードならロードを開始
                match model.ApiData.ProductData.Products with
                | NotStarted -> loadProductsCmd
                | _ -> Cmd.none
            | _ -> Cmd.none

        { model with
            CurrentTab = tab
            CurrentRoute = route },
        cmd

    // RouteChangedメッセージ処理
    | RouteChanged route ->
        // URLが変更されたときの処理
        let tabOption = routeToTab route
        let updatedTab = tabOption |> Option.defaultValue model.CurrentTab

        // RouteChangedに対する処理を拡張 - 製品詳細ビューのサポート
        let cmd =
            match route with
            | Route.Products ->
                // 製品データが未ロードならロードを開始
                match model.ApiData.ProductData.Products with
                | NotStarted -> loadProductsCmd
                | _ -> Cmd.none
            | Route.ProductDetail productId ->
                // 製品詳細ビューへの遷移時に以下を実行:
                // 1. 製品一覧が未ロードならロード
                // 2. 指定された製品の詳細をロード
                let productsCmd =
                    match model.ApiData.ProductData.Products with
                    | NotStarted -> loadProductsCmd
                    | _ -> Cmd.none

                // 標準の製品データ取得コマンド
                let basicProductCmd = loadProductByIdCmd (int64 productId)

                // 詳細データ取得コマンド（新しいエンドポイント使用）
                let detailCmd = loadProductDetailByIdCmd (int64 productId)

                Cmd.batch [ productsCmd; basicProductCmd; detailCmd ]
            | Route.WithParam(resource, id) ->
                // リソースデータの読み込みコマンド
                printfn "Resource parameter: %s, ID: %s" resource id

                if resource = "product" || resource = "products" then
                    // 製品の詳細を取得するためのコマンド
                    match System.Int64.TryParse id with
                    | true, productId ->
                        // 基本情報と詳細情報の両方を取得
                        Cmd.batch [ loadProductByIdCmd productId; loadProductDetailByIdCmd productId ]
                    | _ -> Cmd.none
                elif resource = "user" || resource = "users" then
                    // ユーザーの詳細を取得するためのコマンド
                    match System.Int64.TryParse id with
                    | true, userId -> loadUserByIdCmd userId
                    | _ -> Cmd.none
                else
                    Cmd.none
            | Route.WithQuery(basePath, queries) ->
                // クエリパラメータの処理
                printfn "Base path: %s with %d query parameters" basePath (Map.count queries)
                Cmd.none
            | Route.NotFound ->
                // 404エラー通知を表示
                let notification = warning "ページが見つかりません"
                Cmd.ofMsg (NotificationMsg(Add notification))
            | _ -> Cmd.none

        { model with
            CurrentRoute = route
            CurrentTab = updatedTab },
        cmd
