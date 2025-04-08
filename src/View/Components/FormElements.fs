// src/View/ProductDetail/Components/FormElements.fs
module App.View.Components.FormElements

open Feliz

// テキスト入力フィールドコンポーネント
let renderTextField
    (label: string)
    (fieldId: string)
    (value: string)
    (hasError: bool)
    (errorMessage: string option)
    (onChange: string -> unit)
    (readonly: bool)
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
                      prop.onChange onChange
                      if readonly then
                          prop.disabled true ]
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

// セレクトボックス用コンポーネント
let renderSelectField
    (label: string)
    (fieldId: string)
    (value: string)
    (options: (string * string) list)
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
                Html.select
                    [ prop.id fieldId
                      prop.className (
                          "w-full p-2 border rounded "
                          + if hasError then "border-red-500" else "border-gray-300"
                      )
                      prop.value value
                      prop.onChange onChange
                      prop.children
                          [ for (value, label) in options do
                                Html.option [ prop.value value; prop.text label ] ] ]
                if hasError && errorMessage.IsSome then
                    Html.div [ prop.className "text-red-500 text-sm mt-1"; prop.text errorMessage.Value ] ] ]

// フォーム全体のエラーメッセージ
let renderFormError (message: string) =
    Html.div
        [ prop.className "mb-4 p-3 bg-red-50 text-red-700 rounded-lg"
          prop.children [ Html.p [ prop.text message ] ] ]

// フォームセクションのヘッダー
let renderSectionHeader (title: string) =
    Html.h3 [ prop.className "text-lg font-semibold mb-3 mt-6"; prop.text title ]

// アクションボタングループ（保存とキャンセル）
let renderActionButtons (onSave: Browser.Types.MouseEvent -> unit) (onCancel: unit -> unit) =
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
                      prop.text "保存"
                      prop.onClick onSave ] ] ]
