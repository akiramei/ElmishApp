// JsCore.fs
// JavaScript相互運用のための中核機能

module App.JsCore

open Fable.Core
open Fable.Core.JsInterop
open App.JsBasicTypes

// ========== オブジェクト操作 ==========

// オブジェクトのプロパティをセーフにアクセスする
[<Emit("$0 && $0[$1]")>]
let safeGet (obj: obj) (prop: string) : obj = jsNative

// オブジェクトのプロパティが存在するかチェック
[<Emit("Object.prototype.hasOwnProperty.call($0, $1)")>]
let hasProperty (obj: obj) (prop: string) : bool = jsNative

// JavaScriptのオブジェクトのキーを取得
[<Emit("Object.keys($0)")>]
let getObjectKeys (obj: obj) : string array = jsNative

// ========== 配列操作 ==========

// JavaScriptの配列かどうかを判定
[<Emit("Array.isArray($0)")>]
let isArray (obj: obj) : bool = jsNative

// ========== 関数呼び出し ==========

// JavaScript関数を直接呼び出し、引数を渡す
[<Emit("$0($1)")>]
let callJsFunction (func: obj) (args: obj) : obj = jsNative

[<Emit("$0($1,$2,$3)")>]
let callJsUpdateFunction (func: obj) (messageType: obj) (payload: obj) (model: obj) : obj = jsNative

// JavaScriptのプロミスを待機
[<Emit("$0.then($1)")>]
let thenPromise (promise: JS.Promise<'T>) (callback: 'T -> 'R) : JS.Promise<'R> = jsNative

// プロミスのエラーハンドリング
[<Emit("$0.catch($1)")>]
let catchPromise (promise: JS.Promise<'T>) (callback: obj -> 'R) : JS.Promise<'R> = jsNative

// ========== F#<->JS変換 ==========

// F#のMapをJavaScriptのプレーンオブジェクトに変換
[<Emit("Object.fromEntries(Array.from($0).map(([k, v]) => [k, v]))")>]
let mapToPlainJsObj (map: Map<string, obj>) : obj = jsNative

// JavaScriptのプレーンオブジェクトをF#のMapに変換
let plainJsObjToMap (jsObj: obj) : Map<string, obj> =
    if isNullOrUndefined jsObj then
        Map.empty<string, obj>
    else
        try
            let keys = getObjectKeys jsObj
            let mutable map = Map.empty<string, obj>

            for key in keys do
                let value = jsObj?(key)

                if not (isUndefined value) then
                    map <- map.Add(key, value)

            map
        with ex ->
            printfn "Error converting JS object to Map: %s" ex.Message
            Map.empty<string, obj>

// ========== DOM操作 ==========

// 要素のクラスを変更
[<Emit("$0.classList.toggle($1)")>]
let toggleClass (element: obj) (className: string) : unit = jsNative

// 現在のウィンドウの位置をスクロール
[<Emit("window.scrollTo($0, $1)")>]
let scrollTo (x: float) (y: float) : unit = jsNative
