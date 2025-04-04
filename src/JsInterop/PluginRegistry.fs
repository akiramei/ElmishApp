// PluginRegistry.fs
// プラグインの登録と管理を担当

module App.PluginRegistry

open Fable.Core.JsInterop
open App.Types // アプリケーションの主要な型
open App.JsBasicTypes
open App.JsCore
open App.PluginTypes // プラグインシステム内部の型
open App.MessageBridge

// ===== プラグイン管理 =====

// 登録済みプラグインマップ
let mutable registeredPlugins = Map.empty<string, RegisteredPlugin>

// バージョン互換性チェック関数
let isCompatible (requiredVersion: string) =
    // 簡易バージョン比較
    // 実際の実装ではより詳細なセマンティックバージョニングを行うべき
    let coreVersion = "1.0.0"
    requiredVersion.StartsWith(coreVersion.Split('.')[0])

// エラーロギング関数
let logPluginError (pluginId: string) (operation: string) (ex: exn) =
    printfn "Error in plugin %s during %s: %s" pluginId operation ex.Message
    printfn "Stack trace: %s" ex.StackTrace
// より高度なロギングやテレメトリを実装可能

// プラグイン登録関数
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
        "Plugin '%s' v%s registered with %d views, %d command handlers, and %d tabs"
        plugin.Definition.Name
        plugin.Definition.Version
        plugin.Views.Count
        plugin.CommandHandlers.Count
        plugin.Tabs.Length

    // プラグイン登録のディスパッチ
    match dispatch with
    | Some d ->
        // プラグイン登録のディスパッチ
        d (PluginMsg(PluginRegistered plugin.Definition))

        // タブが追加されたことをディスパッチ
        for tab in plugin.Tabs do
            d (PluginMsg(PluginTabAdded tab))
    | None -> printfn "No dispatch function available, skipping notifications"

    // 登録成功
    true

// JavaScriptオブジェクトからプラグインを登録
let registerPluginFromJs (jsPlugin: obj) (dispatch: (Msg -> unit) option) =
    try
        // プラグインオブジェクトの検証
        if isNullOrUndefined jsPlugin then
            printfn "Invalid plugin object: null or undefined"
            false
        else
            // JavaScriptオブジェクトからプラグイン定義を抽出
            let definition =
                { Id =
                    match safeGet jsPlugin "definition" with
                    | null -> "unknown-plugin"
                    | def ->
                        match safeGet def "id" with
                        | null -> "unknown-plugin"
                        | id -> unbox<string> id

                  Name =
                    match safeGet (safeGet jsPlugin "definition") "name" with
                    | null -> "Unknown Plugin"
                    | name -> unbox<string> name

                  Version =
                    match safeGet (safeGet jsPlugin "definition") "version" with
                    | null -> "0.0.0"
                    | ver -> unbox<string> ver

                  Dependencies =
                    match safeGet (safeGet jsPlugin "definition") "dependencies" with
                    | null -> []
                    | deps ->
                        if isArray deps then
                            (unbox<string[]> deps) |> Array.toList
                        else
                            []

                  Compatibility =
                    match safeGet (safeGet jsPlugin "definition") "compatibility" with
                    | null -> "1.0"
                    | compat -> unbox<string> compat }

            printfn "Registering plugin %s v%s" definition.Id definition.Version

            // ビューハンドラーをMapに変換
            let viewsObj = safeGet jsPlugin "views"
            let mutable views = Map.empty<string, obj -> Feliz.ReactElement>

            if not (isNullOrUndefined viewsObj) then
                let viewKeys = getObjectKeys viewsObj

                for key in viewKeys do
                    let viewFn = viewsObj?(key)

                    if isJsFunction viewFn then
                        let typedViewFn = viewFn |> unbox<obj -> Feliz.ReactElement>
                        views <- views.Add(key, typedViewFn)
                        printfn "Added view for '%s'" key
                    else
                        printfn "Warning: View handler for '%s' is not a function" key

            // コマンドハンドラーをMapに変換
            let commandHandlersObj = safeGet jsPlugin "commandHandlers"
            let mutable commandHandlers = Map.empty<string, obj -> unit>

            if not (isNullOrUndefined commandHandlersObj) then
                let commandKeys = getObjectKeys commandHandlersObj

                for key in commandKeys do
                    let commandFn = commandHandlersObj?(key)

                    if isJsFunction commandFn then
                        let typedCommandFn = commandFn |> unbox<obj -> unit>
                        commandHandlers <- commandHandlers.Add(key, typedCommandFn)
                        printfn "Added command handler for '%s'" key
                    else
                        printfn "Warning: Command handler for '%s' is not a function" key

            // タブを取得
            let tabsArray = safeGet jsPlugin "tabs"

            let tabs =
                if isNullOrUndefined tabsArray || not (isArray tabsArray) then
                    []
                else
                    tabsArray |> unbox<string[]> |> Array.toList

            // updateFunction プロパティを取得
            let updateFunction =
                match safeGet jsPlugin "updateFunction" with
                | null -> None
                | fn ->
                    if isJsFunction fn then
                        Some fn
                    else
                        printfn "Warning: updateFunction is not a function"
                        None

            // プラグインを登録
            let plugin =
                { Definition = definition
                  Views = views
                  UpdateFunction = updateFunction
                  CommandHandlers = commandHandlers
                  Tabs = tabs }

            let result = registerPlugin plugin dispatch

            // 初期化関数の呼び出し
            let initFn = safeGet jsPlugin "init"

            if isJsFunction initFn then
                let typedInitFn = initFn |> unbox<unit -> unit>
                typedInitFn ()
                printfn "Initialized plugin %s" definition.Id

            result
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

    result

// カスタムタブのリスト取得
let getAvailableCustomTabs () =
    registeredPlugins
    |> Map.values
    |> Seq.collect (fun plugin -> plugin.Tabs)
    |> Seq.distinct
    |> Seq.map Tab.CustomTab
    |> Seq.toList

// プラグインIDのリストを取得
let getRegisteredPluginIds () =
    registeredPlugins |> Map.keys |> Seq.toList

// 特定のプラグインを取得
let getPlugin (pluginId: string) : RegisteredPlugin option = Map.tryFind pluginId registeredPlugins
