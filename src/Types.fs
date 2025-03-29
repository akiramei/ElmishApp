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

type LoadingPlugins =
    | Init
    | Loading
    | Done

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

// ルート定義
type Route =
    | Home
    | Counter
    | CustomTab of string
    | WithParam of string * string // resource * id
    | WithQuery of string * Map<string, string> // base path * query params
    | NotFound

type CounterMsg =
    | IncrementCounter
    | DecrementCounter

type PluginMsg =
    | PluginTabAdded of string
    | PluginRegistered of PluginDefinition
    | PluginsLoaded

// アプリケーションのメッセージ
type Msg =
    | NavigateTo of Tab
    | CounterMsg of CounterMsg
    | RouteChanged of Route
    // カスタムメッセージを受け取るための汎用的なメッセージタイプ
    | CustomMsg of string * obj
    // 通知関連メッセージ
    | NotificationMsg of NotificationMsg
    // プラグイン関連メッセージ
    | PluginMsg of PluginMsg

type HomeState = { Message: string }

type CounterState = { Counter: int }


type PluginState =
    { RegisteredPluginIds: string list
      LoadingPlugins: LoadingPlugins }

// アプリケーションのモデル
type Model =
    { CurrentRoute: Route
      CurrentTab: Tab
      CounterState: CounterState
      HomeState: HomeState
      // カスタム拡張用のデータストア
      CustomState: Map<string, obj>
      // 通知情報（旧ErrorStateの拡張版）
      NotificationState: NotificationState
      // プラグイン情報
      PluginState: PluginState }

// 初期状態
let init () =
    { CurrentRoute = Route.Home
      CurrentTab = Tab.Home
      CounterState = { Counter = 0 }
      HomeState = { Message = "Welcome to the F# + Fable + Feliz + Elmish app!" }
      CustomState = Map.empty
      NotificationState =
        { Notifications = List.empty
          LastUpdated = None }
      PluginState =
        { RegisteredPluginIds = []
          LoadingPlugins = LoadingPlugins.Init } }
