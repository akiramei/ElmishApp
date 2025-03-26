// Update.fs
module App.Update

open System
open Elmish
open App.Types
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

    | SetNotification(level, message) ->
        let notificationState =
            { HasNotification = true
              Level = Some level
              Message = Some message
              ErrorCode = None
              Source = Some "core"
              CreatedAt = Some DateTime.Now }

        { model with
            NotificationState = notificationState },
        Cmd.none

    | ClearNotification ->
        let notificationState =
            { HasNotification = false
              Level = None
              Message = None
              ErrorCode = None
              Source = None
              CreatedAt = None }

        { model with
            NotificationState = notificationState },
        Cmd.none

    | NotificationTick now ->
        match model.NotificationState with
        | { HasNotification = true
            CreatedAt = Some createdAt } ->

            let elapsed = now - createdAt

            if elapsed.TotalSeconds >= 3.0 then
                model, Cmd.ofMsg ClearNotification
            else
                model, Cmd.none

        | _ -> model, Cmd.none

    // 後方互換性のためのメソッド
    | SetError errorMsg ->
        // 内部的にSetNotificationを使用
        let notificationState =
            { HasNotification = true
              Level = Some Error
              Message = Some errorMsg
              ErrorCode = None
              Source = Some "core"
              CreatedAt = Some System.DateTime.Now }

        { model with
            NotificationState = notificationState },
        Cmd.none

    | ClearError ->
        // 内部的にClearNotificationと同じ
        let notificationState =
            { HasNotification = false
              Level = None
              Message = None
              ErrorCode = None
              Source = None
              CreatedAt = None }

        { model with
            NotificationState = notificationState },
        Cmd.none

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
        // プラグインのロードが完了したことを通知（情報レベル）
        let notificationState =
            { HasNotification = true
              Level = Some Information
              Message = Some "All plugins loaded successfully"
              ErrorCode = None
              Source = Some "core"
              CreatedAt = Some System.DateTime.Now }

        let model =
            { model with
                LoadingPlugins = false
                NotificationState = notificationState }

        model, Cmd.none

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

            let notificationState =
                { HasNotification = true
                  Level = Some Error
                  Message = Some(sprintf "Error processing custom message: %s" ex.Message)
                  ErrorCode = Some "CUSTOM_MSG_ERROR"
                  Source = Some "core"
                  CreatedAt = Some System.DateTime.Now }

            { model with
                NotificationState = notificationState },
            Cmd.none
