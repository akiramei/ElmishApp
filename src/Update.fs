// Update.fs
module App.Update

open Elmish
open Fable.Core
open Fable.Core.JsInterop
open App.Types
open App.Interop

// 特別なハンドラーのためのカスタム実装
let handleDoubleCounter (payload: obj) (model: Model) : Model =
    printfn "Handling DoubleCounter message with payload: %A" payload

    try
        // カスタム更新関数があればそれを使う
        match Browser.Dom.window?customUpdates with
        | null ->
            printfn "customUpdates is null, using fallback implementation"

            { model with
                Counter = model.Counter * 2 }
        | customUpdates ->
            match customUpdates?DoubleCounter with
            | null ->
                printfn "DoubleCounter handler not found, using fallback implementation"

                { model with
                    Counter = model.Counter * 2 }
            | handler ->
                printfn "Found DoubleCounter handler, calling it"
                // 関数として明示的に型変換してから呼び出し
                let handlerFn = unbox<obj -> obj -> obj> handler
                let updatedModel = handlerFn payload model
                printfn "Handler result: %A" updatedModel

                if isNull updatedModel then
                    printfn "Handler returned null, using fallback implementation"

                    { model with
                        Counter = model.Counter * 2 }
                else
                    // 型変換が必要な場合はここで処理
                    updatedModel |> unbox<Model>
    with ex ->
        printfn "Error in DoubleCounter handler: %s" ex.Message
        printfn "Stack trace: %s" ex.StackTrace
        // エラー時はフォールバック実装を使用
        { model with
            Counter = model.Counter * 2 }

// スライダー値更新のための特別なハンドラー
let handleUpdateSliderValue (payload: obj) (model: Model) : Model =
    printfn "Handling UpdateSliderValue message with payload: %A" payload

    try
        // payloadから値を取得
        let sliderValue = payload?value |> unbox<int>
        printfn "Slider value from payload: %d" sliderValue

        // CustomStateを更新
        let updatedCustomState = model.CustomState.Add("slider-value", box sliderValue)

        printfn "Updated CustomState: %A" updatedCustomState

        { model with
            CustomState = updatedCustomState }
    with ex ->
        printfn "Error in UpdateSliderValue handler: %s" ex.Message
        printfn "Stack trace: %s" ex.StackTrace
        // エラー時は元のモデルを返す
        model

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
            // 特定のメッセージタイプの処理
            match msgType with
            | "DoubleCounter" ->
                // 専用のハンドラーを呼び出す
                let updatedModel = handleDoubleCounter payload model
                updatedModel, Cmd.none

            | "UpdateSliderValue" ->
                // スライダー値更新の専用ハンドラーを呼び出す
                let updatedModel = handleUpdateSliderValue payload model
                updatedModel, Cmd.none

            | _ ->
                // その他のカスタムメッセージは CustomState を対象に処理
                printfn "Unknown custom message type: %s" msgType
                model, Cmd.none
        with ex ->
            // エラーハンドリング
            printfn "Error in CustomMsg handling: %s" ex.Message
            printfn "Stack trace: %s" ex.StackTrace

            let errorState =
                { HasError = true
                  Message = Some(sprintf "Error processing custom message: %s" ex.Message)
                  ErrorCode = Some "CUSTOM_MSG_ERROR"
                  Source = Some "core" }

            { model with ErrorState = errorState }, Cmd.none
