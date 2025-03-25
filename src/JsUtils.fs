// JsUtils.fs
// JavaScript相互運用のための共通ユーティリティ関数群
module App.JsUtils

open Fable.Core
open Fable.Core.JsInterop

// ========== 基本的なJavaScript型操作 ==========

// 空のJavaScriptオブジェクトを作成する関数
[<Emit("{}")>]
let createEmptyJsObj () : obj = jsNative

// オブジェクトがnullかどうかを判定
[<Emit("$0 === null")>]
let isNull (obj: obj) : bool = jsNative

// オブジェクトがundefinedかどうかを判定
[<Emit("$0 === undefined")>]
let isUndefined (obj: obj) : bool = jsNative

// オブジェクトがnullまたはundefinedかどうかを判定
[<Emit("$0 == null")>]
let isNullOrUndefined (obj: obj) : bool = jsNative

// JavaScript の typeof を F# から呼び出すためのヘルパー関数
[<Emit("typeof $0")>]
let jsTypeof (obj: obj) : string = jsNative

// オブジェクトが関数かどうかを判定する関数
[<Emit("typeof $0 === 'function'")>]
let isJsFunction (obj: obj) : bool = jsNative

// ========== JSON操作 ==========

// JSON文字列に変換
[<Emit("JSON.stringify($0, null, 2)")>]
let jsonStringify (obj: obj) : string = jsNative

// JSON文字列からオブジェクトに戻す
[<Emit("JSON.parse($0)")>]
let jsonParse (str: string) : obj = jsNative

// ========== オブジェクト操作 ==========

// オブジェクトのプロパティをセーフにアクセスする
[<Emit("$0 && $0[$1]")>]
let safeGet (obj: obj) (prop: string) : obj = jsNative

// F#のMapをJavaScriptのプレーンオブジェクトに変換
[<Emit("Object.fromEntries(Array.from($0).map(([k, v]) => [k, v]))")>]
let mapToPlainJsObj (map: Map<string, obj>) : obj = jsNative

// JavaScript関数を直接呼び出し、引数を渡す
[<Emit("$0($1)")>]
let callJsFunction (func: obj) (args: obj) : obj = jsNative

// ========== ファイル/ネットワーク操作 ==========

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

// ========== プラグイン関連 ==========

// F#側のプラグインディスパッチ関数をグローバルに公開
[<Emit("window.appPluginDispatch = $0")>]
let exposePluginDispatch (dispatch: obj -> unit) : unit = jsNative

// Store the registerPluginFromJs function directly in a global variable
[<Emit("window._fsharpRegisterPluginFn = $0")>]
let storeRegisterPluginFunction (fn: obj -> (App.Types.Msg -> unit) option -> bool) : unit = jsNative

// Set up the global registration function that will call our stored function
[<Emit("""
window.registerFSharpPlugin = function(plugin) {
    try {
        return window._fsharpRegisterPluginFn(plugin);
    } catch (error) {
        console.error("Error registering plugin:", error);
        return false;
    }
}
""")>]
let setupGlobalRegistration () : unit = jsNative

// グローバルなJavaScriptブリッジ関数を使用して更新関数を呼び出す
[<Emit("window.FSharpJsBridge.callUpdateHandler($0, $1, $2)")>]
let callUpdateHandlerViaJsBridge (updateFn: obj) (payload: obj) (model: obj) : obj = jsNative

// 統合されたupdate関数を呼び出す
[<Emit("window.FSharpJsBridge.callUnifiedUpdateHandler($0, $1, $2, $3)")>]
let callUnifiedUpdateHandler (updateFn: obj) (messageType: string) (payload: obj) (model: obj) : obj = jsNative

// デバッグ用のオブジェクトロギング関数
[<Emit("window.FSharpJsBridge.logObject($0, $1)")>]
let logObjectViaJsBridge (label: string) (obj: obj) : obj = jsNative

// JavaScriptのプレーンオブジェクトをF#のMapに変換
let plainJsObjToMap (jsObj: obj) : Map<string, obj> =
    if isNullOrUndefined jsObj then
        Map.empty<string, obj>
    else
        try
            let keys = Fable.Core.JS.Constructors.Object.keys (jsObj)
            let mutable map = Map.empty<string, obj>

            for key in keys do
                let value = jsObj?(key)

                if not (isUndefined value) then
                    map <- map.Add(key, value)

            map
        with ex ->
            printfn "Error converting JS object to Map: %s" ex.Message
            Map.empty<string, obj>
