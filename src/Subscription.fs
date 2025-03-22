// Subscription.fs - メッセージ処理修正版
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
                    // メッセージのデバッグ出力
                    printfn "New message: %A" msg

                    // CustomMsgとして処理
                    if Fable.Core.JS.Constructors.Array.isArray msg then
                        // 配列形式のメッセージ処理を強化
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
                    printfn "Stack trace: %s" ex.StackTrace

        // Dispatchをプラグインヘルパーライブラリにセット
        setPluginDispatch pluginDispatch

        // レガシーサポート：F#側のdispatchをJavaScript側に公開（改善版）
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

        // レガシーサポート：グローバルにdispatch関数を公開
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
