// slider-tab.js - 関数型アプローチ版

// スライダープラグイン固有のメッセージ定数を定義
const SliderMsg = {
  UPDATE_VALUE: "UpdateSliderValue",
  RESET: "ResetSlider",
  DOUBLE: "DoubleSlider",
  HALVE: "HalveSlider",
};

// グローバル登録（オプション）
if (window.registerMessages) {
  registerMessages("SLIDER", SliderMsg);
}

// 新しいプラグインAPIを使用 - 関数型アプローチ
plugin("slider-tab", {
  name: "Slider Tab Plugin",
  version: "1.0.0",

  // タブとして追加
  tab: "slider",

  // 統一されたupdate関数
  update: function (messageType, payload, model) {
    console.log(`Slider plugin handling message: ${messageType}`, payload);

    const updateModel = function (newValue) {
      return {
        ...model,
        CustomState: {
          ...(model.CustomState || {}),
          "slider-value": newValue,
          "last-updated": new Date().toISOString(),
          "last-operation": "update",
        },
      };
    };

    switch (messageType) {
      case SliderMsg.UPDATE_VALUE:
        // スライダー値更新
        const sliderValue =
          payload && typeof payload.value === "number" ? payload.value : 0;

        // 更新されたモデルを作成
        return updateModel(sliderValue);

      case SliderMsg.RESET:
        // スライダー値をリセット
        return updateModel(0);

      case SliderMsg.DOUBLE:
        // 現在の値を取得
        const currentValue =
          (model.CustomState && model.CustomState["slider-value"]) || 0;
        // 値を2倍にする (最大100まで)
        const doubledValue = Math.min(currentValue * 2, 100);
        return updateModel(doubledValue);

      case SliderMsg.HALVE:
        // 現在の値を取得
        const value =
          (model.CustomState && model.CustomState["slider-value"]) || 0;
        // 値を半分にする
        const halvedValue = Math.floor(value / 2);
        return updateModel(halvedValue);

      default:
        // 処理できないメッセージの場合は元のモデルをそのまま返す
        return model;
    }
  },

  // ビュー実装
  view: function (model, dispatch) {
    // 実際のReactコンポーネントを定義
    const SliderComponent = function () {
      // カスタム状態から永続的な値を取得（存在しない場合は初期値を使用）
      const savedValue =
        model.CustomState && model.CustomState["slider-value"] !== undefined
          ? Number(model.CustomState["slider-value"])
          : 0;

      // Reactのusestate使用例 - UIの一時的な状態のみを管理
      const [sliderValue, setSliderValue] = React.useState(savedValue);
      const [isDragging, setIsDragging] = React.useState(false);

      // コンポーネントマウント時またはモデルの永続値変更時に状態を更新
      React.useEffect(
        function () {
          setSliderValue(savedValue);
        },
        [savedValue]
      );

      // スライダー値変更時の処理
      const handleSliderChange = function (event) {
        const newValue = parseInt(event.target.value, 10);

        // UIの一時的な状態を更新
        setSliderValue(newValue);
        setIsDragging(true);
      };

      // スライダーのドラッグ終了時に値を保存
      const handleSliderRelease = function () {
        if (isDragging) {
          setIsDragging(false);
          // メッセージ定数を使用
          dispatch(SliderMsg.UPDATE_VALUE, { value: sliderValue });
        }
      };

      // スライダー値のリセット
      const handleReset = function () {
        setSliderValue(0);
        dispatch(SliderMsg.RESET);
      };

      // スライダー値を倍にする処理
      const handleDouble = function () {
        const newValue = Math.min(sliderValue * 2, 100); // 100を超えないようにする
        setSliderValue(newValue);
        dispatch(SliderMsg.DOUBLE);
      };

      // スライダー値を半分にする処理
      const handleHalf = function () {
        const newValue = Math.floor(sliderValue / 2);
        setSliderValue(newValue);
        dispatch(SliderMsg.HALVE);
      };

      // スライダー値に応じた色を返す補助関数
      function getColorClass(value) {
        if (value < 34) return "bg-blue-500";
        else if (value < 67) return "bg-green-500";
        else return "bg-red-500";
      }

      // スタイルドライバーを作成
      const sliderClass =
        "h-5 mt-4 rounded transition-all duration-300 " +
        getColorClass(sliderValue);

      // 最後の操作情報を取得
      const lastOp = model.CustomState && model.CustomState["last-operation"];
      const lastUpdated =
        model.CustomState && model.CustomState["last-updated"];

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
              key: "slider-title",
            },
            "Slider"
          ),
          React.createElement(
            "div",
            {
              className: "my-8",
              key: "slider-control",
            },
            [
              React.createElement("input", {
                key: "slider-input",
                type: "range",
                min: 0,
                max: 100,
                value: sliderValue,
                onChange: handleSliderChange,
                onMouseUp: handleSliderRelease,
                onTouchEnd: handleSliderRelease,
                className:
                  "w-full h-2 bg-gray-200 rounded-lg appearance-none cursor-pointer",
              }),
              React.createElement(
                "div",
                {
                  className: "text-xl font-bold my-4",
                  key: "slider-value-container",
                },
                [
                  React.createElement(
                    "span",
                    {
                      key: "slider-value-text",
                    },
                    "Value: " + sliderValue
                  ),
                ]
              ),
              React.createElement(
                "div",
                {
                  className: "flex justify-center gap-2 mb-4",
                  key: "slider-buttons",
                },
                [
                  React.createElement(
                    "button",
                    {
                      key: "reset-button",
                      className:
                        "px-4 py-2 bg-gray-500 text-white rounded hover:bg-gray-600 transition-colors",
                      onClick: handleReset,
                    },
                    "Reset"
                  ),
                  React.createElement(
                    "button",
                    {
                      key: "half-button",
                      className:
                        "px-4 py-2 bg-yellow-500 text-white rounded hover:bg-yellow-600 transition-colors",
                      onClick: handleHalf,
                    },
                    "Half"
                  ),
                  React.createElement(
                    "button",
                    {
                      key: "double-button",
                      className:
                        "px-4 py-2 bg-green-500 text-white rounded hover:bg-green-600 transition-colors",
                      onClick: handleDouble,
                    },
                    "Double"
                  ),
                ]
              ),
            ]
          ),
          // スライダーの値に応じた視覚的なフィードバック
          React.createElement("div", {
            key: "slider-feedback",
            className: sliderClass,
            style: {
              width: sliderValue + "%",
            },
          }),

          // 状態情報の表示
          lastOp &&
            lastUpdated &&
            React.createElement(
              "div",
              {
                className: "mt-4 text-sm text-gray-500",
                key: "state-info",
              },
              `Last operation: ${lastOp} at ${new Date(
                lastUpdated
              ).toLocaleTimeString()}`
            ),
        ]
      );
    };

    // コンポーネントをレンダリング
    return React.createElement(SliderComponent);
  },
});
