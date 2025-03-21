// Subscription.fs
module App.Subscription

open Fable.Core
open App.Types
open App.Interop
open App.PluginLoader
open App.Plugins

// Store the registerPluginFromJs function directly in a global variable
[<Emit("window._fsharpRegisterPluginFn = $0")>]
let storeRegisterPluginFunction (fn: obj -> (Msg -> unit) option -> bool) : unit = jsNative

// Set up the global registration function that will call our stored function
[<Emit("""
window.registerFSharpPlugin = function(plugin) {
    // Create a dispatch option based on whether appPluginDispatch exists
    var dispatchOption = window.appPluginDispatch 
        ? function(msg) { window.appPluginDispatch(msg); return true; } 
        : null;
    
    try {
        return window._fsharpRegisterPluginFn(plugin, dispatchOption);
    } catch (error) {
        console.error("Error registering plugin:", error);
        return false;
    }
}
""")>]
let setupGlobalRegistration () : unit = jsNative

// プラグインローダーサブスクリプション
let pluginLoader =
    let start dispatch =
        // プラグインのディスパッチを公開（より安全なラッパー関数）
        let pluginDispatch =
            fun (msg: obj) ->
                try
                    dispatch (unbox<Msg> msg)
                with ex ->
                    printfn "Error in plugin dispatch: %s" ex.Message

        exposePluginDispatch pluginDispatch

        // F#側のdispatchをJavaScript側に公開（改善版）
        let jsDispatch =
            fun (msg: obj) ->
                try
                    // JavaScript配列のハンドリング
                    printfn "Received message from JS: %A" msg

                    if Fable.Core.JS.Constructors.Array.isArray msg then
                        let msgArray = msg :?> obj[]
                        printfn "Message is an array with length: %d" msgArray.Length

                        if msgArray.Length = 2 then
                            let msgType = string msgArray.[0]
                            let payload = msgArray.[1]
                            printfn "Dispatching CustomMsg with type: %s and payload: %A" msgType payload

                            // CustomMsgを使用してディスパッチ
                            dispatch (CustomMsg(msgType, payload))
                        else
                            printfn "Message array has unexpected length: %d" msgArray.Length
                    else if jsTypeof msg = "string" then
                        // 文字列の場合はそのままCustomMsgとして処理
                        let msgType = unbox<string> msg
                        printfn "Dispatching string message: %s" msgType
                        dispatch (CustomMsg(msgType, null))
                    else
                        printfn "Received unknown message format: %A" msg
                with ex ->
                    printfn "Error in JS dispatch: %s" ex.Message
                    printfn "Stack trace: %s" ex.StackTrace

        // グローバルにdispatch関数を公開
        exposeDispatch jsDispatch

        // Store the registration function and set up the global registration function
        storeRegisterPluginFunction registerPluginFromJs
        setupGlobalRegistration ()

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
