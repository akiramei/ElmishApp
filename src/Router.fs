// Router.fs
// ルーティング管理モジュール
module App.Router

open Feliz
open Feliz.Router
open App.Types
open Fable.Core.JsInterop


// URL文字列からRouteに変換する
let parseRoute (segments: string list) =
    match segments with
    | []
    | [ "" ] -> Route.Home
    | [ "counter" ] -> Route.Counter
    | [ "tab"; tabId ] -> Route.CustomTab tabId
    | [ resource; id ] -> Route.WithParam(resource, id)
    | "not-found" :: _ -> Route.NotFound
    | _ -> Route.NotFound

// URLクエリパラメータをMap型に変換
let parseQueryParams (queryString: string) : Map<string, string> =
    if System.String.IsNullOrEmpty(queryString) then
        Map.empty
    else
        queryString.TrimStart('?').Split('&')
        |> Array.choose (fun param ->
            match param.Split('=') with
            | [| key; value |] -> Some(key, value)
            | _ -> None)
        |> Map.ofArray

// RouteからURL文字列に変換する
let toUrl (route: Route) =
    match route with
    | Route.Home -> Router.formatPath "/"
    | Route.Counter -> Router.formatPath "counter"
    | Route.CustomTab tabId -> Router.formatPath [ "tab"; tabId ]
    | Route.WithParam(resource, id) -> Router.formatPath [ resource; id ]
    | Route.WithQuery(base', queries) ->
        let queryParams =
            queries
            |> Map.toList
            |> List.map (fun (k, v) -> k + "=" + v)
            |> String.concat "&"

        Router.formatPath base' + "?" + queryParams
    | Route.NotFound -> Router.formatPath "not-found"

// TabタイプとRouteタイプの相互変換
let tabToRoute (tab: Tab) : Route =
    match tab with
    | Tab.Home -> Route.Home
    | Tab.Counter -> Route.Counter
    | Tab.CustomTab id -> Route.CustomTab id

let routeToTab (route: Route) : Tab option =
    match route with
    | Route.Home -> Some Tab.Home
    | Route.Counter -> Some Tab.Counter
    | Route.CustomTab id -> Some(Tab.CustomTab id)
    | _ -> None

// 指定されたRouteにナビゲーションする関数
let navigateTo (route: Route) =
    let url = toUrl route
    Router.navigate url

// 現在のURLからRouteを取得
let getCurrentRoute () =
    let segments = Router.currentUrl ()
    printfn "Current URL segments: %A" segments
    let route = parseRoute segments
    printfn "Parsed route: %A" route
    route
