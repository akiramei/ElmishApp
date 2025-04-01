// ProductsView.fs
module App.ProductsView

open Feliz
open App.Types
open App.Shared
open App.ApiClient

// 製品一覧の表示
let renderProducts (model: Model) (dispatch: Msg -> unit) =
    match model.ApiData.Products with
    | NotStarted ->
        Html.div
            [ prop.className "p-5 text-center"
              prop.children
                  [ Html.h1 [ prop.className "text-2xl font-bold mb-4"; prop.text "製品一覧" ]
                    Html.button
                        [ prop.className "px-4 py-2 bg-blue-500 text-white rounded"
                          prop.text "データを読み込む"
                          prop.onClick (fun _ -> dispatch (ApiMsg FetchProducts)) ] ] ]

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
                          prop.text (sprintf "エラー: %s" (getErrorMessage error)) ]
                    Html.button
                        [ prop.className "px-4 py-2 bg-blue-500 text-white rounded"
                          prop.text "再読み込み"
                          prop.onClick (fun _ -> dispatch (ApiMsg FetchProducts)) ] ] ]

    | Success products ->
        Html.div
            [ prop.className "p-5"
              prop.children
                  [ Html.h1 [ prop.className "text-2xl font-bold mb-4"; prop.text "製品一覧" ]

                    if List.isEmpty products then
                        Html.div [ prop.className "text-gray-600 italic text-center"; prop.text "製品がありません" ]
                    else
                        Html.table
                            [ prop.className "min-w-full divide-y divide-gray-200"
                              prop.children
                                  [ Html.thead
                                        [ prop.className "bg-gray-50"
                                          prop.children
                                              [ Html.tr
                                                    [ Html.th
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
                                                            prop.text "在庫" ] ] ] ]
                                    Html.tbody
                                        [ prop.className "bg-white divide-y divide-gray-200"
                                          prop.children
                                              [ for product in products ->
                                                    Html.tr
                                                        [ Html.td
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
                                                                prop.text (string product.Stock) ] ] ] ] ] ] ] ]
