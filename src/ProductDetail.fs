// ProductDetail.fs - 製品詳細表示コンポーネント (詳細情報対応)
module App.ProductDetail

open Feliz
open App.Types
open App.Shared
open App.Notifications

// 追加フィールドのグループを表示するヘルパー関数
let renderAdditionalFields (productDetail: ProductDetailDto) =
    let hasAdditionalFields =
        [
            productDetail.Public01
            productDetail.Public02
            productDetail.Public03
            productDetail.Public04
            productDetail.Public05
            productDetail.Public06
            productDetail.Public07
            productDetail.Public08
            productDetail.Public09
            productDetail.Public10
        ]
        |> List.exists Option.isSome

    if hasAdditionalFields then
        Html.div
            [ prop.className "mt-6 border-t pt-4"
              prop.children
                  [ Html.h3 [ prop.className "text-lg font-semibold mb-3"; prop.text "追加情報" ]
                    Html.div
                        [ prop.className "grid grid-cols-2 md:grid-cols-3 gap-3"
                          prop.children
                              [
                                // Public01-10フィールドの表示
                                if Option.isSome productDetail.Public01 then
                                    Html.div
                                        [ prop.className "border rounded p-2"
                                          prop.children
                                              [ Html.div [ prop.className "text-sm text-gray-500"; prop.text "追加情報 1" ]
                                                Html.div [ prop.className ""; prop.text (Option.defaultValue "-" productDetail.Public01) ] ] ]

                                if Option.isSome productDetail.Public02 then
                                    Html.div
                                        [ prop.className "border rounded p-2"
                                          prop.children
                                              [ Html.div [ prop.className "text-sm text-gray-500"; prop.text "追加情報 2" ]
                                                Html.div [ prop.className ""; prop.text (Option.defaultValue "-" productDetail.Public02) ] ] ]

                                if Option.isSome productDetail.Public03 then
                                    Html.div
                                        [ prop.className "border rounded p-2"
                                          prop.children
                                              [ Html.div [ prop.className "text-sm text-gray-500"; prop.text "追加情報 3" ]
                                                Html.div [ prop.className ""; prop.text (Option.defaultValue "-" productDetail.Public03) ] ] ]

                                if Option.isSome productDetail.Public04 then
                                    Html.div
                                        [ prop.className "border rounded p-2"
                                          prop.children
                                              [ Html.div [ prop.className "text-sm text-gray-500"; prop.text "追加情報 4" ]
                                                Html.div [ prop.className ""; prop.text (Option.defaultValue "-" productDetail.Public04) ] ] ]

                                if Option.isSome productDetail.Public05 then
                                    Html.div
                                        [ prop.className "border rounded p-2"
                                          prop.children
                                              [ Html.div [ prop.className "text-sm text-gray-500"; prop.text "追加情報 5" ]
                                                Html.div [ prop.className ""; prop.text (Option.defaultValue "-" productDetail.Public05) ] ] ]

                                if Option.isSome productDetail.Public06 then
                                    Html.div
                                        [ prop.className "border rounded p-2"
                                          prop.children
                                              [ Html.div [ prop.className "text-sm text-gray-500"; prop.text "追加情報 6" ]
                                                Html.div [ prop.className ""; prop.text (Option.defaultValue "-" productDetail.Public06) ] ] ]

                                if Option.isSome productDetail.Public07 then
                                    Html.div
                                        [ prop.className "border rounded p-2"
                                          prop.children
                                              [ Html.div [ prop.className "text-sm text-gray-500"; prop.text "追加情報 7" ]
                                                Html.div [ prop.className ""; prop.text (Option.defaultValue "-" productDetail.Public07) ] ] ]

                                if Option.isSome productDetail.Public08 then
                                    Html.div
                                        [ prop.className "border rounded p-2"
                                          prop.children
                                              [ Html.div [ prop.className "text-sm text-gray-500"; prop.text "追加情報 8" ]
                                                Html.div [ prop.className ""; prop.text (Option.defaultValue "-" productDetail.Public08) ] ] ]

                                if Option.isSome productDetail.Public09 then
                                    Html.div
                                        [ prop.className "border rounded p-2"
                                          prop.children
                                              [ Html.div [ prop.className "text-sm text-gray-500"; prop.text "追加情報 9" ]
                                                Html.div [ prop.className ""; prop.text (Option.defaultValue "-" productDetail.Public09) ] ] ]

                                if Option.isSome productDetail.Public10 then
                                    Html.div
                                        [ prop.className "border rounded p-2"
                                          prop.children
                                              [ Html.div [ prop.className "text-sm text-gray-500"; prop.text "追加情報 10" ]
                                                Html.div [ prop.className ""; prop.text (Option.defaultValue "-" productDetail.Public10) ] ] ] ] ] ] ]
    else
        Html.none

// 製品詳細パネルをレンダリング - 詳細情報取得機能に対応
let renderProductDetail (model: Model) (dispatch: Msg -> unit) =
    // 基本情報と詳細情報の両方の状態を確認
    let basicProduct = model.ApiData.ProductData.SelectedProduct
    let detailedProduct = model.ApiData.ProductData.SelectedProductDetail

    match basicProduct, detailedProduct with
    | None, _ ->
        // 詳細が選択されていない場合
        Html.div
            [ prop.className "h-full flex items-center justify-center bg-gray-50 rounded-lg"
              prop.children [ Html.p [ prop.className "text-gray-500"; prop.text "製品を選択してください" ] ] ]

    | Some(NotStarted), _ ->
        // まだ詳細取得が開始されていない
        Html.div
            [ prop.className "p-4 text-center"
              prop.children [ Html.p [ prop.className "text-gray-500"; prop.text "製品詳細を取得します..." ] ] ]

    | Some(Loading), _ ->
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

    | Some(Failed error), _ ->
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

    | Some(Success product), detailedProduct ->
        // 詳細表示 - 基本情報は取得できている
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

                    // 製品情報 - スクロール可能なエリア
                    Html.div
                        [ prop.className "flex-grow overflow-auto p-2"
                          prop.children
                              [
                                // 製品名
                                Html.h3 [ prop.className "text-2xl font-bold mb-4"; prop.text product.Name ]

                                // 詳細情報の読み込み状態表示
                                match detailedProduct with
                                | None ->
                                    Html.div
                                        [ prop.className "mb-4 p-2 bg-blue-50 rounded text-blue-700 text-sm"
                                          prop.text "詳細情報を読み込んでいます..." ]
                                | Some Loading ->
                                    Html.div
                                        [ prop.className "mb-4 p-2 bg-blue-50 rounded text-blue-700 text-sm"
                                          prop.text "詳細情報を読み込み中..." ]
                                | Some (Failed error) ->
                                    Html.div
                                        [ prop.className "mb-4 p-2 bg-red-50 rounded text-red-700 text-sm"
                                          prop.text ("詳細情報の取得に失敗しました: " + ApiClient.getErrorMessage error) ]
                                | _ -> Html.none

                                // 基本情報一覧
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
                                                  prop.text (defaultArg product.Description "説明はありません") ] ] ]

                                // 詳細情報がある場合に追加フィールドを表示
                                match detailedProduct with
                                | Some (Success detailData) ->
                                    renderAdditionalFields detailData
                                | _ -> Html.none ] ]

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