// src/View/ProductDetail/ProductDetail.fs
module App.ProductDetail

open Feliz
open App.Types
open App.Shared
open App.Infrastructure.Api
open App.View.Components.Tabs
open App.View.Components.AdditionalFields

// 基本情報タブの内容を表示するコンポーネント
[<ReactComponent>]
let private RenderBasicInfoTab (product: ProductDto) =
    Html.div
        [ prop.className "px-4"
          prop.children
              [
                // 基本情報一覧
                Html.dl
                    [ prop.className "grid grid-cols-3 gap-2"
                      prop.children
                          [
                            // ID
                            Html.dt [ prop.className "text-gray-500 col-span-1"; prop.text "製品ID" ]
                            Html.dd [ prop.className "col-span-2 font-medium"; prop.text (string product.Id) ]

                            Html.dt [ prop.className "text-gray-500 col-span-1"; prop.text "製品コード" ]
                            Html.dd [ prop.className "col-span-2 font-medium"; prop.text product.Code ]

                            Html.dt [ prop.className "text-gray-500 col-span-1"; prop.text "製品名 (コードに基づく)" ]
                            Html.dd [ prop.className "col-span-2"; prop.text product.Name ]

                            // 価格
                            Html.dt [ prop.className "text-gray-500 col-span-1"; prop.text "価格" ]
                            Html.dd
                                [ prop.className "col-span-2 font-medium"
                                  prop.text (sprintf "¥%.0f" product.Price) ]

                            // カテゴリ
                            Html.dt [ prop.className "text-gray-500 col-span-1"; prop.text "カテゴリ" ]
                            Html.dd [ prop.className "col-span-2"; prop.text (defaultArg product.Category "-") ]

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

// 製品詳細パネルをレンダリングするメインコンポーネント
[<ReactComponent>]
let RenderProductDetail (model: Model) (dispatch: Msg -> unit) =
    // 編集モード状態
    let editMode, setEditMode = React.useState (false)

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
                    Html.p [ prop.className "text-red-600"; prop.text (Client.getErrorMessage error) ]
                    Html.button
                        [ prop.className "mt-4 px-4 py-2 bg-blue-500 text-white rounded"
                          prop.text "製品一覧に戻る"
                          prop.onClick (fun _ -> dispatch (ProductsMsg CloseProductDetails)) ] ] ]

    | Some(Success product), detailedProduct ->
        if editMode then
            // 編集モードの場合は、ProductEditForm コンポーネントを表示
            match detailedProduct with
            | Some(Success detailData) ->
                // ProductEditFormをimportして使用
                ProductEditForm.RenderProductEditForm detailData dispatch (fun () -> setEditMode false)
            | _ ->
                // 詳細データがない場合、基本データから編集フォームを生成
                let basicDetailData =
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
                      UpdatedAt = product.UpdatedAt
                      // 追加フィールドは空にする
                      Public01 = None
                      Public02 = None
                      Public03 = None
                      Public04 = None
                      Public05 = None
                      Public06 = None
                      Public07 = None
                      Public08 = None
                      Public09 = None
                      Public10 = None }

                ProductEditForm.RenderProductEditForm basicDetailData dispatch (fun () -> setEditMode false)
        else
            // 詳細表示モード
            Html.div
                [ prop.className "h-full flex flex-col"
                  prop.children
                      [
                        // ヘッダー部分
                        Html.div
                            [ prop.className
                                  "flex justify-between items-center border-b pb-4 mb-4 bg-gray-50 px-6 py-4 rounded-t-lg"
                              prop.children
                                  [ Html.div
                                        [ prop.className "flex items-center space-x-3"
                                          prop.children
                                              [ Html.h2
                                                    [ prop.className "text-xl font-bold text-gray-800"
                                                      prop.text "製品詳細" ]
                                                Html.span
                                                    [ prop.className
                                                          "px-2 py-1 text-sm font-medium rounded-full bg-blue-100 text-blue-800"
                                                      prop.text (sprintf "ID: %d" product.Id) ] ] ]
                                    Html.button
                                        [ prop.className
                                              "p-2 hover:bg-gray-200 rounded-full transition-colors duration-200"
                                          prop.onClick (fun _ -> dispatch (ProductsMsg CloseProductDetails))
                                          prop.children
                                              [ Svg.svg
                                                    [ svg.className "w-5 h-5 text-gray-500"
                                                      svg.children
                                                          [ Svg.path
                                                                [ svg.d "M6 18L18 6M6 6l12 12"
                                                                  svg.stroke "currentColor"
                                                                  svg.strokeWidth 2.0
                                                                  svg.strokeLineCap "round"
                                                                  svg.strokeLineJoin "round" ] ] ] ] ] ] ]

                        // 製品名を常に表示
                        Html.h3 [ prop.className "text-2xl font-bold px-6 mb-4"; prop.text product.Name ]

                        // タブコンテナを使用して内容を表示
                        TabContainer BasicInfo (fun activeTab ->
                            match activeTab with
                            | BasicInfo ->
                                // 基本情報タブの内容
                                RenderBasicInfoTab product

                            | ExtraInfo ->
                                // 追加情報タブの内容
                                match detailedProduct with
                                | Some(Success detailData) -> RenderAdditionalFieldsReadOnly detailData

                                | Some(Loading) ->
                                    Html.div
                                        [ prop.className "p-4 text-center"
                                          prop.children
                                              [ Html.p [ prop.className "text-gray-500"; prop.text "追加情報を読み込み中..." ]
                                                Html.div
                                                    [ prop.className "mt-4 flex justify-center"
                                                      prop.children
                                                          [ Html.div
                                                                [ prop.className
                                                                      "animate-spin h-6 w-6 border-3 border-blue-500 rounded-full border-t-transparent" ] ] ] ] ]

                                | Some(Failed error) ->
                                    Html.div
                                        [ prop.className "p-4 bg-red-50 rounded-lg text-center"
                                          prop.children
                                              [ Html.p
                                                    [ prop.className "text-red-600"
                                                      prop.text ("追加情報の取得に失敗しました: " + Client.getErrorMessage error) ] ] ]

                                | _ ->
                                    Html.div
                                        [ prop.className "p-6 text-center text-gray-500"
                                          prop.children [ Html.p [ prop.text "追加情報は利用できません" ] ] ])

                        // アクションボタン
                        Html.div
                            [ prop.className "border-t pt-4 mt-auto flex gap-2 justify-end px-6"
                              prop.children
                                  [ Html.button
                                        [ prop.className
                                              "px-4 py-2 border rounded bg-yellow-500 text-white rounded hover:bg-yellow-600"
                                          prop.text "編集"
                                          prop.onClick (fun _ -> setEditMode true) ]
                                    Html.button
                                        [ prop.className "px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600"
                                          prop.text "戻る"
                                          prop.onClick (fun _ -> dispatch (ProductsMsg CloseProductDetails)) ] ] ] ] ]
