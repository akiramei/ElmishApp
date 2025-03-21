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

// JSON文字列に変換
[<Emit("JSON.stringify($0)")>]
let jsonStringify (obj: obj) : string = jsNative

// JSON文字列からオブジェクトに戻す
[<Emit("JSON.parse($0)")>]
let jsonParse (str: string) : obj = jsNative

// F#のMapをJavaScriptのプレーンオブジェクトに変換
[<Emit("Object.fromEntries(Array.from($0).map(([k, v]) => [k, v]))")>]
let mapToPlainJsObj (map: Map<string, obj>) : obj = jsNative

// JavaScriptのプレーンオブジェクトをF#のMapに変換
let plainJsObjToMap (jsObj: obj) : Map<string, obj> =
    if isNull jsObj then
        Map.empty<string, obj>
    else
        let keys = Fable.Core.JS.Constructors.Object.keys (jsObj)
        let mutable map = Map.empty<string, obj>

        for key in keys do
            map <- map.Add(key, jsObj?(key))

        map

// F#のモデルをJavaScriptフレンドリーな形式に変換
let convertModelToJS (model: Model) : obj =
    let jsObj = createEmptyJsObj ()

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
    let customStateObj = createEmptyJsObj ()

    for KeyValue(key, value) in model.CustomState do
        customStateObj?(key) <- value

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

    // デバッグ用にオブジェクト構造をログ出力
    printfn "Converted model to JS: %s" (jsonStringify jsObj)

    jsObj

// JSモデルから新しいF#モデルを作成（更新版）- Counterプロパティを扱うため完全な変換を行う
let convertJsModelToFSharp (jsModel: obj) (originalModel: Model) : Model =
    try
        // 必要なプロパティを取得
        let counter =
            if jsTypeof jsModel?Counter = "number" then
                unbox<int> jsModel?Counter
            else
                originalModel.Counter

        let message =
            if jsTypeof jsModel?Message = "string" then
                unbox<string> jsModel?Message
            else
                originalModel.Message

        let customState = plainJsObjToMap (jsModel?CustomState)

        // 新しいモデルを作成
        { originalModel with
            Counter = counter
            Message = message
            CustomState = customState }
    with ex ->
        printfn "Error converting JS model to F#: %s" ex.Message
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
