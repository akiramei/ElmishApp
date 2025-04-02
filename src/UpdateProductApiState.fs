// UpdateProductApiState.fs - Product Domain API Module
module App.UpdateProductApiState

open Elmish
open App.Types
open App.ApiClient
open App.Notifications
open App.Shared

// 製品関連APIの状態更新
let updateProductApiState (msg: ProductApiMsg) (state: ProductApiData) : ProductApiData * Cmd<Msg> =
    match msg with
    | FetchProducts ->
        // 製品一覧取得APIリクエスト
        let productsPromise = ApiClient.getProducts ()

        // 成功/エラーハンドラー
        let successHandler (products: ProductDto list) =
            ApiMsg(ProductApi(FetchProductsSuccess products))

        let errorHandler (error: ApiError) =
            ApiMsg(ProductApi(FetchProductsError error))

        // Loading状態に更新し、APIリクエストコマンドを発行
        { state with Products = Loading }, ApiClient.toCmdWithErrorHandling productsPromise successHandler errorHandler

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
                    |> withDetails (getErrorMessage error)
                    |> fromSource "ProductAPI"
                )
            )
        )

    // 特定製品取得メッセージの処理
    | FetchProduct productId ->
        // 製品詳細取得APIリクエスト
        let productPromise = ApiClient.getProductById productId

        // 成功/エラーハンドラー
        let successHandler (product: ProductDto) =
            ApiMsg(ProductApi(FetchProductSuccess product))

        let errorHandler (error: ApiError) =
            ApiMsg(ProductApi(FetchProductError error))

        // Loading状態に更新し、APIリクエストコマンドを発行
        { state with
            SelectedProduct = Some Loading },
        ApiClient.toCmdWithErrorHandling productPromise successHandler errorHandler

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
                    |> withDetails (getErrorMessage error)
                    |> fromSource "ProductAPI"
                )
            )
        )

    // 製品詳細データ取得メッセージの処理（新規追加）
    | FetchProductDetail productId ->
        // 製品詳細取得APIリクエスト (新しいエンドポイント)
        let productDetailPromise = ApiClient.getProductDetailById productId

        // 成功/エラーハンドラー
        let successHandler (productDetail: ProductDetailDto) =
            ApiMsg(ProductApi(FetchProductDetailSuccess productDetail))

        let errorHandler (error: ApiError) =
            ApiMsg(ProductApi(FetchProductDetailError error))

        // Loading状態に更新し、APIリクエストコマンドを発行
        { state with
            SelectedProductDetail = Some Loading },
        ApiClient.toCmdWithErrorHandling productDetailPromise successHandler errorHandler

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
                    |> withDetails (getErrorMessage error)
                    |> fromSource "ProductDetailAPI"
                )
            )
        )

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
