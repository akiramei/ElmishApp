// src/UI/Components/Forms/SearchableSelector.fs
module App.UI.Components.SearchableSelector

open System
open Feliz
open Fable.Core.JsInterop
open Browser.Types
open Browser.Dom
open Browser // IntersectionObserver用
open Fable.Core // JS.Nullable などのために必要

// 汎用的なアイテム型
type SelectableItem<'T> =
    { Code: string; Name: string; Data: 'T }

// ページング情報型
type PaginationInfo =
    { CurrentPage: int
      HasMore: bool
      TotalItems: int option } // オプションの総数 (API が返す場合)

// 検索型セレクターコンポーネントのプロパティ
type SearchableSelectorProps<'T> =
    { Items: SelectableItem<'T> list
      SelectedItem: SelectableItem<'T> option
      OnChange: SelectableItem<'T> option -> unit
      Placeholder: string
      Label: string option
      ErrorMessage: string option
      IsRequired: bool
      MinSearchLength: int
      MaxResults: int
      LoadItems: (string -> int -> int -> Async<SelectableItem<'T> list * PaginationInfo>) option
    // ↑ query, page, pageSize -> items * pagination info
    }

// デフォルトのプロパティ
let defaultProps<'T> : SearchableSelectorProps<'T> =
    { Items = []
      SelectedItem = None
      OnChange = ignore
      Placeholder = "コードまたは名称で検索..."
      Label = None
      ErrorMessage = None
      IsRequired = false
      MinSearchLength = 1
      MaxResults = 10
      LoadItems = None }

// 検索型セレクターコンポーネント
[<ReactComponent>]
let SearchableSelector<'T>
    (props:
        {| Items: SelectableItem<'T> list
           SelectedItem: SelectableItem<'T> option
           OnChange: SelectableItem<'T> option -> unit
           Placeholder: string
           Label: string option
           ErrorMessage: string option
           IsRequired: bool
           MinSearchLength: int
           MaxResults: int
           LoadItems: (string -> int -> int -> Async<SelectableItem<'T> list * PaginationInfo>) option |})
    =
    // 検索クエリの状態
    let query, setQuery = React.useState ""

    // ドロップダウンの開閉状態
    let isOpen, setIsOpen = React.useState false

    // 検索結果の状態
    let searchResults, setSearchResults =
        React.useState (props.Items |> List.truncate props.MaxResults)

    // ローディング状態
    let isLoading, setIsLoading = React.useState false

    // 参照の作成
    let inputRef = React.useElementRef ()

    // ページング情報
    let paginationInfo, setPaginationInfo =
        React.useState
            { CurrentPage = 1
              HasMore = false
              TotalItems = None }

    // 最近使用したアイテム (例としてローカルストレージから取得)
    let recentItems, setRecentItems = React.useState<SelectableItem<'T> list> ([])

    // インフィニットスクロール用のボトム検出
    let scrollRef = React.useElementRef ()
    let observerRef = React.useRef<Option<IntersectionObserverType>> (None)

    // 検索処理
    let performSearch (searchQuery: string) (page: int) (append: bool) =
        // 検索クエリが最小文字数以上ある場合のみ検索
        if searchQuery.Length >= props.MinSearchLength then
            match props.LoadItems with
            | Some loadFn ->
                setIsLoading true

                async {
                    try
                        let! (items, newPaginationInfo) = loadFn searchQuery page props.MaxResults

                        if append then
                            // 既存結果に追加
                            setSearchResults (searchResults @ items)
                        else
                            // 結果を置き換え
                            setSearchResults items

                        setPaginationInfo newPaginationInfo
                    finally
                        setIsLoading false
                }
                |> Async.StartImmediate
            | None ->
                // クライアントサイドでフィルタリング
                let queryLower = searchQuery.ToLower()

                let filtered =
                    props.Items
                    |> List.filter (fun item ->
                        item.Code.ToLower().Contains queryLower
                        || item.Name.ToLower().Contains queryLower)
                    |> List.truncate props.MaxResults

                setSearchResults filtered

                setPaginationInfo
                    { CurrentPage = 1
                      HasMore = filtered.Length >= props.MaxResults
                      TotalItems = Some(props.Items.Length) }
        else
            setSearchResults []

            setPaginationInfo
                { CurrentPage = 1
                  HasMore = false
                  TotalItems = None }

    // 次のページを読み込む
    let loadNextPage () =
        if paginationInfo.HasMore && not isLoading then
            let nextPage = paginationInfo.CurrentPage + 1
            performSearch query nextPage true

    // インターセクションオブザーバーのセットアップ
    React.useEffect (
        (fun () ->
            match scrollRef.current with
            | None ->
                // 観察対象の要素がない場合は何もしない
                React.createDisposable (fun () -> ())
            | Some element ->
                // 前のオブザーバーがあれば接続解除
                match observerRef.current with
                | Some(observer: IntersectionObserverType) -> observer.disconnect ()
                | None -> ()

                // 観察対象の設定
                let options = createEmpty<IntersectionObserverOptions>
                options.threshold <- U2.Case1 0.1
                options.rootMargin <- "100px"

                // コールバック関数
                let callback (entries: IntersectionObserverEntry[]) (_: IntersectionObserverType) =
                    if
                        entries.Length > 0
                        && entries.[0].isIntersecting
                        && paginationInfo.HasMore
                        && not isLoading
                    then
                        loadNextPage ()

                // 新しいオブザーバーを作成
                let observer = IntersectionObserver.Create(callback, options)
                observerRef.current <- Some observer

                // 要素をobserve
                observer.observe (element)

                // クリーンアップ
                React.createDisposable (fun () ->
                    match observerRef.current with
                    | Some(observer: IntersectionObserverType) -> observer.disconnect ()
                    | None -> ())),
        [| box isOpen
           box isLoading
           box paginationInfo.HasMore
           box scrollRef.current |]
    )

    // クエリが変更されたときの処理
    React.useEffect (
        (fun () ->
            let timeoutId =
                Browser.Dom.window.setTimeout (
                    (fun () ->
                        // 検索を実行 (ページリセット)
                        performSearch query 1 false),
                    300
                )

            React.createDisposable (fun () -> Browser.Dom.window.clearTimeout (timeoutId))),
        [| box query |]
    ) // query の変更を監視

    let handleQueryChange (value: string) =
        let newQuery = value
        setQuery newQuery

        // 空のクエリになった場合は選択をクリア
        if String.IsNullOrWhiteSpace newQuery && props.SelectedItem.IsSome then
            props.OnChange None

    // アイテムが選択されたときの処理
    let handleSelectItem (item: SelectableItem<'T>) =
        Fable.Core.JS.console.log ("Item selected:", item)
        props.OnChange(Some item)
        setQuery (sprintf "%s - %s" item.Code item.Name)
        setIsOpen false

        // 最近使用したアイテムに追加
        let newRecentItems =
            recentItems
            |> List.filter (fun i -> i.Code <> item.Code) // 同じアイテムがあれば除去
            |> List.truncate 9 // 9件に制限

        setRecentItems (item :: newRecentItems) // 選択されたアイテムを先頭に追加

    // クリアボタン処理
    let handleClear (e: MouseEvent) =
        e.preventDefault ()
        e.stopPropagation ()
        props.OnChange None
        setQuery ""

        // クリア後に入力フィールドにフォーカスを戻す
        match inputRef.current with
        | Some element -> element.focus ()
        | None -> ()

    // フォーカス時の処理
    let handleFocus _ = setIsOpen true

    // 外側クリック時の処理 (ドロップダウンを閉じる)
    let handleOutsideClick _ = setIsOpen false

    // 外側クリックを検出するための参照
    let containerRef = React.useElementRef ()

    // 外側クリックのイベントリスナー設定
    React.useEffect (
        (fun () ->
            let handleClickOutside (e: Event) =
                match containerRef.current with
                | Some element ->
                    if not (element.contains (e.target :?> Node)) then
                        setIsOpen false
                | None -> ()

            document.addEventListener ("mousedown", handleClickOutside)

            // クリーンアップ
            React.createDisposable (fun () -> document.removeEventListener ("mousedown", handleClickOutside))),
        [| containerRef :> obj |]
    )

    // 初回マウント時だけ query を初期化
    React.useEffectOnce (fun () ->
        match props.SelectedItem with
        | Some item -> setQuery (sprintf "%s - %s" item.Code item.Name)
        | None -> ())

    let highlightMatch (text: string) (query: string) =
        let lowerText = text.ToLower()
        let lowerQuery = query.ToLower()

        match lowerText.IndexOf(lowerQuery) with
        | -1 -> Html.span [ prop.text text ]
        | index ->
            let before = text.Substring(0, index)
            let matched = text.Substring(index, query.Length)
            let after = text.Substring(index + query.Length)

            Html.span
                [ Html.span [ prop.text before ]
                  Html.span [ prop.className "bg-yellow-100 font-semibold"; prop.text matched ]
                  Html.span [ prop.text after ] ]

    let formatItemDisplay (item: SelectableItem<'T>) = $"[{item.Code}] {item.Name}"

    // コンポーネントのレンダリング
    Html.div
        [ prop.className "relative w-full"
          prop.ref containerRef
          prop.children
              [
                // ラベル (存在する場合)
                match props.Label with
                | Some labelText ->
                    Html.label
                        [ prop.className "block text-sm font-medium text-gray-700 mb-1"
                          prop.text labelText
                          if props.IsRequired then
                              prop.children
                                  [ Html.span [ prop.text labelText ]
                                    Html.span [ prop.className "text-red-500 ml-1"; prop.text "*" ] ]
                          else
                              prop.text labelText ]
                | None -> ()

                // 入力フィールドとドロップダウン
                Html.div
                    [ prop.className "relative"
                      prop.children
                          [
                            // 入力フィールド
                            Html.div
                                [ prop.className (
                                      "flex border rounded w-full "
                                      + if props.ErrorMessage.IsSome then
                                            "border-red-500"
                                        else
                                            "border-gray-300"
                                  )
                                  prop.children
                                      [ Html.input
                                            [ prop.className "w-full px-3 py-2 focus:outline-none"
                                              prop.placeholder props.Placeholder
                                              prop.value query
                                              prop.onChange handleQueryChange
                                              prop.onFocus handleFocus
                                              prop.ref inputRef ]

                                        // 選択中の場合、クリアボタン表示
                                        if props.SelectedItem.IsSome then
                                            Html.button
                                                [ prop.className "px-2 text-gray-400 hover:text-gray-600"
                                                  prop.onClick handleClear
                                                  prop.children
                                                      [ Html.span [ prop.className "text-lg"; prop.text "×" ] ] ]

                                        // ドロップダウン矢印
                                        Html.div
                                            [ prop.className "px-2 flex items-center text-gray-400"
                                              prop.onClick (fun _ -> setIsOpen (not isOpen))
                                              prop.children
                                                  [ Svg.svg
                                                        [ svg.className "h-4 w-4"
                                                          svg.fill "currentColor"
                                                          svg.viewBox (0, 0, 20, 20)
                                                          svg.children
                                                              [ Svg.path
                                                                    [ svg.d
                                                                          "M5.293 7.293a1 1 0 011.414 0L10 10.586l3.293-3.293a1 1 0 111.414 1.414l-4 4a1 1 0 01-1.414 0l-4-4a1 1 0 010-1.414z"
                                                                      svg.clipRule.evenodd
                                                                      svg.custom ("fillRule", "evenodd") ] ] ] ] ] ] ]

                            // エラーメッセージ
                            match props.ErrorMessage with
                            | Some error -> Html.div [ prop.className "text-red-500 text-sm mt-1"; prop.text error ]
                            | None -> ()

                            // ドロップダウンメニュー
                            if isOpen then
                                Html.div
                                    [ prop.className
                                          "absolute z-10 w-full mt-1 bg-white shadow-lg rounded-md border border-gray-200 max-h-60 overflow-auto"
                                      prop.children
                                          [
                                            // ローディングインジケーター (上部)
                                            if isLoading && searchResults.IsEmpty then
                                                Html.div
                                                    [ prop.className "p-3 text-center text-gray-500"
                                                      prop.children
                                                          [ Html.div
                                                                [ prop.className
                                                                      "animate-spin h-5 w-5 border-2 border-blue-500 rounded-full border-t-transparent mx-auto" ]
                                                            Html.div [ prop.className "mt-1"; prop.text "検索中..." ] ] ]
                                            elif query.Length < props.MinSearchLength then
                                                // 検索文字数が足りない
                                                Html.div
                                                    [ prop.className "p-3 text-center text-gray-500"
                                                      prop.text (sprintf "%d文字以上入力してください" props.MinSearchLength) ]
                                            elif List.isEmpty searchResults && List.isEmpty recentItems then
                                                // 検索結果なし
                                                Html.div
                                                    [ prop.className "p-3 text-center text-gray-500"
                                                      prop.text "検索結果がありません" ]
                                            else
                                                Html.div
                                                    [ prop.children
                                                          [
                                                            // 最近使用したアイテム
                                                            if
                                                                not (List.isEmpty recentItems)
                                                                && String.IsNullOrWhiteSpace query
                                                            then
                                                                Html.div
                                                                    [ prop.children
                                                                          [ Html.div
                                                                                [ prop.className
                                                                                      "px-3 py-2 text-xs text-gray-500 bg-gray-50"
                                                                                  prop.text "最近使用したアイテム" ]

                                                                            // 最近使用したアイテムのリスト
                                                                            for item in recentItems do
                                                                                Html.div
                                                                                    [ prop.key item.Code
                                                                                      prop.className
                                                                                          "px-3 py-2 hover:bg-blue-50 cursor-pointer"
                                                                                      prop.onClick (fun _ ->
                                                                                          handleSelectItem item)
                                                                                      prop.children
                                                                                          [ Html.span
                                                                                                [ prop.className
                                                                                                      "font-mono text-gray-600"
                                                                                                  prop.text item.Code ]
                                                                                            Html.span
                                                                                                [ prop.className
                                                                                                      "mx-1 text-gray-400"
                                                                                                  prop.text "-" ]
                                                                                            Html.span
                                                                                                [ prop.text item.Name ] ] ] ] ]

                                                            // 検索結果
                                                            if not (List.isEmpty searchResults) then
                                                                Html.div
                                                                    [ prop.children
                                                                          [ if
                                                                                not (String.IsNullOrWhiteSpace query)
                                                                            then
                                                                                Html.div
                                                                                    [ prop.className
                                                                                          "px-3 py-2 text-xs text-gray-500 bg-gray-50"
                                                                                      prop.text "検索結果" ]

                                                                            // 検索結果リスト
                                                                            for item in searchResults do
                                                                                Html.div
                                                                                    [ prop.key item.Code
                                                                                      prop.className
                                                                                          "px-3 py-2 hover:bg-blue-50 cursor-pointer"
                                                                                      prop.onClick (fun _ ->
                                                                                          handleSelectItem item)
                                                                                      prop.children
                                                                                          [ Html.div
                                                                                                [ prop.className
                                                                                                      "text-sm text-gray-800"
                                                                                                  prop.children
                                                                                                      [ highlightMatch
                                                                                                            (formatItemDisplay
                                                                                                                item)
                                                                                                            query ] ] ] ]

                                                                            // "もっと読み込む" ボタンまたはローディングインジケーター (下部)
                                                                            if paginationInfo.HasMore then
                                                                                if isLoading then
                                                                                    Html.div
                                                                                        [ prop.className
                                                                                              "p-2 text-center text-gray-500"
                                                                                          prop.children
                                                                                              [ Html.div
                                                                                                    [ prop.className
                                                                                                          "animate-spin h-4 w-4 border-2 border-blue-500 rounded-full border-t-transparent mx-auto" ] ] ]
                                                                                else
                                                                                    Html.div
                                                                                        [ prop.ref scrollRef
                                                                                          prop.className
                                                                                              "p-2 text-center text-blue-500 hover:text-blue-700 cursor-pointer"
                                                                                          prop.onClick (fun _ ->
                                                                                              loadNextPage ())
                                                                                          prop.text "さらに読み込む" ]

                                                                            // 総件数表示
                                                                            match paginationInfo.TotalItems with
                                                                            | Some total ->
                                                                                Html.div
                                                                                    [ prop.className
                                                                                          "p-2 text-center text-xs text-gray-500"
                                                                                      prop.text (
                                                                                          sprintf
                                                                                              "全%d件中%d件表示"
                                                                                              total
                                                                                              searchResults.Length
                                                                                      ) ]
                                                                            | None -> () ] ] ] ] ] ] ] ] ] ]
