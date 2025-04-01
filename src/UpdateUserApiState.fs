// UpdateUserApiState.fs - User Domain API Module
module App.UpdateUserApiState

open Elmish
open App.Shared
open App.Types
open App.ApiClient
open App.Notifications

// ユーザー関連APIの状態更新
let updateUserApiState (msg: UserApiMsg) (state: UserApiData) : UserApiData * Cmd<Msg> =
    match msg with
    | FetchUsers ->
        // ユーザー一覧取得APIリクエスト
        let usersPromise = ApiClient.getUsers ()

        // 成功/エラーハンドラー
        let successHandler (users: UserDto list) =
            ApiMsg(UserApi(FetchUsersSuccess users))

        let errorHandler (error: ApiError) = ApiMsg(UserApi(FetchUsersError error))

        // Loading状態に更新し、APIリクエストコマンドを発行
        { state with Users = Loading }, ApiClient.toCmdWithErrorHandling usersPromise successHandler errorHandler

    | FetchUsersSuccess users ->
        // 成功時はデータを保存
        { state with Users = Success users }, Cmd.none

    | FetchUsersError error ->
        // エラー時は状態を更新し、通知を表示
        { state with Users = Failed error },
        Cmd.ofMsg (
            NotificationMsg(
                Add(
                    Notifications.error "ユーザーデータの取得に失敗しました"
                    |> withDetails (getErrorMessage error)
                    |> fromSource "UserAPI"
                )
            )
        )

    // 特定ユーザー取得メッセージの処理
    | FetchUser userId ->
        // ユーザー詳細取得APIリクエスト
        let userPromise = ApiClient.getUserById userId

        // 成功/エラーハンドラー
        let successHandler (user: UserDto) = ApiMsg(UserApi(FetchUserSuccess user))
        let errorHandler (error: ApiError) = ApiMsg(UserApi(FetchUserError error))

        // Loading状態に更新し、APIリクエストコマンドを発行
        { state with
            SelectedUser = Some Loading },
        toCmdWithErrorHandling userPromise successHandler errorHandler

    | FetchUserSuccess user ->
        // 成功時はデータを保存
        { state with
            SelectedUser = Some(Success user) },
        Cmd.none

    | FetchUserError error ->
        // エラー時は状態を更新し、通知を表示
        { state with
            SelectedUser = Some(Failed error) },
        Cmd.ofMsg (
            NotificationMsg(
                Add(
                    Notifications.error "ユーザー詳細の取得に失敗しました"
                    |> withDetails (getErrorMessage error)
                    |> fromSource "UserAPI"
                )
            )
        )

// APIを呼び出すためのコンビニエンス関数
let loadUsersCmd: Cmd<Msg> = Cmd.ofMsg (ApiMsg(UserApi FetchUsers))

let loadUserByIdCmd (userId: int64) : Cmd<Msg> =
    Cmd.ofMsg (ApiMsg(UserApi(FetchUser userId)))
