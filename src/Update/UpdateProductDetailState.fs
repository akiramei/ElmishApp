module App.UpdateProductsDetailState

open Elmish
open App.Types

let updateProductDetailState (msg: ProductDetailMsg) (model: Model) : Model * Cmd<Msg> =
    match msg with
    | EnterEditMode ->
        { model with
            ProductDetailState =
                { model.ProductDetailState with
                    IsEditMode = true } },
        Cmd.none

    | ExitEditMode ->
        { model with
            ProductDetailState =
                { model.ProductDetailState with
                    IsEditMode = false } },
        Cmd.none

    | CloseDetailView ->
        { model with
            ProductDetailState =
                { model.ProductDetailState with
                    IsEditMode = false } },
        Cmd.ofMsg (ProductsMsg CloseProductDetails)
