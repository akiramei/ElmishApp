// UpdateApiState.fs - Updated with pagination support
module App.UpdateApiState

open Elmish
open App.Types
open App.ApiClient
open App.Notifications
open App.Shared

// モックのページングデータを生成する関数
let simulatePagedData (allProducts: ProductDto list) (pageInfo: PageInfo) : ProductDto list =
    let startIndex = (pageInfo.CurrentPage - 1) * pageInfo.PageSize

    allProducts
    |> List.skip (min startIndex (List.length allProducts))
    |> List.truncate pageInfo.PageSize

// API関連の状態更新ロジック
let updateApiState (msg: ApiMsg) (state: ApiData) : ApiData * Cmd<Msg> =
    match msg with
    // ユーザー一覧取得
    | FetchUsers ->
        // まずAPIリクエストを取得
        let usersPromise = ApiClient.getUsers ()

        // 正しいハンドラーを定義 - 明示的にラムダ式で関数を定義
        let successHandler (users: UserDto list) = (FetchUsersSuccess users)
        let errorHandler (error: ApiError) = (FetchUsersError error)

        // 直接Cmd<Msg>を生成
        let cmd = ApiClient.toCmdWithErrorHandling usersPromise successHandler errorHandler

        { state with Users = Loading }, Cmd.map ApiMsg cmd

    | FetchUsersSuccess users -> { state with Users = Success users }, Cmd.none

    | FetchUsersError error ->
        { state with Users = Failed error },
        // 通知システムを使用してエラーを表示
        Cmd.ofMsg (
            NotificationMsg(
                Add(
                    Notifications.error "ユーザーデータの取得に失敗しました"
                    |> withDetails (getErrorMessage error)
                    |> fromSource "API"
                )
            )
        )

    // 単一ユーザー取得
    | FetchUser userId ->
        // まずAPIリクエストを取得
        let userPromise = ApiClient.getUserById userId

        // 正しいハンドラーを定義
        let successHandler (user: UserDto) = ApiMsg(FetchUserSuccess user)
        let errorHandler (error: ApiError) = ApiMsg(FetchUserError error)

        // 直接Cmd<Msg>を生成
        let cmd = ApiClient.toCmdWithErrorHandling userPromise successHandler errorHandler

        { state with
            SelectedUser = Some Loading },
        cmd

    | FetchUserSuccess user ->
        { state with
            SelectedUser = Some(Success user) },
        Cmd.none

    | FetchUserError error ->
        { state with
            SelectedUser = Some(Failed error) },
        Cmd.ofMsg (
            NotificationMsg(
                Add(
                    Notifications.error "ユーザー詳細の取得に失敗しました"
                    |> withDetails (getErrorMessage error)
                    |> fromSource "API"
                )
            )
        )

    // 製品一覧取得
    | FetchProducts ->
        // まずAPIリクエストを取得
        let productsPromise = ApiClient.getProducts ()

        // 正しいハンドラーを定義
        let successHandler (products: ProductDto list) = ApiMsg(FetchProductsSuccess products)
        let errorHandler (error: ApiError) = ApiMsg(FetchProductsError error)

        // 直接Cmd<Msg>を生成
        let cmd =
            ApiClient.toCmdWithErrorHandling productsPromise successHandler errorHandler

        { state with Products = Loading }, cmd

    | FetchProductsSuccess products ->
        { state with
            Products = Success products },
        Cmd.none

    | FetchProductsError error ->
        { state with Products = Failed error },
        Cmd.ofMsg (
            NotificationMsg(
                Add(
                    Notifications.error "製品データの取得に失敗しました"
                    |> withDetails (getErrorMessage error)
                    |> fromSource "API"
                )
            )
        )

    // 単一製品取得
    | FetchProduct productId ->
        // まずAPIリクエストを取得
        let productPromise = ApiClient.getProductById productId

        // 正しいハンドラーを定義
        let successHandler (product: ProductDto) = ApiMsg(FetchProductSuccess product)
        let errorHandler (error: ApiError) = ApiMsg(FetchProductError error)

        // 直接Cmd<Msg>を生成
        let cmd =
            ApiClient.toCmdWithErrorHandling productPromise successHandler errorHandler

        { state with
            SelectedProduct = Some Loading },
        cmd

    | FetchProductSuccess product ->
        { state with
            SelectedProduct = Some(Success product) },
        Cmd.none

    | FetchProductError error ->
        { state with
            SelectedProduct = Some(Failed error) },
        Cmd.ofMsg (
            NotificationMsg(
                Add(
                    Notifications.error "製品詳細の取得に失敗しました"
                    |> withDetails (getErrorMessage error)
                    |> fromSource "API"
                )
            )
        )

// API関連のコンビニエンス関数
let loadUsersCmd: Cmd<Msg> = Cmd.ofMsg (ApiMsg FetchUsers)

let loadProductsCmd: Cmd<Msg> = Cmd.ofMsg (ApiMsg FetchProducts)

let loadUserByIdCmd (userId: int64) : Cmd<Msg> = Cmd.ofMsg (ApiMsg(FetchUser userId))

let loadProductByIdCmd (productId: int64) : Cmd<Msg> =
    Cmd.ofMsg (ApiMsg(FetchProduct productId))
