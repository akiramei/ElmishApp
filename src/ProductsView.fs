// ProductsView.fs - Updated for product deletion
module App.ProductsView

open Feliz
open App.Types
open App.Shared
open App.Infrastructure.Api

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
                    Html.button
                        [ prop.className "px-3 py-1 bg-red-500 text-white rounded hover:bg-red-600"
                          prop.text "選択した製品を削除"
                          prop.onClick (fun _ ->
                              if (Set.count selectedIds) > 0 then
                                  if Browser.Dom.window.confirm ("選択した製品を削除してもよろしいですか？") then
                                      dispatch (ProductsMsg DeleteSelectedProducts)) ] ] ]

// ページングコントロールのレンダリング
let renderPagination (pageInfo: PageInfo) (dispatch: Msg -> unit) =
    Html.div
        [ prop.className "flex items-center justify-between py-3"
          prop.children
              [
                // ページサイズ選択
                Html.div
                    [ prop.className "flex items-center"
                      prop.children
                          [ Html.span [ prop.className "mr-2"; prop.text "表示件数:" ]
                            Html.select
                                [ prop.className "border rounded px-2 py-1"
                                  prop.value (string pageInfo.PageSize)
                                  prop.onChange (fun (s: string) -> int s |> ChangePageSize |> ProductsMsg |> dispatch)
                                  prop.children
                                      [ for size in [ 10; 25; 50 ] ->
                                            Html.option [ prop.value (string size); prop.text (string size) ] ] ] ] ]

                // ページナビゲーション
                Html.div
                    [ prop.className "flex items-center space-x-2"
                      prop.children
                          [
                            // 前ページボタン
                            Html.button
                                [ prop.className "px-3 py-1 border rounded disabled:opacity-50"
                                  prop.disabled (pageInfo.CurrentPage <= 1)
                                  prop.onClick (fun _ ->
                                      if pageInfo.CurrentPage > 1 then
                                          dispatch (ProductsMsg(ChangePage(pageInfo.CurrentPage - 1))))
                                  prop.text "前へ" ]

                            // ページ情報
                            Html.span
                                [ prop.className "mx-2"
                                  prop.text (sprintf "%d / %d ページ" pageInfo.CurrentPage pageInfo.TotalPages) ]

                            // 次ページボタン
                            Html.button
                                [ prop.className "px-3 py-1 border rounded disabled:opacity-50"
                                  prop.disabled (pageInfo.CurrentPage >= pageInfo.TotalPages)
                                  prop.onClick (fun _ ->
                                      if pageInfo.CurrentPage < pageInfo.TotalPages then
                                          dispatch (ProductsMsg(ChangePage(pageInfo.CurrentPage + 1))))
                                  prop.text "次へ" ] ] ] ] ]

// 製品テーブルの拡張
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
          Html.table
              [ prop.className "min-w-full divide-y divide-gray-200"
                prop.children
                    [ Html.thead
                          [ prop.className "bg-gray-50"
                            prop.children
                                [ Html.tr
                                      [
                                        // 全選択チェックボックス
                                        Html.th
                                            [ prop.className "px-2 py-3 w-10"
                                              prop.children
                                                  [ Html.input
                                                        [ prop.type' "checkbox"
                                                          prop.className "rounded"
                                                          prop.isChecked allSelected
                                                          prop.onChange (fun isChecked ->
                                                              dispatch (ProductsMsg(ToggleAllProducts isChecked))) ] ] ]

                                        // 既存のヘッダー
                                        Html.th
                                            [ prop.className
                                                  "px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                                              prop.text "ID" ]
                                        Html.th
                                            [ prop.className
                                                  "px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                                              prop.text "製品名" ]
                                        Html.th
                                            [ prop.className
                                                  "px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                                              prop.text "カテゴリ" ]
                                        Html.th
                                            [ prop.className
                                                  "px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                                              prop.text "価格" ]
                                        Html.th
                                            [ prop.className
                                                  "px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                                              prop.text "在庫" ]

                                        // アクション列の追加
                                        Html.th
                                            [ prop.className
                                                  "px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                                              prop.text "アクション" ] ] ] ]
                      Html.tbody
                          [ prop.className "bg-white divide-y divide-gray-200"
                            prop.children
                                [ for product in products ->
                                      let isSelected = Set.contains product.Id selectedIds

                                      Html.tr
                                          [ prop.className (if isSelected then "bg-blue-50" else "")
                                            prop.children
                                                [
                                                  // 行選択チェックボックス
                                                  Html.td
                                                      [ prop.className "px-2 py-4 whitespace-nowrap"
                                                        prop.children
                                                            [ Html.input
                                                                  [ prop.type' "checkbox"
                                                                    prop.className "rounded"
                                                                    prop.isChecked isSelected
                                                                    prop.onChange (fun (isChecked: bool) ->
                                                                        dispatch (
                                                                            ProductsMsg(
                                                                                ToggleProductSelection product.Id
                                                                            )
                                                                        )) ] ] ]

                                                  // 既存の列
                                                  Html.td
                                                      [ prop.className "px-6 py-4 whitespace-nowrap"
                                                        prop.text (string product.Id) ]
                                                  Html.td
                                                      [ prop.className "px-6 py-4 whitespace-nowrap"
                                                        prop.text product.Name ]
                                                  Html.td
                                                      [ prop.className "px-6 py-4 whitespace-nowrap"
                                                        prop.text (defaultArg product.Category "-") ]
                                                  Html.td
                                                      [ prop.className "px-6 py-4 whitespace-nowrap"
                                                        prop.text (sprintf "¥%.0f" product.Price) ]
                                                  Html.td
                                                      [ prop.className "px-6 py-4 whitespace-nowrap"
                                                        prop.text (string product.Stock) ]

                                                  // アクションボタン - 詳細機能
                                                  Html.td
                                                      [ prop.className "px-6 py-4 whitespace-nowrap"
                                                        prop.children
                                                            [ Html.div
                                                                  [ prop.className "flex space-x-2"
                                                                    prop.children
                                                                        [
                                                                          // 詳細ボタン
                                                                          Html.button
                                                                              [ prop.className
                                                                                    "px-3 py-1 text-sm bg-blue-500 text-white rounded hover:bg-blue-600"
                                                                                prop.text "詳細"
                                                                                prop.onClick (fun _ ->
                                                                                    dispatch (
                                                                                        ProductsMsg(
                                                                                            ViewProductDetails
                                                                                                product.Id
                                                                                        )
                                                                                    )) ]

                                                                          ] ] ] ] ] ] ] ] ] ] ]

// ページングと行選択機能付き製品一覧の表示
let renderProducts (model: Model) (dispatch: Msg -> unit) =
    match model.ApiData.ProductData.Products with
    | NotStarted ->
        Html.div
            [ prop.className "p-5 text-center"
              prop.children
                  [ Html.h1 [ prop.className "text-2xl font-bold mb-4"; prop.text "製品一覧" ]
                    Html.button
                        [ prop.className "px-4 py-2 bg-blue-500 text-white rounded"
                          prop.text "データを読み込む"
                          prop.onClick (fun _ -> dispatch (ApiMsg(ProductApi FetchProducts))) ] ] ]

    | Loading ->
        Html.div
            [ prop.className "p-5 text-center"
              prop.children
                  [ Html.h1 [ prop.className "text-2xl font-bold mb-4"; prop.text "製品一覧" ]
                    Html.div [ prop.className "text-gray-600"; prop.text "読み込み中..." ] ] ]

    | Failed error ->
        Html.div
            [ prop.className "p-5 text-center"
              prop.children
                  [ Html.h1 [ prop.className "text-2xl font-bold mb-4"; prop.text "製品一覧" ]
                    Html.div
                        [ prop.className "text-red-600 mb-4"
                          prop.text (sprintf "エラー: %s" (Client.getErrorMessage error)) ]
                    Html.button
                        [ prop.className "px-4 py-2 bg-blue-500 text-white rounded"
                          prop.text "再読み込み"
                          prop.onClick (fun _ -> dispatch (ApiMsg(ProductApi FetchProducts))) ] ] ]

    | Success products ->
        let pageInfo = model.ProductsState.PageInfo

        // ページング適用したデータ
        let pagedProducts = App.UpdateProductApiState.simulatePagedData products pageInfo

        Html.div
            [ prop.className "p-5"
              prop.children
                  [ Html.h1 [ prop.className "text-2xl font-bold mb-4"; prop.text "製品一覧" ]

                    // ページングコントロール（上部）
                    renderPagination pageInfo dispatch

                    // 製品テーブル
                    renderProductsTable pagedProducts model.ProductsState dispatch

                    // ページングコントロール（下部）
                    renderPagination pageInfo dispatch ] ]

// ProductDetail.fsモジュールを使用して詳細表示
let renderProductDetail (model: Model) (dispatch: Msg -> unit) =
    App.ProductDetail.RenderProductDetail model dispatch
