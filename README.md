# F# + Fable + Feliz + Elmish プラグインアーキテクチャ

## プロジェクト概要

このプロジェクトは、F#のFable+Feliz+Elmishを使用してMVU（Model-View-Update）アーキテクチャに基づいたコアパッケージを開発し、JavaScriptによるカスタマイズを可能にするプラグインシステムを実現するためのサンプルです。

コアパッケージはF#で開発され、顧客ごとのカスタマイズはJavaScriptによるプラグインとして実装されます。このアプローチにより、F#開発者が少ない環境でも、JavaScript開発者が効率的にカスタマイズを行うことができます。

## アーキテクチャの特徴

- **MVUアーキテクチャの一貫性**: F#側のコアパッケージが厳密にMVUパターンに従い、データの流れを一方向に保つ
- **プラグインシステム**: カスタマイズをプラグイン形式で分離し、コアパッケージの安定性を確保
- **動的ローディング**: 設定ファイルに基づいてプラグインを動的に読み込む機能
- **タイプセーフ**: F#の型システムを活かした安全な設計
- **新しいプラグインAPI**: グローバルオブジェクトを隠蔽し、シンプルなインターフェースを提供するヘルパーライブラリ

## ファイル構成

### F#側コードファイル

- **Types.fs**: モデルとメッセージの定義
- **PluginSystem.fs**: プラグイン管理システム
- **PluginLoader.fs**: プラグイン動的読み込み機能
- **Interop.fs**: JavaScript連携機能
- **Subscription.fs**: サブスクリプション管理
- **Update.fs**: 状態更新関数
- **View.fs**: UIレンダリング関数
- **App.fs**: アプリケーションエントリーポイント

### JavaScript側コードファイル

- **plugin-helpers.js**: 新しいプラグイン開発API（`AppPlugins`）
- **counter-extension.js**: カウンターページ拡張プラグイン
- **slider-tab.js**: スライダータブプラグイン
- **example-jsx-plugin.js**: JSXを使用したプラグイン例
- **plugins.json**: プラグイン設定ファイル

## コア機能

- **ホームタブ**: シンプルなメッセージ表示ページ
- **カウンタータブ**: インクリメント・デクリメントボタンを持つカウンター
- **カスタムプラグイン**: 
  - カウンター拡張（2倍にするボタン追加）
  - スライダータブ（0-100の範囲で値を調整できるスライダー）
  - JSXプラグイン例（ビルドプロセスを説明）

## プラグインシステムの主要コンポーネント

### F#側

#### プラグイン定義

```fsharp
// プラグイン定義タイプ
type PluginDefinition = {
    Id: string
    Name: string
    Version: string
    Dependencies: string list
    Compatibility: string
}

// 登録済みプラグイン情報
type RegisteredPlugin = {
    Definition: PluginDefinition
    Views: Map<string, obj -> Feliz.ReactElement>
    UpdateHandlers: Map<string, obj -> obj -> obj>
    CommandHandlers: Map<string, obj -> unit>
    Tabs: string list
}
```

#### プラグインローダー

```fsharp
// プラグイン設定を外部JSONから読み込む
[<Emit("fetch($0).then(r => r.json())")>]
let fetchJson (url: string) : JS.Promise<obj> = jsNative

// 動的スクリプト読み込み
[<Emit("new Promise((resolve, reject) => { const script = document.createElement('script'); script.src = $0; script.onload = () => resolve(); script.onerror = () => reject(new Error('Failed to load script: ' + $0)); document.head.appendChild(script); })")>]
let loadScript (url: string) : JS.Promise<unit> = jsNative
```

#### サブスクリプション

```fsharp
// Elmish v4スタイルのサブスクリプション
let subscribe (model: Model) =
    [
        // プラグインローダーサブスクリプション
        [ "pluginLoader" ], pluginLoader
    ]
```

### JavaScript側

#### 新しいプラグインAPI

```javascript
// プラグインビルダーを使ったプラグイン定義
const builder = AppPlugins.createBuilder(
    "plugin-id",
    "Plugin Name",
    "1.0.0"
);

// ビューの追加
builder.addView("view-id", (model) => {
    // Reactコンポーネントを返す
    return React.createElement(...);
});

// 更新ハンドラーの追加
builder.addUpdateHandler("message-type", (payload, model) => {
    // 更新されたモデルを返す
    return { ...model, someProperty: newValue };
});

// タブの追加
builder.addTab("tab-id");

// プラグイン登録
builder.register();
```

## 統合の流れ

1. F#アプリケーションが起動
2. プラグインローダーがプラグインヘルパーライブラリを読み込む
3. プラグインローダーが静的および動的プラグインを読み込む
4. プラグインがF#側に登録される
5. ユーザーがUIで操作を行うと：
   - F#側のメッセージディスパッチが呼び出される
   - カスタムメッセージの場合、プラグインの更新関数が呼び出される
   - 更新されたモデルでビューが再レンダリングされる

## プラグイン開発ガイド（JavaScript開発者向け）

### 旧式と新式の開発方法

#### 旧式のプラグイン開発（グローバルオブジェクトを直接操作）

```javascript
(function() {
    // グローバルオブジェクトに直接追加
    window.customViews = window.customViews || {};
    window.customViews["my-view"] = function(model) {
        // ...
    };
    
    window.customUpdates = window.customUpdates || {};
    window.customUpdates["my-message"] = function(payload, model) {
        // ...
    };
    
    window.customTabs = window.customTabs || [];
    window.customTabs.push("my-tab");
})();
```

#### 新式のプラグイン開発（`AppPlugins` APIを使用）

```javascript
// プラグインビルダーを作成
const builder = AppPlugins.createBuilder(
    "my-plugin",
    "My Plugin",
    "1.0.0"
);

// ビューを定義
const renderMyView = (model) => {
    // ...
};

// 更新ハンドラーを定義
const handleMyMessage = (payload, model) => {
    // ...
};

// ビルダーにコンポーネントと更新ハンドラーを追加
builder
    .addView("my-view", renderMyView)
    .addUpdateHandler("my-message", handleMyMessage)
    .addTab("my-tab")
    .register();
```

### JSXを使ったプラグイン開発

JSXを使用するには、ビルドプロセスが必要です。以下は、JSXでプラグインを開発する例です（ビルド前のコード）：

```jsx
// プラグインビルダーを作成
const builder = AppPlugins.createBuilder(
    "example-jsx-plugin",
    "Example JSX Plugin",
    "1.0.0"
);

// JSXを使用したコンポーネント
const ExampleJsxComponent = (model) => {
    const [count, setCount] = React.useState(0);

    return (
        <div className="example-jsx-plugin">
            <h1>Example JSX Plugin</h1>
            <p>F# Counter Value: {model.Counter}</p>
            <button onClick={() => AppPlugins.dispatch("IncrementCounter", {})}>
                Increment F# Counter
            </button>
        </div>
    );
};

// プラグインの登録
builder
    .addView("example-jsx", ExampleJsxComponent)
    .addTab("example-jsx")
    .register();
```

### 状態管理のルール

1. **永続的な状態はF#側のモデルで管理**:
   - 重要なアプリケーション状態はF#モデルに保存
   - JavaScript側は`model.CustomState`マップを通じて永続データを保存

2. **一時的なUI状態はReactのフックで管理**:
   - フォームの入力値やUIの一時的な状態は`useState`で管理
   - コンポーネントのマウント/アンマウント処理は`useEffect`で管理

3. **一方向データフロー**:
   - モデルの更新は必ずdispatchを通じて行う
   - 直接モデルを変更しない

### メッセージングのパターン

```javascript
// F#側にメッセージを送信（旧式）
window.appDispatch(["MessageType", { key: "value" }]);

// F#側にメッセージを送信（新式）
AppPlugins.dispatch("MessageType", { key: "value" });

// 更新関数
function updateHandler(payload, model) {
    console.log("Received payload:", payload);
    return {
        ...model,
        CustomState: {
            ...model.CustomState,
            myValue: payload.key
        }
    };
}
```

## デバッグのヒント

- F#側のモデル更新がJavaScript側に反映されない場合、更新関数の戻り値を確認
- JavaScriptからのメッセージがF#側に届かない場合、配列形式 `["type", payload]` になっているか確認
- プラグインが読み込まれない場合、コンソールでエラーメッセージを確認
- 新APIを使用する場合、plugin-helpers.jsが正しく読み込まれているか確認

## F#+Fable+Feliz+Elmishプラグインアーキテクチャ図
```mermaid
flowchart TD
    subgraph "F# Core Package"
        Model["Model\n(F# Record)"] --> Update["Update\n(F# Function)"]
        Update --> Model
        Model --> View["View\n(F# Function)"]
        View --> UserInteraction["User Interaction"]
        UserInteraction --> Dispatch["F# Msg Dispatch"]
        Dispatch --> Update
        
        subgraph "Plugin System"
            PS["Plugin System"] --> PL["Plugin Loader"]
            PS --> PR["Plugin Registry"]
            PL --> DL["Dynamic Loading"]
            PL --> SL["Static Loading"]
            PL --> PH["Plugin Helpers API"]
        end
        
        PR --> Update
        PR --> View
    end
    
    subgraph "JavaScript Plugins"
        PA["AppPlugins API"]
        
        subgraph "Counter Plugin"
            CPD["Plugin Definition"] --> CPV["Custom Views"]
            CPD --> CPU["Update Handlers"]
            CPD --> CPC["Command Handlers"]
        end
        
        subgraph "Slider Plugin"
            SPD["Plugin Definition"] --> SPV["Custom Views"]
            SPD --> SPU["Update Handlers"]
            SPD --> SPC["Command Handlers"]
        end
        
        subgraph "JSX Plugin"
            JPD["Plugin Definition"] --> JPV["JSX Components"]
            JPV --> JPC["Compiled JS"]
        end
    end
    
    PH -.-> PA
    PA -.-> CPD
    PA -.-> SPD
    PA -.-> JPD
    DL -.-> CPD
    DL -.-> SPD
    DL -.-> JPD
    View -.-> CPV
    View -.-> SPV
    View -.-> JPC
    Dispatch -.-> CPU
    Dispatch -.-> SPU
    CPU -.-> PR
    SPU -.-> PR
    
    classDef core fill:#dfd,stroke:#393
    classDef plugin fill:#ddf,stroke:#339
    classDef jsPlugin fill:#ffd,stroke:#993
    classDef jsxPlugin fill:#ffe,stroke:#773
    classDef interaction fill:#fdd,stroke:#933
    
    class Model,Update,View,PS,PL,PR,DL,SL,PH core
    class CPD,CPV,CPU,CPC jsPlugin
    class SPD,SPV,SPU,SPC jsPlugin
    class JPD,JPV,JPC jsxPlugin
    class UserInteraction interaction
    class Dispatch plugin
    class PA plugin
```

## 主な改良点

- プラグイン特有のロジックをUpdate.fsから削除し、プラグインシステムを通じて処理するように変更
- プラグインがタブを追加した際の再描画メカニズムを追加
- プラグイン開発を簡素化するAppPlugins APIを導入
- Modelにプラグイン情報を追加して状態管理を改善
- JSXを使用したプラグイン開発をサポート

## 補足
- ./docにある文書も読むこと