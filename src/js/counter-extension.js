// counter-extension.js - 簡略化版
// フレームワーク側で互換性と初期化を処理するため、プラグイン側のコードは本質的な実装に集中

// 新しいプラグインAPIを使用
plugin("counter-extension", {
  name: "Counter Extension Plugin",
  version: "1.0.0",
  
  // ビュー実装 - モデルを受け取りReactコンポーネントを返す
  view: function(model) {
    // 実際のReactコンポーネントを定義
    const CounterExtensionComponent = function() {
      // Doubleボタンのクリック処理
      const handleDoubleClick = function() {
        // 現在の値を2倍にするメッセージをディスパッチ
        dispatch("DoubleCounter", { currentValue: model.Counter });
      };
      
      // UIを構築
      return React.createElement('div', {
        className: 'mt-6 pt-6 border-t border-gray-200'
      }, [
        React.createElement('div', { 
          className: 'font-bold mb-3',
          key: 'extension-label'
        }, 'Counter Extension:'),
        React.createElement('button', {
          className: 'px-4 py-2 bg-green-500 text-white rounded hover:bg-green-600 transition-colors',
          onClick: handleDoubleClick,
          key: 'double-button'
        }, 'Double'),
        React.createElement('div', {
          className: 'mt-2 text-sm text-gray-500 italic',
          key: 'plugin-info'
        }, `Plugin version: 1.0.0`)
      ]);
    };
    
    // コンポーネントをレンダリング
    return React.createElement(CounterExtensionComponent);
  },
  
  // カウンター値を2倍にする更新ハンドラー
  DoubleCounter: function(payload, model) {
    // Counter値を2倍にする
    const newCounter = model.Counter * 2;
    
    // CustomStateを更新（イミュータブルに）
    const customState = model.CustomState || {};
    
    // 更新されたモデルを作成して返す
    return {
      ...model,
      Counter: newCounter,
      CustomState: {
        ...customState,
        lastDoubledAt: new Date().toISOString()
      }
    };
  }
});