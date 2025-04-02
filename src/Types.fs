// Types.fs - Updated with ProductDetail route
module App.Types

open System
open App.Shared

// アプリケーションのタブ定義
type Tab =
    | Home
    | Counter
    | Products
    // カスタムタブのために拡張可能なタイプ
    | CustomTab of string

// プラグイン定義タイプ
type PluginDefinition =
    { Id: string
      Name: string
      Version: string
      Dependencies: string list
      Compatibility: string }

type LoadingPlugins =
    | Init
    | Loading
    | Done

// 通知レベルの定義
type NotificationLevel =
    | Information
    | Warning
    | Error

// 通知の一意なID
type NotificationId = System.Guid

// 単一の通知
type Notification =
    { Id: NotificationId
      Level: NotificationLevel
      Message: string
      Details: string option // 詳細情報（オプション）
      Source: string option // 通知の発生源
      Metadata: Map<string, obj> // 追加メタデータ（柔軟な拡張用）
      CreatedAt: DateTime
      AutoDismiss: bool // 自動クリア対象かどうか
      ExpiresAfter: float option } // 秒単位、Noneの場合は永続的

// 通知管理システム
type NotificationState =
    { Notifications: Notification list
      LastUpdated: DateTime option }

// 通知メッセージ
type NotificationMsg =
    | Add of Notification
    | Remove of NotificationId
    | ClearAll
    | ClearByLevel of NotificationLevel
    | Tick of DateTime

// ルート定義 - ProductDetailルートを追加
type Route =
    | Home
    | Counter
    | Products
    | ProductDetail of int // 製品IDをパラメータとして持つルートを追加
    | CustomTab of string
    | WithParam of string * string // resource * id
    | WithQuery of string * Map<string, string> // base path * query params
    | NotFound

type CounterMsg =
    | IncrementCounter
    | DecrementCounter

type PluginMsg =
    | PluginTabAdded of string
    | PluginRegistered of PluginDefinition
    | PluginsLoaded

// データ取得状態
type FetchStatus<'T> =
    | NotStarted
    | Loading
    | Success of 'T
    | Failed of ApiClient.ApiError

// ===== ドメイン別APIデータ =====

// ユーザードメインの状態
type UserApiData =
    { Users: FetchStatus<UserDto list>
      SelectedUser: FetchStatus<UserDto> option }

// 製品ドメインの状態
type ProductApiData =
    { Products: FetchStatus<ProductDto list>
      SelectedProduct: FetchStatus<ProductDto> option }

// 全体のAPIデータ
type ApiData =
    { UserData: UserApiData
      ProductData: ProductApiData }

// 初期状態の定義
let initUserApiData =
    { Users = NotStarted
      SelectedUser = None }

let initProductApiData =
    { Products = NotStarted
      SelectedProduct = None }

let initApiData =
    { UserData = initUserApiData
      ProductData = initProductApiData }

// ===== ドメイン別APIメッセージ =====

// ユーザー関連のAPIメッセージ
type UserApiMsg =
    | FetchUsers
    | FetchUsersSuccess of UserDto list
    | FetchUsersError of ApiClient.ApiError
    | FetchUser of int64
    | FetchUserSuccess of UserDto
    | FetchUserError of ApiClient.ApiError

// 製品関連のAPIメッセージ
type ProductApiMsg =
    | FetchProducts
    | FetchProductsSuccess of ProductDto list
    | FetchProductsError of ApiClient.ApiError
    | FetchProduct of int64
    | FetchProductSuccess of ProductDto
    | FetchProductError of ApiClient.ApiError

// APIメッセージのルート型
type ApiMsg =
    | UserApi of UserApiMsg
    | ProductApi of ProductApiMsg

// ページング情報
type PageInfo =
    { CurrentPage: int
      PageSize: int
      TotalItems: int
      TotalPages: int }

// 製品一覧の状態
type ProductsState =
    { PageInfo: PageInfo
      SelectedIds: Set<int> } // 選択された製品IDのセット

// 製品関連のメッセージの拡張 - CloseProductDetailsを追加
type ProductsMsg =
    | ChangePage of int
    | ChangePageSize of int
    | ToggleProductSelection of int
    | ToggleAllProducts of bool
    | ViewProductDetails of int
    | CloseProductDetails // 詳細表示を閉じるメッセージを追加
    | UpdatePageInfo of int // 追加: 総アイテム数を受け取りページング情報を更新

// アプリケーションのメッセージ
type Msg =
    | NavigateTo of Tab
    | CounterMsg of CounterMsg
    | RouteChanged of Route
    // カスタムメッセージを受け取るための汎用的なメッセージタイプ
    | CustomMsg of string * obj
    // 通知関連メッセージ
    | NotificationMsg of NotificationMsg
    // プラグイン関連メッセージ
    | PluginMsg of PluginMsg
    | ApiMsg of ApiMsg
    // 製品関連メッセージ
    | ProductsMsg of ProductsMsg

type HomeState = { Message: string }

type CounterState = { Counter: int }

type PluginState =
    { RegisteredPluginIds: string list
      LoadingPlugins: LoadingPlugins }

// アプリケーションのモデル
type Model =
    { CurrentRoute: Route
      CurrentTab: Tab
      CounterState: CounterState
      HomeState: HomeState
      // カスタム拡張用のデータストア
      CustomState: Map<string, obj>
      // 通知情報（旧ErrorStateの拡張版）
      NotificationState: NotificationState
      // プラグイン情報
      PluginState: PluginState
      ApiData: ApiData
      // 製品一覧の状態
      ProductsState: ProductsState }

// 初期状態
let init () =
    { CurrentRoute = Route.Home
      CurrentTab = Tab.Home
      CounterState = { Counter = 0 }
      HomeState = { Message = "Welcome to the F# + Fable + Feliz + Elmish app!" }
      CustomState = Map.empty
      NotificationState =
        { Notifications = List.empty
          LastUpdated = None }
      PluginState =
        { RegisteredPluginIds = []
          LoadingPlugins = LoadingPlugins.Init }
      ApiData = initApiData
      ProductsState =
        { PageInfo =
            { CurrentPage = 1
              PageSize = 10
              TotalItems = 0
              TotalPages = 1 }
          SelectedIds = Set.empty } }
