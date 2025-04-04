// PluginUpdateHandler.fs
// プラグインのアップデート処理を担当

module App.PluginUpdateHandler

open Fable.Core
open Fable.Core.JsInterop
open App.Types
open App.JsBasicTypes
open App.JsCore
open App.ModelConverter
open App.PluginRegistry
open App.MessageBridge

/// カスタム更新ハンドラーの呼び出し - 完全な変換を行う
let applyCustomUpdate (msgType: string) (payload: obj) (model: Model) : Model =
    printfn "Applying custom update for %s" msgType

    // モデルをJavaScript形式に変換
    let jsModel = convertModelToJS model

    let mutable currentModel = jsModel
    let mutable modelUpdated = false

    // 登録されているプラグインを処理
    for KeyValue(pluginId, plugin) in registeredPlugins do
        if not modelUpdated then
            match plugin.UpdateFunction with
            | Some updateFn ->
                try
                    // updateFnを呼び出し
                    let result = callJsUpdateFunction updateFn msgType payload jsModel

                    if not (isNullOrUndefined result) && result <> jsModel then
                        currentModel <- result
                        modelUpdated <- true
                        printfn "Model updated by plugin %s unified update" pluginId
                    else
                        printfn "Unified handler in plugin %s returned null or undefined" pluginId
                with ex ->
                    logPluginError pluginId (sprintf "unified update for '%s'" msgType) ex
            | None ->
                // 従来の個別ハンドラーは対応していないので無視
                printfn "Unsupported plugin %s" pluginId
                ()

    // 更新されたJSモデルをF#モデルに変換して返す
    if modelUpdated then
        let updatedModel = convertJsModelToFSharp currentModel model
        updatedModel
    else
        model

/// カスタムコマンドの実行
let executeCustomCmd (cmdType: string) (payload: obj) : unit =
    // 関連するすべてのプラグインのコマンドハンドラーを取得して実行
    let mutable handlerFound = false

    for KeyValue(pluginId, plugin) in registeredPlugins do
        match Map.tryFind cmdType plugin.CommandHandlers with
        | Some cmdFn ->
            try
                cmdFn payload
                handlerFound <- true
                printfn "Executed command %s in plugin %s" cmdType pluginId
            with ex ->
                logPluginError pluginId (sprintf "command handler '%s'" cmdType) ex
        | None -> ()

    if not handlerFound then
        printfn "No handler found for command type: %s" cmdType
