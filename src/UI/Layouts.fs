// App.UI.Layouts.fs - 純粋なUIコンポーネント
module App.UI.Layouts

open Feliz
open App.UI.Components

// 標準的なカードレイアウト
let cardLayout (title: string option) (children: ReactElement list) = card title children

// セクションレイアウト（タイトル付き）
let sectionLayout (title: string) (children: ReactElement list) =
    Html.section
        [ prop.className "mb-6"
          prop.children
              [ Html.h2 [ prop.className "text-xl font-bold mb-4"; prop.text title ]
                yield! children ] ]

// マスター/詳細レイアウト（標準）
let masterDetail (master: ReactElement) (detail: ReactElement option) =
    Html.div
        [ prop.className "grid grid-cols-1 lg:grid-cols-3 gap-4"
          prop.children
              [
                // マスターコンテンツ（一覧部分）- デスクトップでは2/3の幅
                Html.div
                    [ prop.className (if detail.IsSome then "lg:col-span-2" else "col-span-full")
                      prop.children [ master ] ]

                // 詳細コンテンツがある場合のみ表示
                if detail.IsSome then
                    Html.div
                        [ prop.className "lg:col-span-1 border rounded-lg shadow"
                          prop.children [ detail.Value ] ] ] ]

// マスター/詳細レイアウト（汎用的なローディング状態対応）
let masterDetailWithLoadingState
    (master: ReactElement)
    (detailContent: ReactElement option)
    (isLoading: bool)
    (loadingContent: ReactElement)
    (hasError: bool)
    (errorContent: ReactElement)
    =

    let detail =
        if isLoading then Some loadingContent
        else if hasError then Some errorContent
        else detailContent

    masterDetail master detail

// レスポンシブ対応のマスター/詳細レイアウト（小画面では詳細が上部に表示）
let responsiveMasterDetail (master: ReactElement) (detail: ReactElement option) (hasDetail: bool) =
    if hasDetail && detail.IsSome then
        // 詳細表示がある場合
        Html.div
            [ prop.className "w-full" // 幅100%を確保
              prop.children
                  [
                    // 小画面用のレイアウト
                    Html.div
                        [ prop.className "block lg:hidden" // lgより小さい画面でのみ表示
                          prop.children
                              [
                                // 詳細（上部）
                                Html.div
                                    [ prop.className "mb-4 border rounded-lg shadow"
                                      prop.children [ detail.Value ] ]

                                // マスター（下部）
                                master ] ]

                    // 大画面用のレイアウト
                    Html.div
                        [ prop.className "hidden lg:block" // lg以上の画面でのみ表示
                          prop.children
                              [ Html.div
                                    [ prop.className "grid grid-cols-3 gap-4"
                                      prop.children
                                          [
                                            // マスター（左側 - 2/3幅）
                                            Html.div [ prop.className "col-span-2"; prop.children [ master ] ]

                                            // 詳細（右側 - 1/3幅）
                                            Html.div
                                                [ prop.className "col-span-1 border rounded-lg shadow"
                                                  prop.children [ detail.Value ] ] ] ] ] ] ] ]
    else
        // 詳細表示がない場合はマスターのみ表示
        master

// レスポンシブ対応のマスター/詳細レイアウト（汎用的なローディング状態対応）
let responsiveMasterDetailWithLoadingState
    (master: ReactElement)
    (detailContent: ReactElement option)
    (hasDetail: bool)
    (isLoading: bool)
    (loadingContent: ReactElement)
    (hasError: bool)
    (errorContent: ReactElement)
    =

    let detail =
        if isLoading then Some loadingContent
        else if hasError then Some errorContent
        else detailContent

    responsiveMasterDetail master detail hasDetail

// 2列グリッドレイアウト
let twoColumnGrid (leftContent: ReactElement) (rightContent: ReactElement) =
    Html.div
        [ prop.className "grid grid-cols-1 md:grid-cols-2 gap-4"
          prop.children
              [ Html.div [ prop.children [ leftContent ] ]
                Html.div [ prop.children [ rightContent ] ] ] ]

// タブレイアウト
let tabLayout (tabs: (string * ReactElement) list) (activeTab: string) (onTabChange: string -> unit) =
    Html.div
        [ prop.children
              [
                // タブヘッダー
                Html.div
                    [ prop.className "flex border-b"
                      prop.children
                          [ for (tabName, _) in tabs ->
                                Html.button
                                    [ prop.className (
                                          if tabName = activeTab then
                                              "px-4 py-2 border-b-2 border-blue-500 text-blue-600 font-medium"
                                          else
                                              "px-4 py-2 text-gray-600 hover:text-gray-800"
                                      )
                                      prop.text tabName
                                      prop.onClick (fun _ -> onTabChange tabName) ] ] ]

                // タブコンテンツ
                Html.div
                    [ prop.className "p-4"
                      prop.children
                          [ let (_, content) = tabs |> List.find (fun (name, _) -> name = activeTab)
                            content ] ] ] ]

// ヘッダーとコンテンツのレイアウト
let pageWithHeader (header: ReactElement) (content: ReactElement) =
    Html.div [ prop.children [ header; Html.div [ prop.className "mt-4"; prop.children [ content ] ] ] ]
