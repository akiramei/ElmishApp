module Tests

open System
open Xunit
open App.Shared.Validation

[<Fact>]
let ``My test`` () = Assert.True(true)

[<Fact>]
let ``Username validation rules should have correct length constraints`` () =
    Assert.Equal(3, Username.MinLength)
    Assert.Equal(50, Username.MaxLength)

[<Fact>]
let ``ProductName validation rules should have correct length constraints`` () =
    Assert.Equal(2, ProductName.MinLength)
    Assert.Equal(100, ProductName.MaxLength)

[<Fact>]
let ``SKU validation rules should have correct length constraint`` () = Assert.Equal(8, SKU.Length)
