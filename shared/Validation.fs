namespace App.Shared

/// 各種入力検証のルール
module Validation =
    /// ユーザー名の検証ルール
    module Username =
        [<Literal>]
        let MinLength = 3

        [<Literal>]
        let MaxLength = 50

    /// 製品名の検証ルール
    module ProductCode =
        [<Literal>]
        let MinLength = 2

        [<Literal>]
        let MaxLength = 100

    /// 製品名の検証ルール
    module ProductName =
        [<Literal>]
        let MinLength = 2

        [<Literal>]
        let MaxLength = 100


    /// 製品SKUの検証ルール
    module SKU =
        [<Literal>]
        let Length = 8
