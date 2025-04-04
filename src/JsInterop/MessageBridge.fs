// MessageBridge.fs
// F#メッセージシステムとJavaScript間のブリッジ

module App.MessageBridge

open Fable.Core
open Fable.Core.JsInterop
open App.Types
open App.JsBasicTypes
open App.JsCore

/// JavaScript側からのメッセージをF#のMsg型に変換する関数
let convertJsMessageToFSharpMsg (msg: obj) (dispatch: Msg -> unit) : unit =
    try
        // メッセージのデバッグ出力
        printfn "JS message received: %A" msg

        // メッセージの形式に応じた処理
        if isArray msg then
            // 配列形式のメッセージ処理
            let msgArray = msg :?> obj[]

            // 配列の長さチェック
            if msgArray.Length >= 2 then
                let msgType = string msgArray.[0]
                let payload = msgArray.[1]

                // 管理者関連メッセージの識別と処理を追加
                if msgType.StartsWith("Admin") then
                    match msgType with
                    | "AdminLoadData" -> dispatch (AdminMsg LoadAdminData)
                    | "AdminExportProducts" -> dispatch (AdminMsg ExportProducts)
                    | "AdminRunDiagnostic" -> dispatch (AdminMsg RunSystemDiagnostic)
                    | _ -> dispatch (CustomMsg(msgType, payload))
                else
                    dispatch (CustomMsg(msgType, payload))
            else if msgArray.Length = 1 then
                // 1要素だけの場合はペイロードなしとして処理
                let msgType = string msgArray.[0]
                dispatch (CustomMsg(msgType, createEmptyJsObj ()))
            else
                // 空配列など想定外の形式
                printfn "Invalid message array format: %A" msg
        else if jsTypeof msg = "string" then
            // 文字列メッセージ
            let msgType = unbox<string> msg
            dispatch (CustomMsg(msgType, null))
        else if jsTypeof msg = "object" && not (isNull msg) then
            // オブジェクト形式の場合、可能ならtypeとpayloadを抽出
            try
                let msgType = safeGet msg "type"
                let payload = safeGet msg "payload"

                if not (isNullOrUndefined msgType) then
                    // 管理者関連メッセージの識別と処理を追加
                    let msgTypeStr = string msgType

                    if msgTypeStr.StartsWith("Admin") then
                        match msgTypeStr with
                        | "AdminLoadData" -> dispatch (AdminMsg LoadAdminData)
                        | "AdminExportProducts" -> dispatch (AdminMsg ExportProducts)
                        | "AdminRunDiagnostic" -> dispatch (AdminMsg RunSystemDiagnostic)
                        | _ -> dispatch (CustomMsg(msgTypeStr, payload))
                    else
                        dispatch (CustomMsg(msgTypeStr, payload))
                else
                    printfn "Unable to process the object message: %A" msg
            with ex ->
                printfn "Error parsing object message: %s" ex.Message
        else
            // その他の未知の形式
            printfn "Unable to process the message: %A" msg
    with ex ->
        printfn "Error in message conversion: %s" ex.Message
        printfn "Stack trace: %s" ex.StackTrace

/// JavaScript側の関数に渡すための安全なdispatch関数を作成
let createJsDispatchFunction (dispatch: Msg -> unit) : obj =
    let jsDispatch = fun (msg: obj) -> convertJsMessageToFSharpMsg msg dispatch
    jsDispatch

/// F#からJavaScriptへのメッセージ送信
let sendMessageToJs (msgType: string) (payload: obj) : unit =
    try
        // グローバルメッセージハンドラが存在する場合呼び出す
        let globalsObject = Fable.Core.JsInterop.emitJsExpr () "window"
        let handler = safeGet globalsObject "receiveMessageFromFSharp"

        if isJsFunction handler then
            let message = createEmptyJsObj ()
            message?messageType <- msgType
            message?payload <- payload
            let _ = callJsFunction handler message
            ()
        else
            printfn "No JavaScript message handler registered"
    with ex ->
        printfn "Error sending message to JavaScript: %s" ex.Message

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

// 統合されたupdate関数を呼び出す
[<Emit("window.FSharpJsBridge.callUnifiedUpdateHandler($0, $1, $2, $3)")>]
let callUnifiedUpdateHandler (updateFn: obj) (messageType: string) (payload: obj) (model: obj) : obj = jsNative
