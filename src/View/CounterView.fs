// CounterView.fs - Elmish MVUアーキテクチャに準拠した実装
module App.CounterView

open Feliz
open App.Types
open App.TabPluginDecorator

// トップレベルで定義されたイベントハンドラ
let private incrementCounter dispatch _ = dispatch (CounterMsg IncrementCounter)

let private decrementCounter dispatch _ = dispatch (CounterMsg DecrementCounter)

// カウンター値表示部分
let renderCounterValue (value: int) =
    Html.div
        [ prop.className "text-4xl font-bold text-blue-600 mb-4"
          prop.text (string value) ]

// ボタン部分
let renderButton (text: string) (ariaLabel: string) (className: string) (onClick) =
    Html.button
        [ prop.className className
          prop.onClick onClick
          prop.role "button"
          prop.tabIndex 0
          prop.ariaLabel ariaLabel
          prop.text text ]

// カウンターコンポーネント本体 - アニメーションなし
let renderCounterBase (counterState: CounterState) (dispatch: Msg -> unit) =
    Html.div
        [
          // アニメーションクラスを削除
          prop.className ""
          prop.children
              [ Html.div
                    [ prop.className "p-5 text-center"
                      prop.children
                          [ Html.h1 [ prop.className "text-2xl font-bold mb-4"; prop.text "Counter" ]

                            // カウンター値を表示するカード
                            Html.div
                                [ prop.className
                                      "bg-white rounded-lg shadow-md overflow-hidden transition-transform duration-300 hover:shadow-lg mb-6"
                                  prop.children
                                      [ Html.div
                                            [ prop.className "px-4 py-3 bg-gray-50 border-b"
                                              prop.children
                                                  [ Html.h3
                                                        [ prop.className "text-lg font-medium text-gray-900"
                                                          prop.text "現在の値" ] ] ]

                                        Html.div
                                            [ prop.className "p-4"
                                              prop.children
                                                  [
                                                    // カウンター値表示
                                                    renderCounterValue counterState.Counter ] ] ] ]

                            // ボタングループ
                            Html.div
                                [ prop.className "flex justify-center gap-4 mt-6"
                                  prop.children
                                      [
                                        // 増加ボタン
                                        renderButton
                                            "+"
                                            "増加"
                                            "px-6 py-2 bg-blue-500 text-white rounded-md hover:bg-blue-600 transition-colors"
                                            (incrementCounter dispatch)

                                        // 減少ボタン
                                        renderButton
                                            "-"
                                            "減少"
                                            "px-6 py-2 bg-red-500 text-white rounded-md hover:bg-red-600 transition-colors"
                                            (decrementCounter dispatch) ] ] ] ] ] ]

// 装飾機能を使ったカウンタータブのレンダリング
let renderCounter (model: Model) (dispatch: Msg -> unit) =
    decorateTabView CounterTab model dispatch (fun () -> renderCounterBase model.CounterState dispatch)
