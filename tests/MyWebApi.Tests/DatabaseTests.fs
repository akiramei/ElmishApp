module DatabaseTests

open Xunit
open FsUnit.Xunit
open MyWebApi.Tests.TestHelpers
open MyWebApi.App
open System.Threading.Tasks

[<Fact>]
let ``fetchAllUsers should return list of users`` () =
    task {
        initializeTestDatabase ()
        let! users = fetchAllUsers () |> Async.AwaitTask
        users |> should not' (be Empty)

        // ユーザーの構造を確認
        let firstUser = users.Head
        firstUser.Id |> should be (greaterThan 0)
        firstUser.Username |> should not' (be Empty)
        firstUser.Email |> should not' (be Empty)
    }

[<Fact>]
let ``fetchAllProducts should return list of products`` () =
    task {
        initializeTestDatabase ()
        let! products = fetchAllProducts () |> Async.AwaitTask
        products |> should not' (be Empty)

        // 製品の構造を確認
        let firstProduct = products.Head
        firstProduct.Id |> should be (greaterThan 0)
        firstProduct.Name |> should not' (be Empty)
        firstProduct.Price |> should be (greaterThan 0.0)
        firstProduct.Stock |> should be (greaterThan 0)
    }

[<Fact>]
let ``fetchProductById should return product when exists`` () =
    task {
        initializeTestDatabase ()
        // 最初の製品を取得
        let! products = fetchAllProducts () |> Async.AwaitTask
        let firstProduct = products.Head

        // その製品をIDで取得
        let! product = fetchProductById (int64 firstProduct.Id) |> Async.AwaitTask
        product.IsSome |> should equal true

        let foundProduct = product.Value
        foundProduct.Id |> should equal firstProduct.Id
        foundProduct.Name |> should equal firstProduct.Name
    }
