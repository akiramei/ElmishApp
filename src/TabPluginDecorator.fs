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

    // JavaScript側に渡すdispatch関数を作成
    // 型変換を行い、ラップした関数をわたす
    let jsDispatch =
        fun (msg: obj) ->
            try
                // メッセージのデバッグ出力
                printfn "Dispatch from plugin: %A" msg

                // CustomMsgとして処理
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
                printfn "Error in plugin dispatch: %s" ex.Message
                printfn " Unable to process the message: %A" msg

    args?dispatch <- jsDispatch
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
