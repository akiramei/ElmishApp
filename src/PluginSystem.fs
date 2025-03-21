// PluginSystem.fs
module App.Plugins

open Fable.Core
open Fable.Core.JsInterop
open App.Types
open Elmish

// デバッグ用ヘルパー
[<Emit("console.log('Debug model:', $0)")>]
let debugModel (model: obj) : unit = jsNative

// コアバージョン
[<Literal>]
let CoreVersion = "1.0.0"

// 登録済みプラグイン情報
type RegisteredPlugin =
    { Definition: PluginDefinition
      Views: Map<string, obj -> Feliz.ReactElement>
      UpdateHandlers: Map<string, obj -> obj -> obj>
      CommandHandlers: Map<string, obj -> unit>
      Tabs: string list }

// JavaScript の typeof を F# から呼び出すためのヘルパー関数
[<Emit("typeof $0")>]
let jsTypeof (obj: obj) : string = jsNative

// オブジェクトが関数かどうかを判定する関数
let isJsFunction (obj: obj) : bool = jsTypeof obj = "function"

// JSON文字列に変換
[<Emit("JSON.stringify($0)")>]
let jsonStringify (obj: obj) : string = jsNative

// オブジェクトがnullかどうかを判定
[<Emit("$0 === null")>]
let isNull (obj: obj) : bool = jsNative

// レガシーモード用のヘルパー関数（トップレベル関数として定義）
[<Emit("window.customViews && window.customViews[$0] ? window.customViews[$0]($1) : null")>]
let getCustomViewLegacy (viewName: string) (props: obj) : Feliz.ReactElement = jsNative

[<Emit("window.customTabs ? window.customTabs : []")>]
let getCustomTabsLegacy () : string[] = jsNative

[<Emit("window.customUpdates && window.customUpdates[$0] ? window.customUpdates[$0]($1, $2 || {}) : $2")>]
let applyCustomUpdateLegacy (updateName: string) (msg: obj) (model: obj) : obj = jsNative

[<Emit("window.customCmdHandlers && window.customCmdHandlers[$0] ? window.customCmdHandlers[$0]($1) : null")>]
let executeCustomCmdLegacy (cmdType: string) (payload: obj) : unit = jsNative

// プラグイン管理
let mutable registeredPlugins = Map.empty<string, RegisteredPlugin>

// バージョン互換性チェック関数
let isCompatible (requiredVersion: string) =
    // 簡易バージョン比較
    // 実際の実装ではより詳細なセマンティックバージョニングを行うべき
    requiredVersion.StartsWith(CoreVersion.Split('.')[0])

// エラーロギング関数
let logPluginError (pluginId: string) (operation: string) (ex: exn) =
    printfn "Error in plugin %s during %s: %s" pluginId operation ex.Message
    printfn "Stack trace: %s" ex.StackTrace
// より高度なロギングやテレメトリを実装可能

// 型を合わせるためのエミット（グローバル関数をエクスポート用）
[<Emit("window.registerFSharpPlugin = function(plugin) { return $0(plugin, window.appPluginDispatch || null); }")>]
let exposePluginRegistration (registrationFn: obj -> obj -> bool) : unit = jsNative

// F#側のプラグインディスパッチ関数をグローバルに公開
[<Emit("window.appPluginDispatch = $0")>]
let exposePluginDispatch (dispatch: obj -> unit) : unit = jsNative

// プラグイン登録関数 - 型の修正
let registerPlugin (plugin: RegisteredPlugin) (dispatch: (Msg -> unit) option) =
    // バージョン互換性チェック
    if not (isCompatible plugin.Definition.Compatibility) then
        // 警告ログまたはエラー表示
        printfn
            "Warning: Plugin %s version %s may not be compatible with core version"
            plugin.Definition.Name
            plugin.Definition.Version

    registeredPlugins <- registeredPlugins.Add(plugin.Definition.Id, plugin)

    // プラグインタブ情報の登録
    printfn
        "Plugin '%s' v%s registered with %d views, %d update handlers, %d command handlers, and %d tabs"
        plugin.Definition.Name
        plugin.Definition.Version
        plugin.Views.Count
        plugin.UpdateHandlers.Count
        plugin.CommandHandlers.Count
        plugin.Tabs.Length

    // プラグイン登録のディスパッチ - 型の修正
    match dispatch with
    | Some d ->
        // プラグイン登録のディスパッチ
        d (PluginRegistered plugin.Definition)

        // タブが追加されたことをディスパッチ
        for tab in plugin.Tabs do
            d (PluginTabAdded tab)
    | None -> printfn "No dispatch function available, skipping notifications"

// JavaScriptオブジェクトからプラグインを登録
let registerPluginFromJs (jsPlugin: obj) (dispatch: (Msg -> unit) option) =
    try
        // JavaScriptオブジェクトからプラグイン定義を抽出
        let definition =
            { Id = jsPlugin?definition?id |> unbox<string>
              Name = jsPlugin?definition?name |> unbox<string>
              Version = jsPlugin?definition?version |> unbox<string>
              Dependencies = jsPlugin?definition?dependencies |> unbox<string[]> |> Array.toList
              Compatibility = jsPlugin?definition?compatibility |> unbox<string> }

        // ビューハンドラーをMapに変換
        let viewsObj = jsPlugin?views |> unbox<obj>
        let mutable views = Map.empty<string, obj -> Feliz.ReactElement>
        let viewKeys = Fable.Core.JS.Constructors.Object.keys (viewsObj)

        for key in viewKeys do
            let viewFn = viewsObj?(key) |> unbox<obj -> Feliz.ReactElement>
            views <- views.Add(key, viewFn)

        // 更新ハンドラーをMapに変換
        let updateHandlersObj = jsPlugin?updateHandlers |> unbox<obj>
        let mutable updateHandlers = Map.empty<string, obj -> obj -> obj>
        let updateKeys = Fable.Core.JS.Constructors.Object.keys (updateHandlersObj)

        for key in updateKeys do
            let updateFn = updateHandlersObj?(key)
            // 関数かどうかをチェック
            if isJsFunction updateFn then
                let typedUpdateFn = updateFn |> unbox<obj -> obj -> obj>
                updateHandlers <- updateHandlers.Add(key, typedUpdateFn)
                printfn "Added update handler for '%s'" key
            else
                printfn "Warning: Update handler for '%s' is not a function" key

        // コマンドハンドラーをMapに変換
        let commandHandlersObj = jsPlugin?commandHandlers |> unbox<obj>
        let mutable commandHandlers = Map.empty<string, obj -> unit>
        let commandKeys = Fable.Core.JS.Constructors.Object.keys (commandHandlersObj)

        for key in commandKeys do
            let commandFn = commandHandlersObj?(key) |> unbox<obj -> unit>
            commandHandlers <- commandHandlers.Add(key, commandFn)

        // タブを取得
        let tabs = jsPlugin?tabs |> unbox<string[]> |> Array.toList

        // プラグインを登録
        let plugin =
            { Definition = definition
              Views = views
              UpdateHandlers = updateHandlers
              CommandHandlers = commandHandlers
              Tabs = tabs }

        registerPlugin plugin dispatch

        // 初期化関数の呼び出し
        if jsPlugin?init <> null then
            let initFn = jsPlugin?init |> unbox<unit -> unit>
            initFn ()

        true
    with ex ->
        printfn "Error registering plugin: %s" ex.Message
        printfn "Stack trace: %s" ex.StackTrace
        false

// カスタムビューの取得関数
let getCustomView (viewId: string) (props: obj) : Feliz.ReactElement option =
    let mutable result = None

    // 全プラグインからビューを検索
    for KeyValue(_, plugin) in registeredPlugins do
        match Map.tryFind viewId plugin.Views with
        | Some viewFn ->
            try
                result <- Some(viewFn props)
            with ex ->
                logPluginError plugin.Definition.Id (sprintf "rendering view '%s'" viewId) ex
        | None -> ()

    // レガシーモードのサポート (window.customViewsを直接使用)
    if result.IsNone then
        try
            let legacyView = getCustomViewLegacy viewId props

            if not (isNull legacyView) then
                result <- Some legacyView
                printfn "Using legacy view for '%s'" viewId
        with ex ->
            printfn "Error using legacy view '%s': %s" viewId ex.Message

    result

// カスタムタブのリスト取得
let getAvailableCustomTabs () =
    let pluginTabs =
        registeredPlugins
        |> Map.values
        |> Seq.collect (fun plugin -> plugin.Tabs)
        |> Seq.distinct
        |> Seq.map CustomTab
        |> Seq.toList

    // レガシーモードのサポート (window.customTabsを直接使用)
    let legacyTabs =
        try
            getCustomTabsLegacy () |> Array.map CustomTab |> Array.toList
        with _ ->
            []

    // 両方のリストを結合して重複を排除
    pluginTabs @ legacyTabs |> List.distinct

// カスタム更新関数を実行 - 修正版（参照セル警告修正）
let applyCustomUpdates (msgType: string) (payload: obj) (model: obj) : obj =
    printfn "Applying custom updates for message type: %s" msgType
    debugModel model

    let mutable currentModel = model

    // 登録されているプラグインをログ
    printfn "Registered plugins: %A" (registeredPlugins |> Map.keys |> Seq.toArray)

    // 関連するすべてのプラグインの更新関数を取得して実行
    let pluginUpdated = ref false

    for KeyValue(pluginId, plugin) in registeredPlugins do
        printfn "Checking plugin %s for handler %s" pluginId msgType

        match Map.tryFind msgType plugin.UpdateHandlers with
        | Some updateFn ->
            printfn "Found update handler for %s in plugin %s" msgType pluginId

            try
                // 更新関数が実際に関数であるかを確認
                if isJsFunction updateFn then
                    printfn "updateFn is a function"
                    // デバッグ用に引数を確認
                    printfn "Payload type: %s, Model type: %s" (jsTypeof payload) (jsTypeof currentModel)

                    // モデルとpayloadを渡す
                    let result = updateFn payload currentModel

                    if not (isNull result) then
                        printfn "Update handler returned a valid result"
                        currentModel <- result
                        pluginUpdated.Value <- true // 参照セルの更新方法を修正
                    else
                        printfn "Update handler returned null"
                else
                    printfn "updateFn is NOT a function, but a %s" (jsTypeof updateFn)
            with ex ->
                logPluginError pluginId (sprintf "update handler '%s'" msgType) ex
        | None -> printfn "No handler found for %s in plugin %s" msgType pluginId

    // レガシーモードのサポート (window.customUpdatesを直接使用)
    if not pluginUpdated.Value then // 参照セルの参照方法を修正
        try
            let result = applyCustomUpdateLegacy msgType payload currentModel

            if not (isNull result) && result <> currentModel then
                printfn "Used legacy update handler for '%s'" msgType
                currentModel <- result
        with ex ->
            printfn "Error using legacy update handler for '%s': %s" msgType ex.Message

    currentModel

// カスタムコマンドの実行
let executeCustomCmd (cmdType: string) (payload: obj) : unit =
    // 関連するすべてのプラグインのコマンドハンドラーを取得して実行
    for KeyValue(pluginId, plugin) in registeredPlugins do
        match Map.tryFind cmdType plugin.CommandHandlers with
        | Some cmdFn ->
            try
                cmdFn payload
            with ex ->
                logPluginError pluginId (sprintf "command handler '%s'" cmdType) ex
        | None -> ()

    // レガシーモードのサポート
    try
        executeCustomCmdLegacy cmdType payload
    with ex ->
        printfn "Error executing legacy command handler for '%s': %s" cmdType ex.Message

// プラグインIDのリストを取得
let getRegisteredPluginIds () =
    registeredPlugins |> Map.keys |> Seq.toList
