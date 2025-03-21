// Subscription.fs
module App.Subscription

open Fable.Core
open Fable.Core.JsInterop
open App.Types
open App.Interop
open App.PluginLoader
open App.Plugins
open Elmish

// 型を合わせたラッパー関数（トップレベル）
let wrapDispatchForPlugin (dispatch: Msg -> unit) (msg: obj) : unit = dispatch (msg |> unbox<Msg>)

// プラグイン登録用のラッパー関数（トップレベル）- 型を修正
let wrapPluginRegistration (plugin: obj) (jsDispatch: obj) (originalFnObj: obj) : bool =
    // オブジェクトから関数に変換
    let originalFn = unbox<obj -> (Msg -> unit) option -> bool> originalFnObj

    let dispatchOption =
        if isNull jsDispatch then
            None
        else
            Some(fun (msg: Msg) -> (unbox<obj -> unit> jsDispatch) (box msg))

    originalFn plugin dispatchOption

// JavaScriptグローバル関数の登録（トップレベル）- 型を修正
[<Emit("window.registerFSharpPlugin = function(plugin) { return $0(plugin, window.appPluginDispatch, $1); }")>]
let setupGlobalRegistration (wrapper: obj -> obj -> obj -> bool) (originalFn: obj) : unit = jsNative

// プラグインローダーサブスクリプション
let pluginLoader =
    let start dispatch =
        // プラグインのディスパッチを公開
        let pluginDispatch = wrapDispatchForPlugin dispatch
        exposePluginDispatch pluginDispatch

        // F#側のdispatchをJavaScript側に公開
        let jsDispatch =
            fun (msg: obj) ->
                // JavaScript配列のハンドリング
                printfn "Received message from JS: %A" msg

                if Fable.Core.JS.Constructors.Array.isArray msg then
                    let msgArray = msg :?> obj[]
                    printfn "Message is an array with length: %d" msgArray.Length

                    if msgArray.Length = 2 then
                        let msgType = string msgArray.[0]
                        let payload = msgArray.[1]
                        printfn "Dispatching CustomMsg with type: %s and payload: %A" msgType payload
                        dispatch (CustomMsg(msgType, payload))
                    else
                        printfn "Message array has unexpected length: %d" msgArray.Length
                else
                    printfn "Received unknown message format: %A" msg

        // グローバルにdispatch関数を公開
        exposeDispatch jsDispatch

        // トップレベル関数を使ってグローバルな登録関数を設定
        // 関数をオブジェクトとしてboxする
        setupGlobalRegistration wrapPluginRegistration (box registerPluginFromJs)

        // Loading状態を設定
        dispatch (SetError "Loading plugins...")

        // プラグインを読み込む
        async {
            let! pluginsLoaded = loadAllPlugins ()

            if pluginsLoaded then
                printfn "All plugins loaded successfully"
                dispatch ClearError
                dispatch PluginsLoaded
            else
                // エラーメッセージをディスパッチ
                dispatch (SetError "Some plugins failed to load. Some features may be unavailable.")
        }
        |> Async.StartImmediate

        // サブスクリプションの無効化ロジック
        { new System.IDisposable with
            member _.Dispose() =
                // プラグインローダーの後処理がある場合はここに実装
                printfn "Plugin loader subscription disposed" }

    start
