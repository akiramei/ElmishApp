// NotificationView.fs - Improved with UI components
module App.NotificationView

open Feliz
open App.Types
open App.UI.Components.Common.Status
open App.UI.Theme.Icons

// 単一の通知を表示
let renderNotification (notification: Notification) (dispatch: Msg -> unit) =
    let (color, bgColor, borderColor, icon) =
        match notification.Level with
        | Information -> ("text-blue-700", "bg-blue-50", "border-blue-300", info)
        | Warning -> ("text-yellow-700", "bg-yellow-50", "border-yellow-300", warning)
        | Error -> ("text-red-700", "bg-red-50", "border-red-300", error)

    let source =
        match notification.Source with
        | Some src -> [ Html.span [ prop.className $"ml-2 {color} text-sm"; prop.text $"[Source: {src}]" ] ]
        | None -> []

    let details =
        match notification.Details with
        | Some detailText -> [ Html.div [ prop.className $"{color} mt-1 text-sm"; prop.text detailText ] ]
        | None -> []

    Html.div
        [ prop.key (string notification.Id)
          prop.className $"{bgColor} border {borderColor} rounded-md p-4 flex items-center justify-between"
          prop.children
              [ Html.div
                    [ prop.className "flex-grow"
                      prop.children
                          [ Html.div
                                [ prop.className "flex items-center"
                                  prop.children (
                                      [ Html.span [ prop.className "mr-2"; prop.text icon ]
                                        Html.span
                                            [ prop.className $"font-medium {color}"; prop.text notification.Message ] ]
                                      @ source
                                  ) ]
                            yield! details ] ]
                Html.button
                    [ prop.className
                          $"ml-4 px-2 py-1 hover:{bgColor} {color} rounded hover:bg-opacity-80 transition-colors text-sm"
                      prop.text "閉じる"
                      prop.onClick (fun _ -> dispatch (NotificationMsg(Remove notification.Id))) ] ] ]

// 通知コンテナ - すべての通知を表示
let renderNotifications (model: Model) (dispatch: Msg -> unit) =
    let notifications = model.NotificationState.Notifications

    if List.isEmpty notifications then
        Html.none
    else
        Html.div
            [ prop.className "notifications-container space-y-2 mb-5"
              prop.children
                  [ for notification in notifications do
                        renderNotification notification dispatch ] ]

// すべての通知を一度に消去するボタン (複数の通知がある場合のみ表示)
let renderClearAllButton (notifications: Notification list) (dispatch: Msg -> unit) =
    if List.length notifications > 1 then
        Html.div
            [ prop.className "flex justify-end mb-2"
              prop.children
                  [ Html.button
                        [ prop.className "text-gray-500 text-sm hover:text-gray-700"
                          prop.onClick (fun _ -> dispatch (NotificationMsg ClearAll))
                          prop.children
                              [ Html.span [ prop.className "mr-1"; prop.text close ]
                                Html.span [ prop.text "すべて消去" ] ] ] ] ]
    else
        Html.none

// 通知エリア全体 - ヘッダーとクリアボタンを含む
let renderNotificationArea (model: Model) (dispatch: Msg -> unit) =
    let notifications = model.NotificationState.Notifications

    if List.isEmpty notifications then
        Html.none
    else
        Html.div
            [ prop.className "mb-5"
              prop.children
                  [ renderClearAllButton notifications dispatch
                    renderNotifications model dispatch ] ]
