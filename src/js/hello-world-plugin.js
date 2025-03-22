// hello-world-plugin.js - dispatch引数化版
// dispatchをグローバル変数に頼らず、引数として受け取る形式に変更

// 新しいプラグインAPIを使用
plugin("hello-world", {
  name: "Hello World Plugin",
  version: "1.0.0",
  
  // タブとして追加
  tab: "hello",
  
  // ビュー実装 - モデルとdispatchを引数で受け取る
  view: function(model, dispatch) {
    // 実際のReactコンポーネントを定義
    const HelloComponent = function() {
      // ローカルカウンターの状態を管理
      const [count, setCount] = React.useState(0);
      
      return React.createElement('div', {
        className: 'p-5 text-center'
      }, [
        React.createElement('h1', {
          className: 'text-2xl font-bold mb-4',
          key: 'title'
        }, 'Hello World Plugin'),
        
        React.createElement('p', {
          className: 'mb-4',
          key: 'f-sharp-counter'
        }, `F# Counter: ${model.Counter}`),
        
        React.createElement('p', {
          className: 'mb-4',
          key: 'local-counter'
        }, `Local Counter: ${count}`),
        
        React.createElement('div', {
          className: 'flex justify-center gap-2',
          key: 'buttons'
        }, [
          React.createElement('button', {
            className: 'px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 transition-colors',
            onClick: () => setCount(count + 1),
            key: 'local-btn'
          }, 'Local +1'),
          
          React.createElement('button', {
            className: 'px-4 py-2 bg-green-500 text-white rounded hover:bg-green-600 transition-colors',
            onClick: () => dispatch("IncrementCounter"),
            key: 'f-sharp-btn'
          }, 'F# Counter +1')
        ])
      ]);
    };
    
    // コンポーネントをレンダリング
    return React.createElement(HelloComponent);
  }
});