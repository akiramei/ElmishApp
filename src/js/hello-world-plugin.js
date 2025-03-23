// hello-world-plugin.js - IIFE (即時実行関数式)でスコープ化した改良版
(function () {
  // プラグインID定義
  const PLUGIN_ID = "hello-world";

  // プラグイン固有のメッセージ
  const HelloMsg = {
    INCREMENT_FSHARP: "IncrementCounter",
    SAVE_LOCAL_COUNT: "SaveLocalCount",
  };

  // 新しいプラグインAPIを使用
  plugin(PLUGIN_ID, {
    name: "Hello World Plugin",
    version: "1.0.0",

    // タブとして追加
    tab: "hello",

    // メッセージハンドラーの定義
    update: function (messageType, payload, model) {
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

        default:
          return model;
      }
    },

    // ビュー実装 - モデルとdispatchを引数で受け取る
    view: function (model, dispatch) {
      // プラグイン固有の状態を取得
      const pluginState = plugin.getState(PLUGIN_ID, model);

      // 実際のReactコンポーネントを定義
      const HelloComponent = function () {
        // 保存された値があればそれを初期値として使用、なければ0
        const initialCount = pluginState.localCount || 0;

        // ローカルカウンターの状態を管理
        const [count, setCount] = React.useState(initialCount);

        // カウンターが変更されたときの処理
        const incrementLocalCounter = () => {
          const newCount = count + 1;
          setCount(newCount);

          // F#状態に保存
          dispatch(HelloMsg.SAVE_LOCAL_COUNT, { count: newCount });
        };

        return React.createElement(
          "div",
          {
            className: "p-5 text-center",
          },
          [
            React.createElement(
              "h1",
              {
                className: "text-2xl font-bold mb-4",
                key: "title",
              },
              "Hello World Plugin"
            ),

            React.createElement(
              "p",
              {
                className: "mb-4",
                key: "f-sharp-counter",
              },
              `F# Counter: ${model.Counter}`
            ),

            React.createElement(
              "p",
              {
                className: "mb-4",
                key: "local-counter",
              },
              `Local Counter: ${count}`
            ),

            pluginState.lastSaved &&
              React.createElement(
                "p",
                {
                  className: "mb-4 text-sm text-gray-500",
                  key: "last-saved",
                },
                `Last saved: ${new Date(
                  pluginState.lastSaved
                ).toLocaleTimeString()}`
              ),

            React.createElement(
              "div",
              {
                className: "flex justify-center gap-2",
                key: "buttons",
              },
              [
                React.createElement(
                  "button",
                  {
                    className:
                      "px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 transition-colors",
                    onClick: incrementLocalCounter,
                    key: "local-btn",
                  },
                  "Local +1"
                ),

                React.createElement(
                  "button",
                  {
                    className:
                      "px-4 py-2 bg-green-500 text-white rounded hover:bg-green-600 transition-colors",
                    onClick: () => dispatch(HelloMsg.INCREMENT_FSHARP),
                    key: "f-sharp-btn",
                  },
                  "F# Counter +1"
                ),
              ]
            ),
          ]
        );
      };

      // コンポーネントをレンダリング
      return React.createElement(HelloComponent);
    },
  });
})();
