# 知っておくべき
- F#は推論を行うため、使用する関数や変数は事前に定義済みでなければならない
- F#ではトップレベルの関数にだけカスタム属性をつけることができる
- Elmish v4ではofSubは使えない。
Program.withSubscription subscribe
このようにsubscribe関数を定義して起動時に指定することになる。
- JavaScriptとの相互運用機能の一部はFable.Core.JsInteropから移動している
Fable.Core.JS.Constructors.Array.isArray
Fable.Core.JS.Constructors.Object.keys
など
- RequireQualifiedAccessAttributeの付いた型は完全修飾名で使う必要がある
Fable.Core.JS.Constructors.Array.isArray
Fable.Core.JS.Constructors.Object.keys
など
- App.fsprojのPackageReferenceを見て使用しているライブラリとバージョンを把握する
- F#では関数名や変数名に記号の'#'は使えない
