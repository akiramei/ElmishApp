// Subscription.fs
module App.Subscription

open System
open Fable.Core
open App.Types
open App.Router
open App.Notifications
open App.MessageBridge
open App.Interop

// プラグインローダーサブスクリプション
let pluginLoader =
    let start dispatch =
        // プラグインのディスパッチを公開（安全なラッパー関数を使用）
        let pluginDispatch = createJsDispatchFunction dispatch

        // Store the registration function and set up the global registration function
        storeRegisterPluginFunction registerPluginFromJs
        setupGlobalRegistration ()

        // 読み込み通知の作成 - 自動消去をオフにする
        let loadingNotification = info "Loading plugins..." |> withAutoDismiss false // 自動消去をオフにする

        // 通知IDを保存して後で明示的に削除できるようにする
        let notificationId = loadingNotification.Id

        // プラグインロード中の通知を設定
        dispatch (NotificationMsg(Add loadingNotification))

        // プラグインを読み込む
        async {
            let! pluginsLoaded = loadAllPlugins ()

            // まず読み込み中の通知を削除
            dispatch (NotificationMsg(Remove(notificationId)))

            if pluginsLoaded then
                printfn "All plugins loaded successfully"
                dispatch (PluginMsg PluginsLoaded)
            else
                // 警告レベルでの通知
                dispatch (
                    NotificationMsg(Add(warning "Some plugins failed to load. Some features may be unavailable."))
                )
        }
        |> Async.StartImmediate

        // サブスクリプションの無効化ロジック
        { new IDisposable with
            member _.Dispose() =
                // プラグインローダーの後処理がある場合はここに実装
                printfn "Plugin loader subscription disposed" }

    start

// Subscription.fs
let notificationTimer =
    let start dispatch =
        let timerId =
            JS.setInterval (fun _ -> dispatch (NotificationMsg(Tick DateTime.Now))) 1000

        { new IDisposable with
            member _.Dispose() = JS.clearInterval timerId }

    start
