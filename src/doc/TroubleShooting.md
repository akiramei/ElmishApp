# カスタムビュー表示のトラブルシューティング

カウンターページに拡張機能（Double ボタンなど）が表示されない場合は、以下の手順でトラブルシューティングしてください。

## 1. プラグインの読み込み確認

開発者ツール（F12）を開き、コンソールで以下を確認します：

```javascript
// プラグインが登録されているか確認
console.log(window.customViews);
console.log(window._pluginRegistry);
```

正しく登録されていれば、`window.customViews` に `counter-extensions` プロパティが存在するはずです。

## 2. レガシーサポートの確認

View.fs で使用されているビュー名は `counter-extensions` です。このビュー名で登録されていることを確認します：

```javascript
// カウンター拡張がレガシー方式で登録されているか確認
console.log("Counter extensions view available:", 
  window.customViews && window.customViews["counter-extensions"] !== undefined);
```

## 3. 手動でビューを確認

ブラウザコンソールで以下を実行して、ビューが正しく機能するか確認できます：

```javascript
// F#のモデルを擬似的に作成
const testModel = { Counter: 42, CustomState: {} };

// ビュー関数を呼び出し
const viewElement = window.customViews["counter-extensions"](testModel);

// 結果を確認
console.log(viewElement);
```

## 4. レガシー互換性のための手動登録

既存のプラグインシステムとの互換性のために、以下のコードをコンソールで実行して手動でビューを登録することもできます：

```javascript
// レガシーレジストリを初期化
window.customViews = window.customViews || {};

// カスタムビューを手動で登録
window.customViews["counter-extensions"] = function(model) {
  // Reactコンポーネントを定義
  const CounterExtension = function() {
    // Doubleボタンのクリック処理
    const handleDoubleClick = function() {
      window.appDispatch(["DoubleCounter", { currentValue: model.Counter }]);
    };
    
    return React.createElement('div', {
      className: 'mt-6 pt-6 border-t border-gray-200'
    }, [
      React.createElement('div', { 
        className: 'font-bold mb-3'
      }, 'Counter Extension (手動登録):'),
      React.createElement('button', {
        className: 'px-4 py-2 bg-green-500 text-white rounded hover:bg-green-600 transition-colors',
        onClick: handleDoubleClick
      }, 'Double')
    ]);
  };
  
  return React.createElement(CounterExtension);
};

console.log("Manual registration complete - reload counter tab");
```

## 5. View.fs の確認

ソースコードの `View.fs` で `getCustomView "counter-extensions"` の呼び出しが正しく行われているかを確認します。適切なエラーハンドリングとフォールバックが存在するか確認してください。

## 6. F#のブリッジ関数のデバッグ

F#側の `getCustomView` 関数をデバッグするために、以下をコンソールで実行します：

```javascript
// ブリッジ関数にデバッグログを追加
const originalGetCustomView = window.FSharpJsBridge.getCustomView;
window.FSharpJsBridge.getCustomView = function(viewName, model) {
  console.log("F# requesting view:", viewName);
  console.log("With model:", model);
  
  const result = originalGetCustomView(viewName, model);
  console.log("Result:", result);
  
  return result;
};
```

これらの手順によって、カスタムビューが表示されない原因を特定し、修正することができるでしょう。