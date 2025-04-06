module ApiTests

open Xunit
open FsUnit.Xunit
open System.Net.Http
open System.Text
open System.Text.Json
open MyWebApi.Tests.TestHelpers
open App.Shared
open MyWebApi.App

[<Fact>]
let ``GET /greeting should return greeting message`` () =
    async {
        initializeTestDatabase ()
        use client = createTestClient ()
        let! response = client.GetAsync("/greeting?name=Test") |> Async.AwaitTask
        response.StatusCode |> should equal System.Net.HttpStatusCode.OK

        let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
        let greeting = JsonSerializer.Deserialize<Models.Message>(content)
        greeting.Text |> should equal "Hello Test, from Giraffe API!"
    }

[<Fact>]
let ``GET /users should return list of users`` () =
    async {
        initializeTestDatabase ()
        use client = createTestClient ()
        let! response = client.GetAsync("/users") |> Async.AwaitTask
        response.StatusCode |> should equal System.Net.HttpStatusCode.OK

        let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
        let users = JsonSerializer.Deserialize<UserDto list>(content)
        users |> should not' (be Empty)
    }

[<Fact>]
let ``GET /products should return list of products`` () =
    async {
        initializeTestDatabase ()
        use client = createTestClient ()
        let! response = client.GetAsync("/products") |> Async.AwaitTask
        response.StatusCode |> should equal System.Net.HttpStatusCode.OK

        let! content = response.Content.ReadAsStringAsync() |> Async.AwaitTask
        let products = JsonSerializer.Deserialize<ProductDto list>(content)
        products |> should not' (be Empty)
    }
