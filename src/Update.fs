// Update.fs
module App.Update

open Elmish
open Fable.Core.JsInterop
open App.Types
open App.Plugins

// アプリケーションの状態更新関数
let update msg model =
    match msg with
    | NavigateTo tab ->
        { model with CurrentTab = tab }, Cmd.none
        
    | IncrementCounter ->
        { model with Counter = model.Counter + 1 }, Cmd.none
        
    | DecrementCounter ->
        { model with Counter = model.Counter - 1 }, Cmd.none
        
    | SetError errorMsg ->
        let errorState = {
            HasError = true
            Message = Some errorMsg
            ErrorCode = None
            Source = Some "core"
        }
        { model with ErrorState = errorState }, Cmd.none
        
    | ClearError ->
        let errorState = {
            HasError = false
            Message = None
            ErrorCode = None
            Source = None
        }
        { model with ErrorState = errorState }, Cmd.none
        
    | CustomMsg (msgType, payload) ->
        try
            // モデルをシリアライズ
            let serializedModel = toPlainJsObj model
            
            // すべてのプラグインのカスタム更新関数を順次適用
            let updatedJsModel = applyCustomUpdates msgType payload serializedModel
            
            // JavaScript側での更新がない場合は元のモデルを返す
            if isNull updatedJsModel then
                model, Cmd.none
            else
                // JavaScriptオブジェクトから更新されたF#モデルに変換
                let updatedModel = updatedJsModel |> unbox<Model>
                
                // 特定のカスタムメッセージタイプに応じた追加処理
                let finalModel =
                    match msgType with
                    | "SetError" ->
                        // エラー設定のカスタムメッセージ処理例
                        let errorMsg = payload?message |> unbox<string>
                        let source = 
                            if isNull payload?source then "plugin"
                            else payload?source |> unbox<string>
                            
                        { updatedModel with 
                            ErrorState = { 
                                HasError = true 
                                Message = Some errorMsg 
                                ErrorCode = None
                                Source = Some source
                            } 
                        }
                        
                    | "ClearError" ->
                        // エラークリアのカスタムメッセージ処理例
                        { updatedModel with 
                            ErrorState = { 
                                HasError = false 
                                Message = None 
                                ErrorCode = None
                                Source = None
                            } 
                        }
                        
                    | _ -> updatedModel
                    
                finalModel, Cmd.none
        with ex ->
            // エラーハンドリング - カスタム更新の失敗を捕捉
            printfn "Error in CustomMsg handling: %s" ex.Message
            
            let errorState = {
                HasError = true
                Message = Some (sprintf "Error processing custom message: %s" ex.Message)
                ErrorCode = Some "CUSTOM_MSG_ERROR"
                Source = Some "core"
            }
            
            { model with ErrorState = errorState }, Cmd.none