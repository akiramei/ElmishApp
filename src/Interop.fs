// Interop.fs
module App.Interop

open Fable.Core
open Fable.Core.JsInterop
open App.Types

// JavaScript側のカスタムビュー関数を呼び出す
[<Emit("window.customViews && window.customViews[$0] ? window.customViews[$0]($1) : null")>]
let getCustomView (viewName: string) (props: obj) : Feliz.ReactElement = jsNative

// JavaScript側のカスタム更新関数を呼び出す
[<Emit("window.customUpdates && window.customUpdates[$0] ? window.customUpdates[$0]($1, $2) : $2")>]
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
