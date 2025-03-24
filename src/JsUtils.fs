// JsUtils.fs
// JavaScript相互運用のための共通ユーティリティ関数群
module App.JsUtils

open Fable.Core
open Fable.Core.JsInterop

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

// JSON文字列に変換
[<Emit("JSON.stringify($0, null, 2)")>]
let jsonStringify (obj: obj) : string = jsNative

// JSON文字列からオブジェクトに戻す
[<Emit("JSON.parse($0)")>]
let jsonParse (str: string) : obj = jsNative

// オブジェクトのプロパティをセーフにアクセスする
[<Emit("$0 && $0[$1]")>]
let safeGet (obj: obj) (prop: string) : obj = jsNative

// F#のMapをJavaScriptのプレーンオブジェクトに変換
[<Emit("Object.fromEntries(Array.from($0).map(([k, v]) => [k, v]))")>]
let mapToPlainJsObj (map: Map<string, obj>) : obj = jsNative

// JavaScript関数を直接呼び出し、引数を渡す
[<Emit("$0($1)")>]
let callJsFunction (func: obj) (args: obj) : obj = jsNative

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
