// Update.fs
module App.Update

open System
open Elmish
open App.Types
open App.Notifications
open App.Interop

// アプリケーションの状態更新関数
let update msg model =
    match msg with
    | NavigateTo tab -> { model with CurrentTab = tab }, Cmd.none

    | IncrementCounter ->
        { model with
            Counter = model.Counter + 1 },
        Cmd.none

    | DecrementCounter ->
        { model with
            Counter = model.Counter - 1 },
        Cmd.none

    // 通知メッセージはサブモジュールに委譲
    | NotificationMsg notificationMsg ->
        let (updatedNotificationState, cmd) =
            Notifications.update notificationMsg model.NotificationState

        { model with
            NotificationState = updatedNotificationState },
        Cmd.map NotificationMsg cmd

    | PluginTabAdded tabId ->
        printfn "Plugin tab added: %s" tabId
        // タブが追加されただけでは再レンダリングが発生するが特別な処理は不要
        model, Cmd.none

    | PluginRegistered definition ->
        printfn "Plugin registered: %s" definition.Id
        // 登録済みのプラグインIDリストを更新
        let updatedPluginIds =
            if model.RegisteredPluginIds |> List.contains definition.Id then
                model.RegisteredPluginIds
            else
                definition.Id :: model.RegisteredPluginIds

        { model with
            RegisteredPluginIds = updatedPluginIds },
        Cmd.none

    | PluginsLoaded ->
        printfn "All plugins loaded"
        let model = { model with LoadingPlugins = false }
        let notification = info "全プラグインの読み込みが完了しました"
        model, Cmd.ofMsg (NotificationMsg(Add notification))

    | CustomMsg(msgType, payload) ->
        printfn "Received CustomMsg: %s with payload %A" msgType payload

        try
            // カスタム更新ハンドラーを呼び出す
            let updatedModel = applyCustomUpdate msgType payload model
            updatedModel, Cmd.none
        with ex ->
            // エラーハンドリング
            printfn "Error in CustomMsg handling: %s" ex.Message
            printfn "Stack trace: %s" ex.StackTrace

            // エラー通知の作成 - メタデータを活用
            let notification =
                error (sprintf "カスタムメッセージ処理エラー: %s" ex.Message)
                |> withDetails ex.StackTrace
                |> withMetadata "messageType" msgType
                |> fromSource "CustomMsgHandler"

            model, Cmd.ofMsg (NotificationMsg(Add notification))
