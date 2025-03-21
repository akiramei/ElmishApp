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
            // スライダーコンポーネントのReact実装
            const SliderComponent = function() {
                // Reactのusestate使用例 - UIの一時的な状態のみを管理
                const [sliderValue, setSliderValue] = React.useState(0);
                const [isDragging, setIsDragging] = React.useState(false);
                
                // カスタム状態から永続的な値を取得 (存在しない場合は初期値を使用)
                const savedValue = model.CustomState && 
                                model.CustomState["slider-value"] ? 
                                model.CustomState["slider-value"] : 0;
                
                // コンポーネントマウント時に保存値を反映
                React.useEffect(() => {
                    setSliderValue(savedValue);
                    console.log("Slider component mounted with saved value:", savedValue);
                    
                    return () => {
                        console.log("Slider component unmounted");
                    };
                }, []);
                
                // スライダー値変更時の処理
                const handleSliderChange = function(event) {
                    const newValue = parseInt(event.target.value, 10);
                    
                    // UIの一時的な状態を更新
                    setSliderValue(newValue);
                    setIsDragging(true);
                };
                
                // スライダーのドラッグ終了時に値を保存
                const handleSliderRelease = function() {
                    if (isDragging) {
                        setIsDragging(false);
                        
                        // アプリケーション状態を更新するためにディスパッチ
                        window.appDispatch(["UpdateSliderValue", { value: sliderValue }]);
                    }
                };
                
                // スライダー値のリセット
                const handleReset = function() {
                    setSliderValue(0);
                    window.appDispatch(["UpdateSliderValue", { value: 0 }]);
                };
                
                return React.createElement('div', {
                    className: 'slider-container'
                }, [
                    React.createElement('h1', {}, 'Slider'),
                    React.createElement('div', { className: 'slider-control' }, [
                        React.createElement('input', {
                            type: 'range',
                            min: 0,
                            max: 100,
                            value: sliderValue,
                            onChange: handleSliderChange,
                            onMouseUp: handleSliderRelease,
                            onTouchEnd: handleSliderRelease,
                            className: 'slider'
                        }),
                        React.createElement('div', { className: 'slider-value' }, [
                            React.createElement('span', {}, 'Value: ' + sliderValue)
                        ]),
                        React.createElement('button', {
                            className: 'slider-reset',
                            onClick: handleReset
                        }, 'Reset')
                    ]),
                    // スライダーの値に応じた視覚的なフィードバック
                    React.createElement('div', {
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
            return React.createElement(SliderComponent, {});
        }
    };
    
    // カスタムアップデート関数
    const updateHandlers = {
        // スライダー値の更新処理
        "UpdateSliderValue": function(msg, model) {
            console.log("UpdateSliderValue handler called with value:", msg.value);
            
            // 現在のカスタム状態を取得
            const currentCustomState = model.CustomState || {};
            
            // 新しいカスタム状態を作成（イミュータブル）
            const newCustomState = {
                ...currentCustomState,
                "slider-value": msg.value
            };
            
            // 更新されたモデルを返す
            return {
                ...model,
                CustomState: newCustomState
            };
        }
    };
    
    // コマンドハンドラー
    const commandHandlers = {
        // 必要に応じてコマンドハンドラーを追加
    };
    
    // タブ定義（このプラグインは新規「slider」タブを追加）
    const tabs = ["slider"];
    
    // プラグイン初期化関数
    function initPlugin() {
        console.log("Initializing Slider Tab Plugin");
    }
    
    // プラグイン登録
    if (window.registerFSharpPlugin) {
        window.registerFSharpPlugin({
            definition: pluginDefinition,
            views: views,
            updateHandlers: updateHandlers,
            commandHandlers: commandHandlers,
            tabs: tabs,
            init: initPlugin
        });
    } else {
        console.error("F# plugin registration function not available. Plugin not registered.");
    }
})();