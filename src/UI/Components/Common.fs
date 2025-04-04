module App.UI.Components.Common

open Feliz

// アクセシビリティを考慮したボタンコンポーネント
let accessibleButton (text: string) (onClick: unit -> unit) (className: string) =
    Html.button
        [ prop.className className
          prop.onClick (fun _ -> onClick ())
          prop.role "button"
          prop.tabIndex 0
          prop.ariaLabel text
          prop.text text ]

// スケルトンローディングコンポーネント
let skeletonLoader (className: string) =
    Html.div [ prop.className $"{className} animate-pulse bg-gray-200 rounded" ]

// エラーメッセージコンポーネント
let errorMessage (message: string) =
    Html.div
        [ prop.className "p-4 bg-red-50 border border-red-200 rounded-md"
          prop.role "alert"
          prop.ariaLabel "エラーメッセージ"
          prop.children
              [ Html.div
                    [ prop.className "flex items-center"
                      prop.children
                          [ Html.div
                                [ prop.className "flex-shrink-0"
                                  prop.children [ Html.div [ prop.className "h-5 w-5 text-red-400"; prop.text "×" ] ] ]
                            Html.div
                                [ prop.className "ml-3"
                                  prop.children
                                      [ Html.h3 [ prop.className "text-sm font-medium text-red-800"; prop.text "エラー" ]
                                        Html.div [ prop.className "mt-2 text-sm text-red-700"; prop.text message ] ] ] ] ] ] ]

// 成功メッセージコンポーネント
let successMessage (message: string) =
    Html.div
        [ prop.className "p-4 bg-green-50 border border-green-200 rounded-md"
          prop.role "alert"
          prop.ariaLabel "成功メッセージ"
          prop.children
              [ Html.div
                    [ prop.className "flex items-center"
                      prop.children
                          [ Html.div
                                [ prop.className "flex-shrink-0"
                                  prop.children [ Html.div [ prop.className "h-5 w-5 text-green-400"; prop.text "✓" ] ] ]
                            Html.div
                                [ prop.className "ml-3"
                                  prop.children
                                      [ Html.h3 [ prop.className "text-sm font-medium text-green-800"; prop.text "成功" ]
                                        Html.div [ prop.className "mt-2 text-sm text-green-700"; prop.text message ] ] ] ] ] ] ]

// ページ遷移アニメーション用のラッパー
let pageTransition (children: ReactElement list) =
    Html.div
        [ prop.className "transition-opacity duration-300 ease-in-out"
          prop.children children ]
