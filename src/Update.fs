// Update.fs
module App.Update

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

    | SetError errorMsg ->
        let errorState =
            { HasError = true
              Message = Some errorMsg
              ErrorCode = None
              Source = Some "core" }

        { model with ErrorState = errorState }, Cmd.none

    | ClearError ->
        let errorState =
            { HasError = false
              Message = None
              ErrorCode = None
              Source = None }

        { model with ErrorState = errorState }, Cmd.none

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
        { model with LoadingPlugins = false }, Cmd.none

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

            let errorState =
                { HasError = true
                  Message = Some(sprintf "Error processing custom message: %s" ex.Message)
                  ErrorCode = Some "CUSTOM_MSG_ERROR"
                  Source = Some "core" }

            { model with ErrorState = errorState }, Cmd.none
