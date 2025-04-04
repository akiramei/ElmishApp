// Interop.fs - 新しいモデル構造に対応した更新版
module App.Interop

open Fable.Core.JsInterop
open Feliz
open App.Types
open App.JsUtils
open App.Plugins

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
            | Tab.Admin -> "Admin" // 管理者タブを追加
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
                    | Failed err -> "failed"

                userObj?Users <-
                    match model.ApiData.UserData.Users with
                    | Success users -> users |> List.toArray
                    | _ -> [||]

                userObj
            // ApiDataの取得時に適切に型チェック
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
                    | _ -> [||] // 空配列を返すように

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
// プラグイン状態構造を維持するように更新
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

        // カスタム状態の取得 (プラグイン名前空間を維持)
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

/// JavaScript側からのメッセージをF#のMsg型に変換する関数
let convertJsMessageToFSharpMsg (msg: obj) (dispatch: Msg -> unit) : unit =
    try
        // メッセージのデバッグ出力
        printfn "JS message received: %A" msg

        // メッセージの形式に応じた処理
        if Fable.Core.JS.Constructors.Array.isArray msg then
            // 配列形式のメッセージ処理
            let msgArray = msg :?> obj[]

            // 配列の長さチェック
            if msgArray.Length >= 2 then
                let msgType = string msgArray.[0]
                let payload = msgArray.[1]

                // 管理者関連メッセージの識別と処理を追加
                if msgType.StartsWith("Admin") then
                    match msgType with
                    | "AdminLoadData" -> dispatch (AdminMsg LoadAdminData)
                    | "AdminExportProducts" -> dispatch (AdminMsg ExportProducts)
                    | "AdminRunDiagnostic" -> dispatch (AdminMsg RunSystemDiagnostic)
                    | _ -> dispatch (CustomMsg(msgType, payload))
                else
                    dispatch (CustomMsg(msgType, payload))
            else if msgArray.Length = 1 then
                // 1要素だけの場合はペイロードなしとして処理
                let msgType = string msgArray.[0]
                dispatch (CustomMsg(msgType, createEmptyJsObj ()))
            else
                // 空配列など想定外の形式
                printfn "Invalid message array format: %A" msg
        else if jsTypeof msg = "string" then
            // 文字列メッセージ
            let msgType = unbox<string> msg
            dispatch (CustomMsg(msgType, null))
        else if jsTypeof msg = "object" && not (isNull msg) then
            // オブジェクト形式の場合、可能ならtypeとpayloadを抽出
            try
                let msgType = safeGet msg "type"
                let payload = safeGet msg "payload"

                if not (isNullOrUndefined msgType) then
                    // 管理者関連メッセージの識別と処理を追加
                    let msgTypeStr = string msgType

                    if msgTypeStr.StartsWith("Admin") then
                        match msgTypeStr with
                        | "AdminLoadData" -> dispatch (AdminMsg LoadAdminData)
                        | "AdminExportProducts" -> dispatch (AdminMsg ExportProducts)
                        | "AdminRunDiagnostic" -> dispatch (AdminMsg RunSystemDiagnostic)
                        | _ -> dispatch (CustomMsg(msgTypeStr, payload))
                    else
                        dispatch (CustomMsg(msgTypeStr, payload))
                else
                    printfn "Unable to process the object message: %A" msg
            with ex ->
                printfn "Error parsing object message: %s" ex.Message
        else
            // その他の未知の形式
            printfn "Unable to process the message: %A" msg
    with ex ->
        printfn "Error in message conversion: %s" ex.Message
        printfn "Stack trace: %s" ex.StackTrace

/// JavaScript側の関数に渡すための安全なdispatch関数を作成
let createJsDispatchFunction (dispatch: Msg -> unit) : obj =
    let jsDispatch = fun (msg: obj) -> convertJsMessageToFSharpMsg msg dispatch
    jsDispatch

// 追加する関数：複数引数を持つJavaScript関数を呼び出すヘルパー
let callJsFunctionWithArgs (func: obj) (args: obj) : Feliz.ReactElement =
    if isJsFunction func then
        try
            // 関数を引数付きで呼び出す
            callJsFunction func args |> unbox
        with ex ->
            printfn "Error calling JS function with args: %s" ex.Message
            // エラー表示用のフォールバックコンポーネント
            Html.div
                [ prop.className "p-3 bg-red-100 text-red-700 rounded"
                  prop.children [ Html.span [ prop.text "Error in plugin view" ] ] ]
    else
        Html.div
            [ prop.className "p-3 bg-yellow-100 text-yellow-700 rounded"
              prop.children [ Html.span [ prop.text "Plugin view is not a function" ] ] ]

/// getCustomView関数を修正 - argsオブジェクトを常に渡す
let getCustomView (viewName: string) (model: Model) (dispatch: Msg -> unit) : Feliz.ReactElement option =
    let jsModel = convertModelToJS model
    let args = createEmptyJsObj ()
    args?model <- jsModel
    args?dispatch <- createJsDispatchFunction dispatch

    // 既存のカスタムビュー取得処理を呼び出す
    getCustomView viewName args

// カスタムタブの取得
let getAvailableCustomTabs () = getAvailableCustomTabs ()

// JavaScript側のカスタムコマンドハンドラーを呼び出す
let executeCustomCmd (cmdType: string) (payload: obj) : unit = executeCustomCmd cmdType payload

// カスタム更新ハンドラーの呼び出し - 完全な変換を行う
let applyCustomUpdate (msgType: string) (payload: obj) (model: Model) : Model =
    printfn "Applying custom update for %s" msgType

    // モデルをJavaScript形式に変換
    let jsModel = convertModelToJS model

    // カスタム更新ハンドラーを呼び出す
    let updatedJsModel = applyCustomUpdates msgType payload jsModel

    // 更新されたJSモデルをF#モデルに変換
    let updatedModel = convertJsModelToFSharp updatedJsModel model

    printfn
        "Updated model Counter: %d, CustomState count: %d"
        updatedModel.CounterState.Counter
        updatedModel.CustomState.Count

    updatedModel
