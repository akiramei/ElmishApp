// Interop.fs
module App.Interop

open Fable.Core
open Fable.Core.JsInterop
open App.Types
open App.Plugins

// 空のJavaScriptオブジェクトを作成する関数
[<Emit("{}")>]
let createEmptyJsObj () : obj = jsNative

// オブジェクトがnullかどうかを判定
[<Emit("$0 === null")>]
let isNull (obj: obj) : bool = jsNative

// オブジェクトがundefinedかどうかを判定
[<Emit("$0 === undefined")>]
let isUndefined (obj: obj) : bool = jsNative

// オブジェクトがnullまたはundefinedかどうかを判定
[<Emit("$0 == null")>]
let isNullOrUndefined (obj: obj) : bool = jsNative

// JSON文字列に変換
[<Emit("JSON.stringify($0, null, 2)")>]
let jsonStringify (obj: obj) : string = jsNative

// JSON文字列からオブジェクトに戻す
[<Emit("JSON.parse($0)")>]
let jsonParse (str: string) : obj = jsNative

// オブジェクトのプロパティをセーフにアクセスする
[<Emit("$0 && $0[$1]")>]
let safeGet (obj: obj) (prop: string) : obj = jsNative

// オブジェクトが関数かどうかを判定
[<Emit("typeof $0 === 'function'")>]
let isFunction (obj: obj) : bool = jsNative

// F#のMapをJavaScriptのプレーンオブジェクトに変換
[<Emit("Object.fromEntries(Array.from($0).map(([k, v]) => [k, v]))")>]
let mapToPlainJsObj (map: Map<string, obj>) : obj = jsNative

// JavaScriptのプレーンオブジェクトをF#のMapに変換
let plainJsObjToMap (jsObj: obj) : Map<string, obj> =
    if isNullOrUndefined jsObj then
        Map.empty<string, obj>
    else
        try
            let keys = Fable.Core.JS.Constructors.Object.keys (jsObj)
            let mutable map = Map.empty<string, obj>

            for key in keys do
                let value = jsObj?(key)

                if not (isUndefined value) then
                    map <- map.Add(key, value)

            map
        with ex ->
            printfn "Error converting JS object to Map: %s" ex.Message
            Map.empty<string, obj>

// F#のモデルをJavaScriptフレンドリーな形式に変換
let convertModelToJS (model: Model) : obj =
    let jsObj = createEmptyJsObj ()

    try
        // 基本プロパティをコピー
        jsObj?Counter <- model.Counter
        jsObj?Message <- model.Message

        // CurrentTabを文字列に変換
        jsObj?CurrentTab <-
            match model.CurrentTab with
            | Home -> "Home"
            | Counter -> "Counter"
            | CustomTab id -> sprintf "CustomTab_%s" id

        // CustomStateをJavaScriptオブジェクトに変換
        let customStateObj =
            if model.CustomState.IsEmpty then
                createEmptyJsObj ()
            else
                mapToPlainJsObj model.CustomState

        jsObj?CustomState <- customStateObj

        // エラー状態をコピー
        let errorStateObj = createEmptyJsObj ()
        errorStateObj?HasError <- model.ErrorState.HasError

        errorStateObj?Message <-
            match model.ErrorState.Message with
            | Some msg -> msg
            | None -> null

        errorStateObj?ErrorCode <-
            match model.ErrorState.ErrorCode with
            | Some code -> code
            | None -> null

        errorStateObj?Source <-
            match model.ErrorState.Source with
            | Some src -> src
            | None -> null

        jsObj?ErrorState <- errorStateObj

        // プラグイン情報を追加
        jsObj?RegisteredPluginIds <- model.RegisteredPluginIds |> List.toArray
        jsObj?LoadingPlugins <- model.LoadingPlugins

        jsObj
    with ex ->
        printfn "Error converting model to JS: %s" ex.Message
        jsObj

// JSモデルから新しいF#モデルを作成（更新版）- 型安全に変換
let convertJsModelToFSharp (jsModel: obj) (originalModel: Model) : Model =
    try
        // カウンターの取得
        let counter =
            let counterValue = safeGet jsModel "Counter"

            if
                not (isNullOrUndefined counterValue)
                && unbox<int> counterValue <> originalModel.Counter
            then
                unbox<int> counterValue
            else
                originalModel.Counter

        // メッセージの取得
        let message =
            let messageValue = safeGet jsModel "Message"

            if not (isNullOrUndefined messageValue) then
                unbox<string> messageValue
            else
                originalModel.Message

        // カスタム状態の取得
        let customState =
            let customStateObj = safeGet jsModel "CustomState"

            if not (isNullOrUndefined customStateObj) then
                plainJsObjToMap customStateObj
            else
                originalModel.CustomState

        // 新しいモデルを作成
        { originalModel with
            Counter = counter
            Message = message
            CustomState = customState }
    with ex ->
        printfn "Error converting JS model to F#: %s" ex.Message
        printfn "Stack trace: %s" ex.StackTrace
        originalModel

// JavaScript側のカスタムビュー関数を呼び出す (変換されたモデルを渡す)
let getCustomView (viewName: string) (model: Model) : Feliz.ReactElement option =
    let jsModel = convertModelToJS model
    Plugins.getCustomView viewName jsModel

// カスタムタブの取得
let getAvailableCustomTabs () = Plugins.getAvailableCustomTabs ()

// カスタムコンポーネントの初期化
[<Emit("window.initCustomComponents && window.initCustomComponents()")>]
let initCustomComponents () : unit = jsNative

// カスタムタブのID一覧を取得してF#のタイプに変換
let getAvailableCustomTabIds () =
    getAvailableCustomTabs ()
    |> List.map (fun tab ->
        match tab with
        | CustomTab id -> id
        | _ -> "")
    |> List.filter (fun id -> id <> "")

// JavaScript側のカスタムコマンドハンドラーを呼び出す
let executeCustomCmd (cmdType: string) (payload: obj) : unit =
    Plugins.executeCustomCmd cmdType payload

// F#側のdispatch関数をグローバルに公開
[<Emit("window.appDispatch = $0")>]
let exposeDispatch (dispatch: obj -> unit) : unit = jsNative

// カスタム更新ハンドラーの呼び出し（更新版）- 完全な変換を行うように修正
let applyCustomUpdate (msgType: string) (payload: obj) (model: Model) : Model =
    printfn "Applying custom update for %s" msgType

    // モデルをJavaScript形式に変換
    let jsModel = convertModelToJS model

    // カスタム更新ハンドラーを呼び出す
    let updatedJsModel = Plugins.applyCustomUpdates msgType payload jsModel

    // 更新されたJSモデルをF#モデルに変換
    let updatedModel = convertJsModelToFSharp updatedJsModel model

    printfn "Updated model Counter: %d, CustomState count: %d" updatedModel.Counter updatedModel.CustomState.Count

    updatedModel
