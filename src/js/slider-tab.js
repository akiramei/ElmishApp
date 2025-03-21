// slider-tab.js
(function() {
    // プラグイン定義
    const pluginDefinition = {
        id: "slider-extension",
        name: "Slider Tab Plugin",
        version: "1.0.0",
        dependencies: [],
        compatibility: "1.0"
    };
    
    // カスタムビュー定義
    const views = {
        // スライダータブのビュー定義
        "slider": function(model) {
            console.log("Slider view function called with model:", model);
            
            // CustomStateの内容を詳細に確認
            if (model && model.CustomState) {
                console.log("CustomState content:", model.CustomState);
                console.log("slider-value in CustomState:", model.CustomState["slider-value"]);
            } else {
                console.log("CustomState is empty or undefined");
            }
            
            // スライダーコンポーネントのReact実装
            const SliderComponent = function() {
                // カスタム状態から永続的な値を取得 (存在しない場合は初期値を使用)
                const savedValue = model.CustomState && 
                                model.CustomState["slider-value"] !== undefined ? 
                                model.CustomState["slider-value"] : 0;
                
                console.log("Initial slider value from model:", savedValue);
                
                // Reactのusestate使用例 - UIの一時的な状態のみを管理
                // 注意：useStateの初期値としてmodelから取得した値を使用
                const [sliderValue, setSliderValue] = React.useState(savedValue);
                const [isDragging, setIsDragging] = React.useState(false);
                
                // コンポーネントマウント時またはモデルの永続値変更時に状態を更新
                React.useEffect(() => {
                    console.log("useEffect triggered, savedValue:", savedValue);
                    setSliderValue(savedValue);
                }, [model.CustomState ? model.CustomState["slider-value"] : 0]); // モデルの値が変わったときだけ実行
                
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
                        
                        // appDispatchの存在確認とその型を出力
                        console.log("appDispatch type:", typeof window.appDispatch);
                        
                        if (window.appDispatch) {
                            console.log("Dispatching message to F# with payload:", { value: sliderValue });
                            try {
                                window.appDispatch(["UpdateSliderValue", { value: sliderValue }]);
                                console.log("Message dispatched successfully");
                            } catch (e) {
                                console.error("Error dispatching message:", e);
                            }
                        } else {
                            console.error("appDispatch function not available");
                        }
                    }
                };
               
                // スライダー値のリセット
                const handleReset = function() {
                    console.log("Reset button clicked");
                    setSliderValue(0);
                    
                    if (window.appDispatch) {
                        window.appDispatch(["UpdateSliderValue", { value: 0 }]);
                    } else {
                        console.error("appDispatch function not available");
                    }
                };
                
                console.log("Rendering slider with value:", sliderValue);
                
                return React.createElement('div', {
                    className: 'slider-container'
                }, [
                    React.createElement('h1', { key: 'slider-title' }, 'Slider'),
                    React.createElement('div', { className: 'slider-control', key: 'slider-control' }, [
                        React.createElement('input', {
                            key: 'slider-input',
                            type: 'range',
                            min: 0,
                            max: 100,
                            value: sliderValue,
                            onChange: handleSliderChange,
                            onMouseUp: handleSliderRelease,
                            onTouchEnd: handleSliderRelease,
                            className: 'slider'
                        }),
                        React.createElement('div', { className: 'slider-value', key: 'slider-value-container' }, [
                            React.createElement('span', { key: 'slider-value-text' }, 'Value: ' + sliderValue)
                        ]),
                        React.createElement('button', {
                            key: 'reset-button',
                            className: 'slider-reset',
                            onClick: handleReset
                        }, 'Reset')
                    ]),
                    // スライダーの値に応じた視覚的なフィードバック
                    React.createElement('div', {
                        key: 'slider-feedback',
                        className: 'slider-visual-feedback',
                        style: {
                            width: sliderValue + '%',
                            backgroundColor: getColorFromValue(sliderValue),
                            height: '20px',
                            transition: 'all 0.3s ease'
                        }
                    })
                ]);
            };
            
            // スライダー値に応じた色を返す補助関数
            function getColorFromValue(value) {
                // 値が0-33: 青、34-66: 緑、67-100: 赤
                if (value < 34) {
                    return '#3498db'; // 青
                } else if (value < 67) {
                    return '#2ecc71'; // 緑
                } else {
                    return '#e74c3c'; // 赤
                }
            }
            
            // Reactコンポーネントをレンダリング
            return React.createElement(SliderComponent, { key: 'slider-component' });
        }
    };
    
    // カスタムアップデート関数
    const updateHandlers = {
        // スライダー値の更新処理
        "UpdateSliderValue": function(msg, model) {
            console.log("UpdateSliderValue handler called with:", msg);
            console.log("Model received:", model);
            
            // すべての必要なプロパティが存在することを確認
            const safeModel = model || {};
            const customState = safeModel.CustomState || {};
            
            // 更新されたモデルを返す
            return {
                ...safeModel,
                CustomState: {
                    ...customState,
                    "slider-value": msg.value
                }
            };
        }
    };
   
    // コマンドハンドラー
    const commandHandlers = {};
    
    // タブ定義（このプラグインは新規「slider」タブを追加）
    const tabs = ["slider"];
    
    // プラグイン初期化関数
    function initPlugin() {
        console.log("Initializing Slider Tab Plugin");
        
        // 登録されたハンドラーの確認
        console.log("Registered update handlers:", Object.keys(updateHandlers));
    }
    
    // プラグイン登録
    if (window.registerFSharpPlugin) {
        console.log("Registering Slider Tab Plugin");
        const registered = window.registerFSharpPlugin({
            definition: pluginDefinition,
            views: views,
            updateHandlers: updateHandlers,
            commandHandlers: commandHandlers,
            tabs: tabs,
            init: initPlugin
        });
        console.log("Slider Tab Plugin registered successfully:", registered);
    } else {
        console.error("F# plugin registration function not available. Plugin not registered.");
    }
})();