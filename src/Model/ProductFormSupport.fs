// ProductFormSupport.fs - Helper module to initialize edit form state
module App.ProductFormSupport

open App.Types
open App.Shared
open App.Model.ProductDetailTypes

// Helper function to create initial edit form state from ProductDetailDto
let createEditFormState (product: ProductDetailDto) : ProductEditFormState =
    // Create the detailed form state first
    let detailedState = createFromProductDto product

    // Convert to basic form state
    let basicFields =
        Map
            [ "Code", product.Code
              "Name", product.Name
              "Description", Option.defaultValue "" product.Description
              "Category", Option.defaultValue "" product.Category
              "Price", string product.Price
              "Stock", string product.Stock
              "SKU", product.SKU
              "IsActive", string product.IsActive ]

    // Create additional fields map
    let additionalFields =
        Map
            [ "Public01", product.Public01
              "Public02", product.Public02
              "Public03", product.Public03
              "Public04", product.Public04
              "Public05", product.Public05
              "Public06", product.Public06
              "Public07", product.Public07
              "Public08", product.Public08
              "Public09", product.Public09
              "Public10", product.Public10 ]

    // Return the new state
    { BasicFields = basicFields
      AdditionalFields = additionalFields
      ValidationErrors = Map.empty
      HasErrors = false
      ActiveTab = BasicInfo }
