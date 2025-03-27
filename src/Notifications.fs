// Notifications.fs - 通知システム専用モジュール

module App.Notifications

open System
open Elmish
open App.Types

// 通知ファクトリ関数
let createNotification level message =
    { Id = Guid.NewGuid()
      Level = level
      Message = message
      Details = None
      Source = Some "core"
      Metadata = Map.empty
      CreatedAt = DateTime.Now
      AutoDismiss = true
      ExpiresAfter = Some 3.0 }

// 便利なヘルパー関数
let info message = createNotification Information message

let infoPersistent message =
    { createNotification Information message with
        AutoDismiss = false
        ExpiresAfter = None }

let warning message =
    { createNotification Warning message with
        ExpiresAfter = Some 5.0 }

let warningPersistent message =
    { createNotification Warning message with
        AutoDismiss = false
        ExpiresAfter = None }

let error message =
    { createNotification Error message with
        ExpiresAfter = Some 8.0 }

let criticalError message =
    { createNotification Error message with
        AutoDismiss = false
        ExpiresAfter = None }

// 詳細情報の追加
let withDetails details notification =
    { notification with
        Details = Some details }

// メタデータの追加
let withMetadata key value notification =
    { notification with
        Metadata = notification.Metadata.Add(key, value) }

let withAutoDismiss autoDismiss notification =
    { notification with
        AutoDismiss = autoDismiss }

// 発生源の指定
let fromSource source notification =
    { notification with
        Source = Some source }

// 通知専用のupdate関数
let update (msg: NotificationMsg) (state: NotificationState) : NotificationState * Cmd<NotificationMsg> =
    match msg with
    | Add notification ->
        let updatedNotifications = notification :: state.Notifications

        { state with
            Notifications = updatedNotifications
            LastUpdated = Some System.DateTime.Now },
        Cmd.none

    | Remove id ->
        let updatedNotifications = state.Notifications |> List.filter (fun n -> n.Id <> id)

        { state with
            Notifications = updatedNotifications
            LastUpdated = Some System.DateTime.Now },
        Cmd.none

    | ClearAll ->
        { state with
            Notifications = []
            LastUpdated = Some System.DateTime.Now },
        Cmd.none

    | ClearByLevel level ->
        let updatedNotifications =
            state.Notifications |> List.filter (fun n -> n.Level <> level)

        { state with
            Notifications = updatedNotifications
            LastUpdated = Some System.DateTime.Now },
        Cmd.none

    | Tick now ->
        let expiredNotifications =
            state.Notifications
            |> List.filter (fun n ->
                n.AutoDismiss
                && match n.ExpiresAfter with
                   | Some seconds -> (now - n.CreatedAt).TotalSeconds >= seconds
                   | None -> false)
            |> List.map (fun n -> n.Id)

        if List.isEmpty expiredNotifications then
            state, Cmd.none
        else
            let updatedNotifications =
                state.Notifications
                |> List.filter (fun n -> not (List.contains n.Id expiredNotifications))

            { state with
                Notifications = updatedNotifications
                LastUpdated = Some now },
            Cmd.none
