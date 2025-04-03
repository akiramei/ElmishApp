module App.UpdateAdminState

open Elmish
open App.Types
open App.Router
open App.Notifications
open App.Interop

// 管理者関連のUpdate処理
let updateAdminState (msg: AdminMsg) (model: Model) : Model * Cmd<Msg> =
    match msg with
    | LoadAdminData ->
        // 管理者データを読み込むコマンドを発行
        model,
        Cmd.batch
            [
              // 必要なデータをロード
              Cmd.ofMsg (ApiMsg(UserApi FetchUsers))
              Cmd.ofMsg (ApiMsg(ProductApi FetchProducts)) ]

    | ExportProducts ->
        // 製品データのエクスポート処理
        // 実際には未実装なので通知のみ
        model,
        Cmd.ofMsg (NotificationMsg(Add(Notifications.info "製品データのエクスポート機能は準備中です" |> Notifications.fromSource "Admin")))

    | RunSystemDiagnostic ->
        // システム診断処理
        // 実際には未実装なので通知のみ
        model, Cmd.ofMsg (NotificationMsg(Add(Notifications.info "システム診断機能は準備中です" |> Notifications.fromSource "Admin")))
