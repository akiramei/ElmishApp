// Interop.fs
module App.Interop

open Fable.Core
open Fable.Core.JsInterop
open App.Types

// 空のJavaScriptオブジェクトを作成する関数
[<Emit("{}")>]
let createEmptyJsObj () : obj = jsNative

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

    jsObj

// JavaScript側のカスタムビュー関数を呼び出す (変換されたモデルを渡す)
[<Emit("window.customViews && window.customViews[$0] ? window.customViews[$0]($1) : null")>]
let getCustomView (viewName: string) (props: obj) : Feliz.ReactElement = jsNative

// JavaScript側のカスタム更新関数を呼び出す
[<Emit("window.customUpdates && window.customUpdates[$0] ? window.customUpdates[$0]($1, $2 || {}) : $2")>]
let applyCustomUpdate (updateName: string) (msg: obj) (model: obj) : obj = jsNative

// F#側のdispatch関数をグローバルに公開
[<Emit("window.appDispatch = $0")>]
let exposeDispatch (dispatch: obj -> unit) : unit = jsNative

// カスタムタブの取得
[<Emit("window.customTabs ? window.customTabs : []")>]
let getCustomTabs () : string[] = jsNative

// カスタムコンポーネントの初期化
[<Emit("window.initCustomComponents && window.initCustomComponents()")>]
let initCustomComponents () : unit = jsNative

// カスタムタブのID一覧を取得してF#のタイプに変換
let getAvailableCustomTabs () =
    getCustomTabs () |> Array.map (fun tabId -> CustomTab tabId) |> Array.toList

// JavaScript側のカスタムコマンドハンドラーを呼び出す
[<Emit("window.customCmdHandlers && window.customCmdHandlers[$0] ? window.customCmdHandlers[$0]($1) : null")>]
let executeCustomCmd (cmdType: string) (payload: obj) : unit = jsNative

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

// オブジェクトがnullかどうかを判定
[<Emit("$0 === null")>]
let isNull (obj: obj) : bool = jsNative

// JSON文字列に変換
[<Emit("JSON.stringify($0)")>]
let jsonStringify (obj: obj) : string = jsNative

// JSON文字列からオブジェクトに戻す
[<Emit("JSON.parse($0)")>]
let jsonParse (str: string) : obj = jsNative
