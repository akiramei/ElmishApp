module App.UpdatePluginState

open Elmish
open App.Types
open App.Notifications

let updatePluginState msg state =
    match msg with
    | PluginTabAdded tabId ->
        printfn "Plugin tab added: %s" tabId
        // タブが追加されただけでは再レンダリングが発生するが特別な処理は不要
        state, Cmd.none

    | PluginRegistered definition ->
        printfn "Plugin registered: %s" definition.Id
        // 登録済みのプラグインIDリストを更新
        let updatedPluginIds =
            if state.RegisteredPluginIds |> List.contains definition.Id then
                state.RegisteredPluginIds
            else
                definition.Id :: state.RegisteredPluginIds

        { state with
            RegisteredPluginIds = updatedPluginIds },
        Cmd.none

    | PluginsLoaded ->
        printfn "All plugins loaded"

        let newState =
            { state with
                LoadingPlugins = LoadingPlugins.Done }

        let notification = info "全プラグインの読み込みが完了しました"
        newState, Cmd.ofMsg (NotificationMsg(Add notification))
