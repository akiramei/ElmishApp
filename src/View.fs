// View.fs
module App.View

open Feliz
open App.Types
open App.Interop
open App.TabPluginDecorator

// タブのレンダリング
let renderTabs (model: Model) (dispatch: Msg -> unit) =
    let tabs = [ Home, "Home"; Counter, "Counter" ]

    // カスタムタブを取得して追加
    let customTabs =
        getAvailableCustomTabs ()
        |> List.map (fun customTab ->
            match customTab with
            | CustomTab id -> customTab, id
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
                          prop.onClick (fun _ -> dispatch (NavigateTo Home)) ] ] ]

// 通知メッセージ表示 (旧renderError関数を改良)
let renderNotification (model: Model) (dispatch: Msg -> unit) =
    if model.NotificationState.HasNotification then
        // 通知レベルに基づいてスタイルを選択
        let (color, bgColor, borderColor, icon) =
            match model.NotificationState.Level with
            | Some Information -> ("text-blue-700", "bg-blue-50", "border-blue-300", "ℹ️")
            | Some Warning -> ("text-yellow-700", "bg-yellow-50", "border-yellow-300", "⚠️")
            | Some Error -> ("text-red-700", "bg-red-50", "border-red-300", "❌")
            | None -> ("text-gray-700", "bg-gray-50", "border-gray-300", "")

        // 通知のソース情報
        let source =
            match model.NotificationState.Source with
            | Some src -> sprintf " [Source: %s]" src
            | None -> ""

        // 通知表示
        Html.div
            [ prop.className $"mb-5 {bgColor} border {borderColor} rounded-md p-4 flex items-center justify-between"
              prop.children
                  [ Html.div
                        [ prop.className "flex-grow flex items-center"
                          prop.children
                              [ // アイコン表示
                                Html.span [ prop.className "mr-2"; prop.text icon ]
                                // メッセージ表示
                                Html.div
                                    [ prop.className "flex-grow"
                                      prop.children
                                          [ Html.span
                                                [ prop.className $"font-medium {color}"
                                                  prop.text (Option.defaultValue "通知" model.NotificationState.Message) ]
                                            Html.span [ prop.className $"ml-2 {color} text-sm"; prop.text source ] ] ] ] ]
                    // 閉じるボタン
                    Html.button
                        [ prop.className
                              $"ml-4 px-2 py-1 hover:{bgColor} {color} rounded hover:bg-opacity-80 transition-colors text-sm"
                          prop.text "閉じる"
                          prop.onClick (fun _ -> dispatch ClearNotification) ] ] ]
    else
        Html.none

// メインビュー
let view (model: Model) (dispatch: Msg -> unit) =
    Html.div
        [ prop.className "max-w-4xl mx-auto p-5 bg-white shadow-md min-h-screen"
          prop.children
              [ renderTabs model dispatch
                renderNotification model dispatch
                Html.div
                    [ prop.className "bg-white rounded-md"
                      prop.children
                          [ match model.CurrentTab with
                            | Home -> renderHome model
                            | Counter -> renderCounter model dispatch
                            | CustomTab id -> renderCustomTab id model dispatch ] ] ] ]
