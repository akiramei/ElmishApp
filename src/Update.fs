// Update.fs
module App.Update

open Elmish
open Fable.Core.JsInterop
open App.Types
open App.Plugins

// アプリケーションの状態更新関数
let update msg model =
    match msg with
    | NavigateTo tab -> { model with CurrentTab = tab }, Cmd.none

    | IncrementCounter ->
        { model with
            Counter = model.Counter + 1 },
        Cmd.none

    | DecrementCounter ->
        { model with
            Counter = model.Counter - 1 },
        Cmd.none

    | SetError errorMsg ->
        let errorState =
            { HasError = true
              Message = Some errorMsg
              ErrorCode = None
              Source = Some "core" }

        { model with ErrorState = errorState }, Cmd.none

    | ClearError ->
        let errorState =
            { HasError = false
              Message = None
              ErrorCode = None
              Source = None }

        { model with ErrorState = errorState }, Cmd.none

    | CustomMsg(msgType, payload) ->
        printfn "Received CustomMsg: %s with payload %A" msgType payload

        try
            // モデルをシリアライズする前にログ出力（ただし型情報は出力しない）
            printfn "Original model before serialization: %A" model

            // モデルをシリアライズ
            let serializedModel = toPlainJsObj model
            printfn "Serialized model: %A" serializedModel

            // カスタム更新関数を適用
            printfn "Applying custom updates for message type: %s" msgType
            let updatedJsModel = applyCustomUpdates msgType payload serializedModel

            // JavaScript側での更新を確認
            if isNull updatedJsModel then
                printfn "No updates from JavaScript plugins (null returned)"
                model, Cmd.none
            else
                printfn "Updates received from JavaScript plugins"

                // JavaScriptオブジェクトから更新されたF#モデルに変換
                try
                    let updatedModel = updatedJsModel |> unbox<Model>
                    printfn "Successfully converted updated model: %A" updatedModel
                    updatedModel, Cmd.none
                with ex ->
                    printfn "Error converting updated model: %s" ex.Message
                    // 変換に失敗した場合は元のモデルを使用
                    model, Cmd.none
        with ex ->
            // エラーハンドリング
            printfn "Error in CustomMsg handling: %s" ex.Message

            let errorState =
                { HasError = true
                  Message = Some(sprintf "Error processing custom message: %s" ex.Message)
                  ErrorCode = Some "CUSTOM_MSG_ERROR"
                  Source = Some "core" }

            { model with ErrorState = errorState }, Cmd.none
