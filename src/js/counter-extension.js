// counter-extension.js - 新しいモデル構造対応版
(function () {
  // プラグイン固有のメッセージ定数を定義
  const CounterMsg = {
    DOUBLE: "DoubleCounter",
    RESET: "ResetCounter",
  };

  // プラグインID定義
  const PLUGIN_ID = "counter-extension";

  // グローバル登録（オプション）
  if (window.registerMessages) {
    registerMessages("COUNTER_EXT", CounterMsg);
  }

  // 新しいプラグインAPIを使用
  plugin(PLUGIN_ID, {
    name: "Counter Extension Plugin",
    version: "1.0.0",

    // 統一されたupdate関数 - 新しいモデル構造対応
    update: function (args) {
      const messageType = args.messageType;
      const payload = args.payload;
      const model = args.model;
      console.log(
        `Counter extension handling message: ${messageType}`,
        payload
      );

      // メッセージタイプによる分岐
      switch (messageType) {
        case CounterMsg.DOUBLE:
          // 新しい構造からカウンター値を取得
          const currentCounter = model.CounterState.Counter;
          const newCounter = currentCounter * 2;

          // 名前空間付きの状態管理を使用
          // カウンター値更新時の適切な構造更新
          return plugin.setState(
            PLUGIN_ID,
            {
              lastDoubledAt: new Date().toISOString(),
              lastOperation: "double",
            },
            {
              ...model,
              CounterState: {
                ...model.CounterState, // オリジナルの構造を保存
                Counter: newCounter,
              }
            }
          );

        case CounterMsg.RESET:
          // カウンターをリセット - 新構造対応
          return plugin.setState(
            PLUGIN_ID,
            {
              lastResetAt: new Date().toISOString(),
              lastOperation: "reset",
            },
            {
              ...model,
              CounterState: {
                ...model.CounterState,
                Counter: 0,
              },
            }
          );

        default:
          return model;
      }
    },

    // ビュー実装 - モデルとdispatchを引数で受け取る
    view: function (args) {
      const model = args.model;
      const dispatch = args.dispatch;

      // プラグイン固有の状態を取得
      const pluginState = plugin.getState(PLUGIN_ID, model);

      // 新しいプラグインヘルパーでカウンター値を取得
      const counterValue = plugin.counter.getValue(model);

      // 実際のReactコンポーネントを定義
      const CounterExtensionComponent = function () {
        // Doubleボタンのクリック処理
        const handleDoubleClick = function () {
          // プラグイン内で定義したメッセージ定数を使用
          dispatch([CounterMsg.DOUBLE, { currentValue: counterValue }]);
        };

        // Resetボタンのクリック処理
        const handleResetClick = function () {
          dispatch([CounterMsg.RESET]);
        };

        // UIを構築
        return React.createElement(
          "div",
          {
            className: "mt-6 pt-6 border-t border-gray-200",
          },
          [
            React.createElement(
              "div",
              {
                className: "font-bold mb-3",
                key: "extension-label",
              },
              "Counter Extension (New Model):"
            ),
            
            // ボタン群をフレックスコンテナで包む
            React.createElement(
              "div",
              {
                className: "flex space-x-2",
                key: "buttons-container",
              },
              [
                React.createElement(
                  "button",
                  {
                    className:
                      "px-4 py-2 bg-green-500 text-white rounded hover:bg-green-600 transition-colors",
                    onClick: handleDoubleClick,
                    key: "double-button",
                  },
                  "Double"
                ),
                React.createElement(
                  "button",
                  {
                    className:
                      "px-4 py-2 bg-red-500 text-white rounded hover:bg-red-600 transition-colors",
                    onClick: handleResetClick,
                    key: "reset-button",
                  },
                  "Reset"
                ),
              ]
            ),
            
            React.createElement(
              "div",
              {
                className: "mt-2 text-sm text-gray-500 italic",
                key: "plugin-info",
              },
              `Plugin version: 1.0.0 - New Model Support`
            ),

            // 最後の操作情報を表示 (あれば)
            pluginState.lastOperation &&
              (pluginState.lastDoubledAt || pluginState.lastResetAt) &&
              React.createElement(
                "div",
                {
                  className: "mt-2 text-sm text-gray-500 italic",
                  key: "last-operation-info",
                },
                `Last operation: ${pluginState.lastOperation} at ${new Date(
                  pluginState.lastDoubledAt || pluginState.lastResetAt
                ).toLocaleTimeString()}`
              ),
          ]
        );
      };

      // コンポーネントをレンダリング
      return React.createElement(CounterExtensionComponent);
    },
  });
})();