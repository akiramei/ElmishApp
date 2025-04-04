// UI/Forms.fs
module App.UI.Forms

open Feliz

// テキスト入力
let textField
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

// テキストエリア
let textareaField
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

// 数値入力
let numberField
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

// チェックボックス
let checkboxField (label: string) (fieldId: string) (isChecked: bool) (onChange: bool -> unit) =
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

// セレクトフィールド
let selectField
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
                          [ for (optionValue, optionText) in options ->
                                Html.option [ prop.value optionValue; prop.text optionText ] ] ]
                if hasError && errorMessage.IsSome then
                    Html.div [ prop.className "text-red-500 text-sm mt-1"; prop.text errorMessage.Value ] ] ]

// ラジオボタングループ
let radioGroup
    (label: string)
    (name: string)
    (options: (string * string) list)
    (selectedValue: string)
    (onChange: string -> unit)
    (hasError: bool)
    (errorMessage: string option)
    =
    Html.fieldSet
        [ prop.className "mb-4"
          prop.children
              [ Html.legend [ prop.className "text-sm font-medium text-gray-700 mb-1"; prop.text label ]
                |> ignore
                for (optionValue, optionText) in options ->
                    Html.div
                        [ prop.className "flex items-center mb-2"
                          prop.children
                              [ Html.input
                                    [ prop.type' "radio"
                                      prop.id $"{name}-{optionValue}"
                                      prop.name name
                                      prop.className "h-4 w-4 text-blue-600 mr-2"
                                      prop.isChecked ((selectedValue = optionValue))
                                      prop.onChange (fun (s: string) -> onChange optionValue) ]
                                Html.label
                                    [ prop.htmlFor $"{name}-{optionValue}"
                                      prop.className "text-sm text-gray-700"
                                      prop.text optionText ] ] ]
                if hasError && errorMessage.IsSome then
                    yield Html.div [ prop.className "text-red-500 text-sm mt-1"; prop.text errorMessage.Value ] ] ]

// フォームアクション（ボタングループ）
let formActions
    (onSubmit: unit -> unit)
    (onCancel: unit -> unit option)
    (submitText: string)
    (cancelText: string option)
    =
    Html.div
        [ prop.className "flex justify-end space-x-3 mt-6"
          prop.children
              [ if cancelText.IsSome && onCancel().IsSome then
                    Html.button
                        [ prop.type' "button"
                          prop.className "px-4 py-2 border rounded text-gray-700 hover:bg-gray-50"
                          prop.text cancelText.Value
                          prop.onClick (fun _ -> onCancel().Value) ]

                Html.button
                    [ prop.type' "submit"
                      prop.className "px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600"
                      prop.text submitText
                      prop.onClick (fun e ->
                          e.preventDefault ()
                          onSubmit ()) ] ] ]
