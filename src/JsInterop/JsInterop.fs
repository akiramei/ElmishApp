// JsInterop.fs
// JsInteropモジュールを整理するためのファサードパターン実装
// 他のファイルの機能を集約して簡単なAPIを提供

module App.Interop

open Feliz
open App.Types
open App.JsBasicTypes
open App.JsCore
open App.ModelConverter
open App.MessageBridge
open App.PluginRegistry
open App.PluginState
open App.PluginUpdateHandler
open App.PluginViewHandler
open App.PluginLoader

// モデル変換関連の機能をエクスポート
let convertModelToJS = ModelConverter.convertModelToJS
let convertJsModelToFSharp = ModelConverter.convertJsModelToFSharp

// メッセージング関連の機能をエクスポート
let convertJsMessageToFSharpMsg = MessageBridge.convertJsMessageToFSharpMsg
let createJsDispatchFunction = MessageBridge.createJsDispatchFunction
let sendMessageToJs = MessageBridge.sendMessageToJs

// プラグイン登録関連の機能をエクスポート
let registerPluginFromJs = PluginRegistry.registerPluginFromJs
let getAvailableCustomTabs = PluginRegistry.getAvailableCustomTabs
let getRegisteredPluginIds = PluginRegistry.getRegisteredPluginIds

// プラグイン状態管理関連の機能をエクスポート
let getPluginState = PluginState.getPluginState
let updatePluginState = PluginState.updatePluginState
let getPluginStateValue = PluginState.getPluginStateValue
let setPluginStateValue = PluginState.setPluginStateValue

// プラグイン更新関連の機能をエクスポート
let applyCustomUpdate = PluginUpdateHandler.applyCustomUpdate
let executeCustomCmd = PluginUpdateHandler.executeCustomCmd

// プラグインビュー関連の機能をエクスポート
let getCustomView = PluginViewHandler.getCustomView
let decorateHomeView = PluginViewHandler.decorateHomeView
let decorateTabView = PluginViewHandler.decorateTabView

// プラグインローディング関連の機能をエクスポート
let loadAllPlugins = PluginLoader.loadAllPlugins
