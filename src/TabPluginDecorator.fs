// TabPluginDecorator.fs
// 既存タブをカスタマイズするためのデコレータ機能

module App.TabPluginDecorator

open Fable.Core.JsInterop
open App.Types
open App.JsUtils
open App.Interop
open App.Plugins

/// 既存タブ用プラグインの種類 (タブID)
type TabPluginType =
    | HomeTab
    | CounterTab

    member this.AsString() =
        match this with
        | HomeTab -> "home"
        | CounterTab -> "counter"

/// タブのデフォルトレンダリング関数の型
type TabRendererFunc = unit -> Feliz.ReactElement

/// JavaScript用の引数オブジェクトを作成する関数
let createJsFunctionArgs (jsModel: obj) (dispatch: Msg -> unit) (defaultRendererFn: unit -> Feliz.ReactElement) : obj =
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
    : Feliz.ReactElement option =
    let tabId = tabType.AsString()
    let pluginId = sprintf "%s-tab-plugin" tabId

    // PluginSystemを利用して登録済みプラグインからビューを取得
    let decoratorView =
        let mutable result = None

        // 全プラグインからタブ装飾用ビューを検索
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
                    printfn "Error in tab decorator plugin: %s" ex.Message
            | None -> ()

        result

    decoratorView

/// 装飾されたHomeタブのビューを取得
let decorateHomeView (model: Model) (defaultRenderer: unit -> Feliz.ReactElement) : Feliz.ReactElement =
    match tryGetTabPluginDecorator HomeTab model ignore defaultRenderer with
    | Some decoratedView -> decoratedView
    | None -> defaultRenderer ()

/// 任意のタブのビューを装飾する汎用関数
let decorateTabView
    (tabType: TabPluginType)
    (model: Model)
    (dispatch: Msg -> unit)
    (defaultRenderer: unit -> Feliz.ReactElement)
    : Feliz.ReactElement =
    match tryGetTabPluginDecorator tabType model dispatch defaultRenderer with
    | Some decoratedView -> decoratedView
    | None -> defaultRenderer ()
