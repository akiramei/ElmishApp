// UpdateProductsState.fs - Product UI Module (詳細表示対応)
module App.UpdateProductsState

open Elmish
open App.Types
open App.Shared
open App.Router
open App.UpdateProductApiState

// 製品UI関連の状態更新
let updateProductsState
    (msg: ProductsMsg)
    (state: ProductsState)
    (products: ProductDto list)
    : ProductsState * Cmd<Msg> =
    match msg with
    | ChangePage page ->
        // ページ変更
        let newPageInfo =
            { state.PageInfo with
                CurrentPage = page }

        { state with PageInfo = newPageInfo }, Cmd.none

    | ChangePageSize size ->
        // ページサイズ変更
        let totalPages = int (ceil (float state.PageInfo.TotalItems / float size))
        let currentPage = min state.PageInfo.CurrentPage totalPages

        let newPageInfo =
            { state.PageInfo with
                PageSize = size
                CurrentPage = currentPage
                TotalPages = totalPages }

        { state with PageInfo = newPageInfo }, Cmd.none

    | ToggleProductSelection id ->
        // 製品選択の切り替え
        let newSelectedIds =
            if Set.contains id state.SelectedIds then
                Set.remove id state.SelectedIds
            else
                Set.add id state.SelectedIds

        { state with
            SelectedIds = newSelectedIds },
        Cmd.none

    | ToggleAllProducts isSelected ->
        // 全製品選択/解除
        let newSelectedIds =
            if isSelected then
                // 全選択: 現在のページの全製品IDを選択
                products |> List.map (fun p -> p.Id) |> Set.ofList
            else
                // 全解除
                Set.empty

        { state with
            SelectedIds = newSelectedIds },
        Cmd.none

    | ViewProductDetails id ->
        // 詳細表示 - ルートを変更するコマンドを発行
        let route = Route.ProductDetail id
        state, Cmd.ofMsg (RouteChanged route)

    | EditProduct id ->
        // 編集モード - 詳細表示と同様にルート変更
        let route = Route.ProductDetail id
        state, Cmd.ofMsg (RouteChanged route)

    | CloseProductDetails ->
        // 詳細を閉じる - 一覧に戻るコマンドを発行
        let route = Route.Products
        state, Cmd.ofMsg (RouteChanged route)

    | UpdatePageInfo totalItems ->
        // APIから取得した製品数に基づいてページング情報を更新
        let pageInfo =
            { state.PageInfo with
                TotalItems = totalItems
                TotalPages =
                    let calculated = int (ceil (float totalItems / float state.PageInfo.PageSize))
                    if calculated = 0 then 1 else calculated }

        { state with PageInfo = pageInfo }, Cmd.none

    | DeleteSelectedProducts ->
        // 選択されている製品IDのリストを取得
        let selectedIds = state.SelectedIds

        if Set.isEmpty selectedIds then
            state, Cmd.none
        else
            // 各製品の削除コマンドを作成
            let deleteCommands =
                selectedIds |> Set.toList |> List.map (int64 >> deleteProductCmd)

            // 選択をクリア
            { state with SelectedIds = Set.empty }, Cmd.batch deleteCommands

    | ChangeSort column ->
        // ソート列の変更
        let newSortDirection, newActiveSort =
            if state.ActiveSort = Some column then
                if state.SortDirection = "asc" then
                    "desc", Some column // 昇順→降順
                else
                    "asc", None // 降順→ソート解除
            else
                "asc", Some column // 未ソート→昇順

        { state with
            ActiveSort = newActiveSort
            SortDirection = newSortDirection },
        Cmd.none

    | ClearSort ->
        // ソートをクリア
        { state with
            ActiveSort = None
            SortDirection = "asc" },
        Cmd.none

    | ChangeSearch value ->
        // 検索値の変更
        { state with SearchValue = value }, Cmd.none

    | SetSelectedProducts ids ->
        // 選択された製品IDのセットを設定
        { state with SelectedIds = ids }, Cmd.none
