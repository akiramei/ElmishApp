// counter-extension.js
(function() {
    // カスタムビュー定義
    const counterExtensionView = function(model) {
        console.log("Counter extension view function called with model:", model);
        
        // モデルの検証
        if (!model || typeof model.Counter !== 'number') {
            console.error("Invalid model or Counter is missing/not a number");
            return null;
        }
        
        // カスタムコンポーネントの実装
        const CounterExtension = function() {
            // コンポーネントがマウントされたときにコンソールにログを出力
            React.useEffect(() => {
                console.log("Counter extension component mounted");
                return () => {
                    console.log("Counter extension component unmounted");
                };
            }, []);
            
            // 2倍にするボタンの実装
            const handleDoubleClick = function() {
                // モデルの値を確認してコンソールに出力
                console.log("Double button clicked, current counter value:", model.Counter);
                
                // F#側にカスタムメッセージをディスパッチ
                if (window.appDispatch) {
                    console.log("Dispatching DoubleCounter message with payload:", { currentValue: model.Counter });
                    window.appDispatch(["DoubleCounter", { currentValue: model.Counter }]);
                } else {
                    console.error("appDispatch function not available");
                }
            };
            
            return React.createElement('div', {
                className: 'custom-counter-extension'
            }, [
                React.createElement('div', { 
                    className: 'extension-label',
                    key: 'extension-label'
                }, 'Custom Extension:'),
                React.createElement('button', {
                    className: 'counter-double-button',
                    onClick: handleDoubleClick,
                    key: 'double-button'
                }, 'Double')
            ]);
        };
        
        // Reactコンポーネントをレンダリング
        return React.createElement(CounterExtension, null);
    };
    
    // グローバルオブジェクトを初期化して登録
    window.customViews = window.customViews || {};
    window.customViews["counter-extensions"] = counterExtensionView;
    
    // カスタムアップデート関数は削除 - F#側で処理するため
    
    console.log("Counter extension plugin initialized");
    console.log("- Registered custom view: counter-extensions");
})();