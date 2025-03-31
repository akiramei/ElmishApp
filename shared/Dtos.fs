namespace App.Shared

/// ユーザーに関するDTO
type UserDto =
    { Id: int
      Username: string
      Email: string }

/// 製品に関するDTO
type ProductDto =
    { Id: int
      Name: string
      Description: string option
      Category: string option
      Price: double
      Stock: int
      SKU: string
      IsActive: bool
      CreatedAt: string
      UpdatedAt: string option }
