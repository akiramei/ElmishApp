// JsBasicTypes.fs
// JavaScript相互運用のための最も基本的な型と関数

module App.JsBasicTypes

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

// デバッグ用のオブジェクトログ関数
let logObject label (obj: obj) : obj =
    printfn "%s: %s" label (jsonStringify obj)
    obj
