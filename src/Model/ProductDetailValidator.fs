// Model/ProductDetailValidator.fs - Updated for new type structure
module App.ProductDetailValidator

open App.Shared
open App.Types
open App.Model.ProductDetailTypes

// ========== 検証関数 ==========

// 製品フォームの検証 - 詳細モデルに対する検証を行う
let validateDetailedProductForm (form: DetailedProductFormState) : Map<string, string> =
    let mutable errors = Map.empty

    // コードのバリデーション（必須）
    if System.String.IsNullOrWhiteSpace form.Code then
        errors <- errors.Add("Code", "製品コードは必須です")
    elif form.Code.Length < Validation.ProductCode.MinLength then
        errors <- errors.Add("Code", $"製品コードは{Validation.ProductCode.MinLength}文字以上必要です")
    elif form.Code.Length > Validation.ProductCode.MaxLength then
        errors <- errors.Add("Code", $"製品コードは{Validation.ProductCode.MaxLength}文字以内である必要があります")

    // 名前のバリデーションはコードが設定されていれば不要
    if
        System.String.IsNullOrWhiteSpace form.Name
        && not (System.String.IsNullOrWhiteSpace form.Code)
    then
        errors <- errors.Add("Name", "製品名が設定されていません。有効な製品コードを選択してください。")
    elif form.Name.Length < Validation.ProductName.MinLength then
        errors <- errors.Add("Name", $"製品名は{Validation.ProductName.MinLength}文字以上必要です")
    elif form.Name.Length > Validation.ProductName.MaxLength then
        errors <- errors.Add("Name", $"製品名は{Validation.ProductName.MaxLength}文字以内である必要があります")

    // 価格のバリデーション
    if form.Price <= 0.0 then
        errors <- errors.Add("Price", "価格は0より大きな値である必要があります")

    // 在庫数のバリデーション
    if form.Stock < 0 then
        errors <- errors.Add("Stock", "在庫数は0以上である必要があります")

    // SKUのバリデーション
    if System.String.IsNullOrWhiteSpace form.SKU then
        errors <- errors.Add("SKU", "SKUは必須です")
    elif form.SKU.Length <> Validation.SKU.Length then
        errors <- errors.Add("SKU", $"SKUは{Validation.SKU.Length}文字である必要があります")

    // 追加フィールドのバリデーション
    form.AdditionalFields
    |> Map.iter (fun key value ->
        match value with
        | Some v when v.Length > 100 -> errors <- errors.Add(key, $"{key}は100文字以内で入力してください")
        | _ -> ())

    errors

// 基本ProductEditFormStateの検証 - Types.fsで定義された型に対する検証
let validateProductForm (form: Types.ProductEditFormState) : Map<string, string> =
    // 詳細モデルに変換してから検証
    let detailedForm = fromBasicFormState form
    validateDetailedProductForm detailedForm

// バリデーションを実行し、成功した場合はDTOを作成する
let validateAndCreateDto (formState: Types.ProductEditFormState) : (Map<string, string> * ProductUpdateDto option) =
    let validationErrors = validateProductForm formState

    if Map.isEmpty validationErrors then
        // 検証成功、更新DTOを作成
        let updateDto = createProductUpdateDto formState
        validationErrors, updateDto
    else
        // 検証エラー
        validationErrors, None

// 検証結果を元に更新されたフォーム状態を返す
let applyValidation (formState: Types.ProductEditFormState) : Types.ProductEditFormState =
    let validationErrors = validateProductForm formState

    { formState with
        ValidationErrors = validationErrors
        HasErrors = not (Map.isEmpty validationErrors) }

// 製品詳細DTOから検証済みのフォーム状態を作成
let createValidatedFormState (product: ProductDetailDto) : Types.ProductEditFormState =
    let basicState = createBasicFormState product
    applyValidation basicState

// 詳細フォーム状態に対する検証を適用
let applyDetailedValidation (state: DetailedProductFormState) : DetailedProductFormState =
    let validationErrors = validateDetailedProductForm state

    { state with
        ValidationErrors = validationErrors
        HasErrors = not (Map.isEmpty validationErrors) }
