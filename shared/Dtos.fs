namespace App.Shared

/// ユーザーに関するDTO
type UserDto =
    { Id: int64
      Username: string
      Email: string }

/// 製品に関するDTO
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
