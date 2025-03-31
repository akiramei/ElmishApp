module Mappers

open MyWebApi.Data.SqlHydraGenerated
open App.Shared

// ユーザーエンティティからDTOへの変換
let toUserDto (user: main.Users) : UserDto =
    { Id = int user.Id
      Username = user.Username
      Email = user.Email }

// 製品エンティティからDTOへの変換
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
