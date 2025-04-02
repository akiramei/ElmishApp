// ProductDetail.fs - 製品詳細表示コンポーネント
module App.ProductDetail

open Feliz
open App.Types
open App.Shared
open App.Notifications

// 製品詳細パネルをレンダリング
let renderProductDetail (model: Model) (dispatch: Msg -> unit) =
    match model.ApiData.ProductData.SelectedProduct with
    | None ->
        // 詳細が選択されていない場合
        Html.div
            [ prop.className "h-full flex items-center justify-center bg-gray-50 rounded-lg"
              prop.children [ Html.p [ prop.className "text-gray-500"; prop.text "製品を選択してください" ] ] ]

    | Some(NotStarted) ->
        // まだ詳細取得が開始されていない
        Html.div
            [ prop.className "p-4 text-center"
              prop.children [ Html.p [ prop.className "text-gray-500"; prop.text "製品詳細を取得します..." ] ] ]

    | Some(Loading) ->
        // 読み込み中
        Html.div
            [ prop.className "p-4 text-center"
              prop.children
                  [ Html.p [ prop.className "text-gray-500"; prop.text "詳細を読み込み中..." ]
                    // ローディングインジケーター
                    Html.div
                        [ prop.className "mt-4 flex justify-center"
                          prop.children
                              [ Html.div
                                    [ prop.className
                                          "animate-spin h-8 w-8 border-4 border-blue-500 rounded-full border-t-transparent" ] ] ] ] ]

    | Some(Failed error) ->
        // エラー表示
        Html.div
            [ prop.className "p-4 bg-red-50 rounded-lg"
              prop.children
                  [ Html.h3
                        [ prop.className "text-lg font-bold text-red-700 mb-2"
                          prop.text "詳細の取得に失敗しました" ]
                    Html.p [ prop.className "text-red-600"; prop.text (ApiClient.getErrorMessage error) ]
                    Html.button
                        [ prop.className "mt-4 px-4 py-2 bg-blue-500 text-white rounded"
                          prop.text "製品一覧に戻る"
                          prop.onClick (fun _ -> dispatch (ProductsMsg CloseProductDetails)) ] ] ]

    | Some(Success product) ->
        // 詳細表示
        Html.div
            [ prop.className "h-full flex flex-col"
              prop.children
                  [
                    // ヘッダー部分
                    Html.div
                        [ prop.className "flex justify-between items-center border-b pb-4 mb-4"
                          prop.children
                              [ Html.h2 [ prop.className "text-xl font-bold"; prop.text "製品詳細" ]
                                Html.button
                                    [ prop.className "text-gray-500 hover:text-gray-700"
                                      prop.onClick (fun _ -> dispatch (ProductsMsg CloseProductDetails))
                                      prop.children [ Html.span [ prop.className "text-xl"; prop.text "×" ] ] ] ] ]

                    // 製品情報
                    Html.div
                        [ prop.className "flex-grow overflow-auto p-2"
                          prop.children
                              [
                                // 製品名
                                Html.h3 [ prop.className "text-2xl font-bold mb-4"; prop.text product.Name ]

                                // 詳細情報一覧
                                Html.dl
                                    [ prop.className "grid grid-cols-3 gap-2"
                                      prop.children
                                          [
                                            // ID
                                            Html.dt [ prop.className "text-gray-500 col-span-1"; prop.text "製品ID" ]
                                            Html.dd
                                                [ prop.className "col-span-2 font-medium"
                                                  prop.text (string product.Id) ]

                                            // 価格
                                            Html.dt [ prop.className "text-gray-500 col-span-1"; prop.text "価格" ]
                                            Html.dd
                                                [ prop.className "col-span-2 font-medium"
                                                  prop.text (sprintf "¥%.0f" product.Price) ]

                                            // カテゴリ
                                            Html.dt [ prop.className "text-gray-500 col-span-1"; prop.text "カテゴリ" ]
                                            Html.dd
                                                [ prop.className "col-span-2"
                                                  prop.text (defaultArg product.Category "-") ]

                                            // 在庫
                                            Html.dt [ prop.className "text-gray-500 col-span-1"; prop.text "在庫数" ]
                                            Html.dd [ prop.className "col-span-2"; prop.text (string product.Stock) ]

                                            // SKU
                                            Html.dt [ prop.className "text-gray-500 col-span-1"; prop.text "SKU" ]
                                            Html.dd [ prop.className "col-span-2 font-mono"; prop.text product.SKU ]

                                            // 状態
                                            Html.dt [ prop.className "text-gray-500 col-span-1"; prop.text "状態" ]
                                            Html.dd
                                                [ prop.className (
                                                      if product.IsActive then
                                                          "col-span-2 text-green-600"
                                                      else
                                                          "col-span-2 text-red-600"
                                                  )
                                                  prop.text (if product.IsActive then "有効" else "無効") ]

                                            // 作成日時
                                            Html.dt [ prop.className "text-gray-500 col-span-1"; prop.text "作成日時" ]
                                            Html.dd [ prop.className "col-span-2"; prop.text product.CreatedAt ]

                                            // 最終更新日時
                                            match product.UpdatedAt with
                                            | Some updateDate ->
                                                Html.dt [ prop.className "text-gray-500 col-span-1"; prop.text "最終更新" ]
                                                Html.dd [ prop.className "col-span-2"; prop.text updateDate ]
                                            | None -> Html.none

                                            // 説明
                                            Html.dt [ prop.className "text-gray-500 col-span-3 mt-4"; prop.text "説明" ]
                                            Html.dd
                                                [ prop.className "col-span-3 mt-2 bg-gray-50 p-3 rounded"
                                                  prop.text (defaultArg product.Description "説明はありません") ] ] ] ] ]

                    // アクションボタン
                    Html.div
                        [ prop.className "border-t pt-4 mt-auto flex gap-2 justify-end"
                          prop.children
                              [ Html.button
                                    [ prop.className "px-4 py-2 border rounded text-gray-700 hover:bg-gray-100"
                                      prop.text "編集"
                                      prop.onClick (fun _ ->
                                          // 将来的に編集機能を実装する場合のプレースホルダー
                                          dispatch (NotificationMsg(Add(Notifications.info "編集機能は現在開発中です")))) ]
                                Html.button
                                    [ prop.className "px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600"
                                      prop.text "戻る"
                                      prop.onClick (fun _ -> dispatch (ProductsMsg CloseProductDetails)) ] ] ] ] ]
