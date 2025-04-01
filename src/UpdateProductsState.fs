// UpdateProductsState.fs - Product UI Module
module App.UpdateProductsState

open Elmish
open App.Types
open App.Shared

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
        // 詳細表示 - 現在は何もしない
        printfn "View product details requested for ID: %d" id
        state, Cmd.none

    | UpdatePageInfo totalItems ->
        // APIから取得した製品数に基づいてページング情報を更新
        let pageInfo =
            { state.PageInfo with
                TotalItems = totalItems
                TotalPages =
                    let calculated = int (ceil (float totalItems / float state.PageInfo.PageSize))
                    if calculated = 0 then 1 else calculated }

        { state with PageInfo = pageInfo }, Cmd.none
