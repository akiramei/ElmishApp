// Subscription.fs
module App.Subscription

open Fable.Core
open Fable.Core.JsInterop
open App.Types
open App.Interop // Interopモジュールを使用
open App.PluginLoader
open Elmish

// プラグインローダーサブスクリプション
let pluginLoader =
    let start dispatch =
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

        // プラグインを読み込む
        async {
            let! pluginsLoaded = loadAllPlugins ()

            if pluginsLoaded then
                printfn "All plugins loaded successfully"
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
