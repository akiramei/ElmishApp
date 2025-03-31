// ApiClient.fs
module App.ApiClient

open Fable.Core
open Fable.Core.JsInterop
open Thoth.Json
open Elmish
open App.Shared // 共有DTOを参照

// APIレスポンスの型
type ApiResponse<'T> =
    | ApiSuccess of 'T
    | ApiError of string

// APIリクエストを実行する関数
let private fetchData<'T> (url: string) : Async<ApiResponse<'T>> =
    async {
        try
            // Promise型をAsync型に変換します
            let! response = Fetch.fetch url [] |> Async.AwaitPromise

            if response.Ok then
                let! text = response.text () |> Async.AwaitPromise

                match Decode.fromString (Decode.Auto.generateDecoder<'T> ()) text with
                | Ok data -> return ApiSuccess data
                | Error err -> return ApiError $"デコードエラー: {err}"
            else
                let status = response.Status
                let! errorText = response.text () |> Async.AwaitPromise
                return ApiError $"APIエラー ({status}): {errorText}"
        with ex ->
            return ApiError $"通信エラー: {ex.Message}"
    }

// エンドポイントのURL定義
let private baseUrl = "/api"

// APIエンドポイント関数
let getUsers () =
    let url = $"{baseUrl}/users"
    fetchData<UserDto list> url

let getUserById (userId: int64) =
    let url = $"{baseUrl}/users/{userId}"
    fetchData<UserDto> url

let getProducts () =
    let url = $"{baseUrl}/products"
    fetchData<ProductDto list> url

let getProductById (productId: int64) =
    let url = $"{baseUrl}/products/{productId}"
    fetchData<ProductDto> url

// APIレスポンスをElmishコマンドに変換するヘルパー関数
let toCmd<'T, 'Msg> (asyncOperation: Async<ApiResponse<'T>>) (onSuccess: 'T -> 'Msg) (onError: string -> 'Msg) =
    Cmd.OfAsync.perform (fun () -> asyncOperation) () (function
        | ApiSuccess data -> onSuccess data
        | ApiError err -> onError err)
