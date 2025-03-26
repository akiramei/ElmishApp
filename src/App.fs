// App.fs
module App.Main

open Elmish
open Elmish.React
open App.Types
open App.Update
open App.View
open App.Subscription

let init () =
    // アプリケーションの初期状態を生成
    let initialModel = Types.init ()

    { initialModel with
        LoadingPlugins = true },
    Cmd.none

// Elmish v4スタイルのサブスクリプション
let subscribe (model: Model) =
    [
      // プラグインローダーサブスクリプション
      [ "pluginLoader" ], pluginLoader

      // 通知の自動クリアサブスクリプション
      [ "notificationTimer" ], notificationTimer

      // モデル状態に応じた条件付きサブスクリプション例
      // if model.SomeCondition then
      //     yield [ "conditionalSub" ], someConditionalSubscription
      ]

// Elmishプログラムのセットアップ
let startApp () =
    Program.mkProgram init update view
    |> Program.withSubscription subscribe
    |> Program.withReactSynchronous "elmish-app"
    |> Program.withConsoleTrace
    |> Program.run

// アプリケーション開始
startApp ()
