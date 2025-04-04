module App.UI.Components.Common.Button

open Feliz

// ボタンコンポーネント - バリエーション対応
let button (text: string) (onClick: unit -> unit) (variant: string) =
    Html.button
        [ prop.className (
              match variant with
              | "primary" -> "px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 transition-colors"
              | "danger" -> "px-4 py-2 bg-red-500 text-white rounded hover:bg-red-600 transition-colors"
              | "outline" -> "px-4 py-2 border border-gray-300 rounded hover:bg-gray-100 transition-colors"
              | "success" -> "px-4 py-2 bg-green-500 text-white rounded hover:bg-green-600 transition-colors"
              | _ -> "px-4 py-2 bg-gray-200 rounded hover:bg-gray-300 transition-colors"
          )
          prop.text text
          prop.onClick (fun _ -> onClick ()) ]

// アイコン付きボタン
let iconButton (iconText: string) (text: string option) (onClick: unit -> unit) (variant: string) =
    Html.button
        [ prop.className (
              match variant with
              | "primary" ->
                  "px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 transition-colors flex items-center"
              | "danger" ->
                  "px-4 py-2 bg-red-500 text-white rounded hover:bg-red-600 transition-colors flex items-center"
              | "outline" ->
                  "px-4 py-2 border border-gray-300 rounded hover:bg-gray-100 transition-colors flex items-center"
              | "success" ->
                  "px-4 py-2 bg-green-500 text-white rounded hover:bg-green-600 transition-colors flex items-center"
              | _ -> "px-4 py-2 bg-gray-200 rounded hover:bg-gray-300 transition-colors flex items-center"
          )
          prop.onClick (fun _ -> onClick ())
          prop.children
              [ Html.span [ prop.className "mr-2"; prop.text iconText ]
                if text.IsSome then
                    Html.span [ prop.text text.Value ] ] ]
