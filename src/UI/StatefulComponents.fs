// UI/StatefulComponents.fs
module App.UI.StatefulComponents

open Feliz

// 折りたたみ可能なパネル - UIの開閉状態のみをローカル管理
[<ReactComponent>]
let CollapsiblePanel (title: string) (children: ReactElement list) =
    // ここのuseStateは純粋にUIの表示制御のみ
    let (isOpen, setIsOpen) = React.useState true

    Html.div
        [ prop.className "border rounded-md mb-4"
          prop.children
              [ Html.div
                    [ prop.className "flex justify-between items-center p-3 cursor-pointer bg-gray-50"
                      prop.onClick (fun _ -> setIsOpen (not isOpen))
                      prop.children
                          [ Html.h3 [ prop.className "font-medium"; prop.text title ]
                            Html.span [ prop.className "text-lg"; prop.text (if isOpen then "▼" else "►") ] ] ]
                if isOpen then
                    Html.div [ prop.className "p-4 border-t"; prop.children children ] ] ]

// タブパネル - UIタブ選択状態のみをローカル管理
[<ReactComponent>]
let TabPanel (tabs: (string * ReactElement) list) (defaultTab: string option) =
    // アクティブタブの管理は純粋なUI表示制御
    let initialTab = defaultArg defaultTab (fst (List.head tabs))
    let (activeTab, setActiveTab) = React.useState initialTab

    Html.div
        [ prop.children
              [ Html.div
                    [ prop.className "flex border-b"
                      prop.children
                          [ for (tabName, _) in tabs ->
                                Html.button
                                    [ prop.className (
                                          if tabName = activeTab then
                                              "px-4 py-2 border-b-2 border-blue-500 text-blue-600 font-medium"
                                          else
                                              "px-4 py-2 text-gray-600 hover:text-gray-800"
                                      )
                                      prop.text tabName
                                      prop.onClick (fun _ -> setActiveTab tabName) ] ] ]
                Html.div
                    [ prop.className "p-4"
                      prop.children
                          [ let (_, content) = tabs |> List.find (fun (name, _) -> name = activeTab)
                            content ] ] ] ]

// モーダルダイアログ - 表示状態のみローカル管理
[<ReactComponent>]
let Modal (isOpen: bool) (onClose: unit -> unit) (title: string) (children: ReactElement list) =
    if not isOpen then
        Html.none
    else
        Html.div
            [ prop.className "fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50"
              prop.onClick (fun _ -> onClose ())
              prop.children
                  [ Html.div
                        [ prop.className "bg-white rounded-lg shadow-xl p-6 max-w-lg w-full max-h-[90vh] overflow-auto"
                          prop.onClick (fun e -> e.stopPropagation ())
                          prop.children
                              [ Html.div
                                    [ prop.className "flex justify-between items-center mb-4"
                                      prop.children
                                          [ Html.h3 [ prop.className "text-lg font-medium"; prop.text title ]
                                            Html.button
                                                [ prop.className "text-gray-400 hover:text-gray-600"
                                                  prop.onClick (fun _ -> onClose ())
                                                  prop.children
                                                      [ Html.span [ prop.className "text-xl"; prop.text "×" ] ] ] ] ]
                                yield! children ] ] ] ]

// 確認ダイアログ
[<ReactComponent>]
let ConfirmDialog (isOpen: bool) (onConfirm: unit -> unit) (onCancel: unit -> unit) (title: string) (message: string) =
    if not isOpen then
        Html.none
    else
        Html.div
            [ prop.className "fixed inset-0 bg-black bg-opacity-50 flex items-center justify-center z-50"
              prop.onClick (fun _ -> onCancel ())
              prop.children
                  [ Html.div
                        [ prop.className "bg-white rounded-lg shadow-xl p-6 max-w-md w-full"
                          prop.onClick (fun e -> e.stopPropagation ())
                          prop.children
                              [ Html.div
                                    [ prop.className "flex justify-between items-center mb-4"
                                      prop.children
                                          [ Html.h3 [ prop.className "text-lg font-medium"; prop.text title ]
                                            Html.button
                                                [ prop.className "text-gray-400 hover:text-gray-600"
                                                  prop.onClick (fun _ -> onCancel ())
                                                  prop.children
                                                      [ Html.span [ prop.className "text-xl"; prop.text "×" ] ] ] ] ]
                                Html.p [ prop.className "mb-6"; prop.text message ]
                                Html.div
                                    [ prop.className "flex justify-end space-x-3"
                                      prop.children
                                          [ Html.button
                                                [ prop.className
                                                      "px-4 py-2 border rounded text-gray-700 hover:bg-gray-50"
                                                  prop.text "キャンセル"
                                                  prop.onClick (fun _ -> onCancel ()) ]
                                            Html.button
                                                [ prop.className
                                                      "px-4 py-2 bg-red-500 text-white rounded hover:bg-red-600"
                                                  prop.text "確認"
                                                  prop.onClick (fun _ -> onConfirm ()) ] ] ] ] ] ] ]

// ドロップダウンメニュー - 開閉状態のみローカル管理
[<ReactComponent>]
let Dropdown (trigger: ReactElement) (items: (string * (unit -> unit)) list) =
    let (isOpen, setIsOpen) = React.useState false

    // クリック以外で閉じる処理
    React.useEffect (
        (fun () ->
            let handleClickOutside = fun _ -> setIsOpen false
            Browser.Dom.document.addEventListener ("click", handleClickOutside)

            { new System.IDisposable with
                member _.Dispose() =
                    Browser.Dom.document.removeEventListener ("click", handleClickOutside) }),
        [||]
    )

    Html.div
        [ prop.className "relative inline-block"
          prop.children
              [ Html.div
                    [ prop.onClick (fun e ->
                          e.stopPropagation ()
                          setIsOpen (not isOpen))
                      prop.children [ trigger ] ]

                if isOpen then
                    Html.div
                        [ prop.className "absolute right-0 mt-2 w-48 bg-white rounded-md shadow-lg z-10"
                          prop.onClick (fun e -> e.stopPropagation ())
                          prop.children
                              [ Html.div
                                    [ prop.className "py-1"
                                      prop.children
                                          [ for (label, onClick) in items ->
                                                Html.a
                                                    [ prop.className
                                                          "block px-4 py-2 text-gray-700 hover:bg-gray-100 cursor-pointer"
                                                      prop.text label
                                                      prop.onClick (fun e ->
                                                          e.stopPropagation ()
                                                          setIsOpen false
                                                          onClick ()) ] ] ] ] ] ] ]

// トースト通知 - 表示とアニメーション状態のみローカル管理
[<ReactComponent>]
let Toast (message: string) (type': string) (onClose: unit -> unit) (autoClose: bool) =
    let (isVisible, setIsVisible) = React.useState true

    // 自動クローズのタイマー設定
    React.useEffect (
        fun () ->
            if autoClose then
                // 5秒後に非表示にするタイマー
                let timeoutId =
                    Browser.Dom.window.setTimeout (
                        (fun () ->
                            setIsVisible false
                            // さらに0.3秒後にonCloseを実行
                            Browser.Dom.window.setTimeout ((fun () -> onClose ()), 300) |> ignore),
                        5000
                    )

                // クリーンアップ時にタイマーをクリア
                { new System.IDisposable with
                    member _.Dispose() =
                        Browser.Dom.window.clearTimeout timeoutId }
            else
                { new System.IDisposable with
                    member _.Dispose() = () }

        , [| box autoClose |]
    )

    // 背景色の設定
    let backgroundColor =
        match type' with
        | "success" -> "bg-green-500"
        | "error" -> "bg-red-500"
        | "warning" -> "bg-yellow-500"
        | _ -> "bg-blue-500"

    // CSSトランジション用のクラス
    let visibilityClass =
        if isVisible then
            "opacity-100 translate-y-0"
        else
            "opacity-0 translate-y-2"

    Html.div
        [ prop.className
              $"fixed bottom-4 right-4 {backgroundColor} text-white rounded-lg p-4 shadow-lg transition-all duration-300 {visibilityClass}"
          prop.children
              [ Html.div
                    [ prop.className "flex justify-between items-center"
                      prop.children
                          [ Html.span [ prop.text message ]
                            Html.button
                                [ prop.className "ml-4 text-white opacity-70 hover:opacity-100"
                                  prop.onClick (fun _ ->
                                      setIsVisible false
                                      // フェードアウト後に完全に閉じる
                                      Browser.Dom.window.setTimeout (onClose, 300) |> ignore)
                                  prop.children [ Html.span [ prop.className "text-xl"; prop.text "×" ] ] ] ] ] ] ]

// オートコンプリート - 候補表示のみをローカル管理
[<ReactComponent>]
let AutocompleteField
    (label: string)
    (fieldId: string)
    (value: string)
    (suggestions: string list)
    (hasError: bool)
    (errorMessage: string option)
    (onChange: string -> unit)
    (onSelect: string -> unit)
    =

    // ドロップダウン表示のみをローカル管理
    let (isFocused, setIsFocused) = React.useState false
    let (filterText, setFilterText) = React.useState value

    // valueが外部から変更されたら同期
    React.useEffect ((fun () -> setFilterText value), [| box value |])

    let filteredSuggestions =
        if System.String.IsNullOrEmpty filterText then
            suggestions
        else
            suggestions |> List.filter (fun s -> s.ToLower().Contains(filterText.ToLower()))

    Html.div
        [ prop.className "form-group mb-4 relative"
          prop.children
              [ Html.label
                    [ prop.htmlFor fieldId
                      prop.className "block text-sm font-medium text-gray-700 mb-1"
                      prop.text label ]
                Html.input
                    [ prop.id fieldId
                      prop.className (
                          "w-full p-2 border rounded "
                          + if hasError then "border-red-500" else "border-gray-300"
                      )
                      prop.value filterText
                      prop.onFocus (fun _ -> setIsFocused true)
                      prop.onBlur (fun _ ->
                          // 少し遅延させてアイテム選択を可能にする
                          async {
                              do! Async.Sleep 100
                              setIsFocused false
                          }
                          |> Async.StartImmediate)
                      prop.onChange (fun text ->
                          setFilterText text
                          onChange text) ]

                if isFocused && not (List.isEmpty filteredSuggestions) then
                    Html.div
                        [ prop.className
                              "absolute z-10 w-full bg-white mt-1 border rounded shadow-md max-h-60 overflow-auto"
                          prop.children
                              [ for suggestion in filteredSuggestions ->
                                    Html.div
                                        [ prop.className "p-2 hover:bg-gray-100 cursor-pointer"
                                          prop.text suggestion
                                          prop.onClick (fun _ ->
                                              onSelect suggestion
                                              setFilterText suggestion
                                              setIsFocused false) ] ] ]

                if hasError && errorMessage.IsSome then
                    Html.div [ prop.className "text-red-500 text-sm mt-1"; prop.text errorMessage.Value ] ] ]
