// src/View/ProductDetail/Components/AdditionalFields.fs
module App.View.Components.AdditionalFields

open Feliz
open App.Shared
open App.View.Components.FormElements

// 追加フィールドの型情報
type AdditionalField =
    { Id: string
      Value: string option
      DisplayName: string }

// 追加フィールドを取得する関数
let getAdditionalFields (productDetail: ProductDetailDto) =
    [
      // フィールド番号、値、表示名のタプル
      (1, productDetail.Public01, "追加情報 01")
      (2, productDetail.Public02, "追加情報 02")
      (3, productDetail.Public03, "追加情報 03")
      (4, productDetail.Public04, "追加情報 04")
      (5, productDetail.Public05, "追加情報 05")
      (6, productDetail.Public06, "追加情報 06")
      (7, productDetail.Public07, "追加情報 07")
      (8, productDetail.Public08, "追加情報 08")
      (9, productDetail.Public09, "追加情報 09")
      (10, productDetail.Public10, "追加情報 10")
      // 必要に応じて追加 (11-99)
      ]
    |> List.map (fun (index, value, displayName) ->
        { Id = sprintf "%02d" index
          Value = value
          DisplayName = displayName })
    |> List.filter (fun field -> Option.isSome field.Value)

// 編集用のフィールド値を更新する関数
let updateAdditionalFieldValue
    (formState: Map<string, string option>)
    (fieldId: string)
    (value: string)
    : Map<string, string option> =

    if System.String.IsNullOrWhiteSpace value then
        Map.add fieldId None formState
    else
        Map.add fieldId (Some value) formState

// 追加情報の表示コンポーネント（読み取り専用）
[<ReactComponent>]
let RenderAdditionalFieldsReadOnly (productDetail: ProductDetailDto) =
    let fields = getAdditionalFields productDetail

    if List.isEmpty fields then
        Html.div
            [ prop.className "p-6 text-center text-gray-500"
              prop.children [ Html.p [ prop.text "追加情報はありません" ] ] ]
    else
        Html.div
            [ prop.className "grid grid-cols-2 md:grid-cols-3 gap-3 p-4"
              prop.children
                  [ for field in fields do
                        Html.div
                            [ prop.key ("field-" + field.Id)
                              prop.className "border rounded p-2"
                              prop.children
                                  [ Html.div [ prop.className "text-sm text-gray-500"; prop.text field.DisplayName ]
                                    Html.div [ prop.className ""; prop.text (Option.defaultValue "-" field.Value) ] ] ] ] ]

// 追加情報の入力フォームコンポーネント
[<ReactComponent>]
let RenderAdditionalFieldsForm
    (productDetail: ProductDetailDto)
    (formState: Map<string, string option>)
    (errors: Map<string, string>)
    (onFieldChange: string -> string -> unit)
    =

    // すべてのフィールド（既存のものも含む）を取得
    let allPossibleFields =
        [
          // フィールド番号、値、表示名のタプル
          (1, productDetail.Public01, "追加情報 01")
          (2, productDetail.Public02, "追加情報 02")
          (3, productDetail.Public03, "追加情報 03")
          (4, productDetail.Public04, "追加情報 04")
          (5, productDetail.Public05, "追加情報 05")
          (6, productDetail.Public06, "追加情報 06")
          (7, productDetail.Public07, "追加情報 07")
          (8, productDetail.Public08, "追加情報 08")
          (9, productDetail.Public09, "追加情報 09")
          (10, productDetail.Public10, "追加情報 10")
          // 必要に応じて追加 (11-99)
          ]
        |> List.map (fun (index, value, displayName) ->
            let fieldId = sprintf "Public%02d" index

            { Id = fieldId
              Value = Map.tryFind fieldId formState |> Option.defaultValue value
              DisplayName = displayName })

    Html.div
        [ prop.className "grid grid-cols-1 md:grid-cols-2 gap-4 p-4"
          prop.children
              [ for field in allPossibleFields do
                    renderTextField
                        field.DisplayName
                        field.Id
                        (Option.defaultValue "" field.Value)
                        (Map.containsKey field.Id errors)
                        (Map.tryFind field.Id errors)
                        (fun value -> onFieldChange field.Id value) ] ]

// 追加フィールドの検証関数
let validateAdditionalFields (formState: Map<string, string option>) : Map<string, string> =
    let errors = Map.empty

    // 追加フィールドの検証ルール（例：特定の値が含まれているか、長さ制限など）
    // 現在はシンプルな実装であり、追加の検証が必要な場合は拡張可能
    formState
    |> Map.fold
        (fun acc key value ->
            match value with
            | Some v when v.Length > 100 -> Map.add key "100文字以内で入力してください" acc
            | _ -> acc)
        errors
