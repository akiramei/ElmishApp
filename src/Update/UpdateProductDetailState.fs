// src/Update/UpdateProductDetailState.fs
module App.UpdateProductDetailState

open Elmish
open App.Types
open App.ProductDetailValidator
open App.UpdateProductApiState

let updateProductDetailState (msg: ProductDetailMsg) (model: Model) : Model * Cmd<Msg> =
    match msg with
    | EnterEditMode ->
        // 編集モードに入る時、製品詳細からフォーム状態を初期化
        let newState =
            match model.ApiData.ProductData.SelectedProductDetail with
            | Some(Success product) -> Some(createFormState product)
            | _ -> None

        { model with
            ProductDetailState =
                { model.ProductDetailState with
                    IsEditMode = true }
            ProductEditFormState = newState },
        Cmd.none

    | ExitEditMode ->
        { model with
            ProductDetailState =
                { model.ProductDetailState with
                    IsEditMode = false }
            ProductEditFormState = None }, // 終了時にフォーム状態をクリア
        Cmd.none

    | CloseDetailView ->
        { model with
            ProductDetailState =
                { model.ProductDetailState with
                    IsEditMode = false }
            ProductEditFormState = None }, // 閉じる時にフォーム状態をクリア
        Cmd.ofMsg (ProductsMsg CloseProductDetails)

    | EditFormTabChanged tab ->
        // タブ変更処理
        match model.ProductEditFormState with
        | Some formState ->
            let updatedFormState = { formState with ActiveTab = tab }

            { model with
                ProductEditFormState = Some updatedFormState },
            Cmd.none
        | None -> model, Cmd.none

    | EditFormFieldChanged fieldChange ->
        // フィールド変更処理
        match model.ProductEditFormState with
        | Some formState ->
            match fieldChange with
            | BasicField(fieldName, value) ->
                // 基本フィールド値の更新
                let updatedFields = Map.add fieldName value formState.BasicFields

                let updatedFormState =
                    { formState with
                        BasicFields = updatedFields
                        ValidationErrors = Map.remove fieldName formState.ValidationErrors
                        HasErrors = false }

                { model with
                    ProductEditFormState = Some updatedFormState },
                Cmd.none

            | AdditionalField(fieldId, value) ->
                // 追加フィールド値の更新
                let fieldValue =
                    if System.String.IsNullOrWhiteSpace(value) then
                        None
                    else
                        Some value

                let updatedFields = Map.add fieldId fieldValue formState.AdditionalFields

                let updatedFormState =
                    { formState with
                        AdditionalFields = updatedFields
                        ValidationErrors = Map.remove fieldId formState.ValidationErrors
                        HasErrors = false }

                { model with
                    ProductEditFormState = Some updatedFormState },
                Cmd.none

            | CodeSelected(code, name, price) ->
                // コード、名前、価格の更新（製品コード選択時）
                let updatedFields =
                    formState.BasicFields
                    |> Map.add "Code" code
                    |> Map.add "Name" name
                    |> Map.add "Price" (string price)

                let updatedFormState =
                    { formState with
                        BasicFields = updatedFields
                        ValidationErrors = Map.empty
                        HasErrors = false }

                { model with
                    ProductEditFormState = Some updatedFormState },
                Cmd.none
        | None -> model, Cmd.none

    | SubmitProductEdit ->
        // フォーム送信処理
        match model.ProductEditFormState, model.ApiData.ProductData.SelectedProductDetail with
        | Some formState, Some(Success product) ->
            // フォーム検証と送信
            let (validationErrors, updateDtoOpt) = validateAndCreateDto formState

            if Map.isEmpty validationErrors then
                // 検証成功、更新コマンドを発行
                match updateDtoOpt with
                | Some updateDto ->
                    let cmd = updateProductCmd (int64 product.Id) updateDto

                    let updatedModel =
                        { model with
                            ProductDetailState =
                                { model.ProductDetailState with
                                    IsEditMode = false }
                            ProductEditFormState = None } // 保存成功時にフォーム状態をクリア

                    updatedModel, cmd
                | None -> model, Cmd.none
            else
                // 検証エラー
                let updatedFormState =
                    { formState with
                        ValidationErrors = validationErrors
                        HasErrors = true }

                { model with
                    ProductEditFormState = Some updatedFormState },
                Cmd.none
        | _, _ -> model, Cmd.none

    | CancelProductEdit ->
        // 編集キャンセル
        { model with
            ProductDetailState =
                { model.ProductDetailState with
                    IsEditMode = false }
            ProductEditFormState = None }, // キャンセル時にフォーム状態をクリア
        Cmd.none
