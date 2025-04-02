# プラグイン開発ガイド - 新モデル構造対応版

このガイドは、F# + Fable + Feliz + Elmish プラグインアーキテクチャにおける新しいモデル構造（`ApiData`含む）に対応したプラグイン開発の方法を説明します。

## 1. モデル構造の変更点

以下の構造変更が行われました：

### 変更前

```fsharp
type Model =
    { CurrentRoute: Route
      CurrentTab: Tab
      Counter: int
      Message: string
      CustomState: Map<string, obj>
      // 他のプロパティ
    }
```

### 変更後

```fsharp
type Model =
    { CurrentRoute: Route
      CurrentTab: Tab
      CounterState: CounterState  // { Counter: int }
      HomeState: HomeState        // { Message: string }
      CustomState: Map<string, obj>
      ApiData: ApiData            // 新規追加
      ProductsState: ProductsState // 新規追加
      // 他のプロパティ
    }

// データ取得状態
type FetchStatus<'T> =
    | NotStarted
    | Loading
    | Success of 'T
    | Failed of ApiClient.ApiError

// APIデータ構造
type ApiData =
    { UserData: UserApiData       // ユーザーデータ
      ProductData: ProductApiData // 製品データ
    }
```

## 2. 推奨アクセスパターン

### 2.1 カウンター値へのアクセス

```javascript
// 非推奨
model.Counter // ✗ 古い構造

// 推奨
model.CounterState.Counter    // ✓ 直接アクセス
plugin.counter.getValue(model) // ✓✓ ヘルパー関数（最も推奨）
```

### 2.2 APIデータへのアクセス

```javascript
// 非推奨
model.Products // ✗ 古い構造 (存在しない)

// 推奨
model.ApiData.ProductData.Products // ✓ 直接アクセス (ただしStatus確認が必要)

// 最も推奨
plugin.api.getProducts(model)         // ✓✓ ヘルパー関数（Successでなければ空配列を返す）
plugin.api.isProductsLoading(model)   // ✓✓ ローディング状態チェック
plugin.api.getProductsStatus(model)   // ✓✓ 状態の取得
```

### 2.3 更新ハンドラー（update関数）

```javascript
// 古い構造（非推奨）
update: function(args) {
  // ...
  return {
    ...model,
    Counter: newValue
  };
}

// 新構造対応（推奨）
update: function(args) {
  // ...
  return {
    ...model,
    CounterState: {
      ...model.CounterState,
      Counter: newValue
    }
  };
}
```

## 3. プラグインヘルパー機能

新しいモデル構造をサポートするため、`plugin-helpers.js`に機能追加されました：

### 3.1 APIデータアクセス

```javascript
// 製品データ関連
plugin.api.getProducts(model)         // 製品一覧取得（配列、Successでなければ空配列）
plugin.api.getProductsStatus(model)   // 製品データの状態取得（"notStarted", "loading", "success", "failed"）
plugin.api.isProductsLoading(model)   // 読み込み中かどうか（boolean）
plugin.api.hasProductsError(model)    // エラーかどうか（boolean）

// ユーザーデータ関連
plugin.api.getUsers(model)            // ユーザー一覧取得
plugin.api.getUsersStatus(model)      // ユーザーデータの状態取得
plugin.api.isUsersLoading(model)      // 読み込み中かどうか
```

### 3.2 カウンター関連

```javascript
plugin.counter.getValue(model)       // カウンター値取得
plugin.counter.increment(dispatch)   // カウンター増加ショートカット
plugin.counter.decrement(dispatch)   // カウンター減少ショートカット
```

### 3.3 ProductsState関連

```javascript
plugin.products.getPageInfo(model)           // ページング情報取得
plugin.products.getSelectedIds(model)        // 選択中のID取得
plugin.products.isSelected(model, productId) // 特定IDが選択されているか
plugin.products.changePage(dispatch, page)   // ページ変更ショートカット
```

## 4. ディスパッチメッセージの変更点

F#側のモデル構造に合わせて、ディスパッチメッセージも変更されています：

### 4.1 カウンター関連メッセージ

```javascript
// 古い形式（非推奨）
dispatch("IncrementCounter");

// 新しい形式（推奨）
dispatch("CounterMsg", { type: "IncrementCounter" });

// ヘルパー関数（より推奨）
plugin.counter.increment(dispatch);
```

### 4.2 API関連メッセージ

```javascript
// API操作：製品データ取得
dispatch("ApiMsg", { type: "ProductApi", action: "FetchProducts" });

// API操作：製品詳細取得
dispatch("ApiMsg", { type: "ProductApi", action: "FetchProduct", id: 123 });

// API操作：ユーザーデータ取得
dispatch("ApiMsg", { type: "UserApi", action: "FetchUsers" });
```

### 4.3 ProductsStateメッセージ

```javascript
// ページング操作
dispatch("ProductsMsg", { type: "ChangePage", page: 2 });

// ページサイズ変更
dispatch("ProductsMsg", { type: "ChangePageSize", size: 25 });

// 製品選択トグル
dispatch("ProductsMsg", { type: "ToggleProductSelection", id: 42 });

// 詳細表示
dispatch("ProductsMsg", { type: "ViewProductDetails", id: 42 });
```

## 5. コード例

### 5.1 基本的なプラグイン（新構造対応）

```javascript
plugin("my-plugin", {
  name: "My Plugin",
  version: "1.0.0",
  
  // update関数 - 新構造対応
  update: function(args) {
    const { messageType, payload, model } = args;
    
    switch (messageType) {
      case "MyCustomMessage":
        // カウンター値を更新
        return {
          ...model,
          CounterState: {
            ...model.CounterState,
            Counter: payload.value
          }
        };
        
      default:
        return model;
    }
  },
  
  // view関数 - 新構造対応
  view: function(args) {
    const { model, dispatch } = args;
    
    // ヘルパー関数でデータ取得
    const counter = plugin.counter.getValue(model);
    const products = plugin.api.getProducts(model);
    const isLoading = plugin.api.isProductsLoading(model);
    
    // Reactコンポーネント
    const MyComponent = function() {
      return React.createElement('div', {}, [
        React.createElement('h2', {}, `Counter: ${counter}`),
        React.createElement('button', {
          onClick: () => plugin.counter.increment(dispatch)
        }, 'Increment'),
        
        // 製品データ表示
        isLoading 
          ? React.createElement('p', {}, 'Loading...')
          : React.createElement('p', {}, `${products.length} products available`)
      ]);
    };
    
    return React.createElement(MyComponent);
  }
});
```

### 5.2 JSXを使ったプラグイン（新構造対応）

```jsx
// JSXの例 - 新構造対応
import React, { useState, useEffect } from 'react';

plugin("jsx-plugin", {
  name: "JSX Plugin",
  version: "1.0.0",
  
  // 更新関数
  update: function(args) {
    const { messageType, payload, model } = args;
    
    // モデル更新処理
    switch (messageType) {
      // ...
    }
    
    return model;
  },
  
  // JSXビュー
  view: function(args) {
    const { model, dispatch } = args;
    
    // カスタムコンポーネント
    const MyJsxComponent = () => {
      // 新構造からデータ取得
      const counter = plugin.counter.getValue(model);
      const products = plugin.api.getProducts(model);
      
      // ローカルの状態管理
      const [localState, setLocalState] = useState(0);
      
      return (
        <div className="p-4">
          <h2 className="text-lg font-bold">JSX Plugin (新構造対応)</h2>
          
          <div className="mt-4">
            <p>F# Counter: {counter}</p>
            <p>Local State: {localState}</p>
            <p>Products: {products.length}</p>
            
            <div className="flex space-x-2 mt-2">
              <button 
                className="px-3 py-1 bg-blue-500 text-white rounded"
                onClick={() => plugin.counter.increment(dispatch)}
              >
                Increment F#
              </button>
              
              <button
                className="px-3 py-1 bg-green-500 text-white rounded"
                onClick={() => setLocalState(localState + 1)}
              >
                Increment Local
              </button>
            </div>
          </div>
        </div>
      );
    };
    
    return React.createElement(MyJsxComponent);
  }
});
```

## 6. テストとデバッグのヒント

### 6.1 モデル構造の確認

開発者コンソールでモデル構造を確認できます：

```javascript
// デバッグ用 - モデル構造確認
console.log("Model structure:", model);
console.log("CounterState:", model.CounterState);
console.log("ApiData:", model.ApiData);
console.log("ProductsState:", model.ProductsState);

// ヘルパー関数を使用
console.log("Counter value:", plugin.counter.getValue(model));
console.log("Products:", plugin.api.getProducts(model));
console.log("Products status:", plugin.api.getProductsStatus(model));
```

### 6.2 プラグイン状態管理のデバッグ

```javascript
// プラグイン固有の状態取得
const pluginState = plugin.getState("my-plugin-id", model);
console.log("Plugin state:", pluginState);

// 更新関数でのデバッグ
update: function(args) {
  console.log("Update called with:", args);
  const { messageType, payload, model } = args;
  
  // 処理...
  
  const updatedModel = { /* 更新されたモデル */ };
  console.log("Updated model:", updatedModel);
  return updatedModel;
}
```

## 7. まとめ

新しいモデル構造への対応は以下のポイントを押さえることが重要です：

1. **ネストされた構造へのアクセス**: 直接アクセスする場合はネストされた構造を正しく参照
2. **ヘルパー関数の活用**: `plugin.api.*`や`plugin.counter.*`などのヘルパー関数を優先的に使用
3. **状態の確認**: APIデータは`FetchStatus`型で状態を持つため、適切に状態を確認
4. **ディスパッチメッセージの使い分け**: 新しいメッセージ体系に合わせたディスパッチ

これらのガイドラインに従うことで、新しいモデル構造に対応したプラグインを効率的に開発できます。また、プラグインヘルパー機能を最大限に活用することで、コードの可読性と保守性を高めることができます。

## 8. 移行チェックリスト

既存のプラグインを新構造に対応させる際は、以下のチェックリストを参考にしてください：

1. [ ] Counter → CounterState.Counter への参照変更
2. [ ] Message → HomeState.Message への参照変更
3. [ ] ヘルパー関数の活用 (plugin.counter.*, plugin.api.*)
4. [ ] update関数での新構造に対応した更新処理
5. [ ] ディスパッチメッセージの形式変更
6. [ ] 不要な後方互換性コードの削除

## 9. 補足資料

より詳細な情報や特殊なケースについては、以下も参照してください：

- `Interop.fs` - F#とJavaScript間のモデル変換ロジック
- `plugin-helpers.js` - プラグイン開発ヘルパー関数の実装
- サンプルプラグイン - `counter-extension.js`, `hello-world-plugin.jsx` など