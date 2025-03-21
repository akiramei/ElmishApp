// Types.fs
module App.Types

// アプリケーションのタブ定義
type Tab =
    | Home
    | Counter
    // カスタムタブのために拡張可能なタイプ
    | CustomTab of string

// アプリケーションのメッセージ
type Msg =
    | NavigateTo of Tab
    | IncrementCounter
    | DecrementCounter
    // カスタムメッセージを受け取るための汎用的なメッセージタイプ
    | CustomMsg of string * obj
    // エラー関連メッセージ
    | SetError of string
    | ClearError

// エラー状態管理
type ErrorState = {
    HasError: bool
    Message: string option
    ErrorCode: string option
    Source: string option  // エラーの発生源（コアかプラグインか）
}

// アプリケーションのモデル
type Model = {
    CurrentTab: Tab
    Counter: int
    Message: string
    // カスタム拡張用のデータストア
    CustomState: Map<string, obj>
    // エラー情報
    ErrorState: ErrorState
}

// 初期状態
let init() = {
    CurrentTab = Home
    Counter = 0
    Message = "Welcome to the F# + Fable + Feliz + Elmish app!"
    CustomState = Map.empty
    ErrorState = { 
        HasError = false
        Message = None
        ErrorCode = None
        Source = None
    }
}