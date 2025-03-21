// PluginLoader.fs
module App.PluginLoader

open Fable.Core
open Fable.Core.JsInterop
open App.Plugins
open Elmish

// プラグイン設定を外部JSONから読み込む
[<Emit("fetch($0).then(r => r.json())")>]
let fetchJson (url: string) : JS.Promise<obj> = jsNative

// 動的スクリプト読み込み - 修正版
[<Emit("new Promise((resolve, reject) => { const script = document.createElement('script'); script.src = $0; script.onload = () => resolve(); script.onerror = () => reject(new Error('Failed to load script: ' + $0)); document.head.appendChild(script); })")>]
let loadScript (url: string) : JS.Promise<unit> = jsNative

// JavaScriptグローバルオブジェクトの初期化
[<Emit("window.customViews = window.customViews || {}; window.customUpdates = window.customUpdates || {}; window.customTabs = window.customTabs || []; window.customCmdHandlers = window.customCmdHandlers || {}")>]
let initJsGlobals () : unit = jsNative

// プラグイン設定ファイルからプラグインを読み込む
let loadPluginsFromConfig () =
    async {
        try
            // JavaScript側のグローバルオブジェクトを初期化
            initJsGlobals ()

            // 設定ファイルを取得
            let! pluginsConfig = fetchJson "/config/plugins.json" |> Async.AwaitPromise
            let pluginsList = pluginsConfig?plugins |> unbox<obj[]>

            // 各プラグインを読み込む
            for pluginConfig in pluginsList do
                let pluginId = pluginConfig?id |> unbox<string>
                let scriptUrl = pluginConfig?scriptUrl |> unbox<string>

                let enabled =
                    match pluginConfig?enabled with
                    | null -> true // デフォルトで有効
                    | v -> unbox<bool> v

                if enabled then
                    printfn "Loading plugin: %s from %s" pluginId scriptUrl

                    try
                        // スクリプトを動的に読み込む
                        do! loadScript scriptUrl |> Async.AwaitPromise
                        printfn "Successfully loaded plugin: %s" pluginId
                    with ex ->
                        printfn "Failed to load plugin %s: %s" pluginId ex.Message

            return true
        with ex ->
            printfn "Error loading plugins configuration: %s" ex.Message
            return false
    }

// 静的に含まれているプラグインを読み込む (開発中または埋め込みプラグイン用)
let loadStaticPlugins () =
    async {
        try
            // JavaScript側のグローバルオブジェクトを初期化
            initJsGlobals ()

            // 静的に含まれているプラグインスクリプトのリスト
            let staticPlugins = [ "/js/counter-extension.js"; "/js/slider-tab.js" ]

            // 各プラグインを読み込む
            for scriptUrl in staticPlugins do
                try
                    printfn "Loading static plugin from %s" scriptUrl
                    do! loadScript scriptUrl |> Async.AwaitPromise
                    printfn "Successfully loaded static plugin from %s" scriptUrl
                with ex ->
                    printfn "Failed to load static plugin %s: %s" scriptUrl ex.Message

            return true
        with ex ->
            printfn "Error loading static plugins: %s" ex.Message
            return false
    }

// すべてのプラグインを読み込む
let loadAllPlugins () =
    async {
        // JavaScript側のグローバルオブジェクトを初期化
        initJsGlobals ()

        // 静的プラグインを読み込む
        let! staticResult = loadStaticPlugins ()

        // 動的プラグインを読み込む (オプション)
        // 実際の環境では設定ファイルが存在するかどうかをチェックしてから読み込むと良い
        let! configResult =
            try
                loadPluginsFromConfig ()
            with _ ->
                // 設定ファイルが存在しない場合など
                printfn "No plugin configuration found, only static plugins loaded"
                async.Return false

        // グローバルオブジェクトの状態を確認
        printfn "After loading plugins, global objects state:"
        printfn "customViews: %A" Browser.Dom.window?customViews
        printfn "customUpdates: %A" Browser.Dom.window?customUpdates
        printfn "customTabs: %A" Browser.Dom.window?customTabs

        return staticResult || configResult
    }

// サブスクリプションのためのプラグインロード処理
let pluginLoaderSubscription dispatch =
    loadAllPlugins () |> Async.Ignore |> Async.StartImmediate
