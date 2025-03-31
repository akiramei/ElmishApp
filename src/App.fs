// App.fs
module App.Main

open Elmish
open Elmish.React
open App.Types
open App.Router
open App.Update
open App.View
open App.Subscription

let init () =
    // アプリケーションの初期状態を生成
    let initialModel = Types.init ()

    // 現在のURLからルートを解析
    let initialRoute = getCurrentRoute ()

    // 初期ルートからタブを決定
    let initialTab = routeToTab initialRoute |> Option.defaultValue Types.Home

    { initialModel with
        CurrentTab = initialTab
        CurrentRoute = initialRoute
        PluginState =
            { LoadingPlugins = Init
              RegisteredPluginIds = [] } },
    Cmd.batch
        [ Cmd.none // 他のコマンド

          // 初期APIデータの読み込み
          UpdateApiState.loadUsersCmd
          UpdateApiState.loadProductsCmd ]

// Elmish v4スタイルのサブスクリプション
let subscribe (model: Model) =
    [
      // プラグインローダーサブスクリプション
      if not (model.PluginState.LoadingPlugins = LoadingPlugins.Done) then
          [ "pluginLoader" ], pluginLoader

      // 通知の自動クリアサブスクリプション
      [ "notificationTimer" ], notificationTimer ]

// Elmishプログラムのセットアップ
let startApp () =
    Program.mkProgram init update view
    |> Program.withSubscription subscribe
    |> Program.withReactSynchronous "elmish-app"
    |> Program.withConsoleTrace
    |> Program.run

// アプリケーション開始
startApp ()
