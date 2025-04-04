// UI/Utils.fs
module App.UI.Utils

open Feliz
open App.Types
open App.UI.Components
open App.UI.Layouts

// ルート部分文字列からアクティブなタブを決定するヘルパー
let getActiveTabFromRoute (currentRoute: Route) : string =
    match currentRoute with
    | Route.Home -> "Home"
    | Route.Counter -> "Counter"
    | Route.Products
    | Route.ProductDetail _ -> "Products"
    | Route.Admin
    | Route.AdminUsers
    | Route.AdminProducts -> "Admin"
    | Route.CustomTab id -> id
    | _ -> "Home"

// 読み込み中、エラー、データなしの各状態に対応したラッパー
let withLoadingStates<'T>
    (state: FetchStatus<'T>)
    (render: 'T -> ReactElement)
    (loadingMessage: string)
    (errorRenderer: App.Infrastructure.Api.Types.ApiError -> ReactElement)
    (emptyRenderer: unit -> ReactElement)
    =

    match state with
    | NotStarted -> emptyRenderer ()
    | Loading -> loadingState loadingMessage
    | Failed error -> errorRenderer error
    | Success data -> render data

// マスター/詳細表示の拡張版 - 詳細データの読み込み状態を考慮
let masterDetailWithLoading
    (master: ReactElement)
    (detailState: FetchStatus<'T> option)
    (renderDetail: 'T -> ReactElement)
    (loadingMessage: string)
    (onDetailError: App.Infrastructure.Api.Types.ApiError -> unit)
    =

    let detailContent =
        match detailState with
        | None -> None
        | Some NotStarted -> None
        | Some Loading -> Some(loadingState loadingMessage)
        | Some(Failed error) ->
            onDetailError error
            Some(errorState (App.Infrastructure.Api.Client.getErrorMessage error) None)
        | Some(Success data) -> Some(renderDetail data)

    masterDetail master detailContent

// フォーム状態とバリデーションヘルパー
type ValidationResult =
    { IsValid: bool
      Errors: Map<string, string> }

// フォーム値を検証するユーティリティ関数
let validateForm (validators: (string * (string -> string option)) list) (values: Map<string, string>) =
    let errors =
        validators
        |> List.choose (fun (field, validator) ->
            match Map.tryFind field values with
            | Some value ->
                match validator value with
                | Some errorMsg -> Some(field, errorMsg)
                | None -> None
            | None -> None)
        |> Map.ofList

    { IsValid = Map.isEmpty errors
      Errors = errors }

// バリデータビルダー
module Validators =
    // 必須フィールド
    let required fieldName value =
        if System.String.IsNullOrWhiteSpace value then
            Some $"{fieldName}は必須です"
        else
            None

    // 最小文字数
    let minLength fieldName minLen value =
        if String.length value < minLen then
            Some $"{fieldName}は{minLen}文字以上である必要があります"
        else
            None

    // 最大文字数
    let maxLength fieldName maxLen value =
        if String.length value > maxLen then
            Some $"{fieldName}は{maxLen}文字以下である必要があります"
        else
            None

    // 数値のみ
    let numbersOnly fieldName (value: string) =
        if System.Text.RegularExpressions.Regex.IsMatch(value, "^[0-9]+$") then
            None
        else
            Some $"{fieldName}は数字のみ入力可能です"

    // 最小値（数値）
    let minValue fieldName minVal (value: string) =
        match System.Double.TryParse value with
        | true, num when num >= minVal -> None
        | true, _ -> Some $"{fieldName}は{minVal}以上である必要があります"
        | _ -> Some $"{fieldName}は有効な数値である必要があります"

    // 最大値（数値）
    let maxValue fieldName maxVal (value: string) =
        match System.Double.TryParse value with
        | true, num when num <= maxVal -> None
        | true, _ -> Some $"{fieldName}は{maxVal}以下である必要があります"
        | _ -> Some $"{fieldName}は有効な数値である必要があります"

    // メールアドレス形式
    let email fieldName (value: string) =
        let pattern = @"^[a-zA-Z0-9._%+-]+@[a-zA-Z0-9.-]+\.[a-zA-Z]{2,}$"

        if System.Text.RegularExpressions.Regex.IsMatch(value, pattern) then
            None
        else
            Some $"{fieldName}は有効なメールアドレスである必要があります"

    // カスタムバリデータ作成ヘルパー
    let custom fieldName predicate errorMsg value =
        if predicate value then None else Some errorMsg

    // 複数のバリデータを組み合わせる
    let compose validators value =
        validators |> List.tryPick (fun validator -> validator value)

// クエリパラメータをパースするユーティリティ
let parseQueryParams (queryString: string) =
    if System.String.IsNullOrEmpty(queryString) then
        Map.empty
    else
        queryString.TrimStart('?').Split('&')
        |> Array.choose (fun param ->
            match param.Split('=') with
            | [| key; value |] -> Some(key, value)
            | _ -> None)
        |> Map.ofArray

// ページング情報を計算するヘルパー
let calculatePagination (totalItems: int) (pageSize: int) (currentPage: int) =
    let totalPages =
        let pages = totalItems / pageSize
        if totalItems % pageSize > 0 then pages + 1 else pages

    let validCurrentPage = max 1 (min currentPage totalPages)

    { CurrentPage = validCurrentPage
      PageSize = pageSize
      TotalItems = totalItems
      TotalPages = max 1 totalPages }

// オブジェクトを表示用の文字列に変換するヘルパー
let formatValue (value: obj) =
    match value with
    | :? System.DateTime as date -> date.ToString("yyyy-MM-dd HH:mm:ss")
    | :? float as num -> num.ToString("N2")
    | :? int as num -> num.ToString()
    | :? string as str -> str
    | :? bool as b -> if b then "はい" else "いいえ"
    | null -> "-"
    | _ -> string value

// 配列をページングするヘルパー
let paginateArray<'T> (array: 'T array) (pageIndex: int) (pageSize: int) =
    let startIndex = (pageIndex - 1) * pageSize
    let endIndex = min (startIndex + pageSize) array.Length

    if startIndex >= array.Length then
        [||]
    else
        array.[startIndex .. endIndex - 1]

// リストをページングするヘルパー
let paginateList<'T> (list: 'T list) (pageIndex: int) (pageSize: int) =
    let startIndex = (pageIndex - 1) * pageSize

    list |> List.skip (min startIndex (List.length list)) |> List.truncate pageSize
