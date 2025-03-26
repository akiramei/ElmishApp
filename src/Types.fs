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

// アプリケーションのメッセージ
type Msg =
    | NavigateTo of Tab
    | IncrementCounter
    | DecrementCounter
    // カスタムメッセージを受け取るための汎用的なメッセージタイプ
    | CustomMsg of string * obj
    // 通知関連メッセージ
    | SetNotification of NotificationLevel * string
    | ClearNotification
    | NotificationTick of DateTime
    // 後方互換性のために残す（内部ではSetNotificationにマッピング）
    | SetError of string
    | ClearError
    // プラグイン関連メッセージ
    | PluginTabAdded of string
    | PluginRegistered of PluginDefinition
    | PluginsLoaded

// 通知状態管理
type NotificationState =
    { HasNotification: bool
      Level: NotificationLevel option
      Message: string option
      ErrorCode: string option // 一部のエラーでのみ使用
      Source: string option // 通知の発生源（コアかプラグインか）
      CreatedAt: System.DateTime option } // 通知が表示された時間

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
        { HasNotification = false
          Level = None
          Message = None
          ErrorCode = None
          Source = None
          CreatedAt = None }
      RegisteredPluginIds = []
      LoadingPlugins = false }

// 後方互換性のために必要な元のErrorState型
type ErrorState =
    { HasError: bool
      Message: string option
      ErrorCode: string option
      Source: string option }

// 後方互換性のために、NotificationStateをErrorStateとして扱うためのヘルパー
module CompatibilityHelpers =
    // NotificationStateからErrorStateプロパティを取得するヘルパープロパティ
    type Model with
        member this.ErrorState: ErrorState =
            { HasError =
                this.NotificationState.HasNotification
                && this.NotificationState.Level = Some Error
              Message = this.NotificationState.Message
              ErrorCode = this.NotificationState.ErrorCode
              Source = this.NotificationState.Source }

    // モデルからErrorStateを取得する関数（Interop.fsで使用）
    let getErrorState (model: Model) : ErrorState =
        { HasError =
            model.NotificationState.HasNotification
            && model.NotificationState.Level = Some Error
          Message = model.NotificationState.Message
          ErrorCode = model.NotificationState.ErrorCode
          Source = model.NotificationState.Source }
