// ProductDetail.fs - 製品詳細表示コンポーネント (詳細情報対応)
module App.ProductDetail

open Feliz
open App.Types
open App.Shared
open App.ProductDetailValidator

// 追加フィールドのグループを表示するヘルパー関数
let renderAdditionalFields (productDetail: ProductDetailDto) =
    let hasAdditionalFields =
        [ productDetail.Public01
          productDetail.Public02
          productDetail.Public03
          productDetail.Public04
          productDetail.Public05
          productDetail.Public06
          productDetail.Public07
          productDetail.Public08
          productDetail.Public09
          productDetail.Public10 ]
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
                                              [ Html.div [ prop.className "text-sm text-gray-500"; prop.text "追加情報 01" ]
                                                Html.div
                                                    [ prop.className ""
                                                      prop.text (Option.defaultValue "-" productDetail.Public01) ] ] ]

                                if Option.isSome productDetail.Public02 then
                                    Html.div
                                        [ prop.className "border rounded p-2"
                                          prop.children
                                              [ Html.div [ prop.className "text-sm text-gray-500"; prop.text "追加情報 02" ]
                                                Html.div
                                                    [ prop.className ""
                                                      prop.text (Option.defaultValue "-" productDetail.Public02) ] ] ]

                                if Option.isSome productDetail.Public03 then
                                    Html.div
                                        [ prop.className "border rounded p-2"
                                          prop.children
                                              [ Html.div [ prop.className "text-sm text-gray-500"; prop.text "追加情報 03" ]
                                                Html.div
                                                    [ prop.className ""
                                                      prop.text (Option.defaultValue "-" productDetail.Public03) ] ] ]

                                if Option.isSome productDetail.Public04 then
                                    Html.div
                                        [ prop.className "border rounded p-2"
                                          prop.children
                                              [ Html.div [ prop.className "text-sm text-gray-500"; prop.text "追加情報 04" ]
                                                Html.div
                                                    [ prop.className ""
                                                      prop.text (Option.defaultValue "-" productDetail.Public04) ] ] ]

                                if Option.isSome productDetail.Public05 then
                                    Html.div
                                        [ prop.className "border rounded p-2"
                                          prop.children
                                              [ Html.div [ prop.className "text-sm text-gray-500"; prop.text "追加情報 05" ]
                                                Html.div
                                                    [ prop.className ""
                                                      prop.text (Option.defaultValue "-" productDetail.Public05) ] ] ]

                                if Option.isSome productDetail.Public06 then
                                    Html.div
                                        [ prop.className "border rounded p-2"
                                          prop.children
                                              [ Html.div [ prop.className "text-sm text-gray-500"; prop.text "追加情報 06" ]
                                                Html.div
                                                    [ prop.className ""
                                                      prop.text (Option.defaultValue "-" productDetail.Public06) ] ] ]

                                if Option.isSome productDetail.Public07 then
                                    Html.div
                                        [ prop.className "border rounded p-2"
                                          prop.children
                                              [ Html.div [ prop.className "text-sm text-gray-500"; prop.text "追加情報 07" ]
                                                Html.div
                                                    [ prop.className ""
                                                      prop.text (Option.defaultValue "-" productDetail.Public07) ] ] ]

                                if Option.isSome productDetail.Public08 then
                                    Html.div
                                        [ prop.className "border rounded p-2"
                                          prop.children
                                              [ Html.div [ prop.className "text-sm text-gray-500"; prop.text "追加情報 08" ]
                                                Html.div
                                                    [ prop.className ""
                                                      prop.text (Option.defaultValue "-" productDetail.Public08) ] ] ]

                                if Option.isSome productDetail.Public09 then
                                    Html.div
                                        [ prop.className "border rounded p-2"
                                          prop.children
                                              [ Html.div [ prop.className "text-sm text-gray-500"; prop.text "追加情報 09" ]
                                                Html.div
                                                    [ prop.className ""
                                                      prop.text (Option.defaultValue "-" productDetail.Public09) ] ] ]

                                if Option.isSome productDetail.Public10 then
                                    Html.div
                                        [ prop.className "border rounded p-2"
                                          prop.children
                                              [ Html.div [ prop.className "text-sm text-gray-500"; prop.text "追加情報 10" ]
                                                Html.div
                                                    [ prop.className ""
                                                      prop.text (Option.defaultValue "-" productDetail.Public10) ] ] ] ] ] ] ]
    else
        Html.none

// 編集フォーム用の入力フィールドコンポーネント
let renderTextField
    (label: string)
    (fieldId: string)
    (value: string)
    (hasError: bool)
    (errorMessage: string option)
    (onChange: string -> unit)
    =
    Html.div
        [ prop.className "form-group mb-4"
          prop.children
              [ Html.label
                    [ prop.htmlFor fieldId
                      prop.className "block text-sm font-medium text-gray-700 mb-1"
                      prop.text label ]
                Html.input
                    [ prop.id fieldId
                      prop.className (
                          "w-full p-2 border rounded "
                          + if hasError then "border-red-500" else "border-gray-300"
                      )
                      prop.value value
                      prop.onChange onChange ]
                if hasError && errorMessage.IsSome then
                    Html.div [ prop.className "text-red-500 text-sm mt-1"; prop.text errorMessage.Value ] ] ]

// テキストエリア用コンポーネント
let renderTextareaField
    (label: string)
    (fieldId: string)
    (value: string)
    (hasError: bool)
    (errorMessage: string option)
    (onChange: string -> unit)
    =
    Html.div
        [ prop.className "form-group mb-4"
          prop.children
              [ Html.label
                    [ prop.htmlFor fieldId
                      prop.className "block text-sm font-medium text-gray-700 mb-1"
                      prop.text label ]
                Html.textarea
                    [ prop.id fieldId
                      prop.className (
                          "w-full p-2 border rounded "
                          + if hasError then "border-red-500" else "border-gray-300"
                      )
                      prop.value value
                      prop.onChange onChange
                      prop.rows 3 ]
                if hasError && errorMessage.IsSome then
                    Html.div [ prop.className "text-red-500 text-sm mt-1"; prop.text errorMessage.Value ] ] ]

// 数値入力用コンポーネント
let renderNumberField
    (label: string)
    (fieldId: string)
    (value: float)
    (hasError: bool)
    (errorMessage: string option)
    (onChange: string -> unit)
    (min: float option)
    (step: float option)
    =
    Html.div
        [ prop.className "form-group mb-4"
          prop.children
              [ Html.label
                    [ prop.htmlFor fieldId
                      prop.className "block text-sm font-medium text-gray-700 mb-1"
                      prop.text label ]
                Html.input
                    [ prop.id fieldId
                      prop.type' "number"
                      if min.IsSome then
                          prop.min min.Value
                      if step.IsSome then
                          prop.step step.Value
                      prop.className (
                          "w-full p-2 border rounded "
                          + if hasError then "border-red-500" else "border-gray-300"
                      )
                      prop.value (string value)
                      prop.onChange onChange ]
                if hasError && errorMessage.IsSome then
                    Html.div [ prop.className "text-red-500 text-sm mt-1"; prop.text errorMessage.Value ] ] ]

// チェックボックス用コンポーネント
let renderCheckboxField (label: string) (fieldId: string) (isChecked: bool) (onChange: bool -> unit) =
    Html.div
        [ prop.className "form-group mb-4 flex items-center"
          prop.children
              [ Html.input
                    [ prop.id fieldId
                      prop.type' "checkbox"
                      prop.className "h-4 w-4 text-blue-600 rounded mr-2"
                      prop.isChecked isChecked
                      prop.onChange onChange ]
                Html.label
                    [ prop.htmlFor fieldId
                      prop.className "text-sm font-medium text-gray-700"
                      prop.text label ] ] ]

// 製品編集フォームコンポーネント
[<ReactComponent>]
let RenderProductEditForm (product: ProductDetailDto) (dispatch: Msg -> unit) (onCancel: unit -> unit) =
    // React Hooksを使用してフォーム状態を管理 - ReactElementとして定義
    // フォーム状態フック
    let initialFormState = createInitialFormState product
    let formState, setFormState = React.useState (initialFormState)

    // フィールド更新関数
    let updateField fieldName value =
        let removeError prevState =
            { prevState with
                ValidationErrors = Map.remove fieldName prevState.ValidationErrors
                HasErrors = false }

        match fieldName with
        | "Name" -> setFormState ({ formState with Name = value } |> removeError)
        | "Description" ->
            setFormState (
                { formState with
                    Description =
                        if System.String.IsNullOrWhiteSpace value then
                            None
                        else
                            Some value }
                |> removeError
            )
        | "Category" ->
            setFormState (
                { formState with
                    Category =
                        if System.String.IsNullOrWhiteSpace value then
                            None
                        else
                            Some value }
                |> removeError
            )
        | "Price" ->
            match System.Double.TryParse value with
            | true, num -> setFormState ({ formState with Price = num } |> removeError)
            | _ -> ()
        | "Stock" ->
            match System.Int32.TryParse value with
            | true, num -> setFormState ({ formState with Stock = num } |> removeError)
            | _ -> ()
        | "SKU" -> setFormState ({ formState with SKU = value } |> removeError)
        | "IsActive" ->
            let isActive =
                match value with
                | "true" -> true
                | _ -> false

            setFormState ({ formState with IsActive = isActive } |> removeError)
        | _ -> ()

    // 保存ハンドラー
    let handleSave (e: Browser.Types.Event) =
        e.preventDefault ()

        // バリデーション実行
        let errors = validateProductForm formState

        if Map.isEmpty errors then
            // バリデーション成功時、更新DTOを作成
            let updateDto = toProductUpdateDto formState

            // APIを呼び出して更新
            dispatch (ApiMsg(ProductApi(UpdateProduct(int64 product.Id, updateDto))))
            onCancel ()
        else
            // バリデーションエラー表示
            setFormState (
                { formState with
                    HasErrors = true
                    ValidationErrors = errors }
            )

    // フォームレンダリング
    Html.form
        [ prop.className "bg-white rounded-lg shadow p-6"
          prop.onSubmit handleSave
          prop.children
              [ Html.h2 [ prop.className "text-xl font-bold mb-6"; prop.text "製品の編集" ]

                // 製品ID (編集不可)
                Html.div
                    [ prop.className "mb-6 p-3 bg-gray-50 rounded-lg"
                      prop.children
                          [ Html.div [ prop.className "text-sm text-gray-500"; prop.text "製品ID (編集不可)" ]
                            Html.div [ prop.className "font-mono font-medium"; prop.text (string product.Id) ] ] ]

                // 製品名
                renderTextField
                    "製品名"
                    "name"
                    formState.Name
                    (Map.containsKey "Name" formState.ValidationErrors)
                    (Map.tryFind "Name" formState.ValidationErrors)
                    (updateField "Name")

                // 説明
                renderTextareaField
                    "説明"
                    "description"
                    (Option.defaultValue "" formState.Description)
                    (Map.containsKey "Description" formState.ValidationErrors)
                    (Map.tryFind "Description" formState.ValidationErrors)
                    (updateField "Description")

                // カテゴリ
                renderTextField
                    "カテゴリ"
                    "category"
                    (Option.defaultValue "" formState.Category)
                    (Map.containsKey "Category" formState.ValidationErrors)
                    (Map.tryFind "Category" formState.ValidationErrors)
                    (updateField "Category")

                // 価格と在庫を横に並べる
                Html.div
                    [ prop.className "grid grid-cols-1 md:grid-cols-2 gap-4"
                      prop.children
                          [
                            // 価格
                            renderNumberField
                                "価格"
                                "price"
                                formState.Price
                                (Map.containsKey "Price" formState.ValidationErrors)
                                (Map.tryFind "Price" formState.ValidationErrors)
                                (updateField "Price")
                                (Some 0.01)
                                (Some 0.01)

                            // 在庫
                            renderNumberField
                                "在庫数"
                                "stock"
                                (float formState.Stock)
                                (Map.containsKey "Stock" formState.ValidationErrors)
                                (Map.tryFind "Stock" formState.ValidationErrors)
                                (updateField "Stock")
                                (Some 0.0)
                                (Some 1.0) ] ]

                // SKU
                renderTextField
                    "SKU"
                    "sku"
                    formState.SKU
                    (Map.containsKey "SKU" formState.ValidationErrors)
                    (Map.tryFind "SKU" formState.ValidationErrors)
                    (updateField "SKU")

                // 有効状態
                renderCheckboxField "製品を有効化する" "isActive" formState.IsActive (fun isChecked ->
                    updateField "IsActive" (if isChecked then "true" else "false"))

                // 全体的なエラーメッセージ (もしあれば)
                if formState.HasErrors then
                    Html.div
                        [ prop.className "mb-4 p-3 bg-red-50 text-red-700 rounded-lg"
                          prop.children [ Html.p [ prop.text "いくつかのフィールドにエラーがあります。修正してください。" ] ] ]

                // ボタン
                Html.div
                    [ prop.className "flex justify-end space-x-3 mt-6"
                      prop.children
                          [ Html.button
                                [ prop.type' "button"
                                  prop.className "px-4 py-2 border rounded text-gray-700 hover:bg-gray-50"
                                  prop.text "キャンセル"
                                  prop.onClick (fun _ -> onCancel ()) ]
                            Html.button
                                [ prop.type' "submit"
                                  prop.className "px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600"
                                  prop.text "保存" ] ] ] ] ]

// 製品詳細パネルをレンダリング - 編集機能対応版
[<ReactComponent>]
let RenderProductDetail (model: Model) (dispatch: Msg -> unit) =
    // React functionComponentとして定義
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
                    Html.p [ prop.className "text-red-600"; prop.text (ApiClient.getErrorMessage error) ]
                    Html.button
                        [ prop.className "mt-4 px-4 py-2 bg-blue-500 text-white rounded"
                          prop.text "製品一覧に戻る"
                          prop.onClick (fun _ -> dispatch (ProductsMsg CloseProductDetails)) ] ] ]

    | Some(Success product), detailedProduct ->
        if editMode then
            // 編集モード表示
            match detailedProduct with
            | Some(Success detailData) ->
                Html.div
                    [ prop.className "h-full flex flex-col"
                      prop.children
                          [
                            // ヘッダー部分
                            Html.div
                                [ prop.className "flex justify-between items-center border-b pb-4 mb-4"
                                  prop.children
                                      [ Html.h2 [ prop.className "text-xl font-bold"; prop.text "製品編集" ]
                                        Html.button
                                            [ prop.className "text-gray-500 hover:text-gray-700"
                                              prop.onClick (fun _ -> setEditMode false)
                                              prop.children [ Html.span [ prop.className "text-xl"; prop.text "×" ] ] ] ] ]

                            // 編集フォーム
                            RenderProductEditForm detailData dispatch (fun () -> setEditMode false) ] ]
            | _ ->
                // 詳細データがない場合でも基本情報から編集フォームを表示
                Html.div
                    [ prop.className "h-full flex flex-col"
                      prop.children
                          [
                            // ヘッダー部分
                            Html.div
                                [ prop.className "flex justify-between items-center border-b pb-4 mb-4"
                                  prop.children
                                      [ Html.h2 [ prop.className "text-xl font-bold"; prop.text "製品編集" ]
                                        Html.button
                                            [ prop.className "text-gray-500 hover:text-gray-700"
                                              prop.onClick (fun _ -> setEditMode false)
                                              prop.children [ Html.span [ prop.className "text-xl"; prop.text "×" ] ] ] ] ]

                            // 基本情報のみから仮の詳細データを作成
                            let basicDetailData =
                                { Id = product.Id
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

                            RenderProductEditForm basicDetailData dispatch (fun () -> setEditMode false) ] ]
        else
            // 詳細表示モード
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
                                    | Some(Failed error) ->
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
                                                Html.dd
                                                    [ prop.className "col-span-2"; prop.text (string product.Stock) ]

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
                                                    Html.dt
                                                        [ prop.className "text-gray-500 col-span-1"; prop.text "最終更新" ]

                                                    Html.dd [ prop.className "col-span-2"; prop.text updateDate ]
                                                | None -> Html.none

                                                // 説明
                                                Html.dt
                                                    [ prop.className "text-gray-500 col-span-3 mt-4"; prop.text "説明" ]
                                                Html.dd
                                                    [ prop.className "col-span-3 mt-2 bg-gray-50 p-3 rounded"
                                                      prop.text (defaultArg product.Description "説明はありません") ] ] ]

                                    // 詳細情報がある場合に追加フィールドを表示
                                    match detailedProduct with
                                    | Some(Success detailData) -> renderAdditionalFields detailData
                                    | _ -> Html.none ] ]

                        // アクションボタン
                        Html.div
                            [ prop.className "border-t pt-4 mt-auto flex gap-2 justify-end"
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
