// ProductDetailValidator.fs
module App.ProductDetailValidator

open App.Shared

// 製品編集フォームの状態
type EditFormState =
    { Name: string
      Description: string option
      Category: string option
      Price: double
      Stock: int
      SKU: string
      IsActive: bool
      HasErrors: bool
      ValidationErrors: Map<string, string> }

// 製品詳細から編集フォーム状態を作成
let createInitialFormState (product: ProductDetailDto) =
    { Name = product.Name
      Description = product.Description
      Category = product.Category
      Price = product.Price
      Stock = product.Stock
      SKU = product.SKU
      IsActive = product.IsActive
      HasErrors = false
      ValidationErrors = Map.empty }

// デフォルトの初期状態（製品データがない場合用）
let defaultFormState =
    { Name = ""
      Description = None
      Category = None
      Price = 0.0
      Stock = 0
      SKU = ""
      IsActive = true
      HasErrors = false
      ValidationErrors = Map.empty }

// 製品フォームのバリデーション
let validateProductForm (form: EditFormState) : Map<string, string> =
    let mutable errors = Map.empty

    // 製品名のバリデーション
    if System.String.IsNullOrWhiteSpace form.Name then
        errors <- errors.Add("Name", "製品名は必須です")
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

    errors

// 更新用DTOへの変換
let toProductUpdateDto (form: EditFormState) : ProductUpdateDto =
    { Name = form.Name
      Description = form.Description
      Category = form.Category
      Price = form.Price
      Stock = form.Stock
      SKU = form.SKU
      IsActive = form.IsActive }
