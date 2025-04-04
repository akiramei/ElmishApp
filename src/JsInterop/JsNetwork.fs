// JsNetwork.fs
// ネットワーク関連のJavaScript相互運用関数

module App.JsNetwork

open Fable.Core
open Fable.Core.JsInterop
open App.JsBasicTypes

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

// 特定のURLからファイルを読み込む
[<Emit("fetch($0).then(response => response.text())")>]
let fetchText (url: string) : JS.Promise<string> = jsNative

// Blobデータとしてファイルを読み込む
[<Emit("fetch($0).then(response => response.blob())")>]
let fetchBlob (url: string) : JS.Promise<obj> = jsNative

// テキストをBlobに変換
[<Emit("new Blob([$0], { type: $1 })")>]
let createBlob (text: string) (mimeType: string) : obj = jsNative

// ファイルダウンロード用リンクを作成してクリック
[<Emit("""
(() => {
    const a = document.createElement('a');
    a.href = URL.createObjectURL($0);
    a.download = $1;
    a.style.display = 'none';
    document.body.appendChild(a);
    a.click();
    setTimeout(() => {
        document.body.removeChild(a);
        URL.revokeObjectURL(a.href);
    }, 100);
})()
""")>]
let downloadBlob (blob: obj) (filename: string) : unit = jsNative
