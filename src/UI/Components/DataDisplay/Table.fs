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
        (activeSortDirection: string)
        (onSort: string -> unit)
        (searchValue: string)
        (onSearch: string -> unit)
        =
        Html.div
            [ prop.className "flex flex-wrap justify-between items-center mb-4"
              prop.children
                  [ Html.div
                        [ prop.className "flex items-center"
                          prop.children
                              [ Html.select
                                    [ prop.className "border rounded px-2 py-1 mr-2"
                                      prop.onChange (fun column -> onSort column)
                                      prop.children
                                          [ Html.option [ prop.text "ソート"; prop.value "" ] |> ignore
                                            for column in columns ->
                                                Html.option
                                                    [ prop.text column
                                                      prop.value column
                                                      if activeSort = Some column then
                                                          prop.selected true ] ] ]

                                if activeSort.IsSome then
                                    Html.div
                                        [ prop.className "flex items-center"
                                          prop.children
                                              [ Html.span
                                                    [ prop.className "text-gray-500 mr-1"
                                                      prop.text (
                                                          match activeSortDirection with
                                                          | "asc" -> "昇順"
                                                          | _ -> "降順"
                                                      ) ]
                                                Html.button
                                                    [ prop.className "p-1 text-gray-500 hover:text-gray-700"
                                                      prop.onClick (fun _ ->
                                                          match activeSort with
                                                          | Some column -> onSort column
                                                          | None -> ())
                                                      prop.children
                                                          [ Html.span
                                                                [ prop.className "text-lg"
                                                                  prop.text (
                                                                      match activeSortDirection with
                                                                      | "asc" -> "↑"
                                                                      | _ -> "↓"
                                                                  ) ] ] ] ] ] ] ] ] ]
