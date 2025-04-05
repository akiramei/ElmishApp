// Types.fs - Updated with ProductDetail support
module App.Types

open System
open App.Shared
open App.Infrastructure

// アプリケーションのタブ定義
type Tab =
    | Home
    | Counter
    | Products
    | Admin // 管理者タブを追加
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
    | ProductDetail of int
    | Admin // 管理者ダッシュボード
    | AdminUsers // ユーザー管理
    | AdminProducts // 製品管理
    | CustomTab of string
    | WithParam of string * string
    | WithQuery of string * Map<string, string>
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
    | Failed of Api.Types.ApiError

// ===== ドメイン別APIデータ =====

// ユーザードメインの状態
type UserApiData =
    { Users: FetchStatus<UserDto list>
      SelectedUser: FetchStatus<UserDto> option }

// 製品ドメインの状態 - 製品詳細を追加
type ProductApiData =
    { Products: FetchStatus<ProductDto list>
      SelectedProduct: FetchStatus<ProductDto> option
      // 詳細データの追加
      SelectedProductDetail: FetchStatus<ProductDetailDto> option }

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
      SelectedProduct = None
      SelectedProductDetail = None }

let initApiData =
    { UserData = initUserApiData
      ProductData = initProductApiData }

// ===== ドメイン別APIメッセージ =====

// ユーザー関連のAPIメッセージ
type UserApiMsg =
    | FetchUsers
    | FetchUsersSuccess of UserDto list
    | FetchUsersError of Api.Types.ApiError
    | FetchUser of int64
    | FetchUserSuccess of UserDto
    | FetchUserError of Api.Types.ApiError

// 製品関連のAPIメッセージ - 詳細系メッセージと削除・更新メッセージを追加
type ProductApiMsg =
    | FetchProducts
    | FetchProductsSuccess of ProductDto list
    | FetchProductsError of Api.Types.ApiError
    | FetchProduct of int64
    | FetchProductSuccess of ProductDto
    | FetchProductError of Api.Types.ApiError
    // 製品詳細用メッセージ
    | FetchProductDetail of int64
    | FetchProductDetailSuccess of ProductDetailDto
    | FetchProductDetailError of Api.Types.ApiError
    // 製品削除メッセージを追加
    | DeleteProduct of int64
    | DeleteProductSuccess
    | DeleteProductError of Api.Types.ApiError
    // 製品更新メッセージを追加
    | UpdateProduct of int64 * ProductUpdateDto
    | UpdateProductSuccess of ProductDetailDto
    | UpdateProductError of Api.Types.ApiError

// 製品関連のメッセージの拡張 - 編集と削除関連を追加
type ProductsMsg =
    | ChangePage of int
    | ChangePageSize of int
    | ToggleProductSelection of int
    | ToggleAllProducts of bool
    | ViewProductDetails of int
    | CloseProductDetails // 詳細表示を閉じるメッセージ
    | UpdatePageInfo of int // 総アイテム数を受け取りページング情報を更新
    | EditProduct of int // 製品編集モードへの切り替え
    | DeleteSelectedProducts // 選択された製品の削除
    | ChangeSort of string // ソート列の変更
    | ChangeSearch of string // 検索値の変更
    | SetSelectedProducts of Set<int> // 選択された製品IDのセットを設定
    | ClearSort // ソートをクリア

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
      SelectedIds: Set<int> // 選択された製品IDのセット
      ActiveSort: string option // 現在のソート列
      SortDirection: string // ソート方向（"asc" または "desc"）
      SearchValue: string } // 検索値

// 管理者関連のメッセージ（必要に応じて追加）
type AdminMsg =
    | LoadAdminData
    | ExportProducts
    | RunSystemDiagnostic

// アプリケーションのメッセージ
type Msg =
    | NavigateTo of Tab
    | CounterMsg of CounterMsg
    | RouteChanged of Route
    | CustomMsg of string * obj
    | NotificationMsg of NotificationMsg
    | PluginMsg of PluginMsg
    | ApiMsg of ApiMsg
    | ProductsMsg of ProductsMsg
    | AdminMsg of AdminMsg // 管理者関連メッセージを追加

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
          SelectedIds = Set.empty
          ActiveSort = None
          SortDirection = "asc"
          SearchValue = "" } }
