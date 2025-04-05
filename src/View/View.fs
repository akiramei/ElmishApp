// View.fs - Improved with UI components
module App.View

open Feliz
open Feliz.Router
open App.Types
open App.Interop
open App.NotificationView
open App.CounterView
open App.UI.Components.Common.Status

// タブのレンダリング - Tailwind CSSを使用
let renderTabs (model: Model) (dispatch: Msg -> unit) =
    let tabs =
        [ Tab.Home, "Home"
          Tab.Counter, "Counter"
          Tab.Products, "Products"
          Tab.Admin, "Admin" ]

    // カスタムタブを取得して追加
    let customTabs =
        getAvailableCustomTabs ()
        |> List.map (fun customTab ->
            match customTab with
            | Tab.CustomTab id -> customTab, id
            | _ -> customTab, "")

    let allTabs = tabs @ customTabs

    Html.div
        [ prop.className "flex space-x-2 border-b border-gray-200"
          prop.children
              [ for (tab, label) in allTabs ->
                    Html.button
                        [ prop.text label
                          prop.className (
                              if model.CurrentTab = tab then
                                  "px-4 py-2 text-blue-600 border-b-2 border-blue-600 font-medium"
                              else
                                  "px-4 py-2 text-gray-500 hover:text-gray-700 hover:border-gray-300"
                          )
                          prop.onClick (fun _ -> dispatch (NavigateTo tab)) ] ] ]

// ホームタブの内容
let renderHome (homeState: HomeState) =
    Html.div
        [ prop.className "p-5 text-center"
          prop.children
              [ Html.h1 [ prop.className "text-2xl font-bold mb-4"; prop.text "Home" ]
                Html.p [ prop.className "text-gray-600"; prop.text homeState.Message ] ] ]

// カスタムタブの内容を取得
let renderCustomTab (tabId: string) (model: Model) (dispatch: Msg -> unit) =
    match getCustomView tabId model dispatch with
    | Some customElement -> customElement
    | None ->
        errorState (sprintf "Custom view for tab '%s' not found" tabId) (Some(fun () -> dispatch (NavigateTo Tab.Home)))

// Not Found ページ
let renderNotFound (dispatch: Msg -> unit) =
    Html.div
        [ prop.className "flex flex-col items-center justify-center p-8 text-center"
          prop.children
              [ Html.h1
                    [ prop.className "text-2xl font-bold mb-4 text-red-600"
                      prop.text "ページが見つかりません" ]
                Html.p
                    [ prop.className "mb-4 text-gray-700"
                      prop.text "お探しのページは存在しないか、移動した可能性があります。" ]
                Html.button
                    [ prop.className "px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 transition-colors"
                      prop.onClick (fun _ -> dispatch (NavigateTo Tab.Home))
                      prop.text "ホームに戻る" ] ] ]

// パラメータ付きルートの表示 - F#らしいアプローチ
let renderResourceWithId (resource: string) (id: string) (dispatch: Msg -> unit) =
    Html.div
        [ prop.className "bg-white rounded-lg shadow-md p-6"
          prop.children
              [ Html.h2 [ prop.className "text-xl font-bold mb-4"; prop.text $"{resource} 詳細" ]
                Html.dl
                    [ prop.className "grid grid-cols-2 gap-2 mb-4"
                      prop.children
                          [ Html.dt [ prop.className "font-medium"; prop.text "リソースタイプ" ]
                            Html.dd [ prop.text resource ]

                            Html.dt [ prop.className "font-medium"; prop.text "ID" ]
                            Html.dd [ prop.text id ] ] ]
                Html.button
                    [ prop.className "px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 transition-colors"
                      prop.onClick (fun _ -> dispatch (NavigateTo Tab.Home))
                      prop.text "戻る" ] ] ]

// クエリパラメータ付きルートの表示 - F#らしいアプローチ
let renderWithQuery (basePath: string) (queries: Map<string, string>) (dispatch: Msg -> unit) =
    Html.div
        [ prop.className "bg-white rounded-lg shadow-md p-6"
          prop.children
              [ Html.h2 [ prop.className "text-xl font-bold mb-4"; prop.text $"{basePath} クエリパラメータ" ]
                Html.div
                    [ prop.className "mb-4"
                      prop.children
                          [
                            // ベースパス
                            Html.dl
                                [ prop.className "mb-3"
                                  prop.children
                                      [ Html.dt [ prop.className "font-medium mb-1"; prop.text "ベースパス" ]
                                        Html.dd [ prop.className "ml-4"; prop.text basePath ] ] ]

                            // クエリパラメータ
                            Html.h3 [ prop.className "font-medium mb-2"; prop.text "クエリパラメータ" ]

                            if Map.isEmpty queries then
                                Html.p [ prop.className "italic text-gray-500"; prop.text "クエリパラメータはありません" ]
                            else
                                Html.dl
                                    [ prop.className "grid grid-cols-2 gap-x-2 gap-y-1 ml-4"
                                      prop.children
                                          [ for KeyValue(key, value) in queries do
                                                Html.dt [ prop.className "font-medium"; prop.text key ]
                                                Html.dd [ prop.text value ] ] ] ] ]
                Html.button
                    [ prop.className "px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 transition-colors"
                      prop.onClick (fun _ -> dispatch (NavigateTo Tab.Home))
                      prop.text "戻る" ] ] ]

// デバッグリンク
let renderDebugLinks () =
    Html.div
        [ prop.className "mt-8 p-4 border rounded bg-gray-50"
          prop.children
              [ Html.h2 [ prop.className "text-lg font-bold mb-3"; prop.text "ルーティングテスト" ]
                Html.div
                    [ prop.className "flex flex-wrap gap-2"
                      prop.children
                          [ Html.a
                                [ prop.href "#/counter"
                                  prop.className "px-3 py-1 bg-blue-100 rounded hover:bg-blue-200"
                                  prop.text "カウンター" ]
                            Html.a
                                [ prop.href "#/products"
                                  prop.className "px-3 py-1 bg-blue-100 rounded hover:bg-blue-200"
                                  prop.text "製品一覧" ]
                            Html.a
                                [ prop.href "#/tab/hello"
                                  prop.className "px-3 py-1 bg-blue-100 rounded hover:bg-blue-200"
                                  prop.text "Hello タブ" ]
                            Html.a
                                [ prop.href "#/product/123"
                                  prop.className "px-3 py-1 bg-blue-100 rounded hover:bg-blue-200"
                                  prop.text "製品詳細" ]
                            Html.a
                                [ prop.href "#/search?query=test&sort=asc"
                                  prop.className "px-3 py-1 bg-blue-100 rounded hover:bg-blue-200"
                                  prop.text "検索クエリ" ]
                            Html.a
                                [ prop.href "#/not-found"
                                  prop.className "px-3 py-1 bg-blue-100 rounded hover:bg-blue-200"
                                  prop.text "404ページ" ] ] ] ] ]

// 現在のルート情報表示コンポーネント
let renderCurrentRouteInfo (model: Model) =
    Html.div
        [ prop.className "fixed bottom-0 right-0 bg-black bg-opacity-70 text-white p-2 text-xs rounded-tl"
          prop.children
              [ Html.div [ prop.className "font-bold"; prop.text "現在のルート:" ]
                Html.div [ prop.className "font-mono"; prop.text (sprintf "%A" model.CurrentRoute) ] ] ]

// メインビュー - Tailwind CSSを使用
let view (model: Model) (dispatch: Msg -> unit) =
    React.router
        [ router.hashMode
          router.onUrlChanged (fun segments ->
              // URLが変更されたときの処理
              printfn "URL changed to segments: %A" segments
              let route = Router.parseRoute segments
              dispatch (RouteChanged route))
          router.children
              [ Html.div
                    [ prop.className "container mx-auto p-5 bg-white shadow-md min-h-screen"
                      prop.children
                          [ renderTabs model dispatch
                            renderNotificationArea model dispatch

                            // メインコンテンツ部分
                            Html.div
                                [ prop.className "bg-white rounded-md"
                                  prop.children
                                      [
                                        // 現在のルートに基づいてコンテンツをレンダリング
                                        match model.CurrentRoute with
                                        | Route.Home -> renderHome model.HomeState
                                        | Route.Counter -> renderCounter model dispatch
                                        | Route.Products
                                        | Route.ProductDetail _ ->
                                            // 製品一覧と詳細を表示
                                            App.ProductsView.renderProductsWithDetail model dispatch
                                        // 管理者関連ルートの追加
                                        | Route.Admin
                                        | Route.AdminUsers
                                        | Route.AdminProducts -> App.AdminView.renderAdmin model dispatch
                                        | Route.CustomTab id -> renderCustomTab id model dispatch
                                        | Route.WithParam(resource, id) -> renderResourceWithId resource id dispatch
                                        | Route.WithQuery(basePath, queries) ->
                                            renderWithQuery basePath queries dispatch
                                        | Route.NotFound -> renderNotFound dispatch ] ]

                            // デバッグリンクとルート情報
                            renderDebugLinks ()
                            renderCurrentRouteInfo model ] ] ] ]
