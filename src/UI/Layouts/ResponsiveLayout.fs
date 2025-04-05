module App.UI.Layouts.ResponsiveLayout

open Feliz

// レスポンシブなグリッドコンテナ
let responsiveGrid (children: ReactElement list) =
    Html.div
        [ prop.className "grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4"
          prop.children children ]

// レスポンシブなカードコンポーネント
let responsiveCard (title: string option) (content: ReactElement list) =
    Html.div
        [ prop.className
              "bg-white rounded-lg shadow-md overflow-hidden transition-transform duration-300 hover:shadow-lg"
          prop.children
              [ match title with
                | Some t ->
                    Html.div
                        [ prop.className "px-4 py-3 bg-gray-50 border-b"
                          prop.children [ Html.h3 [ prop.className "text-lg font-medium text-gray-900"; prop.text t ] ] ]
                | None -> Html.none
                Html.div [ prop.className "p-4"; prop.children content ] ] ]

// レスポンシブなナビゲーションバー
let responsiveNavbar (brand: ReactElement) (items: ReactElement list) =
    Html.nav
        [ prop.className "bg-white shadow-sm"
          prop.children
              [ Html.div
                    [ prop.className "max-w-7xl mx-auto px-4 sm:px-6 lg:px-8"
                      prop.children
                          [ Html.div
                                [ prop.className "flex justify-between h-16"
                                  prop.children
                                      [ Html.div
                                            [ prop.className "flex"
                                              prop.children
                                                  [ Html.div
                                                        [ prop.className "flex-shrink-0 flex items-center"
                                                          prop.children [ brand ] ]
                                                    Html.div
                                                        [ prop.className "hidden sm:ml-6 sm:flex sm:space-x-8"
                                                          prop.children items ] ] ]
                                        Html.div
                                            [ prop.className "hidden sm:ml-6 sm:flex sm:items-center"
                                              prop.children
                                                  [ Html.div [ prop.className "ml-3 relative"; prop.children items ] ] ] ] ] ] ] ] ]

// レスポンシブなフォームレイアウト
let responsiveForm (title: string) (fields: ReactElement list) (actions: ReactElement list) =
    Html.div
        [ prop.className "max-w-2xl mx-auto p-4 sm:p-6 lg:p-8"
          prop.children
              [ Html.div
                    [ prop.className "bg-white shadow sm:rounded-lg"
                      prop.children
                          [ Html.div
                                [ prop.className "px-4 py-5 sm:p-6"
                                  prop.children
                                      [ Html.h3
                                            [ prop.className "text-lg leading-6 font-medium text-gray-900"
                                              prop.text title ]
                                        Html.div [ prop.className "mt-5 space-y-4"; prop.children fields ]
                                        Html.div
                                            [ prop.className "mt-5 flex justify-end space-x-3"; prop.children actions ] ] ] ] ] ] ]

// レスポンシブなマスター/詳細レイアウト
let responsiveMasterDetail (masterContent: ReactElement) (detailContent: ReactElement option) (isDetailView: bool) =
    Html.div
        [ prop.className "flex flex-col lg:flex-row relative"
          prop.children
              [ Html.div
                    [ prop.className (
                          if isDetailView then
                              "hidden lg:block lg:w-1/2"
                          else
                              "w-full lg:w-1/2"
                      )
                      prop.children [ masterContent ] ]
                match detailContent with
                | Some content ->
                    Html.div
                        [ prop.className (
                              if isDetailView then
                                  "w-full lg:w-1/2 lg:border-l lg:relative lg:z-10 bg-white shadow-md"
                              else
                                  "hidden lg:block lg:w-1/2 lg:border-l bg-white"
                          )
                          prop.children [ content ] ]
                | None -> Html.none ] ]
