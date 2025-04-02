namespace App.Shared

/// ユーザーに関するDTO
type UserDto =
    { Id: int
      Username: string
      Email: string }

/// 製品一覧表示用DTO
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

/// 製品詳細表示用DTO - 全てのフィールドを含む
type ProductDetailDto =
    { Id: int
      Name: string
      Description: string option
      Category: string option
      Price: double
      Stock: int
      SKU: string
      IsActive: bool
      CreatedAt: string
      UpdatedAt: string option
      // 追加の詳細フィールド
      Public01: string option
      Public02: string option
      Public03: string option
      Public04: string option
      Public05: string option
      Public06: string option
      Public07: string option
      Public08: string option
      Public09: string option
      Public10: string option }
