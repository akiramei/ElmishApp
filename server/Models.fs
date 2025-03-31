module Models

// ---------------------------------
// Models
// ---------------------------------

type Message = { Text: string }

type UserDto =
    { Id: int64
      Username: string
      Email: string }

// 製品のクライアント向けモデル定義
type ProductDto =
    { Id: int64
      Name: string
      Description: string option
      Category: string option
      Price: double
      Stock: int64
      SKU: string
      IsActive: bool
      CreatedAt: string
      UpdatedAt: string option }
