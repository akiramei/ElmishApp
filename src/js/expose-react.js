// expose-react.js - 改良版
import React from 'react';

// ReactをグローバルWindowオブジェクトに公開
window.React = React;

// 便利なReactフック
window.React.useLocalState = function(initialState) {
  const [state, setState] = React.useState(initialState);
  
  // 値と更新関数を含むオブジェクトを返す
  return {
    value: state,
    set: setState,
    reset: () => setState(initialState)
  };
};

// React Effectのログラッパー
window.React.useLogEffect = function(label, deps = []) {
  React.useEffect(() => {
    console.log(`${label} mounted`);
    return () => {
      console.log(`${label} unmounted`);
    };
  }, deps);
};

// 単純なスタイル付きコンポーネント
window.styled = function(element, styles) {
  return React.forwardRef((props, ref) => {
    const combinedProps = {
      ...props,
      className: `${styles} ${props.className || ''}`,
      ref
    };
    return React.createElement(element, combinedProps);
  });
};

// JSX変換なしでUIを構築しやすくする補助関数
window.ui = {
  // レイアウト要素
  div: (props, ...children) => React.createElement('div', props, ...children),
  section: (props, ...children) => React.createElement('section', props, ...children),
  main: (props, ...children) => React.createElement('main', props, ...children),
  
  // コンテナ要素
  card: (props, ...children) => React.createElement('div', { 
    ...props, 
    className: `p-4 border rounded-lg shadow-sm bg-white ${props?.className || ''}`
  }, ...children),
  
  // テキスト要素
  h1: (text, props) => React.createElement('h1', { 
    ...props, 
    className: `text-2xl font-bold ${props?.className || ''}` 
  }, text),
  
  h2: (text, props) => React.createElement('h2', { 
    ...props,
    className: `text-xl font-bold ${props?.className || ''}`
  }, text),
  
  text: (text, props) => React.createElement('p', props, text),
  
  // 入力要素
  button: (text, onClick, props) => React.createElement('button', {
    ...props,
    onClick,
    className: `px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 transition-colors ${props?.className || ''}`
  }, text),
  
  slider: (value, onChange, props) => React.createElement('input', {
    ...props,
    type: 'range',
    value,
    onChange,
    className: `w-full h-2 bg-gray-200 rounded-lg appearance-none cursor-pointer ${props?.className || ''}`
  })
};

console.log("React and helpers exposed globally:", window.React !== undefined);