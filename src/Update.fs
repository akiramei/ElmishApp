// Update.fs
module App.Update

open Elmish
open App.Types
open App.Router
open App.Notifications
open App.Interop
open App.UpdateCounterState
open App.UpdatePluginState

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

    // update 関数内に追加
    | NavigateTo tab ->
        // 既存のタブハンドリングを維持しながら、ルーティングも更新
        let route = tabToRoute tab
        Router.navigateTo route

        { model with
            CurrentTab = tab
            CurrentRoute = route },
        // Cmd.ofMsg (UpdateUrl route)
        Cmd.none

    // 新しいRouteChangedメッセージハンドラー
    | RouteChanged route ->
        // URLが変更されたときの処理
        let tabOption = routeToTab route
        let updatedTab = tabOption |> Option.defaultValue model.CurrentTab

        { model with
            CurrentRoute = route
            CurrentTab = updatedTab },

        // 必要なら新しいルートに応じた追加処理
        match route with
        | Route.WithParam(resource, id) ->
            // リソースデータの読み込みコマンドなど
            printfn "Resource parameter: %s, ID: %s" resource id
            Cmd.none
        | Route.WithQuery(basePath, queries) ->
            // クエリパラメータの処理
            printfn "Base path: %s with %d query parameters" basePath (Map.count queries)
            Cmd.none
        | Route.NotFound ->
            // 404エラー通知を表示
            let notification = Notifications.warning "ページが見つかりません"
            Cmd.ofMsg (NotificationMsg(Add notification))
        | _ -> Cmd.none
