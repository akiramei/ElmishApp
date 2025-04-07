// Model/ProductDetailTypes.fs (新しいファイル)
module App.Model.ProductDetailTypes

open System
open App.Shared

// 共通型定義
type ProductFormState =
    {
      // 基本情報
      Name: string
      Description: string option
      Category: string option
      Price: float
      Stock: int
      SKU: string
      IsActive: bool

      // UIステート
      HasErrors: bool
      ValidationErrors: Map<string, string>

      // 追加フィールド
      // 将来の拡張性を考慮して Map 形式で保持
      AdditionalFields: Map<string, string option> }

// ヘルパー関数
let getAdditionalField fieldName state =
    Map.tryFind fieldName state.AdditionalFields |> Option.defaultValue None

let setAdditionalField fieldName value state =
    let newFields =
        if String.IsNullOrWhiteSpace(value) then
            Map.add fieldName None state.AdditionalFields
        else
            Map.add fieldName (Some value) state.AdditionalFields

    { state with
        AdditionalFields = newFields }

// 初期状態の作成
let createFromProductDto (product: ProductDetailDto) =
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

    { Name = product.Name
      Description = product.Description
      Category = product.Category
      Price = product.Price
      Stock = product.Stock
      SKU = product.SKU
      IsActive = product.IsActive

      HasErrors = false
      ValidationErrors = Map.empty

      AdditionalFields = additionalFields }

// DTOへの変換
let toProductUpdateDto (state: ProductFormState) : ProductUpdateDto =
    { Name = state.Name
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
