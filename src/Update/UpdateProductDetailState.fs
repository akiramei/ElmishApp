// src/Update/UpdateProductDetailState.fs - Updated for new type structure
module App.UpdateProductDetailState

open Elmish
open App.Types
open App.ProductDetailValidator
open App.Model.ProductDetailTypes
open App.UpdateProductApiState

// 製品詳細の状態更新
let updateProductDetailState (msg: ProductDetailMsg) (model: Model) : Model * Cmd<Msg> =
    match msg with
    | EnterEditMode ->
        // 編集モードに入る時、製品詳細からフォーム状態を初期化
        let newState =
            match model.ApiData.ProductData.SelectedProductDetail with
            | Some(Success product) ->
                // 製品詳細DTOから検証済みのフォーム状態を作成
                Some(createValidatedFormState product)
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
            // フィールド値の更新
            let detailedForm = fromBasicFormState formState
            let updatedDetailedForm = updateField fieldChange detailedForm

            // 更新されたフィールドの検証エラーをクリア
            let fieldName =
                match fieldChange with
                | BasicField(name, _) -> name
                | AdditionalField(id, _) -> id
                | CodeSelected(_, _, _) -> "Code"

            let validationErrors =
                if formState.ValidationErrors.ContainsKey fieldName then
                    formState.ValidationErrors.Remove fieldName
                else
                    formState.ValidationErrors

            // 基本フォーム状態に変換して返す
            let updatedFormState =
                toBasicFormState updatedDetailedForm
                |> fun state ->
                    { state with
                        ValidationErrors = validationErrors }

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
