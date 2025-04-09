// ProductEditForm.fs - Converted to Elmish MVU pattern
module App.ProductEditForm

open Feliz
open Fable.Core
open App.Infrastructure.Api.Types
open App.Types
open App.Shared
open App.UI.Components.SearchableSelector
open App.View.Components.Tabs
open App.View.Components.FormElements

// 製品マスターの型を作成
type ProductMasterItem =
    { Code: string
      Name: string
      Price: float
      CreatedAt: string }

// 製品マスタ検索API呼び出し関数
let loadProductMasters
    (query: string)
    (page: int)
    (pageSize: int)
    : Async<
          App.UI.Components.SearchableSelector.SelectableItem<ProductMasterItem> list *
          App.UI.Components.SearchableSelector.PaginationInfo
       >
    =
    async {
        // API呼び出し (実際の実装に合わせて調整)
        let! (result: Result<ProductMasterDto list * bool * int option, ApiError>) =
            Infrastructure.Api.Products.searchProductMastersWithPaging query page pageSize
            |> Async.AwaitPromise

        match result with
        | Ok(items, hasMore, totalItemsOpt) ->
            // 検索結果をSelectableItemに変換
            let selectableItems =
                items
                |> List.map (fun master ->
                    { Code = master.Code
                      Name = master.Name
                      Data =
                        { Code = master.Code
                          Name = master.Name
                          Price = master.Price
                          CreatedAt = master.CreatedAt } }
                    : App.UI.Components.SearchableSelector.SelectableItem<ProductMasterItem>)

            let paginationInfo =
                { CurrentPage = page
                  HasMore = hasMore
                  TotalItems = totalItemsOpt }

            return (selectableItems, paginationInfo)
        | Result.Error _ ->
            // エラー時は空のリストを返す
            return
                ([],
                 { CurrentPage = page
                   HasMore = false
                   TotalItems = None })
    }

// 基本フィールドの値を取得するヘルパー
let getBasicFieldValue (formState: ProductEditFormState) (fieldName: string) (defaultValue: string) =
    Map.tryFind fieldName formState.BasicFields |> Option.defaultValue defaultValue

// 追加フィールドの値を取得するヘルパー
let getAdditionalFieldValue (formState: ProductEditFormState) (fieldId: string) =
    Map.tryFind fieldId formState.AdditionalFields |> Option.flatten

// 基本情報タブをレンダリング
let renderBasicInfoTab (formState: ProductEditFormState) (product: ProductDetailDto) (dispatch: Msg -> unit) =
    let tryParseDouble (s: string) =
        match System.Double.TryParse(s) with
        | true, v -> Some v
        | false, _ -> None

    let tryParseInt (s: string) =
        match System.Int32.TryParse(s) with
        | true, v -> Some v
        | false, _ -> None

    // 現在選択されている製品マスターアイテムを作成
    let selectedMaster: App.UI.Components.SearchableSelector.SelectableItem<ProductMasterItem> option =
        let code = getBasicFieldValue formState "Code" product.Code
        let name = getBasicFieldValue formState "Name" product.Name

        if not (System.String.IsNullOrEmpty code) then
            // 基本フィールドから価格を取得
            let price =
                match System.Double.TryParse(getBasicFieldValue formState "Price" (string product.Price)) with
                | true, value -> value
                | _ -> product.Price

            Some
                { Code = code
                  Name = name
                  Data =
                    { Code = code
                      Name = name
                      Price = price
                      CreatedAt = "" } }
        else
            None

    Html.div
        [ prop.className "px-4"
          prop.children
              [ SearchableSelector
                    {| App.UI.Components.SearchableSelector.defaultProps with
                        Label = Some "製品コード"
                        Placeholder = "コードまたは名称で検索..."
                        IsRequired = true
                        SelectedItem = selectedMaster
                        OnChange =
                            fun selected ->
                                match selected with
                                | Some item ->
                                    // 製品コード選択時に値を更新
                                    dispatch (
                                        ProductDetailMsg(
                                            EditFormFieldChanged(CodeSelected(item.Code, item.Name, item.Data.Price))
                                        )
                                    )
                                | None ->
                                    // 選択解除時
                                    ()
                        MinSearchLength = 1
                        MaxResults = 10
                        LoadItems = Some loadProductMasters
                        ErrorMessage = Map.tryFind "Code" formState.ValidationErrors |}

                // 製品名（読み取り専用）
                renderTextField
                    "製品名 (コードにより自動設定)"
                    "Name"
                    (getBasicFieldValue formState "Name" product.Name)
                    (Map.containsKey "Name" formState.ValidationErrors)
                    (Map.tryFind "Name" formState.ValidationErrors)
                    (fun _ -> ()) // 読み取り専用なので変更しない
                    true

                // 説明
                renderTextareaField
                    "説明"
                    "Description"
                    (getBasicFieldValue formState "Description" (Option.defaultValue "" product.Description))
                    (Map.containsKey "Description" formState.ValidationErrors)
                    (Map.tryFind "Description" formState.ValidationErrors)
                    (fun value -> dispatch (ProductDetailMsg(EditFormFieldChanged(BasicField("Description", value)))))

                // カテゴリ
                renderTextField
                    "カテゴリ"
                    "Category"
                    (getBasicFieldValue formState "Category" (Option.defaultValue "" product.Category))
                    (Map.containsKey "Category" formState.ValidationErrors)
                    (Map.tryFind "Category" formState.ValidationErrors)
                    (fun value -> dispatch (ProductDetailMsg(EditFormFieldChanged(BasicField("Category", value)))))
                    false

                // 価格と在庫を横に並べる
                Html.div
                    [ prop.className "grid grid-cols-1 md:grid-cols-2 gap-4"
                      prop.children
                          [ let price =
                                tryParseDouble (getBasicFieldValue formState "Price" (string product.Price))
                                |> Option.defaultValue product.Price

                            let stock =
                                tryParseInt (getBasicFieldValue formState "Stock" (string product.Stock))
                                |> Option.defaultValue product.Stock

                            // 価格
                            renderNumberField
                                "価格"
                                "Price"
                                price
                                (Map.containsKey "Price" formState.ValidationErrors)
                                (Map.tryFind "Price" formState.ValidationErrors)
                                (fun value ->
                                    dispatch (ProductDetailMsg(EditFormFieldChanged(BasicField("Price", value)))))
                                (Some 0.01)
                                (Some 0.01)

                            // 在庫
                            renderNumberField
                                "在庫数"
                                "Stock"
                                (float stock)
                                (Map.containsKey "Stock" formState.ValidationErrors)
                                (Map.tryFind "Stock" formState.ValidationErrors)
                                (fun value ->
                                    dispatch (ProductDetailMsg(EditFormFieldChanged(BasicField("Stock", value)))))
                                (Some 0.0)
                                (Some 1.0) ] ]

                // SKU
                renderTextField
                    "SKU"
                    "SKU"
                    (getBasicFieldValue formState "SKU" product.SKU)
                    (Map.containsKey "SKU" formState.ValidationErrors)
                    (Map.tryFind "SKU" formState.ValidationErrors)
                    (fun value -> dispatch (ProductDetailMsg(EditFormFieldChanged(BasicField("SKU", value)))))
                    false

                // 有効状態
                renderCheckboxField
                    "製品を有効化する"
                    "IsActive"
                    (getBasicFieldValue formState "IsActive" (string product.IsActive) = "true")
                    (fun isChecked ->
                        dispatch (ProductDetailMsg(EditFormFieldChanged(BasicField("IsActive", string isChecked))))) ] ]

// 追加情報タブをレンダリング
let renderAdditionalInfoTab (formState: ProductEditFormState) (product: ProductDetailDto) (dispatch: Msg -> unit) =
    // フィールド表示名のマッピング
    let fieldMapping =
        [ "Public01", "追加情報 01"
          "Public02", "追加情報 02"
          "Public03", "追加情報 03"
          "Public04", "追加情報 04"
          "Public05", "追加情報 05"
          "Public06", "追加情報 06"
          "Public07", "追加情報 07"
          "Public08", "追加情報 08"
          "Public09", "追加情報 09"
          "Public10", "追加情報 10" ]

    Html.div
        [ prop.className "grid grid-cols-1 md:grid-cols-2 gap-4 p-4"
          prop.children
              [ for (fieldId, displayName) in fieldMapping do
                    renderTextField
                        displayName
                        fieldId
                        (Option.defaultValue "" (getAdditionalFieldValue formState fieldId))
                        (Map.containsKey fieldId formState.ValidationErrors)
                        (Map.tryFind fieldId formState.ValidationErrors)
                        (fun value ->
                            dispatch (ProductDetailMsg(EditFormFieldChanged(AdditionalField(fieldId, value)))))
                        false ] ]

// 製品編集フォームをレンダリング（メイン関数）
let renderProductEditForm (model: Model) (dispatch: Msg -> unit) =
    match model.ApiData.ProductData.SelectedProductDetail with
    | Some(Success product) ->
        // フォーム状態が存在しない場合は初期化
        let formState =
            match model.ProductEditFormState with
            | Some state -> state
            | None -> createFormState product

        Html.div
            [ prop.className "h-full flex flex-col"
              prop.children
                  [
                    // ヘッダー
                    Html.div
                        [ prop.className
                              "flex justify-between items-center border-b pb-4 mb-4 bg-gray-50 px-6 py-4 rounded-t-lg"
                          prop.children
                              [ Html.div
                                    [ prop.className "flex items-center space-x-3"
                                      prop.children
                                          [ Html.h2
                                                [ prop.className "text-xl font-bold text-gray-800"; prop.text "製品編集" ]
                                            Html.span
                                                [ prop.className
                                                      "px-2 py-1 text-sm font-medium rounded-full bg-blue-100 text-blue-800"
                                                  prop.text (sprintf "ID: %d" product.Id) ] ] ]
                                Html.button
                                    [ prop.className "p-2 hover:bg-gray-200 rounded-full transition-colors duration-200"
                                      prop.onClick (fun _ -> dispatch (ProductDetailMsg CancelProductEdit))
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

                    // 製品ID（編集不可）
                    Html.div
                        [ prop.className "mb-6 px-6"
                          prop.children
                              [ Html.div
                                    [ prop.className "p-3 bg-gray-50 rounded-lg"
                                      prop.children
                                          [ Html.div [ prop.className "text-sm text-gray-500"; prop.text "製品ID (編集不可)" ]
                                            Html.div
                                                [ prop.className "font-mono font-medium"
                                                  prop.text (string product.Id) ] ] ] ] ]

                    // フォーム
                    Html.form
                        [ prop.className "flex-grow overflow-auto"
                          prop.onSubmit (fun e ->
                              e.preventDefault ()
                              dispatch (ProductDetailMsg SubmitProductEdit))
                          prop.children
                              [
                                // タブナビゲーション
                                TabNavigation formState.ActiveTab (fun tab ->
                                    dispatch (ProductDetailMsg(EditFormTabChanged tab)))

                                // タブコンテンツ
                                match formState.ActiveTab with
                                | BasicInfo -> renderBasicInfoTab formState product dispatch
                                | ExtraInfo -> renderAdditionalInfoTab formState product dispatch

                                // エラーメッセージ（存在する場合）
                                if formState.HasErrors then
                                    Html.div
                                        [ prop.className "px-6 mb-4"
                                          prop.children [ renderFormError "いくつかのフィールドにエラーがあります。修正してください。" ] ]

                                // ボタン
                                Html.div
                                    [ prop.className "border-t pt-4 mt-auto flex gap-2 justify-end px-6 py-3"
                                      prop.children
                                          [ Html.button
                                                [ prop.type' "button"
                                                  prop.className
                                                      "px-4 py-2 border rounded text-gray-700 hover:bg-gray-50"
                                                  prop.text "キャンセル"
                                                  prop.onClick (fun _ -> dispatch (ProductDetailMsg CancelProductEdit)) ]
                                            Html.button
                                                [ prop.type' "submit"
                                                  prop.className
                                                      "px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600"
                                                  prop.text "保存" ] ] ] ] ] ] ]
    | Some Loading ->
        // ローディング表示
        Html.div
            [ prop.className "p-8 text-center"
              prop.children
                  [ Html.p [ prop.className "text-gray-500"; prop.text "製品詳細を読み込み中..." ]
                    Html.div
                        [ prop.className "mt-4 flex justify-center"
                          prop.children
                              [ Html.div
                                    [ prop.className
                                          "animate-spin h-8 w-8 border-4 border-blue-500 rounded-full border-t-transparent" ] ] ] ] ]
    | Some(Failed error) ->
        // エラー表示
        Html.div
            [ prop.className "p-8 text-center bg-red-50 rounded-lg"
              prop.children
                  [ Html.p [ prop.className "text-red-600 font-bold mb-2"; prop.text "エラーが発生しました" ]
                    Html.p
                        [ prop.className "text-red-500"
                          prop.text (App.Infrastructure.Api.Client.getErrorMessage error) ]
                    Html.button
                        [ prop.className "mt-4 px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600"
                          prop.text "戻る"
                          prop.onClick (fun _ -> dispatch (ProductDetailMsg CancelProductEdit)) ] ] ]
    | _ ->
        // データなし表示
        Html.div
            [ prop.className "p-8 text-center"
              prop.children
                  [ Html.p [ prop.className "text-gray-500"; prop.text "製品データが利用できません" ]
                    Html.button
                        [ prop.className "mt-4 px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600"
                          prop.text "戻る"
                          prop.onClick (fun _ -> dispatch (ProductDetailMsg CancelProductEdit)) ] ] ]
