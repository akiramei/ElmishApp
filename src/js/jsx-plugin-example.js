// jsx-plugin-example.js - improved version with namespaced state
// 注意: このファイルはBabelやesbuildなどでJSXトランスパイルが必要です

// Reactをインポート
import React, { useState, useEffect } from "react";

// プラグインID定義
const PLUGIN_ID = "jsx-example";

// プラグイン固有のメッセージ
const JsxMsg = {
  RESET_COUNTERS: "ResetCounters",
  SAVE_NOTES: "SaveNotes",
};

// 新しいプラグインAPIを使用してJSXプラグインを登録
plugin(PLUGIN_ID, {
  name: "JSX Example Plugin",
  version: "1.0.0",

  // タブとして追加
  tab: "jsx-demo",

  // 更新関数
  update: function (args) {
    const messageType = args.messageType;
    const payload = args.payload;
    const model = args.model;
    switch (messageType) {
      case JsxMsg.RESET_COUNTERS:
        return {
          ...model,
          Counter: payload.value || 0,
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

      default:
        return model;
    }
  },

  // JSXを使ったビュー実装
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

      // コンポーネントがマウントされたときの処理
      useEffect(() => {
        console.log("JSX Demo component mounted");
        return () => {
          console.log("JSX Demo component unmounted");
        };
      }, []);

      // モデルのカウンター値が変更されたときの処理
      useEffect(() => {
        console.log("F# Counter changed:", model.Counter);
      }, [model.Counter]);

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

      // F#カウンター増加処理
      const incrementFSharpCounter = () => {
        dispatch("IncrementCounter");
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

      return (
        <div className="p-5">
          <h1 className="text-2xl font-bold mb-4">JSX Plugin Example</h1>

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
                <div className="text-3xl font-bold my-2">{model.Counter}</div>
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
