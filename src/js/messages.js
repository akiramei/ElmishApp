// messages.js - メッセージ定数の一元管理
/**
 * @namespace AppMsg
 * @description アプリケーションのメッセージ定数
 */
const AppMsg = {
  // コアメッセージ
  INCREMENT_COUNTER: "IncrementCounter",
  DECREMENT_COUNTER: "DecrementCounter",
  NAVIGATE_TO: "NavigateTo",
  CLEAR_ERROR: "ClearError",
  SET_ERROR: "SetError",

  // プラグイン関連メッセージ
  PLUGIN_TAB_ADDED: "PluginTabAdded",
  PLUGIN_REGISTERED: "PluginRegistered",
  PLUGINS_LOADED: "PluginsLoaded",
};

/**
 * プラグイン固有のメッセージを登録するためのヘルパー関数
 * @param {string} namespace 名前空間
 * @param {Object} messages メッセージ定義オブジェクト
 * @returns {Object} 登録されたメッセージオブジェクト
 */
function registerMessages(namespace, messages) {
  if (!AppMsg[namespace]) {
    AppMsg[namespace] = {};
  }

  Object.keys(messages).forEach((key) => {
    AppMsg[namespace][key] = messages[key];
  });

  return AppMsg[namespace];
}

// ブラウザ環境で使用
window.AppMsg = AppMsg;
window.registerMessages = registerMessages;

console.log("Message constants loaded");
