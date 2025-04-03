module App.Infrastructure.Api.Users

open Fable.Core.JS
open App.Shared // 共有DTOを参照

open App.Infrastructure.Api.Types
open App.Infrastructure.Api.Client

// APIエンドポイント関数
let getUsers () : Promise<Result<UserDto list, ApiError>> =
    let path = $"/users"
    fetchData<unit, UserDto list> GET path None

let getUserById (userId: int64) : Promise<Result<UserDto, ApiError>> =
    let path = $"/users/{userId}"
    fetchData<unit, UserDto> GET path None
