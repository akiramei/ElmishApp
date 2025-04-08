// src/View/ProductDetail/ProductEditForm.fs
module App.ProductEditForm

open Fable.Core
open Feliz
open App.Types
open App.Shared
open App.Model.ProductDetailTypes
open App.ProductDetailValidator
open App.UI.Components.SearchableSelector
open App.View.Components.Tabs
open App.View.Components.FormElements
open App.View.Components.AdditionalFields

// 製品マスタの型を作成 (App.Shared.ProdocutMasterDto に基づく)
type ProductMasterItem =
    { Code: string
      Name: string
      Price: double
      CreatedAt: string }

// フォームの初期ステートを生成
let createInitialFormState (product: ProductDetailDto) : ProductFormState =
    // 追加フィールドのマップを構築
    let additionalFields =
        [ ("Public01", product.Public01)
          ("Public02", product.Public02)
          ("Public03", product.Public03)
          ("Public04", product.Public04)
          ("Public05", product.Public05)
          ("Public06", product.Public06)
          ("Public07", product.Public07)
          ("Public08", product.Public08)
          ("Public09", product.Public09)
          ("Public10", product.Public10) ]
        |> Map.ofList

    { Code = product.Code
      Name = product.Name
      Description = product.Description
      Category = product.Category
      Price = product.Price
      Stock = product.Stock
      SKU = product.SKU
      IsActive = product.IsActive

      AdditionalFields = additionalFields

      HasErrors = false
      ValidationErrors = Map.empty }

// フォームステートからDTOを生成
let toProductUpdateDto (formState: ProductFormState) : ProductUpdateDto =
    { Name = formState.Name
      Description = formState.Description
      Category = formState.Category
      Price = formState.Price
      Stock = formState.Stock
      SKU = formState.SKU
      IsActive = formState.IsActive

      // 追加フィールド
      Public01 = Map.tryFind "Public01" formState.AdditionalFields |> Option.defaultValue None
      Public02 = Map.tryFind "Public02" formState.AdditionalFields |> Option.defaultValue None
      Public03 = Map.tryFind "Public03" formState.AdditionalFields |> Option.defaultValue None
      Public04 = Map.tryFind "Public04" formState.AdditionalFields |> Option.defaultValue None
      Public05 = Map.tryFind "Public05" formState.AdditionalFields |> Option.defaultValue None
      Public06 = Map.tryFind "Public06" formState.AdditionalFields |> Option.defaultValue None
      Public07 = Map.tryFind "Public07" formState.AdditionalFields |> Option.defaultValue None
      Public08 = Map.tryFind "Public08" formState.AdditionalFields |> Option.defaultValue None
      Public09 = Map.tryFind "Public09" formState.AdditionalFields |> Option.defaultValue None
      Public10 = Map.tryFind "Public10" formState.AdditionalFields |> Option.defaultValue None }

// 基本情報タブのフォーム
[<ReactComponent>]
let private RenderBasicInfoForm
    (formState: ProductFormState)
    (updateField: string -> string -> unit)
    (onCodeSelected: SelectableItem<ProductMasterItem> -> unit)
    =
    // 製品コードの入力状態を保持するためのステート
    let currentSearch, setCurrentSearch = React.useState ""

    // SearchableSelector のクエリ変更をトラッキングするためのオーバーライド
    let queryDebug, setQueryDebug = React.useState ""

    // Products.fs からマスタデータを取得する関数
    let loadProductMasters (query: string) : Async<SelectableItem<ProductMasterItem> list> =
        async {
            Fable.Core.JS.console.log ("Searching for product masters with query:", query)
            setQueryDebug (query) // クエリをステートに保存
            // API リクエスト
            let! result = Infrastructure.Api.Products.searchProductMasters query |> Async.AwaitPromise

            match result with
            | Ok masters ->
                Fable.Core.JS.console.log ("Found results:", masters.Length)
                Fable.Core.JS.console.log ("First few results:", masters |> List.truncate 3)

                return
                    masters
                    |> List.map (fun master ->
                        { Code = master.Code
                          Name = master.Name
                          Data =
                            { Code = master.Code
                              Name = master.Name
                              Price = master.Price
                              CreatedAt = master.CreatedAt } }
                        : SelectableItem<ProductMasterItem>)
            | Result.Error err ->
                // エラーをコンソールに出力
                Fable.Core.JS.console.log ("API error:", err)
                Fable.Core.JS.console.error ("API error details:", App.Infrastructure.Api.Client.getErrorMessage err)

                return []
        }

    // コード変更ハンドラー
    let handleCodeChange (selected: SelectableItem<ProductMasterItem> option) =
        Fable.Core.JS.console.log ("Code selection changed:", selected)
        Fable.Core.JS.console.log ("Current formState:", formState)

        match selected with
        | Some item ->
            // コードが選択された場合は、製品名と価格も自動設定
            onCodeSelected item
        | None ->
            // 選択解除された場合
            Fable.Core.JS.console.log ("Selection cleared but keeping current input")

        Fable.Core.JS.console.log ("Updated formState:", formState) // 製品コードの現在の選択状態

    let selectedMaster: SelectableItem<ProductMasterItem> option =
        if not (System.String.IsNullOrEmpty formState.Code) then
            Some
                { Code = formState.Code
                  Name = formState.Name
                  Data =
                    { Code = formState.Code
                      Name = formState.Name
                      Price = formState.Price
                      CreatedAt = "" } }
        else
            None

    Html.div
        [ prop.className "px-4"
          prop.children
              [
                // 製品コード (SearchableSelectorを使用)
                Html.div
                    [ prop.className "mb-4"
                      prop.children
                          [ SearchableSelector
                                {| defaultProps with
                                    Label = Some "製品コード"
                                    Placeholder = "コードまたは名称で検索..."
                                    IsRequired = true
                                    SelectedItem = selectedMaster
                                    OnChange = handleCodeChange
                                    MinSearchLength = 1
                                    MaxResults = 100
                                    LoadItems = Some loadProductMasters
                                    ErrorMessage = Map.tryFind "Code" formState.ValidationErrors |} ] ]

                // 製品名 (読み取り専用)
                renderTextField
                    "製品名 (コードにより自動設定)"
                    "Name"
                    formState.Name
                    (Map.containsKey "Name" formState.ValidationErrors)
                    (Map.tryFind "Name" formState.ValidationErrors)
                    (fun _ -> ()) // 編集不可
                    true // 読み取り専用

                // 説明
                renderTextareaField
                    "説明"
                    "Description"
                    (Option.defaultValue "" formState.Description)
                    (Map.containsKey "Description" formState.ValidationErrors)
                    (Map.tryFind "Description" formState.ValidationErrors)
                    (updateField "Description")

                // カテゴリ
                renderTextField
                    "カテゴリ"
                    "Category"
                    (Option.defaultValue "" formState.Category)
                    (Map.containsKey "Category" formState.ValidationErrors)
                    (Map.tryFind "Category" formState.ValidationErrors)
                    (updateField "Category")
                    false

                // 価格と在庫を横に並べる
                Html.div
                    [ prop.className "grid grid-cols-1 md:grid-cols-2 gap-4"
                      prop.children
                          [
                            // 価格
                            renderNumberField
                                "価格"
                                "Price"
                                formState.Price
                                (Map.containsKey "Price" formState.ValidationErrors)
                                (Map.tryFind "Price" formState.ValidationErrors)
                                (updateField "Price")
                                (Some 0.01)
                                (Some 0.01)

                            // 在庫
                            renderNumberField
                                "在庫数"
                                "Stock"
                                (float formState.Stock)
                                (Map.containsKey "Stock" formState.ValidationErrors)
                                (Map.tryFind "Stock" formState.ValidationErrors)
                                (updateField "Stock")
                                (Some 0.0)
                                (Some 1.0) ] ]

                // SKU
                renderTextField
                    "SKU"
                    "SKU"
                    formState.SKU
                    (Map.containsKey "SKU" formState.ValidationErrors)
                    (Map.tryFind "SKU" formState.ValidationErrors)
                    (updateField "SKU")
                    false

                // 有効状態
                renderCheckboxField "製品を有効化する" "isActive" formState.IsActive (fun isChecked ->
                    updateField "IsActive" (if isChecked then "true" else "false")) ] ]

// 製品編集フォームコンポーネント
[<ReactComponent>]
let RenderProductEditForm (product: ProductDetailDto) (dispatch: Msg -> unit) (onCancel: unit -> unit) =
    // フォーム状態フック
    let initialFormState = createInitialFormState product
    let formState, setFormState = React.useState (initialFormState)

    // アクティブタブ
    let activeTab, setActiveTab = React.useState (BasicInfo)

    let onCodeSelected (item: SelectableItem<ProductMasterItem>) =
        // ここで親コンポーネントの状態を更新
        setFormState
            { formState with
                Code = item.Code
                Name = item.Name
                Price = item.Data.Price
                ValidationErrors = Map.empty
                HasErrors = false }

    // フィールド更新関数 - 基本フィールド用
    let updateBasicField fieldName value =
        Fable.Core.JS.console.log ($"Updating field: {fieldName} with value: {value}")

        let removeError prevState : ProductFormState =
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

    // フィールド更新関数 - 追加フィールド用
    let updateAdditionalField fieldId value =
        let updatedFields =
            updateAdditionalFieldValue formState.AdditionalFields fieldId value

        let updatedState =
            { formState with
                AdditionalFields = updatedFields
                ValidationErrors = Map.remove fieldId formState.ValidationErrors
                HasErrors = false }

        setFormState updatedState

    // 保存ハンドラー
    let handleSave (e: Browser.Types.Event) =
        e.preventDefault ()

        // バリデーション実行 - 基本情報とカスタムフィールドの両方
        let basicErrors = validateProductForm formState
        let additionalErrors = validateAdditionalFields formState.AdditionalFields

        // すべてのエラーを結合
        let allErrors =
            Map.fold (fun acc key value -> Map.add key value acc) basicErrors additionalErrors

        if Map.isEmpty allErrors then
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
                    ValidationErrors = allErrors }
            )

    // フォームレンダリング
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
                                      [ Html.h2 [ prop.className "text-xl font-bold text-gray-800"; prop.text "製品編集" ]
                                        Html.span
                                            [ prop.className
                                                  "px-2 py-1 text-sm font-medium rounded-full bg-blue-100 text-blue-800"
                                              prop.text (sprintf "ID: %d" product.Id) ] ] ]
                            Html.button
                                [ prop.className "p-2 hover:bg-gray-200 rounded-full transition-colors duration-200"
                                  prop.onClick (fun _ -> onCancel ())
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

                // 製品ID (編集不可)
                Html.div
                    [ prop.className "mb-6 px-6"
                      prop.children
                          [ Html.div
                                [ prop.className "p-3 bg-gray-50 rounded-lg"
                                  prop.children
                                      [ Html.div [ prop.className "text-sm text-gray-500"; prop.text "製品ID (編集不可)" ]
                                        Html.div
                                            [ prop.className "font-mono font-medium"; prop.text (string product.Id) ] ] ] ] ]

                // フォーム
                Html.form
                    [ prop.className "flex-grow overflow-auto"
                      prop.onSubmit handleSave
                      prop.children
                          [
                            // タブナビゲーション
                            TabNavigation activeTab setActiveTab

                            // タブコンテンツ
                            match activeTab with
                            | BasicInfo ->
                                // 基本情報タブ
                                RenderBasicInfoForm formState updateBasicField onCodeSelected

                            | ExtraInfo ->
                                // 追加情報タブ
                                RenderAdditionalFieldsForm
                                    product
                                    formState.AdditionalFields
                                    formState.ValidationErrors
                                    updateAdditionalField

                            // 全体的なエラーメッセージ (もしあれば)
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
                                              prop.className "px-4 py-2 border rounded text-gray-700 hover:bg-gray-50"
                                              prop.text "キャンセル"
                                              prop.onClick (fun _ -> onCancel ()) ]
                                        Html.button
                                            [ prop.type' "submit"
                                              prop.className
                                                  "px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600"
                                              prop.text "保存" ] ] ] ] ] ] ]
