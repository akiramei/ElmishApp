namespace App.Shared

/// API路のエンドポイント定義
module Routes =
    /// APIのベースパス
    [<Literal>]
    let ApiBase = "/api"

    /// ユーザー関連のエンドポイント
    module Users =
        [<Literal>]
        let GetAll = ApiBase + "/users"

        let GetById (id: int64) = ApiBase + "/users/" + string id

    /// 製品関連のエンドポイント
    module Products =
        [<Literal>]
        let GetAll = ApiBase + "/products"

        let GetById (id: int64) = ApiBase + "/products/" + string id

        // 製品詳細取得用の新エンドポイント
        let GetDetail (id: int64) =
            ApiBase + "/products/" + string id + "/detail"
