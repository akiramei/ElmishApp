// Interop.fs - 名前空間付き状態管理サポート
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
        jsObj?Counter <- model.CounterState.Counter
        jsObj?Message <- model.HomeState.Message

        // CurrentTabを文字列に変換
        jsObj?CurrentTab <-
            match model.CurrentTab with
            | Tab.Home -> "Home"
            | Tab.Counter -> "Counter"
            | Tab.CustomTab id -> sprintf "CustomTab_%s" id

        // CustomStateをJavaScriptオブジェクトに変換
        let customStateObj =
            if model.CustomState.IsEmpty then
                createEmptyJsObj ()
            else
                mapToPlainJsObj model.CustomState

        jsObj?CustomState <- customStateObj

        // プラグイン情報を追加
        jsObj?RegisteredPluginIds <- model.PluginState.RegisteredPluginIds |> List.toArray
        jsObj?LoadingPlugins <- model.PluginState.LoadingPlugins

        jsObj
    with ex ->
        printfn "Error converting model to JS: %s" ex.Message
        jsObj

// JSモデルから新しいF#モデルを作成（更新版）- 型安全に変換
// プラグイン状態構造を維持するように更新
let convertJsModelToFSharp (jsModel: obj) (originalModel: Model) : Model =
    try
        // カウンターの取得
        let counter =
            let counterValue = safeGet jsModel "Counter"

            if
                not (isNullOrUndefined counterValue)
                && unbox<int> counterValue <> originalModel.CounterState.Counter
            then
                unbox<int> counterValue
            else
                originalModel.CounterState.Counter

        // メッセージの取得
        let message =
            let messageValue = safeGet jsModel "Message"

            if not (isNullOrUndefined messageValue) then
                unbox<string> messageValue
            else
                originalModel.HomeState.Message

        // カスタム状態の取得 (プラグイン名前空間を維持)
        let customState =
            let customStateObj = safeGet jsModel "CustomState"

            if not (isNullOrUndefined customStateObj) then
                plainJsObjToMap customStateObj
            else
                originalModel.CustomState

        // 新しいモデルを作成
        { originalModel with
            CounterState = { Counter = counter }
            HomeState = { Message = message }
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
                    dispatch (CustomMsg(string msgType, payload))
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
