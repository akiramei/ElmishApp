// Subscription.fs
module App.Subscription

open System
open Fable.Core
open App.Types
open App.JsUtils
open App.PluginLoader
open App.Plugins
open App.Interop

// プラグインローダーサブスクリプション
let pluginLoader =
    let start dispatch =
        // プラグインのディスパッチを公開（安全なラッパー関数を使用）
        let pluginDispatch = createJsDispatchFunction dispatch

        // Store the registration function and set up the global registration function
        storeRegisterPluginFunction registerPluginFromJs
        setupGlobalRegistration ()

        // プラグインロード中の通知を設定（情報レベル）
        dispatch (SetNotification(Information, "Loading plugins..."))

        // プラグインを読み込む
        async {
            let! pluginsLoaded = loadAllPlugins ()

            if pluginsLoaded then
                printfn "All plugins loaded successfully"
                dispatch ClearNotification
                dispatch PluginsLoaded
            else
                // 警告レベルでの通知
                dispatch (SetNotification(Warning, "Some plugins failed to load. Some features may be unavailable."))
        }
        |> Async.StartImmediate

        // サブスクリプションの無効化ロジック
        { new System.IDisposable with
            member _.Dispose() =
                // プラグインローダーの後処理がある場合はここに実装
                printfn "Plugin loader subscription disposed" }

    start

// 通知の自動クリアを管理するサブスクリプション
let notificationTimer =
    let start dispatch =
        // サブスクリプションの開始
        printfn "Notification timer subscription started"

        let timerId =
            JS.setInterval (fun _ -> dispatch (NotificationTick DateTime.Now)) 1000

        // サブスクリプションの無効化ロジック
        { new IDisposable with
            member _.Dispose() =
                // タイマーをクリア
                JS.clearInterval timerId
                printfn "Notification timer subscription disposed" }

    // サブスクリプション関数を返す
    start
