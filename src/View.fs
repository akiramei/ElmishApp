// View.fs
module App.View

open Feliz
open Elmish
open App.Types
open App.Plugins
open Fable.Core.JsInterop

// タブのレンダリング
let renderTabs (model: Model) (dispatch: Msg -> unit) =
    let tabs = [
        Home, "Home"
        Counter, "Counter"
    ]
    
    // カスタムタブを取得して追加
    let customTabs = getAvailableCustomTabs()
                     |> List.map (fun customTab -> 
                         match customTab with
                         | CustomTab id -> customTab, id
                         | _ -> customTab, "")
    
    let allTabs = tabs @ customTabs
    
    Html.div [
        prop.className "tabs"
        prop.children [
            for (tab, label) in allTabs do
                Html.button [
                    prop.text label
                    prop.className (if model.CurrentTab = tab then "active-tab" else "tab")
                    prop.onClick (fun _ -> dispatch (NavigateTo tab))
                ]
        ]
    ]

// ホームタブの内容
let renderHome (model: Model) =
    Html.div [
        prop.className "home-container"
        prop.children [
            Html.h1 [
                prop.text "Home"
            ]
            Html.p [
                prop.text model.Message
            ]
        ]
    ]

// カウンタータブの内容
let renderCounter (model: Model) (dispatch: Msg -> unit) =
    Html.div [
        prop.className "counter-container"
        prop.children [
            Html.h1 [
                prop.text "Counter"
            ]
            Html.div [
                prop.className "counter-value"
                prop.children [
                    Html.span [
                        prop.text (sprintf "Current value: %d" model.Counter)
                    ]
                ]
            ]
            Html.div [
                prop.className "counter-buttons"
                prop.children [
                    Html.button [
                        prop.text "+"
                        prop.onClick (fun _ -> dispatch IncrementCounter)
                    ]
                    Html.button [
                        prop.text "-"
                        prop.onClick (fun _ -> dispatch DecrementCounter)
                    ]
                ]
            ]
            
            // カスタムビューの追加ポイント
            match getCustomView "counter-extensions" (toPlainJsObj model) with
            | Some customElement -> customElement
            | None -> Html.none
        ]
    ]

// カスタムタブの内容を取得
let renderCustomTab (tabId: string) (model: Model) (dispatch: Msg -> unit) =
    match getCustomView tabId (toPlainJsObj model) with
    | Some customElement -> 
        customElement
    | None ->
        Html.div [
            prop.className "error-container"
            prop.children [
                Html.h1 [
                    prop.text "Custom Tab Error"
                ]
                Html.p [
                    prop.text (sprintf "Custom view for tab '%s' not found" tabId)
                ]
                Html.button [
                    prop.text "Go to Home"
                    prop.onClick (fun _ -> dispatch (NavigateTo Home))
                ]
            ]
        ]

// エラー表示
let renderError (model: Model) (dispatch: Msg -> unit) =
    if model.ErrorState.HasError then
        let source = 
            match model.ErrorState.Source with
            | Some src -> sprintf " [Source: %s]" src
            | None -> ""
            
        Html.div [
            prop.className "error-message"
            prop.children [
                Html.div [
                    prop.className "error-content"
                    prop.children [
                        Html.span [
                            prop.className "error-text"
                            prop.text (Option.defaultValue "An error occurred" model.ErrorState.Message)
                        ]
                        Html.span [
                            prop.className "error-source"
                            prop.text source
                        ]
                    ]
                ]
                Html.button [
                    prop.className "error-dismiss"
                    prop.text "Dismiss"
                    prop.onClick (fun _ -> 
                        // メッセージをディスパッチしてエラーをクリア
                        dispatch ClearError
                    )
                ]
            ]
        ]
    else
        Html.none

// メインビュー
let view (model: Model) (dispatch: Msg -> unit) =
    Html.div [
        prop.className "app-container"
        prop.children [
            renderTabs model dispatch
            renderError model dispatch
            
            Html.div [
                prop.className "tab-content"
                prop.children [
                    match model.CurrentTab with
                    | Home -> renderHome model
                    | Counter -> renderCounter model dispatch
                    | CustomTab id -> renderCustomTab id model dispatch
                ]
            ]
        ]
    ]