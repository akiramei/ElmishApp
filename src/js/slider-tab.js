// slider-tab.js
// Tailwind CSSクラスを適切に使用するバージョン

// 変数の重複宣言を防止
(function() {
    // ヘルパーライブラリが読み込まれているか確認
    if (typeof window.AppPlugins === 'undefined') {
        console.error("AppPlugins helper is not loaded. This plugin requires plugin-helpers.js");
        return;
    }

    // プラグインがすでに登録されているか確認
    if (window.registeredPlugins && window.registeredPlugins["slider-tab"]) {
        console.log("Slider tab plugin already registered, skipping");
        return;
    }

    console.log("Initializing slider tab plugin");

    // プラグインビルダーを作成
    const builder = window.AppPlugins.createBuilder(
        "slider-tab",
        "Slider Tab Plugin",
        "1.0.0"
    );

    // スライダービューを定義
    const renderSlider = function(model) {
        console.log("Slider view rendering with model:", model);
        
        // CustomStateの確認
        if (!model || !model.CustomState) {
            console.error("Invalid model or CustomState is missing");
            return React.createElement('div', { 
                className: 'p-8 text-center bg-red-50 rounded-lg'
            }, [
                React.createElement('h1', { 
                    key: 'error-title',
                    className: 'text-2xl font-bold text-red-600 mb-4'
                }, 'Error'),
                React.createElement('p', { 
                    key: 'error-message',
                    className: 'text-red-500'
                }, 'Model or CustomState is missing')
            ]);
        }
        
        // スライダーコンポーネントのReact実装
        const SliderComponent = function() {
            // カスタム状態から永続的な値を取得 (存在しない場合は初期値を使用)
            const savedValue = model.CustomState && 
                            model.CustomState["slider-value"] !== undefined ? 
                            Number(model.CustomState["slider-value"]) : 0;
            
            console.log("Initial slider value from model:", savedValue);
            
            // Reactのusestate使用例 - UIの一時的な状態のみを管理
            const [sliderValue, setSliderValue] = React.useState(savedValue);
            const [isDragging, setIsDragging] = React.useState(false);
            
            // コンポーネントマウント時またはモデルの永続値変更時に状態を更新
            React.useEffect(function() {
                console.log("useEffect triggered, savedValue:", savedValue);
                setSliderValue(savedValue);
            }, [savedValue]); // 保存値が変わったときだけ実行
            
            // スライダー値変更時の処理
            const handleSliderChange = function(event) {
                const newValue = parseInt(event.target.value, 10);
                console.log("Slider changed to:", newValue);
                
                // UIの一時的な状態を更新
                setSliderValue(newValue);
                setIsDragging(true);
            };

            // スライダーのドラッグ終了時に値を保存
            const handleSliderRelease = function() {
                if (isDragging) {
                    setIsDragging(false);
                    // 新APIを使用
                    window.AppPlugins.dispatch("UpdateSliderValue", { value: sliderValue });
                    console.log("Message dispatched with new API");
                }
            };
           
            // スライダー値のリセット
            const handleReset = function() {
                console.log("Reset button clicked");
                setSliderValue(0);
                window.AppPlugins.dispatch("UpdateSliderValue", { value: 0 });
            };

            // スライダー値を倍にする処理
            const handleDouble = function() {
                const newValue = Math.min(sliderValue * 2, 100); // 100を超えないようにする
                setSliderValue(newValue);
                window.AppPlugins.dispatch("UpdateSliderValue", { value: newValue });
            };
            
            // スライダー値を半分にする処理
            const handleHalf = function() {
                const newValue = Math.floor(sliderValue / 2);
                setSliderValue(newValue);
                window.AppPlugins.dispatch("UpdateSliderValue", { value: newValue });
            };
            
            // スライダー値に応じた色を返す補助関数 - ここを明示的な文字列に
            function getColorFromValue(value) {
                // 値が0-33: 青、34-66: 緑、67-100: 赤
                if (value < 34) {
                    return "bg-blue-500"; // 青
                } else if (value < 67) {
                    return "bg-green-500"; // 緑
                } else {
                    return "bg-red-500"; // 赤
                }
            }
            
            console.log("Rendering slider with value:", sliderValue);
            
            // スタイルドライバーを作成
            const sliderClass = "h-5 mt-4 rounded transition-all duration-300 " + getColorFromValue(sliderValue);
            
            return React.createElement('div', {
                className: 'p-5 text-center'
            }, [
                React.createElement('h1', { 
                    key: 'slider-title',
                    className: 'text-2xl font-bold mb-4'
                }, 'Slider'),
                React.createElement('div', { 
                    className: 'text-sm text-gray-500 italic mb-4',
                    key: 'plugin-info'
                }, `Plugin ID: ${builder.definition.id}, Version: ${builder.definition.version}`),
                React.createElement('div', { 
                    className: 'my-8',
                    key: 'slider-control'
                }, [
                    React.createElement('input', {
                        key: 'slider-input',
                        type: 'range',
                        min: 0,
                        max: 100,
                        value: sliderValue,
                        onChange: handleSliderChange,
                        onMouseUp: handleSliderRelease,
                        onTouchEnd: handleSliderRelease,
                        className: 'w-full h-2 bg-gray-200 rounded-lg appearance-none cursor-pointer'
                    }),
                    React.createElement('div', { 
                        className: 'text-xl font-bold my-4',
                        key: 'slider-value-container'
                    }, [
                        React.createElement('span', { 
                            key: 'slider-value-text'
                        }, 'Value: ' + sliderValue)
                    ]),
                    React.createElement('div', { 
                        className: 'flex justify-center gap-2 mb-4',
                        key: 'slider-buttons'
                    }, [
                        React.createElement('button', {
                            key: 'reset-button',
                            className: 'px-4 py-2 bg-gray-500 text-white rounded hover:bg-gray-600 transition-colors',
                            onClick: handleReset
                        }, 'Reset'),
                        React.createElement('button', {
                            key: 'half-button',
                            className: 'px-4 py-2 bg-yellow-500 text-white rounded hover:bg-yellow-600 transition-colors',
                            onClick: handleHalf
                        }, 'Half'),
                        React.createElement('button', {
                            key: 'double-button',
                            className: 'px-4 py-2 bg-green-500 text-white rounded hover:bg-green-600 transition-colors',
                            onClick: handleDouble
                        }, 'Double')
                    ])
                ]),
                // スライダーの値に応じた視覚的なフィードバック - 動的スタイルもクラスで
                React.createElement('div', {
                    key: 'slider-feedback',
                    className: sliderClass,
                    style: {
                        width: sliderValue + '%'
                    }
                })
            ]);
        };
        
        // Reactコンポーネントをレンダリング
        return React.createElement(SliderComponent, { key: 'slider-component' });
    };

    // スライダー値更新のハンドラー
    const handleUpdateSliderValue = function(payload, model) {
        console.log("Updating slider value with payload:", payload);
        console.log("Model in update handler:", model);
        
        if (!model || !model.CustomState) {
            console.error("Invalid model or CustomState is missing in update handler");
            return model;  // 変更なしで元のモデルを返す
        }
        
        try {
            // payloadから値を取得
            const sliderValue = payload && typeof payload.value === 'number' ? payload.value : 0;
            
            // 更新されたモデルを作成（イミュータブルに）
            const updatedModel = {
                ...model,
                CustomState: {
                    ...model.CustomState,
                    "slider-value": sliderValue,
                    "last-updated": new Date().toISOString()
                }
            };
            
            console.log("Created updated model:", updatedModel);
            return updatedModel;
        } catch (error) {
            console.error("Error in slider update handler:", error);
            return model;  // エラー時は元のモデルを返す
        }
    };

    // プラグイン初期化関数
    const initPlugin = function() {
        console.log("Slider tab plugin initialized with new API");
    };

    // ビルダーにコンポーネントと更新ハンドラーを追加
    builder
        .addView("slider", renderSlider)
        .addUpdateHandler("UpdateSliderValue", handleUpdateSliderValue)
        .addTab("slider")
        .withInitFunction(initPlugin)
        .register();

    // プラグイン登録を記録（重複登録防止用）
    window.registeredPlugins = window.registeredPlugins || {};
    window.registeredPlugins["slider-tab"] = true;

    console.log("Slider tab plugin registered successfully");
})();