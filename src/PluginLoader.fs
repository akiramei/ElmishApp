// PluginLoader.fs - レガシーコード削除版
module App.PluginLoader

open Fable.Core
open Fable.Core.JsInterop

// プラグイン設定を外部JSONから読み込む
[<Emit("fetch($0).then(r => r.json())")>]
let fetchJson (url: string) : JS.Promise<obj> = jsNative

// 動的スクリプト読み込み - 重複読み込み防止バージョン＋デバッグ強化
[<Emit("""
new Promise((resolve, reject) => { 
    const selector = `script[src='${$0}']`;
    if (document.querySelector(selector)) { 
        console.log('Script already loaded: ' + $0); 
        resolve(); 
        return; 
    } 
    const script = document.createElement('script'); 
    script.src = $0; 
    
    script.onload = () => {
        console.log('Successfully loaded script: ' + $0);
        resolve();
    }; 
    
    script.onerror = (error) => {
        console.error('Failed to load script: ' + $0, error);
        reject(new Error('Failed to load script: ' + $0));
    }; 
    
    document.head.appendChild(script);
    console.log('Appended script to head: ' + $0);
})
""")>]
let loadScript (url: string) : JS.Promise<unit> = jsNative

// HTMLからプラグインスクリプトを自動的に読み込む
[<Emit("""
new Promise((resolve, reject) => {
    try {
        const pluginScripts = document.querySelectorAll('script[type="plugin"]');
        console.log('Found ' + pluginScripts.length + ' plugin scripts');
        
        if (pluginScripts.length === 0) {
            resolve();
            return;
        }
        
        const promises = [];
        
        for (const script of pluginScripts) {
            const src = script.getAttribute('src');
            if (src) {
                console.log('Loading plugin script: ' + src);
                promises.push(new Promise((innerResolve, innerReject) => {
                    const scriptEl = document.createElement('script');
                    scriptEl.src = src;
                    scriptEl.onload = innerResolve;
                    scriptEl.onerror = innerReject;
                    document.head.appendChild(scriptEl);
                }));
            }
        }
        
        Promise.all(promises).then(resolve).catch(reject);
    } catch (error) {
        console.error('Error loading plugin scripts:', error);
        reject(error);
    }
})
""")>]
let loadPluginScriptTags () : JS.Promise<unit> = jsNative

// プラグインヘルパーライブラリを読み込む
let loadPluginHelpers () =
    async {
        try
            printfn "Loading F# JS bridge..."
            do! loadScript "/js/fsharp-js-bridge.js" |> Async.AwaitPromise
            printfn "Successfully loaded F# JS bridge"

            printfn "Loading plugin helpers..."
            do! loadScript "/js/plugin-helpers.js" |> Async.AwaitPromise
            printfn "Successfully loaded plugin helpers"
            return true
        with ex ->
            printfn "Failed to load plugin helpers: %s" ex.Message
            return false
    }

// プラグイン設定ファイルからプラグインを読み込む
let loadPluginsFromConfig () =
    async {
        try
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

// HTML内のプラグインタグからプラグインを読み込む
let loadPluginsFromHtml () =
    async {
        try
            printfn "Loading plugins from HTML..."
            do! loadPluginScriptTags () |> Async.AwaitPromise
            printfn "Successfully loaded plugins from HTML"
            return true
        with ex ->
            printfn "Error loading plugins from HTML: %s" ex.Message
            return false
    }

// すべてのプラグインを読み込む
let loadAllPlugins () =
    async {
        // まずプラグインヘルパーを読み込む - 最も優先度が高い
        let! helpersResult = loadPluginHelpers ()

        if not helpersResult then
            printfn "Warning: Plugin helpers could not be loaded, plugins may not function correctly"

        // HTML内のプラグインタグから読み込む
        let! htmlResult = loadPluginsFromHtml ()

        // 動的プラグインを読み込む (オプション)
        let! configResult =
            try
                loadPluginsFromConfig ()
            with _ ->
                // 設定ファイルが存在しない場合など
                printfn "No plugin configuration found, only static plugins loaded"
                async.Return false

        return helpersResult && (htmlResult || configResult)
    }
