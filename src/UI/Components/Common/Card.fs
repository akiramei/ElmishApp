module App.UI.Components.Common.Card

open Feliz

// カードコンポーネント
let card (title: string option) (children: ReactElement list) =
    Html.div
        [ prop.className "bg-white rounded-lg shadow-md p-4"
          prop.children
              [ if title.IsSome then
                    Html.h3 [ prop.className "text-lg font-semibold mb-3"; prop.text title.Value ]
                yield! children ] ]
