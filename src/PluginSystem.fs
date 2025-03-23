// PluginSystem.fs
module App.Plugins

open Fable.Core
open Fable.Core.JsInterop
open App.Types

// コアバージョン
[<Literal>]
let CoreVersion = "1.0.0"

// 登録済みプラグイン情報
type RegisteredPlugin =
    { Definition: PluginDefinition
      Views: Map<string, obj -> Feliz.ReactElement>
      UpdateHandlers: Map<string, obj -> obj -> obj>
      // 関数型アプローチ用の統一update関数
      UpdateFunction: Option<obj>
      CommandHandlers: Map<string, obj -> unit>
      Tabs: string list }

// ===== JavaScript相互運用ヘルパー =====

// JavaScript の typeof を F# から呼び出すためのヘルパー関数
[<Emit("typeof $0")>]
let jsTypeof (obj: obj) : string = jsNative

// オブジェクトが関数かどうかを判定する関数
[<Emit("typeof $0 === 'function'")>]
let isJsFunction (obj: obj) : bool = jsNative

// JSON文字列に変換
[<Emit("JSON.stringify($0, null, 2)")>]
let jsonStringify (obj: obj) : string = jsNative

// オブジェクトがnullかどうかを判定
[<Emit("$0 === null")>]
let isNull (obj: obj) : bool = jsNative

// オブジェクトがundefinedかどうかを判定
[<Emit("$0 === undefined")>]
let isUndefined (obj: obj) : bool = jsNative

// オブジェクトがnullまたはundefinedかどうかを判定
[<Emit("$0 == null")>]
let isNullOrUndefined (obj: obj) : bool = jsNative

// オブジェクトのプロパティをセーフにアクセスする
[<Emit("$0 && $0[$1]")>]
let safeGet (obj: obj) (prop: string) : obj = jsNative

// ===== プラグイン管理 =====

// 登録済みプラグインマップ
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

// F#側のプラグインディスパッチ関数をグローバルに公開
[<Emit("window.appPluginDispatch = $0")>]
let exposePluginDispatch (dispatch: obj -> unit) : unit = jsNative

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
        "Plugin '%s' v%s registered with %d views, %d update handlers, %d command handlers, and %d tabs"
        plugin.Definition.Name
        plugin.Definition.Version
        plugin.Views.Count
        plugin.UpdateHandlers.Count
        plugin.CommandHandlers.Count
        plugin.Tabs.Length

    // プラグイン登録のディスパッチ
    match dispatch with
    | Some d ->
        // プラグイン登録のディスパッチ
        d (PluginRegistered plugin.Definition)

        // タブが追加されたことをディスパッチ
        for tab in plugin.Tabs do
            d (PluginTabAdded tab)
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
                        if Fable.Core.JS.Constructors.Array.isArray deps then
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
                let viewKeys = Fable.Core.JS.Constructors.Object.keys (viewsObj)

                for key in viewKeys do
                    let viewFn = viewsObj?(key)

                    if isJsFunction viewFn then
                        let typedViewFn = viewFn |> unbox<obj -> Feliz.ReactElement>
                        views <- views.Add(key, typedViewFn)
                        printfn "Added view for '%s'" key
                    else
                        printfn "Warning: View handler for '%s' is not a function" key

            // 更新ハンドラーをMapに変換
            let updateHandlersObj = safeGet jsPlugin "updateHandlers"
            let mutable updateHandlers = Map.empty<string, obj -> obj -> obj>

            if not (isNullOrUndefined updateHandlersObj) then
                let updateKeys = Fable.Core.JS.Constructors.Object.keys (updateHandlersObj)

                for key in updateKeys do
                    let updateFn = updateHandlersObj?(key)

                    if isJsFunction updateFn then
                        let typedUpdateFn = updateFn |> unbox<obj -> obj -> obj>
                        updateHandlers <- updateHandlers.Add(key, typedUpdateFn)
                        printfn "Added update handler for '%s'" key
                    else
                        printfn "Warning: Update handler for '%s' is not a function" key

            // コマンドハンドラーをMapに変換
            let commandHandlersObj = safeGet jsPlugin "commandHandlers"
            let mutable commandHandlers = Map.empty<string, obj -> unit>

            if not (isNullOrUndefined commandHandlersObj) then
                let commandKeys = Fable.Core.JS.Constructors.Object.keys (commandHandlersObj)

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
                if
                    isNullOrUndefined tabsArray
                    || not (Fable.Core.JS.Constructors.Array.isArray tabsArray)
                then
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
                  UpdateHandlers = updateHandlers
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
    |> Seq.map CustomTab
    |> Seq.toList

// グローバルなJavaScriptブリッジ関数を使用して更新関数を呼び出す
[<Emit("window.FSharpJsBridge.callUpdateHandler($0, $1, $2)")>]
let callUpdateHandlerViaJsBridge (updateFn: obj) (payload: obj) (model: obj) : obj = jsNative

// 統合されたupdate関数を呼び出す
[<Emit("window.FSharpJsBridge.callUnifiedUpdateHandler($0, $1, $2, $3)")>]
let callUnifiedUpdateHandler (updateFn: obj) (messageType: string) (payload: obj) (model: obj) : obj = jsNative

// デバッグ用のオブジェクトロギング関数
[<Emit("window.FSharpJsBridge.logObject($0, $1)")>]
let logObjectViaJsBridge (label: string) (obj: obj) : obj = jsNative

let applyCustomUpdates (msgType: string) (payload: obj) (model: obj) : obj =
    printfn "Applying custom updates for message type: %s" msgType

    // デバッグ情報をログに出力
    let _ = logObjectViaJsBridge (sprintf "Update for %s - Payload" msgType) payload
    let _ = logObjectViaJsBridge (sprintf "Update for %s - Model" msgType) model

    let mutable currentModel = model
    let mutable modelUpdated = false

    // 登録されているプラグインを処理
    for KeyValue(pluginId, plugin) in registeredPlugins do
        // 統一update関数があれば優先使用
        match plugin.UpdateFunction with
        | Some updateFn ->
            try
                // 統合update関数を呼び出す
                let result = callUnifiedUpdateHandler updateFn msgType payload currentModel

                if not (isNullOrUndefined result) then
                    currentModel <- result
                    modelUpdated <- true
                    printfn "Model updated by plugin %s unified update" pluginId
                else
                    printfn "Unified handler in plugin %s returned null or undefined" pluginId
            with ex ->
                logPluginError pluginId (sprintf "unified update for '%s'" msgType) ex

        // 従来の個別ハンドラーを使用
        | None ->
            match Map.tryFind msgType plugin.UpdateHandlers with
            | Some updateFn ->
                printfn "Found update handler for %s in plugin %s" msgType pluginId

                try
                    // JavaScriptブリッジを通じて更新関数を呼び出す
                    let result = callUpdateHandlerViaJsBridge updateFn payload currentModel

                    // 結果が有効であればモデルを更新
                    if not (isNullOrUndefined result) then
                        currentModel <- result
                        modelUpdated <- true
                        printfn "Model updated by plugin %s" pluginId
                    else
                        printfn "Handler in plugin %s returned null or undefined" pluginId
                with ex ->
                    logPluginError pluginId (sprintf "update handler '%s'" msgType) ex
            | None ->
                // このプラグインにはこのメッセージタイプのハンドラーがない
                ()

    currentModel

// カスタムコマンドの実行
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

// プラグインIDのリストを取得
let getRegisteredPluginIds () =
    registeredPlugins |> Map.keys |> Seq.toList
