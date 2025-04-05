namespace App.UI.Components.DataDisplay

open Feliz

module Table =
    // ページング処理
    let paginateList<'T> (currentPage: int) (pageSize: int) (items: 'T list) =
        items |> List.skip ((currentPage - 1) * pageSize) |> List.truncate pageSize

    // ページヘッダー
    let pageHeader (title: string) (actions: ReactElement list) =
        Html.div
            [ prop.className "flex justify-between items-center mb-6"
              prop.children
                  [ Html.h1 [ prop.className "text-2xl font-semibold text-gray-900"; prop.text title ]
                    if not (List.isEmpty actions) then
                        Html.div [ prop.className "flex space-x-3"; prop.children actions ] ] ]

    // データテーブル
    let dataTable<'T> (headers: ReactElement list) (rows: 'T list) (renderRow: 'T -> ReactElement list) =
        Html.table
            [ prop.className "min-w-full divide-y divide-gray-200"
              prop.children
                  [ Html.thead
                        [ prop.className "bg-gray-50"
                          prop.children [ Html.tr [ prop.children headers ] ] ]
                    Html.tbody
                        [ prop.className "bg-white divide-y divide-gray-200"
                          prop.children [ for row in rows -> Html.tr [ prop.children (renderRow row) ] ] ] ] ]

    // データテーブル（文字列ヘッダー用）
    let dataTableWithStringHeaders<'T> (headers: string list) (rows: 'T list) (renderRow: 'T -> ReactElement list) =
        let headerElements =
            [ for header in headers ->
                  Html.th
                      [ prop.className "px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                        prop.text header ] ]

        dataTable headerElements rows renderRow

    // テーブル行アクション
    let tableRowActions (actions: (string * (unit -> unit) * string) list) =
        Html.div
            [ prop.className "flex space-x-2"
              prop.children
                  [ for (label, onClick, color) in actions ->
                        Html.button
                            [ prop.className $"px-3 py-1 text-sm {color} rounded hover:opacity-80"
                              prop.text label
                              prop.onClick (fun _ -> onClick ()) ] ] ]

    // ページング制御
    let paginationControl (currentPage: int) (totalPages: int) (onPageChange: int -> unit) =
        let pages =
            if totalPages <= 7 then
                [ 1..totalPages ] |> List.map Some
            else
                let startPages = if currentPage <= 4 then [ 1..5 ] else [ 1; 2 ]

                let endPages =
                    if currentPage >= totalPages - 3 then
                        [ totalPages - 4 .. totalPages ]
                    else
                        [ totalPages - 1; totalPages ]

                let middlePages =
                    if currentPage <= 4 then []
                    elif currentPage >= totalPages - 3 then []
                    else [ currentPage - 1; currentPage; currentPage + 1 ]

                let combinedPages =
                    (startPages |> List.map Some)
                    @ (if currentPage > 5 then [ None ] else [])
                    @ (middlePages |> List.map Some)
                    @ (if currentPage < totalPages - 4 then [ None ] else [])
                    @ (endPages |> List.map Some)

                combinedPages |> List.distinct

        Html.div
            [ prop.className "flex items-center justify-center space-x-1 mt-4"
              prop.children
                  [
                    // 前ページボタン
                    Html.button
                        [ prop.className "px-3 py-1 border rounded disabled:opacity-50"
                          prop.disabled (currentPage <= 1)
                          prop.onClick (fun _ ->
                              if currentPage > 1 then
                                  onPageChange (currentPage - 1))
                          prop.text "前へ" ]

                    // ページ番号
                    for page in pages do
                        match page with
                        | Some pageNum ->
                            Html.button
                                [ prop.className (
                                      if pageNum = currentPage then
                                          "px-3 py-1 bg-blue-500 text-white rounded"
                                      else
                                          "px-3 py-1 border rounded hover:bg-gray-100"
                                  )
                                  prop.text (string pageNum)
                                  prop.onClick (fun _ -> onPageChange pageNum) ]
                        | None -> Html.span [ prop.className "px-2"; prop.text "..." ]

                    // 次ページボタン
                    Html.button
                        [ prop.className "px-3 py-1 border rounded disabled:opacity-50"
                          prop.disabled (currentPage >= totalPages)
                          prop.onClick (fun _ ->
                              if currentPage < totalPages then
                                  onPageChange (currentPage + 1))
                          prop.text "次へ" ] ] ]

    // データ行カウント表示
    let dataCountDisplay (pageSize: int) (currentPage: int) (totalItems: int) (onPageSizeChange: int -> unit) =
        let startItem = (currentPage - 1) * pageSize + 1
        let endItem = min (currentPage * pageSize) totalItems

        Html.div
            [ prop.className "flex items-center text-sm text-gray-500"
              prop.children
                  [ Html.span [ prop.text $"{startItem}-{endItem} / {totalItems}件" ]
                    Html.span [ prop.className "mx-2"; prop.text "表示件数:" ]
                    Html.select
                        [ prop.className "border rounded px-2 py-1"
                          prop.value (string pageSize)
                          prop.onChange (fun (value: string) ->
                              match System.Int32.TryParse value with
                              | true, size -> onPageSizeChange size
                              | _ -> ())
                          prop.children
                              [ for size in [ 10; 25; 50; 100 ] ->
                                    Html.option [ prop.value (string size); prop.text (string size) ] ] ] ] ]

    // フィルター・ソートコントロール
    let tableFilterControl
        (columns: string list)
        (activeSort: string option)
        (sortDirection: string)
        (onSortChange: string -> unit)
        (searchValue: string)
        (onSearchChange: string -> unit)
        (onClearSort: unit -> unit)
        =
        let sortOptions =
            [ yield Html.option [ prop.value ""; prop.text "選択してください" ]
              for col in columns -> Html.option [ prop.value col; prop.text col ] ]

        let sortControl =
            Html.div
                [ prop.className "flex items-center gap-2"
                  prop.children
                      [ Html.span [ prop.className "text-gray-700"; prop.text "並び替え:" ]
                        Html.div
                            [ prop.className "flex items-center gap-1"
                              prop.children
                                  [ Html.select
                                        [ prop.className
                                              "block w-40 rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
                                          prop.value (Option.defaultValue "" activeSort)
                                          prop.onChange (fun (value: string) -> onSortChange value)
                                          prop.children sortOptions ]
                                    if activeSort.IsSome then
                                        Html.div
                                            [ prop.className "flex items-center gap-1"
                                              prop.children
                                                  [ Html.span
                                                        [ prop.className "text-sm text-gray-600"
                                                          prop.text (if sortDirection = "asc" then "(昇順)" else "(降順)") ]
                                                    Html.i
                                                        [ prop.className (
                                                              if sortDirection = "asc" then
                                                                  "fas fa-sort-up text-gray-600"
                                                              else
                                                                  "fas fa-sort-down text-gray-600"
                                                          )
                                                          prop.style
                                                              [ style.marginTop (
                                                                    if sortDirection = "desc" then -4 else 0
                                                                ) ] ]
                                                    Html.button
                                                        [ prop.className
                                                              "px-2 py-1 text-sm text-gray-600 hover:text-gray-900 hover:bg-gray-100 rounded"
                                                          prop.onClick (fun _ -> onClearSort ())
                                                          prop.children [ Html.i [ prop.className "fas fa-times" ] ] ] ] ] ] ] ] ]

        let searchControl =
            Html.div
                [ prop.className "flex items-center gap-2"
                  prop.children
                      [ Html.span [ prop.className "text-gray-700 whitespace-nowrap"; prop.text "検索:" ]
                        Html.input
                            [ prop.type' "text"
                              prop.className
                                  "block w-full rounded-md border-gray-300 shadow-sm focus:border-blue-500 focus:ring-blue-500 sm:text-sm"
                              prop.placeholder "検索キーワードを入力..."
                              prop.value searchValue
                              prop.onChange onSearchChange ] ] ]

        Html.div
            [ prop.className "flex flex-col sm:flex-row justify-between items-center gap-4 mb-4"
              prop.children [ sortControl; searchControl ] ]
