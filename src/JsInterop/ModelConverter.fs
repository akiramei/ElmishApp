// ModelConverter.fs
// F#のモデルとJavaScriptオブジェクト間の変換を担当

module App.ModelConverter

open Fable.Core.JsInterop
open App.Types
open App.JsBasicTypes
open App.JsCore

// F#のモデルをJavaScriptフレンドリーな形式に変換
let convertModelToJS (model: Model) : obj =
    let jsObj = createEmptyJsObj ()

    try
        // 基本プロパティをコピー
        jsObj?CounterState <-
            let counterObj = createEmptyJsObj ()
            counterObj?Counter <- model.CounterState.Counter
            counterObj

        jsObj?HomeState <-
            let homeObj = createEmptyJsObj ()
            homeObj?Message <- model.HomeState.Message
            homeObj

        // CurrentTabを文字列に変換
        jsObj?CurrentTab <-
            match model.CurrentTab with
            | Tab.Home -> "Home"
            | Tab.Counter -> "Counter"
            | Tab.Products -> "Products"
            | Tab.Admin -> "Admin"
            | Tab.CustomTab id -> sprintf "CustomTab_%s" id

        // ApiDataを変換
        jsObj?ApiData <-
            let apiObj = createEmptyJsObj ()

            // UserData
            apiObj?UserData <-
                let userObj = createEmptyJsObj ()

                userObj?Status <-
                    match model.ApiData.UserData.Users with
                    | NotStarted -> "notStarted"
                    | Loading -> "loading"
                    | Success _ -> "success"
                    | Failed _ -> "failed"

                userObj?Users <-
                    match model.ApiData.UserData.Users with
                    | Success users -> users |> List.toArray
                    | _ -> [||]

                userObj

            // ProductData
            apiObj?ProductData <-
                let prodObj = createEmptyJsObj ()

                prodObj?Status <-
                    match model.ApiData.ProductData.Products with
                    | NotStarted -> "notStarted"
                    | Loading -> "loading"
                    | Success _ -> "success"
                    | Failed _ -> "failed"

                prodObj?Products <-
                    match model.ApiData.ProductData.Products with
                    | Success products -> products |> List.toArray
                    | _ -> [||]

                prodObj

            apiObj

        // CustomStateをJavaScriptオブジェクトに変換
        let customStateObj =
            if model.CustomState.IsEmpty then
                createEmptyJsObj ()
            else
                mapToPlainJsObj model.CustomState

        jsObj?CustomState <- customStateObj

        // プラグイン情報を追加
        jsObj?PluginState <-
            let pluginObj = createEmptyJsObj ()
            pluginObj?RegisteredPluginIds <- model.PluginState.RegisteredPluginIds |> List.toArray

            pluginObj?LoadingPlugins <-
                match model.PluginState.LoadingPlugins with
                | Init -> "init"
                | LoadingPlugins.Loading -> "loading"
                | Done -> "done"

            pluginObj

        // ProductsStateを追加
        jsObj?ProductsState <-
            let prodStateObj = createEmptyJsObj ()

            prodStateObj?PageInfo <-
                let pageObj = createEmptyJsObj ()
                pageObj?CurrentPage <- model.ProductsState.PageInfo.CurrentPage
                pageObj?PageSize <- model.ProductsState.PageInfo.PageSize
                pageObj?TotalItems <- model.ProductsState.PageInfo.TotalItems
                pageObj?TotalPages <- model.ProductsState.PageInfo.TotalPages
                pageObj

            prodStateObj?SelectedIds <- model.ProductsState.SelectedIds |> Set.toArray
            prodStateObj

        jsObj
    with ex ->
        printfn "Error converting model to JS: %s" ex.Message
        jsObj

// JSモデルから新しいF#モデルを作成 - 型安全に変換
let convertJsModelToFSharp (jsModel: obj) (originalModel: Model) : Model =
    try
        // CounterStateの取得
        let counterState =
            let counterStateObj = safeGet jsModel "CounterState"

            if not (isNullOrUndefined counterStateObj) then
                let counter = safeGet counterStateObj "Counter"

                if not (isNullOrUndefined counter) then
                    { Counter = unbox<int> counter }
                else
                    originalModel.CounterState
            else
                originalModel.CounterState

        // HomeStateの取得
        let homeState =
            let homeStateObj = safeGet jsModel "HomeState"

            if not (isNullOrUndefined homeStateObj) then
                let message = safeGet homeStateObj "Message"

                if not (isNullOrUndefined message) then
                    { Message = unbox<string> message }
                else
                    originalModel.HomeState
            else
                originalModel.HomeState

        // カスタム状態の取得
        let customState =
            let customStateObj = safeGet jsModel "CustomState"

            if not (isNullOrUndefined customStateObj) then
                plainJsObjToMap customStateObj
            else
                originalModel.CustomState

        // 新しいモデルを作成 - APIデータは更新しない
        { originalModel with
            CounterState = counterState
            HomeState = homeState
            CustomState = customState }
    with ex ->
        printfn "Error converting JS model to F#: %s" ex.Message
        printfn "Stack trace: %s" ex.StackTrace
        originalModel
