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
                    // F#側にカスタムメッセージをディスパッチ
                    window.appDispatch(["DoubleCounter", { currentValue: model.Counter }]);
                };
                
                return React.createElement('div', {
                    className: 'custom-counter-extension'
                }, [
                    React.createElement('div', { 
                        className: 'extension-label'
                    }, 'Custom Extension:'),
                    React.createElement('button', {
                        className: 'counter-double-button',
                        onClick: handleDoubleClick
                    }, 'Double')
                ]);
            };
            
            // Reactコンポーネントをレンダリング
            return React.createElement(CounterExtension, {});
        }
    };
    
    // カスタムアップデート関数
    const updateHandlers = {
        // カウンターを2倍にするアップデート処理
        "DoubleCounter": function(msg, model) {
            console.log("DoubleCounter update handler called with value:", model.Counter);
            
            // 現在の状態をコピーして更新（イミュータブル）
            return {
                ...model,
                Counter: model.Counter * 2
            };
        }
    };
    
    // コマンドハンドラー
    const commandHandlers = {
        // 必要に応じてコマンドハンドラーを追加
    };
    
    // タブ定義（このプラグインは新規タブを追加しない）
    const tabs = [];
    
    // プラグイン初期化関数
    function initPlugin() {
        console.log("Initializing Counter Extension Plugin");
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