module Mappers

open MyWebApi.Data.SqlHydraGenerated
open Models

// ユーザーエンティティからDTOへの変換
let toUserDto (user: main.Users) : UserDto =
    { Id = user.Id
      Username = user.Username
      Email = user.Email }

// 製品エンティティからDTOへの変換
let toProductDto (product: main.Products) : ProductDto =
    { Id = product.Id
      Name = product.Name
      Description = product.Description
      Category = product.Category
      Price = product.Price
      Stock = product.Stock
      SKU = product.SKU
      IsActive = product.IsActive <> 0L
      CreatedAt = product.CreatedAt
      UpdatedAt = product.UpdatedAt }
