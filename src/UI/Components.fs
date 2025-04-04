// UI/Components.fs
module App.UI.Components

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

// カードコンポーネント
let card (title: string option) (children: ReactElement list) =
    Html.div
        [ prop.className "bg-white rounded-lg shadow-md p-4"
          prop.children
              [ if title.IsSome then
                    Html.h3 [ prop.className "text-lg font-semibold mb-3"; prop.text title.Value ]
                yield! children ] ]

// データテーブル
let dataTable<'T> (headers: string list) (rows: 'T list) (renderRow: 'T -> ReactElement list) =
    Html.table
        [ prop.className "min-w-full divide-y divide-gray-200"
          prop.children
              [ Html.thead
                    [ prop.className "bg-gray-50"
                      prop.children
                          [ Html.tr
                                [ prop.children
                                      [ for header in headers ->
                                            Html.th
                                                [ prop.className
                                                      "px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                                                  prop.text header ] ] ] ] ]
                Html.tbody
                    [ prop.className "bg-white divide-y divide-gray-200"
                      prop.children [ for row in rows -> Html.tr [ prop.children (renderRow row) ] ] ] ] ]

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

// ページヘッダー
let pageHeader (title: string) (actions: ReactElement list) =
    Html.div
        [ prop.className "flex justify-between items-center mb-6"
          prop.children
              [ Html.h1 [ prop.className "text-2xl font-bold"; prop.text title ]
                if not (List.isEmpty actions) then
                    Html.div [ prop.className "flex space-x-2"; prop.children actions ] ] ]
