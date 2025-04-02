// jsx-plugin-example.jsx - 新しいモデル構造対応版
// 注意: このファイルはBabelやesbuildなどでJSXトランスパイルが必要です

// Reactをインポート
import React, { useState, useEffect } from "react";

// プラグインID定義
const PLUGIN_ID = "jsx-example";

// プラグイン固有のメッセージ
const JsxMsg = {
  RESET_COUNTERS: "ResetCounters",
  SAVE_NOTES: "SaveNotes",
  LOAD_PRODUCTS: "LoadProducts",
};

// 新しいプラグインAPIを使用してJSXプラグインを登録
plugin(PLUGIN_ID, {
  name: "JSX Example Plugin",
  version: "1.0.0",

  // タブとして追加
  tab: "jsx-demo",

  // 更新関数 - 新モデル構造対応
  update: function (args) {
    const messageType = args.messageType;
    const payload = args.payload;
    const model = args.model;
    
    switch (messageType) {
      case JsxMsg.RESET_COUNTERS:
        // カウンターをリセット - 新モデル構造対応
        return {
          ...model,
          CounterState: {
            ...model.CounterState,
            Counter: payload.value || 0,
          }
        };

      case JsxMsg.SAVE_NOTES:
        // 名前空間付きの状態管理を使用
        return plugin.setState(
          PLUGIN_ID,
          {
            notes: payload.notes,
            lastSaved: new Date().toLocaleString(),
          },
          model
        );
        
      case JsxMsg.LOAD_PRODUCTS:
        // 製品データ読み込みアクションを発行
        // このメッセージはF#側でProductApiメッセージに変換される
        // 実際のデータ取得はモデルを変更しないため、modelをそのまま返す
        return model;

      default:
        return model;
    }
  },

  // JSXを使ったビュー実装 - 新モデル構造対応
  view: function (args) {
    const model = args.model;
    const dispatch = args.dispatch;

    // プラグイン固有の状態を取得
    const pluginState = plugin.getState(PLUGIN_ID, model);

    // JSXコンポーネント定義
    const JSXDemoView = () => {
      // ローカルのカウンター状態
      const [localCounter, setLocalCounter] = useState(0);
      const [notes, setNotes] = useState(pluginState.notes || "");
      
      // 新しいモデル構造からデータ取得 - 各種ヘルパー関数を使用
      const fsharpCounter = plugin.counter.getValue(model);
      const products = plugin.api.getProducts(model);
      const productsStatus = plugin.api.getProductsStatus(model);
      const isProductsLoading = plugin.api.isProductsLoading(model);

      // コンポーネントがマウントされたときの処理
      useEffect(() => {
        console.log("JSX Demo component mounted");
        return () => {
          console.log("JSX Demo component unmounted");
        };
      }, []);

      // モデルのカウンター値が変更されたときの処理
      useEffect(() => {
        console.log("F# Counter changed:", fsharpCounter);
      }, [fsharpCounter]);

      // プラグイン状態が変更されたときのノート更新
      useEffect(() => {
        if (pluginState.notes !== undefined && pluginState.notes !== notes) {
          setNotes(pluginState.notes);
        }
      }, [pluginState.notes]);

      // ローカルカウンター増加処理
      const incrementLocalCounter = () => {
        setLocalCounter((prev) => prev + 1);
      };

      // F#カウンター増加処理 - 新モデル構造対応
      const incrementFSharpCounter = () => {
        dispatch(["CounterMsg", { type: "IncrementCounter" }]);
      };

      // F#カウンターリセット処理
      const resetCounters = () => {
        setLocalCounter(0);
        dispatch([JsxMsg.RESET_COUNTERS, { value: 0 }]);
      };

      // ノート更新処理
      const handleNotesChange = (e) => {
        setNotes(e.target.value);
      };

      // ノート保存処理
      const saveNotes = () => {
        dispatch([JsxMsg.SAVE_NOTES, { notes }]);
      };
      
      // 製品データ読み込み処理
      const loadProductsData = () => {
        // APIリクエスト発行 - 直接F#側のAPIメッセージをディスパッチ
        dispatch(["ApiMsg", { type: "ProductApi", action: "FetchProducts" }]);
      };

      return (
        <div className="p-5">
          <h1 className="text-2xl font-bold mb-4">JSX Plugin Example (新モデル構造対応)</h1>

          <div className="bg-white rounded-lg shadow p-6 mb-6">
            <h2 className="text-xl font-semibold mb-3">Counter Comparison</h2>

            <div className="grid grid-cols-2 gap-4 mb-4">
              <div className="border rounded p-3 text-center">
                <div className="text-lg font-medium">Local State Counter</div>
                <div className="text-3xl font-bold my-2">{localCounter}</div>
                <button
                  className="px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 transition-colors"
                  onClick={incrementLocalCounter}
                >
                  Increment Local
                </button>
              </div>

              <div className="border rounded p-3 text-center">
                <div className="text-lg font-medium">F# Global Counter</div>
                <div className="text-3xl font-bold my-2">{fsharpCounter}</div>
                <button
                  className="px-4 py-2 bg-green-500 text-white rounded hover:bg-green-600 transition-colors"
                  onClick={incrementFSharpCounter}
                >
                  Increment F#
                </button>
              </div>
            </div>

            <button
              className="w-full py-2 bg-gray-500 text-white rounded hover:bg-gray-600 transition-colors"
              onClick={resetCounters}
            >
              Reset Both Counters
            </button>
          </div>

          {/* 製品データセクション (新モデル構造を活用) */}
          <div className="bg-white rounded-lg shadow p-6 mb-6">
            <h2 className="text-xl font-semibold mb-3">Products Data</h2>
            <div className="mb-4">
              <div className="flex items-center justify-between mb-3">
                <div>
                  <span className="font-medium">Status: </span>
                  <span className={
                    isProductsLoading 
                      ? "text-blue-500" 
                      : productsStatus === "success" 
                        ? "text-green-500" 
                        : "text-red-500"
                  }>
                    {productsStatus}
                  </span>
                </div>
                <button
                  className="px-3 py-1 bg-blue-500 text-white text-sm rounded hover:bg-blue-600 transition-colors"
                  onClick={loadProductsData}
                >
                  Refresh Data
                </button>
              </div>
              
              <div className="border rounded overflow-hidden">
                <div className="bg-gray-100 p-2 font-medium border-b">
                  Product List ({products.length} items)
                </div>
                <div className="p-2 max-h-40 overflow-y-auto">
                  {isProductsLoading ? (
                    <div className="text-center py-2 text-gray-500">Loading...</div>
                  ) : products.length > 0 ? (
                    <ul className="divide-y">
                      {products.slice(0, 5).map(product => (
                        <li key={product.Id} className="py-1">
                          <div className="font-medium">{product.Name}</div>
                          <div className="text-sm text-gray-500">
                            ¥{product.Price.toLocaleString()} - {product.Stock} in stock
                          </div>
                        </li>
                      ))}
                      {products.length > 5 && (
                        <li className="py-1 text-center text-sm text-gray-500">
                          And {products.length - 5} more items...
                        </li>
                      )}
                    </ul>
                  ) : (
                    <div className="text-center py-2 text-gray-500">No products available</div>
                  )}
                </div>
              </div>
            </div>
          </div>

          <div className="bg-white rounded-lg shadow p-6">
            <h2 className="text-xl font-semibold mb-3">Persistent Notes</h2>
            <textarea
              className="w-full border rounded p-2 mb-3"
              rows="4"
              value={notes}
              onChange={handleNotesChange}
              placeholder="Type your notes here..."
            ></textarea>

            <div className="flex justify-end">
              <button
                className="px-4 py-2 bg-purple-500 text-white rounded hover:bg-purple-600 transition-colors"
                onClick={saveNotes}
              >
                Save Notes
              </button>
            </div>

            {pluginState.lastSaved && (
              <div className="mt-2 text-sm text-gray-500">
                Last saved: {pluginState.lastSaved}
              </div>
            )}
          </div>
        </div>
      );
    };

    // コンポーネントをレンダリング
    return React.createElement(JSXDemoView);
  },
});