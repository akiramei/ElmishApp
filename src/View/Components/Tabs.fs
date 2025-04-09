// src/View/ProductDetail/Components/Tabs.fs
module App.View.Components.Tabs

open Feliz
open App.Types

[<ReactComponent>]
let TabNavigation (activeTab: DetailTab) (onTabChange: DetailTab -> unit) =
    Html.div
        [ prop.className "px-6 mb-4 border-b"
          prop.children
              [ Html.div
                    [ prop.className "flex space-x-1"
                      prop.children
                          [
                            // 基本情報タブ
                            Html.button
                                [ prop.type' "button"
                                  prop.className (
                                      "px-4 py-2 font-medium "
                                      + if activeTab = BasicInfo then
                                            "border-b-2 border-blue-500 text-blue-600"
                                        else
                                            "text-gray-500 hover:text-gray-700"
                                  )
                                  prop.text "基本情報"
                                  prop.onClick (fun _ -> onTabChange BasicInfo) ]

                            // 追加情報タブ
                            Html.button
                                [ prop.type' "button"
                                  prop.className (
                                      "px-4 py-2 font-medium "
                                      + if activeTab = ExtraInfo then
                                            "border-b-2 border-blue-500 text-blue-600"
                                        else
                                            "text-gray-500 hover:text-gray-700"
                                  )
                                  prop.text "追加情報"
                                  prop.onClick (fun _ -> onTabChange ExtraInfo) ] ] ] ] ]

// タブコンテンツを表示するためのコンテナ
[<ReactComponent>]
let TabContent (activeTab: DetailTab) (children: (DetailTab -> ReactElement)) =
    Html.div
        [ prop.className "flex-grow overflow-auto p-2"
          prop.children [ children activeTab ] ]

// タブ付きコンテナコンポーネント - タブとコンテンツを組み合わせる
[<ReactComponent>]
let TabContainer (initialTab: DetailTab) (renderTabContent: DetailTab -> ReactElement) =
    let activeTab, setActiveTab = React.useState (initialTab)

    Html.div
        [ prop.className "flex flex-col"
          prop.children [ TabNavigation activeTab setActiveTab; TabContent activeTab renderTabContent ] ]
