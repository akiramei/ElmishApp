module App.Infrastructure.Api.Types

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
