// counter-extension-decorator.js - Updated with unified args pattern
(function () {
  // プラグイン固有のメッセージ定数を定義
  const CounterMsg = {
    DOUBLE: "DoubleCounter",
    RESET: "ResetCounter",
  };

  // プラグインのID (状態管理に使用)
  const PLUGIN_ID = "counter-tab-plugin";

  // グローバル登録（オプション）
  if (window.registerMessages) {
    registerMessages("COUNTER_EXT", CounterMsg);
  }

  // 新しいプラグインAPIを使用 - 統一されたargsパターン
  plugin(PLUGIN_ID, {
    name: "Counter Tab Decorator Plugin",
    version: "1.0.0",

    // 統一されたargsパターンでupdate関数を実装
    update: function(args) {
      const messageType = args.messageType;
      const payload = args.payload;
      const model = args.model;
      
      console.log(`Counter decorator handling message: ${messageType}`, payload);

      switch (messageType) {
        case CounterMsg.DOUBLE:
          // カウンター値を2倍にする
          const newCounter = model.Counter * 2;

          // 名前空間付きの状態管理を使用
          return plugin.setState(
            PLUGIN_ID,
            {
              lastDoubledAt: new Date().toISOString(),
              lastOperation: "double",
            },
            {
              ...model,
              Counter: newCounter,
            }
          );

        case CounterMsg.RESET:
          // カウンターをリセット
          return plugin.setState(
            PLUGIN_ID,
            {
              lastResetAt: new Date().toISOString(),
              lastOperation: "reset",
            },
            {
              ...model,
              Counter: 0,
            }
          );

        default:
          return model;
      }
    },

    // デコレーターパターンによるビュー実装 - 既存のargsパターン
    view: function(args) {
      // argsから引数を取り出す
      const model = args.model;
      const defaultRenderer = args.defaultRenderer;
      const dispatch = args.dispatch;

      // プラグイン固有の状態を取得
      const pluginState = plugin.getState(PLUGIN_ID, model);

      // Reactコンポーネントを定義
      const DecoratedCounter = function() {
        // Reactフックを利用
        const [showExtensions, setShowExtensions] = React.useState(true);

        // デフォルトのカウンターコンポーネントを実行
        const defaultCounterView = defaultRenderer();

        // Doubleボタンのクリック処理
        const handleDoubleClick = function() {
          dispatch([CounterMsg.DOUBLE, { currentValue: model.Counter }]);
        };

        // Resetボタンのクリック処理
        const handleResetClick = function() {
          dispatch(CounterMsg.RESET);
        };

        // 最後の操作情報 - プラグイン固有の状態から取得
        const lastOperation = pluginState.lastOperation;
        const lastOperationTime =
          pluginState.lastDoubledAt || pluginState.lastResetAt;

        // 拡張部分の表示/非表示の切り替え
        const toggleExtensions = () => {
          setShowExtensions(!showExtensions);
        };

        return React.createElement(
          "div",
          {
            className: "counter-container",
          },
          [
            // デフォルトのカウンターコンポーネントを表示
            React.createElement(
              "div",
              { key: "default-counter" },
              defaultCounterView
            ),

            // 拡張機能のトグルボタン
            React.createElement(
              "div",
              {
                className: "mt-4 text-right",
                key: "toggle-btn",
              },
              [
                React.createElement(
                  "button",
                  {
                    className:
                      "px-3 py-1 text-sm bg-gray-200 text-gray-700 rounded hover:bg-gray-300 transition-colors",
                    onClick: toggleExtensions,
                    key: "toggle-button",
                  },
                  showExtensions ? "拡張機能を隠す" : "拡張機能を表示"
                ),
              ]
            ),

            // 条件付きで拡張機能を表示
            showExtensions &&
              React.createElement(
                "div",
                {
                  className: "mt-6 pt-6 border-t border-gray-200",
                  key: "extension-panel",
                },
                [
                  React.createElement(
                    "div",
                    {
                      className: "font-bold mb-3",
                      key: "extension-label",
                    },
                    "Counter Extensions (統一args):"
                  ),

                  React.createElement(
                    "div",
                    {
                      className: "flex justify-center gap-2",
                      key: "extension-buttons",
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

                  // 最後の操作情報を表示
                  lastOperation &&
                    lastOperationTime &&
                    React.createElement(
                      "div",
                      {
                        className: "mt-3 text-sm text-gray-500 italic",
                        key: "last-operation-info",
                      },
                      `Last operation: ${lastOperation} at ${new Date(
                        lastOperationTime
                      ).toLocaleTimeString()}`
                    ),
                ]
              ),
          ]
        );
      };

      // コンポーネントをレンダリング
      return React.createElement(DecoratedCounter);
    },
  });
})();