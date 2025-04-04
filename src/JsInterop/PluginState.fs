// PluginState.fs
// プラグイン固有の状態管理を担当するモジュール

module App.PluginState

open Fable.Core.JsInterop
open App.Types
open App.JsBasicTypes
open App.JsCore

/// プラグイン固有の状態を取得
let getPluginState (pluginId: string) (model: Model) : Map<string, obj> =
    match model.CustomState.TryFind pluginId with
    | Some stateObj when not (isNullOrUndefined stateObj) ->
        // JavaScriptのプレーンオブジェクトの場合はMap型に変換
        if jsTypeof stateObj = "object" then
            plainJsObjToMap stateObj
        // すでにMap型の場合はそのまま使用
        else
            // 型変換が必要な場合
            try
                unbox<Map<string, obj>> stateObj
            with _ ->
                plainJsObjToMap stateObj
    | _ -> Map.empty

/// プラグイン固有の状態を更新
let updatePluginState (pluginId: string) (state: Map<string, obj>) (model: Model) : Model =
    let newCustomState = model.CustomState.Add(pluginId, mapToPlainJsObj state)

    { model with
        CustomState = newCustomState }

/// プラグイン固有の状態の特定の値を取得
let getPluginStateValue<'T> (pluginId: string) (key: string) (defaultValue: 'T) (model: Model) : 'T =
    let pluginState = getPluginState pluginId model

    match pluginState.TryFind key with
    | Some value when not (isNullOrUndefined value) ->
        try
            unbox<'T> value
        with _ ->
            defaultValue
    | _ -> defaultValue

/// プラグイン固有の状態の特定の値を設定
let setPluginStateValue (pluginId: string) (key: string) (value: obj) (model: Model) : Model =
    let pluginState = getPluginState pluginId model
    let updatedState = pluginState.Add(key, value)
    updatePluginState pluginId updatedState model

/// プラグイン状態をクリア
let clearPluginState (pluginId: string) (model: Model) : Model =
    { model with
        CustomState = model.CustomState.Remove pluginId }

/// 全てのプラグイン状態をクリア
let clearAllPluginStates (model: Model) : Model = { model with CustomState = Map.empty }

/// プラグイン状態がキーを含むか確認
let hasPluginStateKey (pluginId: string) (key: string) (model: Model) : bool =
    let state = getPluginState pluginId model
    state.ContainsKey key

/// プラグイン状態から複数の値を取得
let getPluginStateMultipleValues (pluginId: string) (keys: string list) (model: Model) : Map<string, obj> =
    let state = getPluginState pluginId model

    keys
    |> List.fold
        (fun (acc: Map<string, obj>) key ->
            match state.TryFind key with
            | Some value -> acc.Add(key, value)
            | None -> acc)
        Map.empty
