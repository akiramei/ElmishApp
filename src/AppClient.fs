// ApiClient.fs
module App.ApiClient

open Fable.Core.JS
open Thoth.Fetch
open Thoth.Json
open Elmish
open App.Shared // 共有DTOを参照

type HttpMethod =
    | GET
    | POST
    | PUT
    | DELETE

// APIエラー型の定義
type ApiError =
    | NetworkError of string
    | DecodingError of string
    | ServerError of int * string // ステータスコードとエラーメッセージ
    | PreparationError of string
    | UnknownError of string

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

// APIリクエストを実行する関数
let inline private fetchData<'Input, 'Output>
    (httpMethod: HttpMethod)
    (url: string)
    (data: 'Input option)
    : Promise<Result<'Output, ApiError>> =
    promise {
        try
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
let private baseUrl = "http://localhost:5000/api"

// APIエンドポイント関数
let getUsers () : Promise<Result<UserDto list, ApiError>> =
    let url = $"{baseUrl}/users"
    fetchData<unit, UserDto list> HttpMethod.GET url None

let getUserById (userId: int64) : Promise<Result<UserDto, ApiError>> =
    let url = $"{baseUrl}/users/{userId}"
    fetchData<unit, UserDto> HttpMethod.GET url None

let getProducts () : Promise<Result<ProductDto list, ApiError>> =
    let url = $"{baseUrl}/products"
    fetchData<unit, ProductDto list> HttpMethod.GET url None

let getProductById (productId: int64) : Promise<Result<ProductDto, ApiError>> =
    let url = $"{baseUrl}/products/{productId}"
    fetchData<unit, ProductDto> HttpMethod.GET url None

// 製品詳細を取得する関数
let getProductDetailById (productId: int64) : Promise<Result<ProductDetailDto, ApiError>> =
    let url = $"{baseUrl}/products/{productId}/detail"
    fetchData<unit, ProductDetailDto> HttpMethod.GET url None

// 製品を削除する関数
let deleteProduct (productId: int64) : Promise<Result<ApiSuccessResponse, ApiError>> =
    let url = $"{baseUrl}/products/{productId}"
    fetchData<unit, ApiSuccessResponse> HttpMethod.DELETE url None

// 製品を更新する関数
let updateProduct (productId: int64) (productUpdate: ProductUpdateDto) : Promise<Result<ProductDetailDto, ApiError>> =
    let url = $"{baseUrl}/products/{productId}"
    fetchData<ProductUpdateDto, ProductDetailDto> HttpMethod.PUT url (Some productUpdate)

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