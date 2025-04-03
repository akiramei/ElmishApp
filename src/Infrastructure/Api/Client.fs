// ApiClient.fs
module App.Infrastructure.Api.Client

open Fable.Core.JS
open Thoth.Fetch
open Elmish
open App.Infrastructure.Api.Types

// 環境設定
let private baseUrl =
#if DEBUG
    "http://localhost:5000/api"
#else
    "https://api.example.com/v1"
#endif

// FetchErrorをApiErrorに変換
let fromFetchError (error: FetchError) : ApiError =
    match error with
    | PreparingRequestFailed exn -> PreparationError exn.Message
    | FetchError.NetworkError exn -> NetworkError exn.Message
    | FetchFailed response -> ServerError(int response.Status, response.StatusText)
    | DecodingFailed errorMsg -> DecodingError errorMsg

// エラーメッセージの取得
let getErrorMessage (error: ApiError) : string =
    match error with
    | NetworkError msg -> $"ネットワークエラー: {msg}"
    | DecodingError msg -> $"デコードエラー: {msg}"
    | ServerError(status, msg) -> $"サーバーエラー ({status}): {msg}"
    | PreparationError msg -> $"リクエスト準備エラー: {msg}"
    | UnknownError msg -> $"不明なエラー: {msg}"

// 相対パスからフルURLを構築
let buildUrl (path: string) =
    if path.StartsWith("/") then
        baseUrl + path
    else
        baseUrl + "/" + path

// APIリクエストを実行する関数
let inline fetchData<'Input, 'Output>
    (httpMethod: HttpMethod)
    (path: string)
    (data: 'Input option)
    : Promise<Result<'Output, ApiError>> =

    promise {
        try
            let url = buildUrl path

            let! response =
                match httpMethod with
                | GET -> Fetch.tryGet<unit, 'Output> url
                | POST ->
                    match data with
                    | Some d -> Fetch.tryPost<'Input, 'Output> (url, d)
                    | None -> Fetch.tryPost<unit, 'Output> (url, ())
                | PUT ->
                    match data with
                    | Some d -> Fetch.tryPut<'Input, 'Output> (url, d)
                    | None -> Fetch.tryPut<unit, 'Output> (url, ())
                | DELETE -> Fetch.tryDelete<unit, 'Output> url

            match response with
            | Ok data -> return Ok data
            | Error fetchError -> return Error(fromFetchError fetchError)
        with ex ->
            return Error(UnknownError ex.Message)
    }

// エンドポイントのURL定義

// APIレスポンスをElmishコマンドに変換するヘルパー関数 - 簡単なバージョン
let toCmd<'T, 'Msg>
    (promiseOperation: Promise<Result<'T, ApiError>>)
    (onSuccess: 'T -> 'Msg)
    (onError: string -> 'Msg)
    =
    Cmd.OfPromise.perform (fun () -> promiseOperation) () (function
        | Ok data -> onSuccess data
        | Error err -> onError (getErrorMessage err))

// APIレスポンスをElmishコマンドに変換するヘルパー関数 - 詳細なエラーハンドリング用
let toCmdWithErrorHandling<'T, 'Msg>
    (promiseOperation: Promise<Result<'T, ApiError>>)
    (onSuccess: 'T -> 'Msg)
    (onError: ApiError -> 'Msg)
    : Cmd<'Msg> =

    Cmd.OfPromise.perform (fun () -> promiseOperation) () (function
        | Ok data -> onSuccess data
        | Error err -> onError err)
