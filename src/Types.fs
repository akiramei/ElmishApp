// Types.fs
module App.Types

open System

// アプリケーションのタブ定義
type Tab =
    | Home
    | Counter
    // カスタムタブのために拡張可能なタイプ
    | CustomTab of string

// プラグイン定義タイプ
type PluginDefinition =
    { Id: string
      Name: string
      Version: string
      Dependencies: string list
      Compatibility: string }

// 通知レベルの定義
type NotificationLevel =
    | Information
    | Warning
    | Error

// 通知の一意なID
type NotificationId = System.Guid

// 単一の通知
type Notification =
    { Id: NotificationId
      Level: NotificationLevel
      Message: string
      Details: string option // 詳細情報（オプション）
      Source: string option // 通知の発生源
      Metadata: Map<string, obj> // 追加メタデータ（柔軟な拡張用）
      CreatedAt: System.DateTime
      AutoDismiss: bool // 自動クリア対象かどうか
      ExpiresAfter: float option } // 秒単位、Noneの場合は永続的

// 通知管理システム
type NotificationState =
    { Notifications: Notification list
      LastUpdated: System.DateTime option }
// Types.fs
type NotificationMsg =
    | Add of Notification
    | Remove of NotificationId
    | ClearAll
    | ClearByLevel of NotificationLevel
    | Tick of System.DateTime

// アプリケーションのメッセージ
type Msg =
    | NavigateTo of Tab
    | IncrementCounter
    | DecrementCounter
    // カスタムメッセージを受け取るための汎用的なメッセージタイプ
    | CustomMsg of string * obj
    // 通知関連メッセージ
    | NotificationMsg of NotificationMsg
    // プラグイン関連メッセージ
    | PluginTabAdded of string
    | PluginRegistered of PluginDefinition
    | PluginsLoaded

// アプリケーションのモデル
type Model =
    { CurrentTab: Tab
      Counter: int
      Message: string
      // カスタム拡張用のデータストア
      CustomState: Map<string, obj>
      // 通知情報（旧ErrorStateの拡張版）
      NotificationState: NotificationState
      // プラグイン情報
      RegisteredPluginIds: string list
      LoadingPlugins: bool }

// 初期状態
let init () =
    { CurrentTab = Home
      Counter = 0
      Message = "Welcome to the F# + Fable + Feliz + Elmish app!"
      CustomState = Map.empty
      NotificationState =
        { Notifications = List.empty
          LastUpdated = None }
      RegisteredPluginIds = []
      LoadingPlugins = false }
