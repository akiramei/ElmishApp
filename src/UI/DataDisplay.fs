// UI/DataDisplay.fs
module App.UI.DataDisplay

open Feliz

// キー/値表示
let keyValuePair (key: string) (value: string) (isHighlighted: bool) =
    Html.div
        [ prop.className "flex justify-between py-2"
          prop.children
              [ Html.span [ prop.className "text-gray-500"; prop.text key ]
                Html.span
                    [ prop.className (if isHighlighted then "font-medium" else "")
                      prop.text value ] ] ]

// 詳細表示パネル
let detailPanel (title: string) (items: (string * string * bool) list) =
    Html.div
        [ prop.className "bg-white rounded-lg shadow-md p-4"
          prop.children
              [ Html.h3 [ prop.className "text-lg font-semibold mb-3"; prop.text title ]
                Html.div
                    [ prop.className "border-t pt-2"
                      prop.children [ for (key, value, isHighlighted) in items -> keyValuePair key value isHighlighted ] ] ] ]

// 定義リスト表示
let definitionList (items: (string * ReactElement) list) =
    Html.dl
        [ prop.className "grid grid-cols-3 gap-2"
          prop.children
              [ for (term, details) in items ->
                    React.fragment
                        [ Html.dt [ prop.className "text-gray-500 col-span-1"; prop.text term ]
                          Html.dd [ prop.className "col-span-2"; prop.children [ details ] ] ] ] ]

// メトリックカード (ダッシュボード用)
let metricCard (title: string) (value: string) (description: string option) (icon: string option) (color: string) =
    Html.div
        [ prop.className "bg-white rounded-lg shadow-md p-6"
          prop.children
              [ Html.div
                    [ prop.className "flex items-center"
                      prop.children
                          [ if icon.IsSome then
                                Html.div
                                    [ prop.className $"rounded-full {color} p-3 mr-4"
                                      prop.children [ Html.span [ prop.className "text-2xl"; prop.text icon.Value ] ] ]
                            Html.div
                                [ prop.children
                                      [ Html.h3 [ prop.className "text-gray-500 text-sm"; prop.text title ]
                                        Html.p [ prop.className "text-2xl font-bold"; prop.text value ]
                                        if description.IsSome then
                                            Html.p
                                                [ prop.className "text-gray-500 text-sm mt-1"
                                                  prop.text description.Value ] ] ] ] ] ] ]

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
                                                              ) ] ] ] ] ] ] ]

                Html.div
                    [ prop.className "relative"
                      prop.children
                          [ Html.input
                                [ prop.className "pl-8 pr-2 py-1 border rounded"
                                  prop.placeholder "検索..."
                                  prop.value searchValue
                                  prop.onChange onSearch ]
                            Html.div
                                [ prop.className "absolute left-2 top-1/2 transform -translate-y-1/2 text-gray-400"
                                  prop.children [ Html.span [ prop.className "text-lg"; prop.text "🔍" ] ] ] ] ] ] ]
