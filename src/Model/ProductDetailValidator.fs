// Model/ProductDetailValidator.fs (更新)
module App.ProductDetailValidator

open App.Shared
open App.Types
open App.Model.ProductDetailTypes

// 製品編集フォームの状態
type EditFormState =
    { // 基本情報
      Code: string
      Name: string
      Description: string option
      Category: string option
      Price: double
      Stock: int
      SKU: string
      IsActive: bool

      // 追加フィールド
      Public01: string option
      Public02: string option
      Public03: string option
      Public04: string option
      Public05: string option
      Public06: string option
      Public07: string option
      Public08: string option
      Public09: string option
      Public10: string option

      // 検証関連
      HasErrors: bool
      ValidationErrors: Map<string, string> }

// 製品詳細から編集フォーム状態を作成
let createInitialFormState (product: ProductDetailDto) : EditFormState =
    { Code = product.Code
      Name = product.Name
      Description = product.Description
      Category = product.Category
      Price = product.Price
      Stock = product.Stock
      SKU = product.SKU
      IsActive = product.IsActive

      // 追加フィールド
      Public01 = product.Public01
      Public02 = product.Public02
      Public03 = product.Public03
      Public04 = product.Public04
      Public05 = product.Public05
      Public06 = product.Public06
      Public07 = product.Public07
      Public08 = product.Public08
      Public09 = product.Public09
      Public10 = product.Public10

      HasErrors = false
      ValidationErrors = Map.empty }

// デフォルトの初期状態（製品データがない場合用）
let defaultFormState: EditFormState =
    { Code = ""
      Name = ""
      Description = None
      Category = None
      Price = 0.0
      Stock = 0
      SKU = ""
      IsActive = true

      // 追加フィールド
      Public01 = None
      Public02 = None
      Public03 = None
      Public04 = None
      Public05 = None
      Public06 = None
      Public07 = None
      Public08 = None
      Public09 = None
      Public10 = None

      HasErrors = false
      ValidationErrors = Map.empty }

// 製品フォームのバリデーション
// 製品フォームのバリデーション
let validateProductForm (form: ProductFormState) : Map<string, string> =
    let mutable errors = Map.empty

    // コードのバリデーション（必須）
    if System.String.IsNullOrWhiteSpace form.Code then
        errors <- errors.Add("Code", "製品コードは必須です")
    elif form.Name.Length < Validation.ProductCode.MinLength then
        errors <- errors.Add("Name", $"製品名は{Validation.ProductCode.MinLength}文字以上必要です")
    elif form.Name.Length > Validation.ProductCode.MaxLength then
        errors <- errors.Add("Name", $"製品名は{Validation.ProductCode.MaxLength}文字以内である必要があります")

    // 名前のバリデーションはコードが設定されていれば不要
    if
        System.String.IsNullOrWhiteSpace form.Name
        && not (System.String.IsNullOrWhiteSpace form.Code)
    then
        errors <- errors.Add("Name", "製品名が設定されていません。有効な製品コードを選択してください。")

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
// 更新用DTOへの変換
let toProductUpdateDto (form: EditFormState) : ProductUpdateDto =
    { Code = form.Code
      Name = form.Name
      Description = form.Description
      Category = form.Category
      Price = form.Price
      Stock = form.Stock
      SKU = form.SKU
      IsActive = form.IsActive

      // 追加フィールド
      Public01 = form.Public01
      Public02 = form.Public02
      Public03 = form.Public03
      Public04 = form.Public04
      Public05 = form.Public05
      Public06 = form.Public06
      Public07 = form.Public07
      Public08 = form.Public08
      Public09 = form.Public09
      Public10 = form.Public10 }

// バリデーションと更新DTOへの変換
let validateAndCreateDto (formState: ProductEditFormState) : (Map<string, string> * ProductUpdateDto option) =
    // ProductFormState型に変換する
    let productFormState =
        { Code = Map.find "Code" formState.BasicFields
          Name = Map.find "Name" formState.BasicFields
          Description =
            let desc = Map.find "Description" formState.BasicFields

            if System.String.IsNullOrWhiteSpace desc then
                None
            else
                Some desc
          Category =
            let cat = Map.find "Category" formState.BasicFields

            if System.String.IsNullOrWhiteSpace cat then
                None
            else
                Some cat
          Price =
            match System.Double.TryParse(Map.find "Price" formState.BasicFields) with
            | true, value -> value
            | _ -> 0.0
          Stock =
            match System.Int32.TryParse(Map.find "Stock" formState.BasicFields) with
            | true, value -> value
            | _ -> 0
          SKU = Map.find "SKU" formState.BasicFields
          IsActive =
            match Map.find "IsActive" formState.BasicFields with
            | "true" -> true
            | _ -> false
          AdditionalFields = formState.AdditionalFields
          HasErrors = false
          ValidationErrors = Map.empty }

    // バリデーション実行
    let validationErrors = validateProductForm productFormState

    if Map.isEmpty validationErrors then
        // バリデーション成功 - ProductUpdateDtoを作成
        let updateDto =
            { Code = productFormState.Code
              Name = productFormState.Name
              Description = productFormState.Description
              Category = productFormState.Category
              Price = productFormState.Price
              Stock = productFormState.Stock
              SKU = productFormState.SKU
              IsActive = productFormState.IsActive

              // 追加フィールド
              Public01 = Map.tryFind "Public01" formState.AdditionalFields |> Option.defaultValue None
              Public02 = Map.tryFind "Public02" formState.AdditionalFields |> Option.defaultValue None
              Public03 = Map.tryFind "Public03" formState.AdditionalFields |> Option.defaultValue None
              Public04 = Map.tryFind "Public04" formState.AdditionalFields |> Option.defaultValue None
              Public05 = Map.tryFind "Public05" formState.AdditionalFields |> Option.defaultValue None
              Public06 = Map.tryFind "Public06" formState.AdditionalFields |> Option.defaultValue None
              Public07 = Map.tryFind "Public07" formState.AdditionalFields |> Option.defaultValue None
              Public08 = Map.tryFind "Public08" formState.AdditionalFields |> Option.defaultValue None
              Public09 = Map.tryFind "Public09" formState.AdditionalFields |> Option.defaultValue None
              Public10 = Map.tryFind "Public10" formState.AdditionalFields |> Option.defaultValue None }

        validationErrors, Some updateDto
    else
        // バリデーションエラー
        validationErrors, None
