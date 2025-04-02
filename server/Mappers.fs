module Mappers

open MyWebApi.Data.SqlHydraGenerated
open App.Shared

// ユーザーエンティティからDTOへの変換
let toUserDto (user: main.Users) : UserDto =
    { Id = int user.Id
      Username = user.Username
      Email = user.Email }

// 製品エンティティから一覧表示用DTOへの変換
let toProductDto (product: main.Products) : ProductDto =
    { Id = int product.Id
      Name = product.Name
      Description = product.Description
      Category = product.Category
      Price = product.Price
      Stock = int product.Stock
      SKU = product.SKU
      IsActive = product.IsActive <> 0L
      CreatedAt = product.CreatedAt
      UpdatedAt = product.UpdatedAt }

// 製品エンティティから詳細表示用DTOへの変換
let toProductDetailDto (product: main.Products) : ProductDetailDto =
    { Id = int product.Id
      Name = product.Name
      Description = product.Description
      Category = product.Category
      Price = product.Price
      Stock = int product.Stock
      SKU = product.SKU
      IsActive = product.IsActive <> 0L
      CreatedAt = product.CreatedAt
      UpdatedAt = product.UpdatedAt
      // 追加の詳細フィールド
      Public01 = product.Public01
      Public02 = product.Public02
      Public03 = product.Public03
      Public04 = product.Public04
      Public05 = product.Public05
      Public06 = product.Public06
      Public07 = product.Public07
      Public08 = product.Public08
      Public09 = product.Public09
      Public10 = product.Public10 }
