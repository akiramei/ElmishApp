// AdminView.fs - 管理者用インターフェースモジュール
module App.AdminView

open Feliz
open Feliz.Router
open App.Types
open App.ApiClient

// サイドバーナビゲーションアイテムのレンダリング
let private renderSidebarItem
    (currentRoute: Route)
    (targetRoute: Route)
    (icon: string)
    (label: string)
    (dispatch: Msg -> unit)
    =
    Html.li
        [ prop.className (
              if currentRoute = targetRoute then
                  "bg-blue-100 text-blue-800"
              else
                  "hover:bg-gray-100"
          )
          prop.children
              [ Html.a
                    [ prop.href "#"
                      prop.className "flex items-center p-3 text-gray-700"
                      prop.onClick (fun _ -> dispatch (RouteChanged targetRoute))
                      prop.children
                          [ Html.span [ prop.className "mr-3 text-xl"; prop.text icon ]
                            Html.span [ prop.text label ] ] ] ] ]

// 管理者サイドバーのレンダリング
let renderAdminSidebar (model: Model) (dispatch: Msg -> unit) =
    Html.div
        [ prop.className "bg-white shadow-md rounded-lg w-64 py-4"
          prop.children
              [ Html.div
                    [ prop.className "px-6 py-2 border-b"
                      prop.children [ Html.h2 [ prop.className "text-xl font-bold text-gray-800"; prop.text "管理パネル" ] ] ]
                Html.ul
                    [ prop.className "mt-4"
                      prop.children
                          [ renderSidebarItem model.CurrentRoute Route.Admin "📊" "ダッシュボード" dispatch
                            renderSidebarItem model.CurrentRoute Route.AdminUsers "👥" "ユーザー管理" dispatch
                            renderSidebarItem model.CurrentRoute Route.AdminProducts "📦" "製品管理" dispatch

                            // その他の管理者メニュー項目
                            Html.li
                                [ prop.className "mt-4 border-t pt-4 px-6 text-sm text-gray-600"
                                  prop.text "システム" ]
                            renderSidebarItem model.CurrentRoute Route.Products "🔍" "製品一覧に戻る" dispatch
                            renderSidebarItem model.CurrentRoute Route.Home "🏠" "ホームに戻る" dispatch ] ] ] ]

// ダッシュボード概要パネルのレンダリング
let renderDashboardPanel (title: string) (value: string) (description: string) (icon: string) (color: string) =
    Html.div
        [ prop.className "bg-white rounded-lg shadow-md p-6"
          prop.children
              [ Html.div
                    [ prop.className "flex items-center"
                      prop.children
                          [ Html.div
                                [ prop.className $"rounded-full {color} p-3 mr-4"
                                  prop.children [ Html.span [ prop.className "text-2xl"; prop.text icon ] ] ]
                            Html.div
                                [ prop.children
                                      [ Html.h3 [ prop.className "text-gray-500 text-sm"; prop.text title ]
                                        Html.p [ prop.className "text-2xl font-bold"; prop.text value ]
                                        Html.p [ prop.className "text-gray-500 text-sm mt-1"; prop.text description ] ] ] ] ] ] ]

// 管理者ダッシュボードのレンダリング
let renderAdminDashboard (model: Model) (dispatch: Msg -> unit) =
    // モデルからデータを取得
    let productsData =
        match model.ApiData.ProductData.Products with
        | Success products -> products
        | _ -> []

    let usersData =
        match model.ApiData.UserData.Users with
        | Success users -> users
        | _ -> []

    // 製品とユーザーの総数
    let totalProducts = List.length productsData
    let totalUsers = List.length usersData

    // 在庫切れの製品数
    let outOfStockProducts =
        productsData |> List.filter (fun p -> p.Stock = 0) |> List.length

    // アクティブな製品
    let activeProducts =
        productsData |> List.filter (fun p -> p.IsActive) |> List.length

    Html.div
        [ prop.children
              [ Html.h1 [ prop.className "text-2xl font-bold mb-6"; prop.text "管理者ダッシュボード" ]

                // データ概要パネル
                Html.div
                    [ prop.className "grid grid-cols-1 md:grid-cols-2 lg:grid-cols-4 gap-4 mb-8"
                      prop.children
                          [ renderDashboardPanel
                                "総製品数"
                                (string totalProducts)
                                "登録されている全製品"
                                "📦"
                                "bg-blue-100 text-blue-800"
                            renderDashboardPanel
                                "総ユーザー数"
                                (string totalUsers)
                                "登録ユーザー"
                                "👥"
                                "bg-green-100 text-green-800"
                            renderDashboardPanel
                                "在庫切れ"
                                (string outOfStockProducts)
                                "在庫がない製品"
                                "⚠️"
                                "bg-yellow-100 text-yellow-800"
                            renderDashboardPanel
                                "有効製品"
                                (string activeProducts)
                                "アクティブな製品"
                                "✅"
                                "bg-purple-100 text-purple-800" ] ]

                // 最近の製品/アクティビティ
                Html.div
                    [ prop.className "grid grid-cols-1 lg:grid-cols-2 gap-6"
                      prop.children
                          [
                            // 最近の製品
                            Html.div
                                [ prop.className "bg-white rounded-lg shadow-md p-6"
                                  prop.children
                                      [ Html.h2 [ prop.className "text-lg font-bold mb-4"; prop.text "最近の製品" ]

                                        if List.isEmpty productsData then
                                            Html.p [ prop.className "text-gray-500 italic"; prop.text "製品データがありません" ]
                                        else
                                            Html.table
                                                [ prop.className "min-w-full"
                                                  prop.children
                                                      [ Html.thead
                                                            [ prop.children
                                                                  [ Html.tr
                                                                        [ prop.children
                                                                              [ Html.th
                                                                                    [ prop.className "text-left pb-2"
                                                                                      prop.text "名前" ]
                                                                                Html.th
                                                                                    [ prop.className "text-left pb-2"
                                                                                      prop.text "価格" ]
                                                                                Html.th
                                                                                    [ prop.className "text-left pb-2"
                                                                                      prop.text "在庫" ] ] ] ] ]
                                                        Html.tbody
                                                            [ prop.children
                                                                  [ for product in List.truncate 5 productsData ->
                                                                        Html.tr
                                                                            [ prop.children
                                                                                  [ Html.td
                                                                                        [ prop.className "py-2"
                                                                                          prop.text product.Name ]
                                                                                    Html.td
                                                                                        [ prop.className "py-2"
                                                                                          prop.text (
                                                                                              sprintf
                                                                                                  "¥%.0f"
                                                                                                  product.Price
                                                                                          ) ]
                                                                                    Html.td
                                                                                        [ prop.className (
                                                                                              if product.Stock = 0 then
                                                                                                  "py-2 text-red-600"
                                                                                              else
                                                                                                  "py-2"
                                                                                          )
                                                                                          prop.text (
                                                                                              string product.Stock
                                                                                          ) ] ] ] ] ] ] ]

                                        Html.div
                                            [ prop.className "mt-4 text-right"
                                              prop.children
                                                  [ Html.button
                                                        [ prop.className "text-blue-600 hover:text-blue-800"
                                                          prop.text "すべての製品を表示"
                                                          prop.onClick (fun _ ->
                                                              dispatch (RouteChanged Route.AdminProducts)) ] ] ] ] ]

                            // システム情報/ステータス
                            Html.div
                                [ prop.className "bg-white rounded-lg shadow-md p-6"
                                  prop.children
                                      [ Html.h2 [ prop.className "text-lg font-bold mb-4"; prop.text "システム情報" ]

                                        Html.div
                                            [ prop.className "space-y-4"
                                              prop.children
                                                  [ Html.div
                                                        [ prop.className "flex justify-between"
                                                          prop.children
                                                              [ Html.span [ prop.text "読み込まれたプラグイン" ]
                                                                Html.span
                                                                    [ prop.className "font-medium"
                                                                      prop.text (
                                                                          // PluginSystemモジュールから直接登録済みプラグイン数を取得
                                                                          let pluginCount =
                                                                              App.Plugins.getRegisteredPluginIds ()
                                                                              |> List.length

                                                                          string pluginCount
                                                                      ) ] ] ]

                                                    Html.div
                                                        [ prop.className "flex justify-between"
                                                          prop.children
                                                              [ Html.span [ prop.text "アプリケーションバージョン" ]
                                                                Html.span
                                                                    [ prop.className "font-medium"; prop.text "1.0.0" ] ] ]

                                                    Html.div
                                                        [ prop.className "flex justify-between"
                                                          prop.children
                                                              [ Html.span [ prop.text "最終データ更新" ]
                                                                Html.span
                                                                    [ prop.className "font-medium"
                                                                      prop.text (
                                                                          System.DateTime.Now.ToString(
                                                                              "yyyy-MM-dd HH:mm:ss"
                                                                          )
                                                                      ) ] ] ]

                                                    // データエクスポート機能
                                                    Html.div
                                                        [ prop.className "mt-6 pt-4 border-t"
                                                          prop.children
                                                              [ Html.button
                                                                    [ prop.className
                                                                          "px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 mr-2"
                                                                      prop.text "製品データエクスポート"
                                                                      prop.onClick (fun _ ->
                                                                          // エクスポート機能は将来的に実装
                                                                          dispatch (
                                                                              NotificationMsg(
                                                                                  Add(
                                                                                      Notifications.info
                                                                                          "エクスポート機能は準備中です"
                                                                                      |> Notifications.fromSource
                                                                                          "Admin"
                                                                                  )
                                                                              )
                                                                          )) ]

                                                                Html.button
                                                                    [ prop.className
                                                                          "px-4 py-2 bg-green-500 text-white rounded hover:bg-green-600"
                                                                      prop.text "システム診断"
                                                                      prop.onClick (fun _ ->
                                                                          // 診断機能は将来的に実装
                                                                          dispatch (
                                                                              NotificationMsg(
                                                                                  Add(
                                                                                      Notifications.info
                                                                                          "システム診断機能は準備中です"
                                                                                      |> Notifications.fromSource
                                                                                          "Admin"
                                                                                  )
                                                                              )
                                                                          )) ] ] ] ] ] ] ] ] ] ] ]

// ユーザー管理画面のレンダリング
let renderUserManagement (model: Model) (dispatch: Msg -> unit) =
    let users =
        match model.ApiData.UserData.Users with
        | Success users -> users
        | Loading -> []
        | Failed _ -> []
        | NotStarted -> []

    let isLoading =
        match model.ApiData.UserData.Users with
        | Loading -> true
        | _ -> false

    Html.div
        [ prop.children
              [ Html.div
                    [ prop.className "flex justify-between items-center mb-6"
                      prop.children
                          [ Html.h1 [ prop.className "text-2xl font-bold"; prop.text "ユーザー管理" ]

                            Html.button
                                [ prop.className "px-4 py-2 bg-green-500 text-white rounded hover:bg-green-600"
                                  prop.text "新規ユーザー"
                                  prop.onClick (fun _ ->
                                      // 新規ユーザー作成機能は将来的に実装
                                      dispatch (
                                          NotificationMsg(
                                              Add(
                                                  Notifications.info "ユーザー作成機能は準備中です"
                                                  |> Notifications.fromSource "Admin"
                                              )
                                          )
                                      )) ] ] ]

                // ユーザーテーブル
                Html.div
                    [ prop.className "bg-white rounded-lg shadow-md overflow-hidden"
                      prop.children
                          [ if isLoading then
                                Html.div
                                    [ prop.className "p-8 text-center"
                                      prop.children
                                          [ Html.p [ prop.className "text-gray-500"; prop.text "ユーザーデータを読み込み中..." ] ] ]
                            elif List.isEmpty users then
                                Html.div
                                    [ prop.className "p-8 text-center"
                                      prop.children
                                          [ Html.p [ prop.className "text-gray-500"; prop.text "ユーザーがいません" ]
                                            Html.button
                                                [ prop.className
                                                      "mt-4 px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600"
                                                  prop.text "ユーザーデータ読み込み"
                                                  prop.onClick (fun _ -> dispatch (ApiMsg(UserApi FetchUsers))) ] ] ]
                            else
                                Html.table
                                    [ prop.className "min-w-full divide-y divide-gray-200"
                                      prop.children
                                          [ Html.thead
                                                [ prop.className "bg-gray-50"
                                                  prop.children
                                                      [ Html.tr
                                                            [ prop.children
                                                                  [ Html.th
                                                                        [ prop.className
                                                                              "px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                                                                          prop.text "ID" ]
                                                                    Html.th
                                                                        [ prop.className
                                                                              "px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                                                                          prop.text "ユーザー名" ]
                                                                    Html.th
                                                                        [ prop.className
                                                                              "px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                                                                          prop.text "メールアドレス" ]
                                                                    Html.th
                                                                        [ prop.className
                                                                              "px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                                                                          prop.text "アクション" ] ] ] ] ]
                                            Html.tbody
                                                [ prop.className "bg-white divide-y divide-gray-200"
                                                  prop.children
                                                      [ for user in users ->
                                                            Html.tr
                                                                [ prop.children
                                                                      [ Html.td
                                                                            [ prop.className
                                                                                  "px-6 py-4 whitespace-nowrap text-sm text-gray-500"
                                                                              prop.text (string user.Id) ]
                                                                        Html.td
                                                                            [ prop.className
                                                                                  "px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900"
                                                                              prop.text user.Username ]
                                                                        Html.td
                                                                            [ prop.className
                                                                                  "px-6 py-4 whitespace-nowrap text-sm text-gray-500"
                                                                              prop.text user.Email ]
                                                                        Html.td
                                                                            [ prop.className
                                                                                  "px-6 py-4 whitespace-nowrap text-sm text-gray-500"
                                                                              prop.children
                                                                                  [ Html.button
                                                                                        [ prop.className
                                                                                              "text-blue-600 hover:text-blue-900 mr-3"
                                                                                          prop.text "編集"
                                                                                          prop.onClick (fun _ ->
                                                                                              // 編集機能は将来的に実装
                                                                                              dispatch (
                                                                                                  NotificationMsg(
                                                                                                      Add(
                                                                                                          Notifications.info
                                                                                                              "ユーザー編集機能は準備中です"
                                                                                                          |> Notifications.fromSource
                                                                                                              "Admin"
                                                                                                      )
                                                                                                  )
                                                                                              )) ]
                                                                                    Html.button
                                                                                        [ prop.className
                                                                                              "text-red-600 hover:text-red-900"
                                                                                          prop.text "削除"
                                                                                          prop.onClick (fun _ ->
                                                                                              // 削除機能は将来的に実装
                                                                                              dispatch (
                                                                                                  NotificationMsg(
                                                                                                      Add(
                                                                                                          Notifications.info
                                                                                                              "ユーザー削除機能は準備中です"
                                                                                                          |> Notifications.fromSource
                                                                                                              "Admin"
                                                                                                      )
                                                                                                  )
                                                                                              )) ] ] ] ] ] ] ] ] ]

                                // データが空の場合はユーザーデータ読み込みボタンを表示
                                if List.isEmpty users then
                                    Html.div
                                        [ prop.className "p-6 text-center"
                                          prop.children
                                              [ Html.button
                                                    [ prop.className
                                                          "px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600"
                                                      prop.text "データを読み込む"
                                                      prop.onClick (fun _ -> dispatch (ApiMsg(UserApi FetchUsers))) ] ] ] ] ] ] ]

// 製品管理画面のレンダリング
let renderProductManagement (model: Model) (dispatch: Msg -> unit) =
    let products =
        match model.ApiData.ProductData.Products with
        | Success products -> products
        | _ -> []

    let isLoading =
        match model.ApiData.ProductData.Products with
        | Loading -> true
        | _ -> false

    Html.div
        [ prop.children
              [ Html.div
                    [ prop.className "flex justify-between items-center mb-6"
                      prop.children
                          [ Html.h1 [ prop.className "text-2xl font-bold"; prop.text "製品管理" ]

                            Html.button
                                [ prop.className "px-4 py-2 bg-green-500 text-white rounded hover:bg-green-600"
                                  prop.text "新規製品"
                                  prop.onClick (fun _ ->
                                      // 新規製品作成機能は将来的に実装
                                      dispatch (
                                          NotificationMsg(
                                              Add(
                                                  Notifications.info "製品作成機能は準備中です" |> Notifications.fromSource "Admin"
                                              )
                                          )
                                      )) ] ] ]

                // 製品テーブル
                Html.div
                    [ prop.className "bg-white rounded-lg shadow-md overflow-hidden"
                      prop.children
                          [ if isLoading then
                                Html.div
                                    [ prop.className "p-8 text-center"
                                      prop.children
                                          [ Html.p [ prop.className "text-gray-500"; prop.text "製品データを読み込み中..." ] ] ]
                            elif List.isEmpty products then
                                Html.div
                                    [ prop.className "p-8 text-center"
                                      prop.children
                                          [ Html.p [ prop.className "text-gray-500"; prop.text "製品がありません" ]
                                            Html.button
                                                [ prop.className
                                                      "mt-4 px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600"
                                                  prop.text "製品データ読み込み"
                                                  prop.onClick (fun _ -> dispatch (ApiMsg(ProductApi FetchProducts))) ] ] ]
                            else
                                Html.table
                                    [ prop.className "min-w-full divide-y divide-gray-200"
                                      prop.children
                                          [ Html.thead
                                                [ prop.className "bg-gray-50"
                                                  prop.children
                                                      [ Html.tr
                                                            [ prop.children
                                                                  [ Html.th
                                                                        [ prop.className
                                                                              "px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                                                                          prop.text "ID" ]
                                                                    Html.th
                                                                        [ prop.className
                                                                              "px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                                                                          prop.text "製品名" ]
                                                                    Html.th
                                                                        [ prop.className
                                                                              "px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                                                                          prop.text "価格" ]
                                                                    Html.th
                                                                        [ prop.className
                                                                              "px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                                                                          prop.text "在庫" ]
                                                                    Html.th
                                                                        [ prop.className
                                                                              "px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                                                                          prop.text "状態" ]
                                                                    Html.th
                                                                        [ prop.className
                                                                              "px-6 py-3 text-left text-xs font-medium text-gray-500 uppercase tracking-wider"
                                                                          prop.text "アクション" ] ] ] ] ]
                                            Html.tbody
                                                [ prop.className "bg-white divide-y divide-gray-200"
                                                  prop.children
                                                      [ for product in products ->
                                                            Html.tr
                                                                [ prop.children
                                                                      [ Html.td
                                                                            [ prop.className
                                                                                  "px-6 py-4 whitespace-nowrap text-sm text-gray-500"
                                                                              prop.text (string product.Id) ]
                                                                        Html.td
                                                                            [ prop.className
                                                                                  "px-6 py-4 whitespace-nowrap text-sm font-medium text-gray-900"
                                                                              prop.text product.Name ]
                                                                        Html.td
                                                                            [ prop.className
                                                                                  "px-6 py-4 whitespace-nowrap text-sm text-gray-500"
                                                                              prop.text (sprintf "¥%.0f" product.Price) ]
                                                                        Html.td
                                                                            [ prop.className (
                                                                                  if product.Stock = 0 then
                                                                                      "px-6 py-4 whitespace-nowrap text-sm font-medium text-red-600"
                                                                                  else
                                                                                      "px-6 py-4 whitespace-nowrap text-sm text-gray-500"
                                                                              )
                                                                              prop.text (string product.Stock) ]
                                                                        Html.td
                                                                            [ prop.className
                                                                                  "px-6 py-4 whitespace-nowrap text-sm text-gray-500"
                                                                              prop.children
                                                                                  [ Html.span
                                                                                        [ prop.className (
                                                                                              if product.IsActive then
                                                                                                  "px-2 inline-flex text-xs leading-5 font-semibold rounded-full bg-green-100 text-green-800"
                                                                                              else
                                                                                                  "px-2 inline-flex text-xs leading-5 font-semibold rounded-full bg-red-100 text-red-800"
                                                                                          )
                                                                                          prop.text (
                                                                                              if product.IsActive then
                                                                                                  "有効"
                                                                                              else
                                                                                                  "無効"
                                                                                          ) ] ] ]
                                                                        Html.td
                                                                            [ prop.className
                                                                                  "px-6 py-4 whitespace-nowrap text-sm text-gray-500"
                                                                              prop.children
                                                                                  [ Html.button
                                                                                        [ prop.className
                                                                                              "text-blue-600 hover:text-blue-900 mr-3"
                                                                                          prop.text "編集"
                                                                                          prop.onClick (fun _ ->
                                                                                              // 製品詳細ページに移動
                                                                                              dispatch (
                                                                                                  ProductsMsg(
                                                                                                      ViewProductDetails
                                                                                                          product.Id
                                                                                                  )
                                                                                              )) ]
                                                                                    Html.button
                                                                                        [ prop.className
                                                                                              "text-red-600 hover:text-red-900"
                                                                                          prop.text "削除"
                                                                                          prop.onClick (fun _ ->
                                                                                              // 確認ダイアログを表示して削除
                                                                                              if
                                                                                                  Browser
                                                                                                      .Dom
                                                                                                      .window
                                                                                                      .confirm (
                                                                                                          sprintf
                                                                                                              "製品「%s」を削除してもよろしいですか？"
                                                                                                              product.Name
                                                                                                      )
                                                                                              then
                                                                                                  dispatch (
                                                                                                      ApiMsg(
                                                                                                          ProductApi(
                                                                                                              DeleteProduct(
                                                                                                                  int64
                                                                                                                      product.Id
                                                                                                              )
                                                                                                          )
                                                                                                      )
                                                                                                  )) ] ] ] ] ] ] ] ] ] ] ] ] ]

// 管理者画面全体のレイアウト
let renderAdminLayout (model: Model) (content: Model -> (Msg -> unit) -> Feliz.ReactElement) (dispatch: Msg -> unit) =
    Html.div
        [ prop.className "flex flex-col min-h-screen"
          prop.children
              [
                // メインコンテンツエリア
                Html.div
                    [ prop.className "flex-grow flex"
                      prop.children
                          [
                            // サイドバー
                            Html.div
                                [ prop.className "w-64 flex-shrink-0 hidden md:block"
                                  prop.children [ renderAdminSidebar model dispatch ] ]

                            // コンテンツエリア
                            Html.div [ prop.className "flex-grow p-6"; prop.children [ content model dispatch ] ] ] ] ] ]

// 管理者画面全体のレンダリング - ルートに基づいてコンテンツを切り替え
let renderAdmin (model: Model) (dispatch: Msg -> unit) =
    // 管理者データをロード（まだデータがなければ）
    let cmd =
        match model.ApiData.UserData.Users, model.ApiData.ProductData.Products with
        | NotStarted, _ -> dispatch (ApiMsg(UserApi FetchUsers))
        | _, NotStarted -> dispatch (ApiMsg(ProductApi FetchProducts))
        | _ -> ()

    // ルートに基づいて適切なコンテンツを表示
    match model.CurrentRoute with
    | Route.Admin -> renderAdminLayout model renderAdminDashboard dispatch
    | Route.AdminUsers -> renderAdminLayout model renderUserManagement dispatch
    | Route.AdminProducts -> renderAdminLayout model renderProductManagement dispatch
    | _ ->
        // デフォルトはダッシュボード
        renderAdminLayout model renderAdminDashboard dispatch
