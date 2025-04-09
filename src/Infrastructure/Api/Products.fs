module App.Infrastructure.Api.Products

open System
open Fable.Core.JS
open App.Shared // 共有DTOを参照
open App.Infrastructure.Api.Types
open App.Infrastructure.Api.Client

let getProducts () : Promise<Result<ProductDto list, ApiError>> =
    let path = $"/products"
    fetchData<unit, ProductDto list> GET path None

let getProductById (productId: int64) : Promise<Result<ProductDto, ApiError>> =
    let path = $"/products/{productId}"
    fetchData<unit, ProductDto> GET path None

// 製品詳細を取得する関数
let getProductDetailById (productId: int64) : Promise<Result<ProductDetailDto, ApiError>> =
    let path = $"/products/{productId}/detail"
    fetchData<unit, ProductDetailDto> GET path None

// 製品を削除する関数
let deleteProduct (productId: int64) : Promise<Result<ApiSuccessResponse, ApiError>> =
    let path = $"/products/{productId}"
    fetchData<unit, ApiSuccessResponse> DELETE path None

// 製品を更新する関数
let updateProduct (productId: int64) (productUpdate: ProductUpdateDto) : Promise<Result<ProductDetailDto, ApiError>> =
    let path = $"/products/{productId}"
    fetchData<ProductUpdateDto, ProductDetailDto> PUT path (Some productUpdate)

// 製品マスタを検索（ページング対応・実際のAPIに合わせて修正）
let searchProductMasters
    (query: string)
    (page: int)
    (pageSize: int)
    : Promise<Result<ProductMasterDto list, ApiError>> =

    let path =
        $"/productmasters/search?query={Uri.EscapeDataString(query)}&page={page}&pageSize={pageSize}"

    fetchData<unit, ProductMasterDto list> GET path None

// ページング情報を推測して返す拡張バージョン（実際のAPIに合わせて実装）
let searchProductMastersWithPaging
    (query: string)
    (page: int)
    (pageSize: int)
    : Promise<Result<ProductMasterDto list * bool * int option, ApiError>> =

    promise {
        let! result = searchProductMasters query page pageSize

        return
            result
            |> Result.map (fun items ->
                // ページングの推測：
                // 1. アイテム数がページサイズと同じ場合、次のページがある可能性が高い
                // 2. アイテム数がページサイズより少ない場合、次のページはない
                let hasMore = items.Length >= pageSize

                // 総件数は推測できないのでNone
                let totalItems = None

                (items, hasMore, totalItems))
    }
