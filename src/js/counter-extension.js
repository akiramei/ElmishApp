// counter-extension.js - dispatch引数化版
// プラグイン内部でメッセージ定数を定義するバージョン

// プラグイン固有のメッセージ定数を定義
const CounterMsg = {
  DOUBLE: "DoubleCounter",
  RESET: "ResetCounter",
};

// グローバル登録（オプション）
if (window.registerMessages) {
  registerMessages("COUNTER_EXT", CounterMsg);
}

// 新しいプラグインAPIを使用
plugin("counter-extension", {
  name: "Counter Extension Plugin",
  version: "1.0.0",

  // 統一されたupdate関数
  update: function (messageType, payload, model) {
    console.log(`Counter extension handling message: ${messageType}`, payload);

    // メッセージタイプによる分岐
    switch (messageType) {
      case CounterMsg.DOUBLE:
        // カウンター値を2倍にする
        const newCounter = model.Counter * 2;

        // イミュータブルな更新
        return {
          ...model,
          Counter: newCounter,
          CustomState: {
            ...(model.CustomState || {}),
            lastDoubledAt: new Date().toISOString(),
            lastOperation: "double",
          },
        };

      // その他のメッセージの処理...
      default:
        return model;
    }
  },

  // ビュー実装 - モデルとdispatchを引数で受け取る
  view: function (model, dispatch) {
    // 実際のReactコンポーネントを定義
    const CounterExtensionComponent = function () {
      // Doubleボタンのクリック処理
      const handleDoubleClick = function () {
        // プラグイン内で定義したメッセージ定数を使用
        dispatch(CounterMsg.DOUBLE, { currentValue: model.Counter });
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
            "Counter Extension:"
          ),
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
            "div",
            {
              className: "mt-2 text-sm text-gray-500 italic",
              key: "plugin-info",
            },
            `Plugin version: 1.0.0`
          ),
        ]
      );
    };

    // コンポーネントをレンダリング
    return React.createElement(CounterExtensionComponent);
  },
});
