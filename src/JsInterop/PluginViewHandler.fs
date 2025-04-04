// PluginViewHandler.fs
// プラグインのビュー処理を担当

module App.PluginViewHandler

open Fable.Core.JsInterop
open Feliz
open App.Types
open App.JsBasicTypes
open App.JsCore
open App.PluginRegistry
open App.ModelConverter
open App.MessageBridge

/// 追加する関数：複数引数を持つJavaScript関数を呼び出すヘルパー
let callJsFunctionWithArgs (func: obj) (args: obj) : ReactElement =
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
let getCustomView (viewName: string) (model: Model) (dispatch: Msg -> unit) : ReactElement option =
    let jsModel = convertModelToJS model
    let args = createEmptyJsObj ()
    args?model <- jsModel
    args?dispatch <- createJsDispatchFunction dispatch

    // PluginRegistryを利用して登録済みプラグインからビューを取得
    getCustomView viewName args

/// 既存タブ用プラグインの種類 (タブID)
type TabPluginType =
    | HomeTab
    | CounterTab

    member this.AsString() =
        match this with
        | HomeTab -> "home"
        | CounterTab -> "counter"

/// タブのデフォルトレンダリング関数の型
type TabRendererFunc = unit -> ReactElement

/// JavaScript用の引数オブジェクトを作成する関数
let createJsFunctionArgs (jsModel: obj) (dispatch: Msg -> unit) (defaultRendererFn: unit -> ReactElement) : obj =
    let args = createEmptyJsObj ()
    args?model <- jsModel
    args?defaultRenderer <- defaultRendererFn
    args?dispatch <- createJsDispatchFunction dispatch
    args

/// 既存タブのレンダリング関数をプラグインで装飾するための関数
let tryGetTabPluginDecorator
    (tabType: TabPluginType)
    (model: Model)
    (dispatch: Msg -> unit)
    (defaultRenderer: TabRendererFunc)
    : ReactElement option =
    let tabId = tabType.AsString()
    let pluginId = sprintf "%s-tab-plugin" tabId

    // 全プラグインからタブ装飾用ビューを検索
    let mutable result = None

    for KeyValue(_, plugin) in registeredPlugins do
        // タブ固有の装飾ビューを探す
        match Map.tryFind pluginId plugin.Views with
        | Some viewFn ->
            try
                // デフォルトのレンダリング関数も引数として渡す
                // JavaScriptからは args オブジェクトで引数を受け取ることを想定
                let jsModel = convertModelToJS model
                // ここでJavaScript側関数に追加の引数を渡す
                result <- Some(viewFn (createJsFunctionArgs jsModel dispatch (fun () -> defaultRenderer ())))
            with ex ->
                logPluginError plugin.Definition.Id (sprintf "tab decorator plugin: %s" ex.Message) ex
        | None -> ()

    result

/// 装飾されたHomeタブのビューを取得
let decorateHomeView (model: Model) (defaultRenderer: unit -> ReactElement) : ReactElement =
    match tryGetTabPluginDecorator HomeTab model ignore defaultRenderer with
    | Some decoratedView -> decoratedView
    | None -> defaultRenderer ()

/// 任意のタブのビューを装飾する汎用関数
let decorateTabView
    (tabType: TabPluginType)
    (model: Model)
    (dispatch: Msg -> unit)
    (defaultRenderer: unit -> ReactElement)
    : ReactElement =
    match tryGetTabPluginDecorator tabType model dispatch defaultRenderer with
    | Some decoratedView -> decoratedView
    | None -> defaultRenderer ()
