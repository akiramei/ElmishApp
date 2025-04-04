// UI/Layouts.fs
module App.UI.Layouts

open Feliz

// 2カラムレイアウト
let twoColumnLayout (sidebar: ReactElement) (content: ReactElement) =
    Html.div
        [ prop.className "flex flex-col md:flex-row"
          prop.children
              [ Html.div
                    [ prop.className "w-full md:w-64 flex-shrink-0 mb-4 md:mb-0 md:mr-4"
                      prop.children [ sidebar ] ]
                Html.div [ prop.className "flex-grow"; prop.children [ content ] ] ] ]

// グリッドレイアウト
let gridLayout (columns: int) (items: ReactElement list) =
    let columnClass =
        match columns with
        | 1 -> "grid-cols-1"
        | 2 -> "grid-cols-1 md:grid-cols-2"
        | 3 -> "grid-cols-1 md:grid-cols-2 lg:grid-cols-3"
        | 4 -> "grid-cols-1 md:grid-cols-2 lg:grid-cols-4"
        | _ -> "grid-cols-1 md:grid-cols-3"

    Html.div [ prop.className $"grid {columnClass} gap-4"; prop.children items ]

// マスター/詳細レイアウト (レスポンシブ)
let masterDetailLayout (master: ReactElement) (detail: ReactElement option) =
    Html.div
        [ prop.className "grid grid-cols-1 lg:grid-cols-3 gap-4"
          prop.children
              [ Html.div
                    [ prop.className (if detail.IsSome then "lg:col-span-2" else "col-span-full")
                      prop.children [ master ] ]
                if detail.IsSome then
                    Html.div
                        [ prop.className "lg:col-span-1 border rounded-lg shadow"
                          prop.children [ detail.Value ] ] ] ]

// ダッシュボードレイアウト
let dashboardLayout (header: ReactElement) (main: ReactElement) (sidebar: ReactElement option) =
    Html.div
        [ prop.className "flex flex-col min-h-screen"
          prop.children
              [
                // ヘッダー
                header

                // メインコンテンツエリア
                Html.div
                    [ prop.className "flex-grow flex flex-col md:flex-row"
                      prop.children
                          [ if sidebar.IsSome then
                                Html.div
                                    [ prop.className "w-full md:w-64 flex-shrink-0 mb-4 md:mb-0 md:mr-4"
                                      prop.children [ sidebar.Value ] ]

                            Html.div [ prop.className "flex-grow"; prop.children [ main ] ] ] ] ] ]

// タブコンテナレイアウト
let tabContainerLayout (tabs: (string * ReactElement) list) (activeTab: string) (onTabChange: string -> unit) =
    Html.div
        [ prop.className "flex flex-col"
          prop.children
              [
                // タブヘッダー
                Html.div
                    [ prop.className "flex flex-wrap border-b border-gray-200 mb-5"
                      prop.children
                          [ for (tabId, _) in tabs ->
                                Html.button
                                    [ prop.text tabId
                                      prop.className (
                                          if tabId = activeTab then
                                              "px-5 py-2.5 bg-white border border-gray-200 border-b-white -mb-px font-medium"
                                          else
                                              "px-5 py-2.5 bg-gray-100 border border-gray-200 hover:bg-gray-200 transition-colors"
                                      )
                                      prop.onClick (fun _ -> onTabChange tabId) ] ] ]

                // タブコンテンツ
                let (_, activeContent) = tabs |> List.find (fun (tabId, _) -> tabId = activeTab)

                activeContent ] ]
