// counter-extension.js
// 新しいAppPlugins APIを使用したよりクリーンな実装

// 変数の重複宣言を防止
(function() {
    // ヘルパーライブラリが読み込まれているか確認
    if (typeof window.AppPlugins === 'undefined') {
        console.error("AppPlugins helper is not loaded. This plugin requires plugin-helpers.js");
        return;
    }

    // プラグインがすでに登録されているか確認
    if (window.registeredPlugins && window.registeredPlugins["counter-extension"]) {
        console.log("Counter extension plugin already registered, skipping");
        return;
    }

    console.log("Initializing counter extension plugin");

    // プラグインビルダーを作成
    const builder = window.AppPlugins.createBuilder(
        "counter-extension",
        "Counter Extension Plugin",
        "1.0.0"
    );

    // カウンター拡張ビューを定義
    const renderCounterExtension = function(model) {
        console.log("Counter extension view rendering with model:", model);
        
        // モデルの検証
        if (!model || typeof model.Counter !== 'number') {
            console.error("Invalid model or Counter is missing/not a number");
            return null;
        }
        
        // コンポーネントの実装
        const CounterExtension = function() {
            // コンポーネントがマウントされたときにコンソールにログを出力
            React.useEffect(function() {
                console.log("Counter extension component mounted");
                return function() {
                    console.log("Counter extension component unmounted");
                };
            }, []);
            
            // 2倍にするボタンの実装
            const handleDoubleClick = function() {
                // モデルの値を確認してコンソールに出力
                console.log("Double button clicked, current counter value:", model.Counter);
                
                // AppPlugins APIを使用してディスパッチ
                window.AppPlugins.dispatch("DoubleCounter", { currentValue: model.Counter });
            };
            
            return React.createElement('div', {
                className: 'custom-counter-extension'
            }, [
                React.createElement('div', { 
                    className: 'extension-label',
                    key: 'extension-label'
                }, 'Counter Extension (新APIを使用):'),
                React.createElement('button', {
                    className: 'counter-double-button',
                    onClick: handleDoubleClick,
                    key: 'double-button'
                }, 'Double'),
                React.createElement('div', {
                    className: 'plugin-info',
                    key: 'plugin-info'
                }, `Plugin ID: ${builder.definition.id}, Version: ${builder.definition.version}`)
            ]);
        };
        
        // Reactコンポーネントをレンダリング
        return React.createElement(CounterExtension, null);
    };

    // カウンター値を2倍にする更新ハンドラー - 修正版
    const handleDoubleCounter = function(payload, model) {
        console.log("Doubling counter value. Model:", model);
        
        if (!model || typeof model.Counter !== 'number') {
            console.error("Invalid model or Counter is missing/not a number in update handler");
            return model;  // 変更なしで元のモデルを返す
        }
        
        try {
            // Counter値を2倍にする
            const newCounter = model.Counter * 2;
            console.log(`Changing Counter from ${model.Counter} to ${newCounter}`);
            
            // CustomStateを更新（イミュータブルに）
            const customState = model.CustomState || {};
            
            // 更新されたモデルを作成
            const updatedModel = {
                ...model,
                Counter: newCounter,
                CustomState: {
                    ...customState,
                    lastDoubledAt: new Date().toISOString()  // 2倍にした時刻を記録
                }
            };
            
            console.log("Created updated model:", updatedModel);
            return updatedModel;
        } catch (error) {
            console.error("Error in double counter handler:", error);
            return model;  // エラー時は元のモデルを返す
        }
    };

    // プラグイン初期化関数
    const initPlugin = function() {
        console.log("Counter extension plugin initialized with new API");
    };

    // ビルダーにコンポーネントと更新ハンドラーを追加して登録
    builder
        .addView("counter-extensions", renderCounterExtension)
        .addUpdateHandler("DoubleCounter", handleDoubleCounter)
        .withInitFunction(initPlugin)
        .register();

    // プラグイン登録を記録（重複登録防止用）
    window.registeredPlugins = window.registeredPlugins || {};
    window.registeredPlugins["counter-extension"] = true;

    console.log("Counter extension plugin registered successfully");
})();