// View.fs
module App.View

open Feliz
open Feliz.Router
open App.Types
open App.Interop
open App.TabPluginDecorator
open App.NotificationView

// タブのレンダリング
let renderTabs (model: Model) (dispatch: Msg -> unit) =
    let tabs = [ Tab.Home, "Home"; Tab.Counter, "Counter" ]

    // カスタムタブを取得して追加
    let customTabs =
        getAvailableCustomTabs ()
        |> List.map (fun customTab ->
            match customTab with
            | Tab.CustomTab id -> customTab, id
            | _ -> customTab, "")

    let allTabs = tabs @ customTabs

    Html.div
        [ prop.className "flex flex-wrap border-b border-gray-200 mb-5"
          prop.children
              [ for (tab, label) in allTabs do
                    Html.button
                        [ prop.text label
                          prop.className (
                              if model.CurrentTab = tab then
                                  "px-5 py-2.5 bg-white border border-gray-200 border-b-white -mb-px font-medium"
                              else
                                  "px-5 py-2.5 bg-gray-100 border border-gray-200 hover:bg-gray-200 transition-colors"
                          )
                          prop.onClick (fun _ -> dispatch (NavigateTo tab)) ] ] ]

// ホームタブの内容
let renderHome (model: Model) =
    Html.div
        [ prop.className "p-5 text-center"
          prop.children
              [ Html.h1 [ prop.className "text-2xl font-bold mb-4"; prop.text "Home" ]
                Html.p [ prop.className "text-gray-700"; prop.text model.Message ] ] ]

// カウンタータブの内容 (装飾されていないバージョン)
let renderCounterBase (model: Model) (dispatch: Msg -> unit) =
    Html.div
        [ prop.className "p-5 text-center"
          prop.children
              [ Html.h1 [ prop.className "text-2xl font-bold mb-4"; prop.text "Counter" ]
                Html.div
                    [ prop.className "text-xl mb-5"
                      prop.children
                          [ Html.span
                                [ prop.className "font-medium"
                                  prop.text (sprintf "Current value: %d" model.Counter) ] ] ]
                Html.div
                    [ prop.className "flex justify-center gap-2 mb-6"
                      prop.children
                          [ Html.button
                                [ prop.className
                                      "px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 transition-colors"
                                  prop.text "+"
                                  prop.onClick (fun _ -> dispatch IncrementCounter) ]
                            Html.button
                                [ prop.className
                                      "px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 transition-colors"
                                  prop.text "-"
                                  prop.onClick (fun _ -> dispatch DecrementCounter) ] ] ] ] ]

// 装飾機能を使ったカウンタータブのレンダリング
let renderCounter (model: Model) (dispatch: Msg -> unit) =
    // デコレーター機能を使って、カウンタービューをプラグインで拡張できるようにする
    decorateTabView CounterTab model dispatch (fun () -> renderCounterBase model dispatch)

// カスタムタブの内容を取得
let renderCustomTab (tabId: string) (model: Model) (dispatch: Msg -> unit) =
    match getCustomView tabId model dispatch with
    | Some customElement -> customElement
    | None ->
        Html.div
            [ prop.className "p-8 text-center bg-red-50 rounded-lg"
              prop.children
                  [ Html.h1
                        [ prop.className "text-2xl font-bold text-red-600 mb-4"
                          prop.text "Custom Tab Error" ]
                    Html.p
                        [ prop.className "text-red-500 mb-4"
                          prop.text (sprintf "Custom view for tab '%s' not found" tabId) ]
                    Html.button
                        [ prop.className "px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 transition-colors"
                          prop.text "Go to Home"
                          prop.onClick (fun _ -> dispatch (NavigateTo Tab.Home)) ] ] ]

let renderNotFound (model: Model) (dispatch: Msg -> unit) =
    Html.div
        [ prop.className "p-5 text-center"
          prop.children
              [ Html.h1
                    [ prop.className "text-2xl font-bold mb-4 text-red-600"
                      prop.text "ページが見つかりません" ]
                Html.p
                    [ prop.className "mb-4 text-gray-700"
                      prop.text "お探しのページは存在しないか、移動した可能性があります。" ]
                Html.button
                    [ prop.className "px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 transition-colors"
                      prop.text "ホームに戻る"
                      prop.onClick (fun _ -> dispatch (NavigateTo Tab.Home)) ] ] ]

// パラメータ付きルートの表示
let renderResourceWithId (resource: string) (id: string) (model: Model) (dispatch: Msg -> unit) =
    Html.div
        [ prop.className "p-5"
          prop.children
              [ Html.h1
                    [ prop.className "text-2xl font-bold mb-4"
                      prop.text (sprintf "%s 詳細" resource) ]
                Html.div
                    [ prop.className "bg-white rounded-lg p-4 shadow-sm"
                      prop.children
                          [ Html.p
                                [ prop.className "mb-2"
                                  prop.children
                                      [ Html.span [ prop.className "font-medium"; prop.text "リソースタイプ: " ]
                                        Html.span [ prop.text resource ] ] ]
                            Html.p
                                [ prop.className "mb-4"
                                  prop.children
                                      [ Html.span [ prop.className "font-medium"; prop.text "ID: " ]
                                        Html.span [ prop.text id ] ] ]
                            Html.button
                                [ prop.className
                                      "px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 transition-colors"
                                  prop.text "戻る"
                                  prop.onClick (fun _ -> dispatch (NavigateTo Tab.Home)) ] ] ] ] ]

// クエリパラメータ付きルートの表示
let renderWithQuery (basePath: string) (queries: Map<string, string>) (model: Model) (dispatch: Msg -> unit) =
    Html.div
        [ prop.className "p-5"
          prop.children
              [ Html.h1
                    [ prop.className "text-2xl font-bold mb-4"
                      prop.text (sprintf "%s クエリパラメータ" basePath) ]
                Html.div
                    [ prop.className "bg-white rounded-lg p-4 shadow-sm mb-4"
                      prop.children
                          [ Html.p
                                [ prop.className "mb-4"
                                  prop.children
                                      [ Html.span [ prop.className "font-medium"; prop.text "ベースパス: " ]
                                        Html.span [ prop.text basePath ] ] ]
                            Html.h2 [ prop.className "font-medium mb-2"; prop.text "クエリパラメータ:" ]
                            if Map.isEmpty queries then
                                Html.p [ prop.className "italic text-gray-500"; prop.text "クエリパラメータはありません" ]
                            else
                                Html.ul
                                    [ prop.className "list-disc pl-5"
                                      prop.children
                                          [ for KeyValue(key, value) in queries ->
                                                Html.li
                                                    [ prop.key key
                                                      prop.children
                                                          [ Html.span
                                                                [ prop.className "font-medium"; prop.text (key + ": ") ]
                                                            Html.span [ prop.text value ] ] ] ] ] ] ]
                Html.button
                    [ prop.className "px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 transition-colors"
                      prop.text "戻る"
                      prop.onClick (fun _ -> dispatch (NavigateTo Tab.Home)) ] ] ]

// View.fs の renderHome 関数に追加
let renderDebugLinks (dispatch: Msg -> unit) =
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
                    [ prop.className "max-w-4xl mx-auto p-5 bg-white shadow-md min-h-screen"
                      prop.children
                          [ renderTabs model dispatch
                            renderNotifications model dispatch
                            // デバッグリンクもここに配置可能
                            renderDebugLinks dispatch
                            renderCurrentRouteInfo model

                            // メインコンテンツ部分
                            Html.div
                                [ prop.className "bg-white rounded-md"
                                  prop.children
                                      [
                                        // 現在のルートに基づいてコンテンツをレンダリング
                                        match model.CurrentRoute with
                                        | Route.Home -> renderHome model
                                        | Route.Counter -> renderCounter model dispatch
                                        | Route.CustomTab id -> renderCustomTab id model dispatch
                                        | Route.WithParam(resource, id) ->
                                            renderResourceWithId resource id model dispatch
                                        | Route.WithQuery(basePath, queries) ->
                                            renderWithQuery basePath queries model dispatch
                                        | Route.NotFound -> renderNotFound model dispatch ] ] ] ] ] ]
