// counter-extension.js
(function() {
    // プラグイン定義
    const pluginDefinition = {
        id: "counter-extension",
        name: "Counter Extension Plugin",
        version: "1.0.0",
        dependencies: [],
        compatibility: "1.0"
    };

    // カスタムビュー定義
    const views = {
        // カウンターの拡張機能（2倍にするボタン）
        "counter-extensions": function(model) {
            console.log("Counter extension view function called with model:", model);
            
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
                        key: 'extension-label'  // keyプロパティを追加
                    }, 'Custom Extension:'),
                    React.createElement('button', {
                        className: 'counter-double-button',
                        onClick: handleDoubleClick,
                        key: 'double-button'  // keyプロパティを追加
                    }, 'Double')
                ]);
            };
            
            // Reactコンポーネントをレンダリング
            return React.createElement(CounterExtension, null);
        }
    };
    
    // カスタムアップデート関数
    const updateHandlers = {
        // カウンターを2倍にするアップデート処理
        "DoubleCounter": function(payload, model) {
            console.log("DoubleCounter update handler called with payload:", payload);
            console.log("Current model before update:", model);
            
            // 現在の状態をコピーして更新（イミュータブル）
            const updatedModel = {
                ...model,
                Counter: model.Counter * 2
            };
            
            console.log("Updated model:", updatedModel);
            return updatedModel;
        }
    };
    
    // コマンドハンドラー
    const commandHandlers = {};
    
    // タブ定義（このプラグインは新規タブを追加しない）
    const tabs = [];
    
    // プラグイン初期化関数
    function initPlugin() {
        console.log("Initializing Counter Extension Plugin");
        
        // グローバルスコープのappDispatchをチェック
        console.log("Is appDispatch available:", typeof window.appDispatch === 'function');
        
        // カスタムハンドラーの登録を確認
        console.log("Registered update handlers:", Object.keys(updateHandlers));
    }
    
    // プラグイン登録
    if (window.registerFSharpPlugin) {
        console.log("Registering Counter Extension Plugin");
        const registered = window.registerFSharpPlugin({
            definition: pluginDefinition,
            views: views,
            updateHandlers: updateHandlers,
            commandHandlers: commandHandlers,
            tabs: tabs,
            init: initPlugin
        });
        console.log("Counter Extension Plugin registered successfully:", registered);
    } else {
        console.error("F# plugin registration function not available. Plugin not registered.");
    }
})();