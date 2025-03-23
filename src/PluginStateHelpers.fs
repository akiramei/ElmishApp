// PluginStateHelpers.fs
module App.PluginStateHelpers

open Fable.Core
open Fable.Core.JsInterop
open App.Types
open App.Interop

/// JavaScriptオブジェクトからMap<string, obj>型へ変換する
let jsObjectToMap (jsObj: obj) : Map<string, obj> =
    if isNullOrUndefined jsObj then
        Map.empty
    else
        try
            let keys = Fable.Core.JS.Constructors.Object.keys (jsObj)
            let mutable map = Map.empty

            for key in keys do
                let value = jsObj?(key)

                if not (isNullOrUndefined value) then
                    map <- map.Add(key, value)

            map
        with ex ->
            printfn "Error converting JS object to Map: %s" ex.Message
            Map.empty

/// プラグイン固有の状態を取得
let getPluginState (pluginId: string) (model: Model) : Map<string, obj> =
    match model.CustomState.TryFind pluginId with
    | Some stateObj when not (isNullOrUndefined stateObj) ->
        // JavaScriptのプレーンオブジェクトの場合はMap型に変換
        if jsTypeof stateObj = "object" then
            jsObjectToMap stateObj
        // すでにMap型の場合はそのまま使用
        else
            // 型変換が必要な場合
            try
                unbox<Map<string, obj>> stateObj
            with _ ->
                jsObjectToMap stateObj
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
