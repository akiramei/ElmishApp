// PluginTypes.fs
// プラグインシステムの内部実装詳細用の型定義

module App.PluginTypes

open Fable.Core
open Feliz
open App.Types // アプリケーションの主要な型を参照

// ========== プラグインシステム内部の型 ==========

// 登録済みプラグイン情報
type RegisteredPlugin =
    { Definition: PluginDefinition // App.Typesからの型を参照
      Views: Map<string, obj -> ReactElement>
      UpdateFunction: obj option
      CommandHandlers: Map<string, obj -> unit>
      Tabs: string list }

// プラグインの読み込み状態
type PluginLoadingState =
    | NotStarted
    | Loading
    | LoadedSuccessfully of RegisteredPlugin
    | LoadingFailed of string

// プラグイン登録結果
type PluginRegistrationResult =
    | Success of RegisteredPlugin
    | InvalidPlugin of string
    | IncompatibleVersion of string * string
    | MissingDependency of string
    | RegistrationError of exn

// プラグインエラーの種類
type PluginErrorType =
    | ValidationError
    | RuntimeError
    | LifecycleError

// プラグインエラー情報
type PluginError =
    { ErrorType: PluginErrorType
      PluginId: string
      Message: string
      Details: string option
      Exception: exn option }

// ========== プラグインインターフェース ==========

// プラグイン登録関数の型
type PluginRegistrationFunction = RegisteredPlugin -> (App.Types.Msg -> unit) option -> bool

// プラグインビューハンドラーの型
type PluginViewHandler = obj -> ReactElement

// プラグイン更新ハンドラーの型
type PluginUpdateHandler = string -> obj -> obj -> obj

// プラグインコマンドハンドラーの型
type PluginCommandHandler = obj -> unit

// プラグイン初期化関数の型
type PluginInitFunction = unit -> unit
