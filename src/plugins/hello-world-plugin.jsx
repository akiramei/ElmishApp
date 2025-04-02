// hello-world-plugin.jsx - 新しいモデル構造対応版
import React, { useState, useEffect } from 'react';

// プラグインID定義
const PLUGIN_ID = "hello-world";

// プラグイン固有のメッセージ
const HelloMsg = {
  INCREMENT_FSHARP: "IncrementCounter", // F#側の標準メッセージと同じ名前
  SAVE_LOCAL_COUNT: "SaveLocalCount",
};

// 新しいプラグインAPIを使用
plugin(PLUGIN_ID, {
  name: "Hello World Plugin",
  version: "1.0.0",

  // タブとして追加
  tab: "hello",

  // メッセージハンドラーの定義 - 新モデル構造対応
  update: function (args) {
    const messageType = args.messageType;
    const payload = args.payload;
    const model = args.model;
    
    switch (messageType) {
      case HelloMsg.SAVE_LOCAL_COUNT:
        // ローカルカウンターの値をプラグイン状態に保存
        return plugin.setState(
          PLUGIN_ID,
          {
            localCount: payload.count,
            lastSaved: new Date().toISOString(),
          },
          model
        );

      case HelloMsg.INCREMENT_FSHARP:
        // F#のカウンターを+1する - 新モデル構造対応
        return {
          ...model,
          CounterState: {
            ...model.CounterState,
            Counter: model.CounterState.Counter + 1
          }
        };

      default:
        return model;
    }
  },

  // JSXを使用したビュー実装 - 新モデル構造対応
  view: function (args) {
    // argsから引数を取り出す
    const model = args.model;
    const dispatch = args.dispatch;

    // プラグイン固有の状態を取得
    const pluginState = plugin.getState(PLUGIN_ID, model);

    // JSXコンポーネント定義
    const HelloComponent = () => {
      // 保存された値があればそれを初期値として使用、なければ0
      const initialCount = pluginState.localCount || 0;

      // ローカルカウンターの状態を管理
      const [count, setCount] = useState(initialCount);

      // 新モデル構造対応 - カウンター値を取得
      const fsharpCounter = plugin.counter.getValue(model);

      // APIデータの取得 - 新モデル構造対応
      const products = plugin.api.getProducts(model);
      const isProductsLoading = plugin.api.isProductsLoading(model);

      // F#側の値が変わった時に再レンダリングするためのeffect
      useEffect(() => {
        console.log("F# Counter value:", fsharpCounter);
      }, [fsharpCounter]);

      // プラグイン状態が変わった時に再レンダリングするためのeffect
      useEffect(() => {
        if (pluginState.localCount !== undefined && pluginState.localCount !== count) {
          setCount(pluginState.localCount);
        }
      }, [pluginState.localCount, count]);

      // カウンターが変更されたときの処理
      const incrementLocalCounter = () => {
        const newCount = count + 1;
        setCount(newCount);

        // F#状態に保存
        dispatch([HelloMsg.SAVE_LOCAL_COUNT, { count: newCount }]);
      };

      return (
        <div className="p-5 text-center">
          <h1 className="text-2xl font-bold mb-4">Hello World Plugin (新構造対応)</h1>

          <div className="mb-6 p-4 bg-blue-50 rounded-lg">
            <h2 className="text-lg font-semibold mb-2">モデル構造情報</h2>
            <p className="mb-2">F# Counter: {fsharpCounter}</p>
            <p className="mb-2">製品データ: {products.length} 件</p>
            <p className="text-sm text-gray-500">
              {isProductsLoading ? "データ読み込み中..." : "データ読み込み完了"}
            </p>
          </div>

          <div className="mb-6 p-4 bg-green-50 rounded-lg">
            <h2 className="text-lg font-semibold mb-2">ローカルカウンター</h2>
            <p className="text-xl font-bold mb-3">{count}</p>

            {pluginState.lastSaved && (
              <p className="mb-4 text-sm text-gray-500">
                Last saved: {new Date(pluginState.lastSaved).toLocaleTimeString()}
              </p>
            )}

            <div className="flex justify-center gap-2">
              <button
                className="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 transition-colors"
                onClick={incrementLocalCounter}
              >
                Local +1
              </button>

              <button
                className="px-4 py-2 bg-green-500 text-white rounded hover:bg-green-600 transition-colors"
                onClick={() => dispatch(HelloMsg.INCREMENT_FSHARP)}
              >
                F# Counter +1
              </button>
            </div>
          </div>
          
          {/* 製品データの表示 - 新モデル構造対応 */}
          <div className="mt-6 p-4 bg-gray-50 rounded-lg">
            <h2 className="text-lg font-semibold mb-2">製品データ (最初の3件)</h2>
            {isProductsLoading ? (
              <p>読み込み中...</p>
            ) : products.length > 0 ? (
              <ul className="text-left">
                {products.slice(0, 3).map(product => (
                  <li key={product.Id} className="mb-1 pb-1 border-b">
                    <span className="font-medium">{product.Name}</span>
                    <span className="ml-2 text-gray-500">¥{product.Price}</span>
                  </li>
                ))}
                {products.length > 3 && (
                  <li className="text-sm text-gray-500 mt-2">
                    他 {products.length - 3} 件のデータがあります
                  </li>
                )}
              </ul>
            ) : (
              <p className="text-gray-500">データがありません</p>
            )}
          </div>
        </div>
      );
    };

    // Reactコンポーネントをレンダリング
    return React.createElement(HelloComponent);
  },
});