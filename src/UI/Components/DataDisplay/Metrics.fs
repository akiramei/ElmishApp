module App.UI.Components.DataDisplay.Metrics

open Feliz

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
