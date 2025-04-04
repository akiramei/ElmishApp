module App.UI.Theme.Animations

open Feliz
open Feliz.Styles

// アニメーションの種類を定義
type AnimationType =
    | Fade
    | Slide
    | Scale
    | Bounce

// アニメーションの方向を定義
type AnimationDirection =
    | In
    | Out

// アニメーションのクラスを取得
let getAnimationClass (animationType: AnimationType) (direction: AnimationDirection) =
    match animationType, direction with
    | Fade, In -> "animate-fade-in"
    | Fade, Out -> "animate-fade-out"
    | Slide, In -> "animate-slide-in"
    | Slide, Out -> "animate-slide-out"
    | Scale, In -> "animate-scale-in"
    | Scale, Out -> "animate-scale-out"
    | Bounce, In -> "animate-bounce-in"
    | Bounce, Out -> "animate-bounce-out"

// アニメーション付きのコンポーネント
let animatedComponent (animationType: AnimationType) (direction: AnimationDirection) (children: ReactElement list) =
    Html.div
        [ prop.className (getAnimationClass animationType direction)
          prop.children children ]

// ページ遷移アニメーション
let pageTransitionAnimation (children: ReactElement list) = animatedComponent Fade In children

// リストアイテムのアニメーション
let listItemAnimation (index: int) (children: ReactElement list) =
    Html.div
        [ prop.className "animate-fade-in"
          prop.custom ("style", sprintf "animation-delay: %dms" (index * 100))
          prop.children children ]

// ホバーエフェクト付きのコンポーネント
let hoverEffect (children: ReactElement list) =
    Html.div
        [ prop.className "transition-all duration-300 ease-in-out hover:scale-105"
          prop.children children ]

// ローディングスピナー
let loadingSpinner (size: string) =
    let sizeInt =
        match System.Int32.TryParse(size) with
        | true, value -> value
        | false, _ -> 40 // デフォルトサイズ

    Html.div
        [ prop.className "animate-spin rounded-full border-4 border-gray-200 border-t-blue-500"
          prop.custom ("style", sprintf "width: %dpx; height: %dpx;" sizeInt sizeInt)
          prop.children [] ]

// プログレスバー
let progressBar (progress: int) =
    Html.div
        [ prop.className "w-full bg-gray-200 rounded-full h-2.5"
          prop.children
              [ Html.div
                    [ prop.className "bg-blue-600 h-2.5 rounded-full transition-all duration-300"
                      prop.custom ("style", sprintf "width: %d%%" progress) ] ] ]
