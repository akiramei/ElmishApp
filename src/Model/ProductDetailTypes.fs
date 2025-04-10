// Model/ProductDetailTypes.fs - Enhanced with comprehensive implementation
module App.Model.ProductDetailTypes

open System
open App.Shared
open App.Types

// 詳細な製品フォームの状態を表す型
// Types.fsの基本的なProductEditFormStateよりも詳細な実装
type DetailedProductFormState =
    {
      // 基本情報
      Code: string
      Name: string
      Description: string option
      Category: string option
      Price: float
      Stock: int
      SKU: string
      IsActive: bool

      // UIステート - モデルの振る舞いに関する状態
      ActiveTab: DetailTab
      HasErrors: bool
      ValidationErrors: Map<string, string>

      // 追加フィールド
      // 将来の拡張性を考慮して Map 形式で保持
      AdditionalFields: Map<string, string option> }

// ========== 変換関数 ==========

// DetailedProductFormState -> Types.ProductEditFormState (上位モジュール用)
let toBasicFormState (detailed: DetailedProductFormState) : App.Types.ProductEditFormState =
    // 基本フィールドをマップ形式に変換
    let basicFields =
        Map
            [ "Code", detailed.Code
              "Name", detailed.Name
              "Description", Option.defaultValue "" detailed.Description
              "Category", Option.defaultValue "" detailed.Category
              "Price", string detailed.Price
              "Stock", string detailed.Stock
              "SKU", detailed.SKU
              "IsActive", string detailed.IsActive ]

    { BasicFields = basicFields
      AdditionalFields = detailed.AdditionalFields
      ValidationErrors = detailed.ValidationErrors
      HasErrors = detailed.HasErrors
      ActiveTab = detailed.ActiveTab }

// Types.ProductEditFormState -> DetailedProductFormState (下位モジュール用)
let fromBasicFormState (basic: App.Types.ProductEditFormState) : DetailedProductFormState =
    // Map から値を取得するヘルパー関数
    let tryGetValue key defaultValue (map: Map<string, string>) =
        match map.TryFind key with
        | Some value -> value
        | None -> defaultValue

    let tryParseOption key (map: Map<string, string>) =
        match map.TryFind key with
        | Some value when String.IsNullOrWhiteSpace value -> None
        | Some value -> Some value
        | None -> None

    let tryParseFloat key defaultValue (map: Map<string, string>) =
        match map.TryFind key with
        | Some value ->
            match Double.TryParse value with
            | true, floatValue -> floatValue
            | _ -> defaultValue
        | None -> defaultValue

    let tryParseInt key defaultValue (map: Map<string, string>) =
        match map.TryFind key with
        | Some value ->
            match Int32.TryParse value with
            | true, intValue -> intValue
            | _ -> defaultValue
        | None -> defaultValue

    let tryParseBool key (map: Map<string, string>) =
        match map.TryFind key with
        | Some value -> value = "true" || value = "True"
        | None -> false

    { Code = tryGetValue "Code" "" basic.BasicFields
      Name = tryGetValue "Name" "" basic.BasicFields
      Description = tryParseOption "Description" basic.BasicFields
      Category = tryParseOption "Category" basic.BasicFields
      Price = tryParseFloat "Price" 0.0 basic.BasicFields
      Stock = tryParseInt "Stock" 0 basic.BasicFields
      SKU = tryGetValue "SKU" "" basic.BasicFields
      IsActive = tryParseBool "IsActive" basic.BasicFields

      ActiveTab = basic.ActiveTab
      HasErrors = basic.HasErrors
      ValidationErrors = basic.ValidationErrors
      AdditionalFields = basic.AdditionalFields }

// ========== ヘルパー関数 ==========

// 追加フィールドの値を取得するヘルパー関数
let getAdditionalField fieldName (state: DetailedProductFormState) =
    Map.tryFind fieldName state.AdditionalFields |> Option.defaultValue None

// 追加フィールドの値を設定するヘルパー関数
let setAdditionalField fieldName value (state: DetailedProductFormState) =
    let newFields =
        if String.IsNullOrWhiteSpace(value) then
            Map.add fieldName None state.AdditionalFields
        else
            Map.add fieldName (Some value) state.AdditionalFields

    { state with
        AdditionalFields = newFields }

// ========== 製品DTOとの変換関数 ==========

// DTOから詳細モデルを作成する関数
let createFromProductDto (product: ProductDetailDto) : DetailedProductFormState =
    let additionalFields =
        [ "Public01", product.Public01
          "Public02", product.Public02
          "Public03", product.Public03
          "Public04", product.Public04
          "Public05", product.Public05
          "Public06", product.Public06
          "Public07", product.Public07
          "Public08", product.Public08
          "Public09", product.Public09
          "Public10", product.Public10 ]
        |> Map.ofList

    { Code = product.Code
      Name = product.Name
      Description = product.Description
      Category = product.Category
      Price = product.Price
      Stock = product.Stock
      SKU = product.SKU
      IsActive = product.IsActive

      HasErrors = false
      ValidationErrors = Map.empty
      ActiveTab = BasicInfo

      AdditionalFields = additionalFields }

// Types.fsで定義された基本ProductEditFormStateを生成するヘルパー
let createBasicFormState (product: ProductDetailDto) : App.Types.ProductEditFormState =
    let detailed = createFromProductDto product
    toBasicFormState detailed

// 更新用DTOへの変換
let toProductUpdateDto (state: DetailedProductFormState) : ProductUpdateDto =
    { Code = state.Code
      Name = state.Name
      Description = state.Description
      Category = state.Category
      Price = state.Price
      Stock = state.Stock
      SKU = state.SKU
      IsActive = state.IsActive

      Public01 = getAdditionalField "Public01" state
      Public02 = getAdditionalField "Public02" state
      Public03 = getAdditionalField "Public03" state
      Public04 = getAdditionalField "Public04" state
      Public05 = getAdditionalField "Public05" state
      Public06 = getAdditionalField "Public06" state
      Public07 = getAdditionalField "Public07" state
      Public08 = getAdditionalField "Public08" state
      Public09 = getAdditionalField "Public09" state
      Public10 = getAdditionalField "Public10" state }

// ProductEditFormStateからProductUpdateDtoを作成するヘルパー関数
let createProductUpdateDto (formState: App.Types.ProductEditFormState) : ProductUpdateDto option =
    let detailed = fromBasicFormState formState
    Some(toProductUpdateDto detailed)

// フィールド更新用のヘルパー関数
let updateField (field: App.Types.ProductEditFormField) (state: DetailedProductFormState) : DetailedProductFormState =
    match field with
    | App.Types.BasicField(name, value) ->
        match name with
        | "Code" -> { state with Code = value }
        | "Name" -> { state with Name = value }
        | "Description" ->
            { state with
                Description = if String.IsNullOrWhiteSpace value then None else Some value }
        | "Category" ->
            { state with
                Category = if String.IsNullOrWhiteSpace value then None else Some value }
        | "Price" ->
            match Double.TryParse value with
            | true, price -> { state with Price = price }
            | _ -> state
        | "Stock" ->
            match Int32.TryParse value with
            | true, stock -> { state with Stock = stock }
            | _ -> state
        | "SKU" -> { state with SKU = value }
        | "IsActive" ->
            { state with
                IsActive = value = "true" || value = "True" }
        | _ -> state

    | App.Types.AdditionalField(fieldId, value) -> setAdditionalField fieldId value state

    | App.Types.CodeSelected(code, name, price) ->
        { state with
            Code = code
            Name = name
            Price = price }
