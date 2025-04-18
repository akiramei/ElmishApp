// UpdateProductApiState.fs - Product Domain API Module
module App.UpdateProductApiState

open Elmish
open App.Infrastructure
open App.Types
open App.Notifications
open App.Shared

// モックのページングデータを生成する関数
let simulatePagedData (allProducts: ProductDto list) (pageInfo: PageInfo) : ProductDto list =
    let startIndex = (pageInfo.CurrentPage - 1) * pageInfo.PageSize

    allProducts
    |> List.skip (min startIndex (List.length allProducts))
    |> List.truncate pageInfo.PageSize

// APIを呼び出すためのコンビニエンス関数
let loadProductsCmd: Cmd<Msg> = Cmd.ofMsg (ApiMsg(ProductApi FetchProducts))

let loadProductByIdCmd (productId: int64) : Cmd<Msg> =
    Cmd.ofMsg (ApiMsg(ProductApi(FetchProduct productId)))

// 製品詳細を取得するためのコンビニエンス関数
let loadProductDetailByIdCmd (productId: int64) : Cmd<Msg> =
    Cmd.ofMsg (ApiMsg(ProductApi(FetchProductDetail productId)))

// 製品削除のためのコンビニエンス関数
let deleteProductCmd (productId: int64) : Cmd<Msg> =
    Cmd.ofMsg (ApiMsg(ProductApi(DeleteProduct productId)))

// 製品更新のためのコンビニエンス関数
let updateProductCmd (productId: int64) (product: ProductUpdateDto) : Cmd<Msg> =
    Cmd.ofMsg (ApiMsg(ProductApi(UpdateProduct(productId, product))))

// 製品関連APIの状態更新
let updateProductApiState (msg: ProductApiMsg) (state: ProductApiData) : ProductApiData * Cmd<Msg> =
    match msg with
    | FetchProducts ->
        // 製品一覧取得APIリクエスト
        let productsPromise = Api.Products.getProducts ()

        // 成功/エラーハンドラー
        let successHandler (products: ProductDto list) =
            ApiMsg(ProductApi(FetchProductsSuccess products))

        let errorHandler (error: Api.Types.ApiError) =
            ApiMsg(ProductApi(FetchProductsError error))

        // Loading状態に更新し、APIリクエストコマンドを発行
        { state with Products = Loading }, Api.Client.toCmdWithErrorHandling productsPromise successHandler errorHandler

    | FetchProductsSuccess products ->
        // 成功時はデータを保存
        let newState =
            { state with
                Products = Success products }

        // 製品データ取得成功時にはページング情報を更新するメッセージも発行
        let updatePageInfoCmd =
            let totalItems = List.length products
            Cmd.ofMsg (ProductsMsg(UpdatePageInfo totalItems))

        newState, updatePageInfoCmd

    | FetchProductsError error ->
        // エラー時は状態を更新し、通知を表示
        { state with Products = Failed error },
        Cmd.ofMsg (
            NotificationMsg(
                Add(
                    Notifications.error "製品データの取得に失敗しました"
                    |> withDetails (Api.Client.getErrorMessage error)
                    |> fromSource "ProductAPI"
                )
            )
        )

    // 特定製品取得メッセージの処理
    | FetchProduct productId ->
        // 製品詳細取得APIリクエスト
        let productPromise = Api.Products.getProductById productId

        // 成功/エラーハンドラー
        let successHandler (product: ProductDto) =
            ApiMsg(ProductApi(FetchProductSuccess product))

        let errorHandler (error: Api.Types.ApiError) =
            ApiMsg(ProductApi(FetchProductError error))

        // Loading状態に更新し、APIリクエストコマンドを発行
        { state with
            SelectedProduct = Some Loading },
        Api.Client.toCmdWithErrorHandling productPromise successHandler errorHandler

    | FetchProductSuccess product ->
        // 成功時はデータを保存
        { state with
            SelectedProduct = Some(Success product) },
        Cmd.none

    | FetchProductError error ->
        // エラー時は状態を更新し、通知を表示
        { state with
            SelectedProduct = Some(Failed error) },
        Cmd.ofMsg (
            NotificationMsg(
                Add(
                    Notifications.error "製品詳細の取得に失敗しました"
                    |> withDetails (Api.Client.getErrorMessage error)
                    |> fromSource "ProductAPI"
                )
            )
        )

    // 製品詳細データ取得メッセージの処理
    | FetchProductDetail productId ->
        // 製品詳細取得APIリクエスト (新しいエンドポイント)
        let productDetailPromise = Api.Products.getProductDetailById productId

        // 成功/エラーハンドラー
        let successHandler (productDetail: ProductDetailDto) =
            ApiMsg(ProductApi(FetchProductDetailSuccess productDetail))

        let errorHandler (error: Api.Types.ApiError) =
            ApiMsg(ProductApi(FetchProductDetailError error))

        // Loading状態に更新し、APIリクエストコマンドを発行
        { state with
            SelectedProductDetail = Some Loading },
        Api.Client.toCmdWithErrorHandling productDetailPromise successHandler errorHandler

    | FetchProductDetailSuccess productDetail ->
        // 詳細データ取得成功時の処理
        { state with
            SelectedProductDetail = Some(Success productDetail) },
        Cmd.none

    | FetchProductDetailError error ->
        // 詳細データ取得失敗時の処理
        { state with
            SelectedProductDetail = Some(Failed error) },
        Cmd.ofMsg (
            NotificationMsg(
                Add(
                    Notifications.error "製品詳細データの取得に失敗しました"
                    |> withDetails (Api.Client.getErrorMessage error)
                    |> fromSource "ProductDetailAPI"
                )
            )
        )

    // 製品削除処理
    | DeleteProduct productId ->
        // API リクエスト
        let deletePromise = Api.Products.deleteProduct productId

        // 成功/エラーハンドラー
        let successHandler (_: ApiSuccessResponse) = ApiMsg(ProductApi DeleteProductSuccess)

        let errorHandler (error: Api.Types.ApiError) =
            ApiMsg(ProductApi(DeleteProductError error))

        // 状態はそのまま、APIリクエストコマンドを発行
        state, Api.Client.toCmdWithErrorHandling deletePromise successHandler errorHandler

    | DeleteProductSuccess ->
        // 削除成功時はリストを再読み込み
        state,
        Cmd.batch
            [ loadProductsCmd
              Cmd.ofMsg (NotificationMsg(Add(Notifications.info "製品が正常に削除されました" |> fromSource "ProductAPI"))) ]

    | DeleteProductError error ->
        // エラー時は通知を表示
        state,
        Cmd.ofMsg (
            NotificationMsg(
                Add(
                    Notifications.error "製品の削除に失敗しました"
                    |> withDetails (Api.Client.getErrorMessage error)
                    |> fromSource "ProductAPI"
                )
            )
        )

    // 製品更新処理
    | UpdateProduct(productId, productUpdate) ->
        // API リクエスト
        let updatePromise = Api.Products.updateProduct productId productUpdate

        // 成功/エラーハンドラー
        let successHandler (product: ProductDetailDto) =
            ApiMsg(ProductApi(UpdateProductSuccess product))

        let errorHandler (error: Api.Types.ApiError) =
            ApiMsg(ProductApi(UpdateProductError error))

        // 状態を更新中に変更、APIリクエストコマンドを発行
        { state with
            SelectedProductDetail = Some Loading },
        Api.Client.toCmdWithErrorHandling updatePromise successHandler errorHandler

    | UpdateProductSuccess product ->
        // 成功時は詳細情報と選択情報を更新し通知
        { state with
            SelectedProductDetail = Some(Success product)
            SelectedProduct =
                Some(
                    Success
                        { Id = product.Id
                          Code = product.Code
                          Name = product.Name
                          Description = product.Description
                          Category = product.Category
                          Price = product.Price
                          Stock = product.Stock
                          SKU = product.SKU
                          IsActive = product.IsActive
                          CreatedAt = product.CreatedAt
                          UpdatedAt = product.UpdatedAt }
                ) },
        Cmd.batch
            [
              // 製品一覧を再読み込み
              loadProductsCmd
              // 成功通知
              Cmd.ofMsg (NotificationMsg(Add(Notifications.info "製品が正常に更新されました" |> fromSource "ProductAPI"))) ]

    | UpdateProductError error ->
        // エラー時は状態を更新し、通知を表示
        { state with
            SelectedProductDetail = Some(Failed error) },
        Cmd.ofMsg (
            NotificationMsg(
                Add(
                    Notifications.error "製品の更新に失敗しました"
                    |> withDetails (Api.Client.getErrorMessage error)
                    |> fromSource "ProductAPI"
                )
            )
        )
