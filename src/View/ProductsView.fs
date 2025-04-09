// ProductsView.fs - Updated with pure UI components
module App.ProductsView

open Feliz
open App.Types
open App.Shared
open App.Infrastructure.Api
open App.UI.Components.Common.Button
open App.UI.Components.Common.Status
open App.UI.Components.DataDisplay
open App.UI.Layouts.ResponsiveLayout
open App.UI.Theme.Icons

// 製品一覧ツールバー（削除ボタン付き）
let renderToolbar (selectedIds: Set<int>) (dispatch: Msg -> unit) =
    if Set.isEmpty selectedIds then
        Html.none
    else
        Html.div
            [ prop.className "bg-blue-50 p-3 mb-3 flex items-center justify-between rounded"
              prop.children
                  [ Html.span
                        [ prop.className "text-blue-700 font-medium"
                          prop.text (sprintf "%d 件選択中" (Set.count selectedIds)) ]
                    iconButton
                        delete
                        (Some "選択した製品を削除")
                        (fun () ->
                            if (Set.count selectedIds) > 0 then
                                if Browser.Dom.window.confirm ("選択した製品を削除してもよろしいですか？") then
                                    dispatch (ProductsMsg DeleteSelectedProducts))
                        "danger" ] ]

// 製品テーブルの拡張（全選択機能付き）
let renderProductsTable (products: ProductDto list) (productsState: ProductsState) (dispatch: Msg -> unit) =
    let selectedIds = productsState.SelectedIds

    let allSelected =
        not (Set.isEmpty selectedIds)
        && List.forall (fun (p: ProductDto) -> Set.contains p.Id selectedIds) products

    Html.div
        [
          // 選択状態に応じたツールバー表示
          renderToolbar selectedIds dispatch

          // テーブル
          Table.dataTable
              [ Html.th
                    [ prop.className "w-16 px-6 py-4 whitespace-nowrap text-center"
                      prop.children
                          [ Html.input
                                [ prop.type' "checkbox"
                                  prop.className "rounded w-4 h-4"
                                  prop.isChecked allSelected
                                  prop.onChange (fun (isChecked: bool) ->
                                      if isChecked then
                                          // 全選択
                                          products
                                          |> List.map (fun p -> p.Id)
                                          |> Set.ofList
                                          |> (fun ids -> dispatch (ProductsMsg(SetSelectedProducts ids)))
                                      else
                                          // 全解除
                                          dispatch (ProductsMsg(SetSelectedProducts Set.empty))) ] ] ]
                Html.th
                    [ prop.className "w-28 px-6 py-4 whitespace-nowrap text-center"
                      prop.text "アクション" ]
                Html.th
                    [ prop.className "w-20 px-6 py-4 whitespace-nowrap text-center"
                      prop.text "ID" ]
                Html.th
                    [ prop.className "w-1/4 px-6 py-4 whitespace-nowrap cursor-pointer hover:bg-gray-50"
                      prop.onClick (fun _ -> dispatch (ProductsMsg(ChangeSort "製品名")))
                      prop.children
                          [ Html.div
                                [ prop.className "flex items-center space-x-1"
                                  prop.children
                                      [ Html.span [ prop.text "製品名" ]
                                        if productsState.ActiveSort = Some "製品名" then
                                            Html.span
                                                [ prop.className "text-blue-500"
                                                  prop.children
                                                      [ if productsState.SortDirection = "asc" then
                                                            Html.i [ prop.className "fas fa-sort-up" ]
                                                        else
                                                            Html.i [ prop.className "fas fa-sort-down" ] ] ] ] ] ] ]
                Html.th
                    [ prop.className "w-1/6 px-6 py-4 whitespace-nowrap cursor-pointer hover:bg-gray-50"
                      prop.onClick (fun _ -> dispatch (ProductsMsg(ChangeSort "カテゴリ")))
                      prop.children
                          [ Html.div
                                [ prop.className "flex items-center space-x-1"
                                  prop.children
                                      [ Html.span [ prop.text "カテゴリ" ]
                                        if productsState.ActiveSort = Some "カテゴリ" then
                                            Html.span
                                                [ prop.className "text-blue-500"
                                                  prop.children
                                                      [ if productsState.SortDirection = "asc" then
                                                            Html.i [ prop.className "fas fa-sort-up" ]
                                                        else
                                                            Html.i [ prop.className "fas fa-sort-down" ] ] ] ] ] ] ]
                Html.th
                    [ prop.className "w-32 px-6 py-4 whitespace-nowrap cursor-pointer hover:bg-gray-50"
                      prop.onClick (fun _ -> dispatch (ProductsMsg(ChangeSort "価格")))
                      prop.children
                          [ Html.div
                                [ prop.className "flex items-center space-x-1"
                                  prop.children
                                      [ Html.span [ prop.text "価格" ]
                                        if productsState.ActiveSort = Some "価格" then
                                            Html.span
                                                [ prop.className "text-blue-500"
                                                  prop.children
                                                      [ if productsState.SortDirection = "asc" then
                                                            Html.i [ prop.className "fas fa-sort-up" ]
                                                        else
                                                            Html.i [ prop.className "fas fa-sort-down" ] ] ] ] ] ] ]
                Html.th
                    [ prop.className "w-24 px-6 py-4 whitespace-nowrap cursor-pointer hover:bg-gray-50"
                      prop.onClick (fun _ -> dispatch (ProductsMsg(ChangeSort "在庫")))
                      prop.children
                          [ Html.div
                                [ prop.className "flex items-center space-x-1"
                                  prop.children
                                      [ Html.span [ prop.text "在庫" ]
                                        if productsState.ActiveSort = Some "在庫" then
                                            Html.span
                                                [ prop.className "text-blue-500"
                                                  prop.children
                                                      [ if productsState.SortDirection = "asc" then
                                                            Html.i [ prop.className "fas fa-sort-up" ]
                                                        else
                                                            Html.i [ prop.className "fas fa-sort-down" ] ] ] ] ] ] ] ]
              products
              (fun product ->
                  let isSelected = Set.contains product.Id selectedIds

                  [
                    // 行選択チェックボックス
                    Html.td
                        [ prop.className "w-16 px-6 py-4 whitespace-nowrap text-center"
                          prop.children
                              [ Html.input
                                    [ prop.type' "checkbox"
                                      prop.className "rounded w-4 h-4"
                                      prop.isChecked isSelected
                                      prop.onChange (fun (isChecked: bool) ->
                                          dispatch (ProductsMsg(ToggleProductSelection product.Id))) ] ] ]

                    // アクションボタン
                    Html.td
                        [ prop.className "w-28 px-6 py-4 whitespace-nowrap text-center"
                          prop.children
                              [ Table.tableRowActions
                                    [ "詳細",
                                      (fun () -> dispatch (ProductsMsg(ViewProductDetails product.Id))),
                                      "bg-blue-500 text-white" ] ] ]

                    // 製品ID
                    Html.td
                        [ prop.className "w-20 px-6 py-4 whitespace-nowrap text-center"
                          prop.text (string product.Id) ]

                    // 製品名
                    Html.td [ prop.className "w-1/4 px-6 py-4 whitespace-nowrap"; prop.text product.Name ]

                    // カテゴリ
                    Html.td
                        [ prop.className "w-1/6 px-6 py-4 whitespace-nowrap"
                          prop.text (defaultArg product.Category "-") ]

                    // 価格
                    Html.td
                        [ prop.className "w-32 px-6 py-4 whitespace-nowrap text-right"
                          prop.text (sprintf "¥%.2f" product.Price) ]

                    // 在庫
                    Html.td
                        [ prop.className (
                              if product.Stock = 0 then
                                  "w-24 px-6 py-4 whitespace-nowrap text-right font-medium text-red-600"
                              else
                                  "w-24 px-6 py-4 whitespace-nowrap text-right"
                          )
                          prop.text (string product.Stock) ] ]) ]

// ページングと行選択機能付き製品一覧の表示
let renderProducts (model: Model) (dispatch: Msg -> unit) =
    match model.ApiData.ProductData.Products with
    | NotStarted ->
        emptyState
            "製品データを読み込むには下のボタンをクリックしてください"
            (Some "データを読み込む")
            (Some(fun () -> dispatch (ApiMsg(ProductApi FetchProducts))))
    | Loading -> loadingState "製品データを読み込み中..."
    | Failed error ->
        errorState
            (sprintf "エラー: %s" (Client.getErrorMessage error))
            (Some(fun () -> dispatch (ApiMsg(ProductApi FetchProducts))))
    | Success products ->
        if List.isEmpty products then
            emptyState "製品データがありません" (Some "データを読み込む") (Some(fun () -> dispatch (ApiMsg(ProductApi FetchProducts))))
        else
            let pageInfo = model.ProductsState.PageInfo

            Html.div
                [ prop.className "p-5"
                  prop.children
                      [ Table.pageHeader "製品一覧" []

                        // 検索・フィルター
                        Table.tableFilterControl
                            [ "製品名"; "カテゴリ"; "価格"; "在庫" ]
                            model.ProductsState.ActiveSort
                            model.ProductsState.SortDirection
                            (fun column -> dispatch (ProductsMsg(ChangeSort column)))
                            model.ProductsState.SearchValue
                            (fun value -> dispatch (ProductsMsg(ChangeSearch value)))
                            (fun () -> dispatch (ProductsMsg(ClearSort)))

                        // 製品テーブル
                        let filteredProducts =
                            products
                            |> List.filter (fun product ->
                                let searchValue = model.ProductsState.SearchValue.ToLower()

                                if System.String.IsNullOrEmpty(searchValue) then
                                    true
                                else
                                    product.Name.ToLower().Contains(searchValue)
                                    || (defaultArg product.Category "").ToLower().Contains(searchValue)
                                    || (sprintf "%.2f" product.Price).Contains(searchValue)
                                    || (string product.Stock).Contains(searchValue))
                            |> List.sortWith (fun a b ->
                                match model.ProductsState.ActiveSort with
                                | Some "製品名" ->
                                    if model.ProductsState.SortDirection = "asc" then
                                        compare a.Name b.Name
                                    else
                                        compare b.Name a.Name
                                | Some "カテゴリ" ->
                                    let categoryA = defaultArg a.Category ""
                                    let categoryB = defaultArg b.Category ""

                                    if model.ProductsState.SortDirection = "asc" then
                                        compare categoryA categoryB
                                    else
                                        compare categoryB categoryA
                                | Some "価格" ->
                                    if model.ProductsState.SortDirection = "asc" then
                                        compare a.Price b.Price
                                    else
                                        compare b.Price a.Price
                                | Some "在庫" ->
                                    if model.ProductsState.SortDirection = "asc" then
                                        compare a.Stock b.Stock
                                    else
                                        compare b.Stock a.Stock
                                | _ -> 0)

                        let pagedProducts =
                            Table.paginateList pageInfo.CurrentPage pageInfo.PageSize filteredProducts

                        renderProductsTable pagedProducts model.ProductsState dispatch

                        // フッター部分
                        Html.div
                            [ prop.className "flex justify-between items-center mt-4"
                              prop.children
                                  [
                                    // ページサイズと表示件数
                                    Table.dataCountDisplay
                                        pageInfo.PageSize
                                        pageInfo.CurrentPage
                                        pageInfo.TotalItems
                                        (fun size -> dispatch (ProductsMsg(ChangePageSize size)))

                                    // ページネーション
                                    Table.paginationControl pageInfo.CurrentPage pageInfo.TotalPages (fun page ->
                                        dispatch (ProductsMsg(ChangePage page))) ] ] ] ]

// ProductDetail.fsモジュールを使用して詳細表示
let renderProductDetail (model: Model) (dispatch: Msg -> unit) =
    ProductDetail.renderProductDetail model dispatch

// 製品表示全体（リストと詳細）- レスポンシブ対応
let renderProductsWithDetail (model: Model) (dispatch: Msg -> unit) =
    // URLルートに基づいて詳細表示の有無を判断
    let isDetailView =
        match model.CurrentRoute with
        | Route.ProductDetail _ -> true
        | _ -> false

    // 詳細データの状態と表示内容
    let detailContent =
        match model.CurrentRoute with
        | Route.ProductDetail _ ->
            match model.ApiData.ProductData.SelectedProductDetail with
            | None -> None
            | Some state ->
                match state with
                | NotStarted -> None
                | Loading -> Some(loadingState "詳細情報を読み込み中...")
                | Failed error ->
                    dispatch (
                        NotificationMsg(
                            Add(
                                Notifications.error "詳細情報の取得に失敗しました"
                                |> Notifications.withDetails (Client.getErrorMessage error)
                                |> Notifications.fromSource "ProductDetail"
                            )
                        )
                    )

                    Some(errorState "詳細情報の取得に失敗しました" None)
                | Success _ -> Some(renderProductDetail model dispatch)
        | _ -> None

    // 製品一覧の取得
    let productsListView = renderProducts model dispatch

    // レスポンシブレイアウトを適用（詳細表示がある場合のみ）
    if isDetailView then
        // 詳細表示がある場合はresponsiveMasterDetailを使用
        responsiveMasterDetail productsListView detailContent isDetailView
    else
        // 詳細表示がない場合は製品一覧のみを表示
        productsListView
