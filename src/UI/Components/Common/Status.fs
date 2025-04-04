module App.UI.Components.Common.Status

open Feliz
open App.UI.Components.Common.Button

// ロード中表示コンポーネント
let loadingState (message: string) =
    Html.div
        [ prop.className "p-8 text-center"
          prop.children
              [ Html.div
                    [ prop.className
                          "animate-spin h-8 w-8 border-4 border-blue-500 rounded-full border-t-transparent mx-auto mb-4" ]
                Html.p [ prop.className "text-gray-500"; prop.text message ] ] ]

// エラー表示コンポーネント
let errorState (message: string) (onRetry: (unit -> unit) option) =
    Html.div
        [ prop.className "p-8 text-center bg-red-50 rounded-lg"
          prop.children
              [ Html.div [ prop.className "text-red-600 text-lg mb-2"; prop.text "エラーが発生しました" ]
                Html.p [ prop.className "text-red-500 mb-4"; prop.text message ]
                if onRetry.IsSome then
                    button "再試行" onRetry.Value "primary" ] ]

// 空データ表示コンポーネント
let emptyState (message: string) (actionText: string option) (onAction: (unit -> unit) option) =
    Html.div
        [ prop.className "p-8 text-center"
          prop.children
              [ Html.p [ prop.className "text-gray-500 mb-4"; prop.text message ]
                if actionText.IsSome && onAction.IsSome then
                    button actionText.Value onAction.Value "primary" ] ]

// ステータスバッジ
let statusBadge (text: string) (status: string) =
    let (bgColor, textColor) =
        match status with
        | "success" -> "bg-green-100", "text-green-800"
        | "warning" -> "bg-yellow-100", "text-yellow-800"
        | "error" -> "bg-red-100", "text-red-800"
        | "info" -> "bg-blue-100", "text-blue-800"
        | _ -> "bg-gray-100", "text-gray-800"

    Html.span
        [ prop.className $"px-2 inline-flex text-xs leading-5 font-semibold rounded-full {bgColor} {textColor}"
          prop.text text ]
