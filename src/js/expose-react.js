// expose-react.js
import React from 'react';

// ReactをグローバルWindowオブジェクトに公開
window.React = React;
console.log("React exposed globally:", window.React !== undefined);