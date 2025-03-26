// hello-world-plugin.jsx - Improved with unified args pattern
import React, { useState, useEffect } from 'react';

// プラグインID定義
const PLUGIN_ID = "hello-world";

// プラグイン固有のメッセージ
const HelloMsg = {
  INCREMENT_FSHARP: "IncrementCounter", // F#側の標準メッセージと同じ名前
  SAVE_LOCAL_COUNT: "SaveLocalCount",
};

// 新しいプラグインAPIを使用 - 統一されたargsパターン
plugin(PLUGIN_ID, {
  name: "Hello World Plugin",
  version: "1.0.0",

  // タブとして追加
  tab: "hello",

  // 統一されたargsパターンでupdate関数を実装
  update: function(args) {
    // 堅牢に引数を抽出
    const msgType = args.msgType || args.type || args.messageType;
    const payload = args.payload || {};
    const model = args.model;
    
    if (!msgType) {
      console.warn("Missing message type in update function", args);
      return model;
    }
    
    console.log(`Hello World plugin handling message: ${msgType}`, payload);
    
    switch (msgType) {
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
        // F#のカウンターを+1する
        return {
          ...model,
          Counter: model.Counter + 1,
        };

      default:
        return model;
    }
  },

  // JSXを使用したビュー実装 - 既存のargsパターン
  view: function(args) {
    // argsから引数を取り出す
    const model = args.model;
    const dispatch = args.dispatch;

    if (!model) {
      console.warn("Missing model in view function", args);
      return React.createElement("div", { className: "error" }, "Error: Model is undefined");
    }

    // プラグイン固有の状態を取得
    const pluginState = plugin.getState(PLUGIN_ID, model);

    // JSXコンポーネント定義
    const HelloComponent = () => {
      // 保存された値があればそれを初期値として使用、なければ0
      const initialCount = pluginState.localCount || 0;

      // ローカルカウンターの状態を管理
      const [count, setCount] = useState(initialCount);

      // F#側の値が変わった時に再レンダリングするためのeffect
      useEffect(() => {
        console.log("F# Counter value:", model.Counter);
      }, [model.Counter]);

      // プラグイン状態が変わった時に再レンダリングするためのeffect
      useEffect(() => {
        if (pluginState.localCount !== undefined && pluginState.localCount !== count) {
          setCount(pluginState.localCount);
        }
      }, [pluginState.localCount]);

      // カウンターが変更されたときの処理
      const incrementLocalCounter = () => {
        const newCount = count + 1;
        setCount(newCount);

        // F#状態に保存
        dispatch([HelloMsg.SAVE_LOCAL_COUNT, { count: newCount }]);
      };

      return (
        <div className="p-5 text-center">
          <h1 className="text-2xl font-bold mb-4">Hello World Plugin (Improved)</h1>

          <p className="mb-4">F# Counter: {model.Counter}</p>

          <p className="mb-4">Local Counter: {count}</p>

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
          
          <div className="mt-6 text-sm text-gray-500">
            <p>Version 1.0.0 - Using unified args pattern</p>
          </div>
        </div>
      );
    };

    // Reactコンポーネントをレンダリング
    return React.createElement(HelloComponent);
  },
});