// plugin-helpers.js - dispatch引数化版

/**
 * F#/Elmishプラグイン開発のためのシンプルなAPIを提供するライブラリ
 * Elmishパターンに合わせてdispatchを引数として渡す形式に変更
 */

// グローバル変数が既に存在する場合は再定義しない
if (typeof window.plugin === 'undefined') {
  console.log("Initializing Plugin Framework");
  
  // プラグイン登録を追跡するためのレジストリ
  window._pluginRegistry = window._pluginRegistry || {};
  
  // グローバル変数の初期化 - フレームワーク側の責任
  window.customViews = window.customViews || {};
  window.customUpdates = window.customUpdates || {};
  window.customTabs = window.customTabs || [];
  window.customCmdHandlers = window.customCmdHandlers || {};
  
  /**
   * F#側からのdispatch関数の参照を保持
   * @private
   */
  let _fsharpDispatch = null;
  
  /**
   * F#側からdispatch関数を設定する
   * @private
   */
  function _setFSharpDispatch(dispatch) {
    _fsharpDispatch = dispatch;
    console.log("F# dispatch function registered");
  }
  
  /**
   * F#へメッセージをディスパッチする関数を生成
   * @private
   */
  function _createDispatchFunction() {
    return function(type, payload = {}) {
      if (_fsharpDispatch) {
        console.log(`Dispatching: ${type}`, payload);
        // 重要：F#側は配列形式 [type, payload] を期待している
        _fsharpDispatch([type, payload]);
        return true;
      } else {
        console.error('F# dispatch function not available');
        return false;
      }
    };
  }
  
  /**
   * レガシー互換性を確保する関数
   * @private
   */
  function ensureLegacyCompatibility(id, config, dispatchFn) {
    console.log(`Setting up legacy compatibility for plugin: ${id}`);
    
    // ビューの登録 - dispatch関数をラップして渡す
    if (config.view) {
      // 元のビュー関数を保存
      const originalViewFn = config.view;
      
      // タブIDでビューを登録 - dispatchを渡すようにラップ
      if (config.tab) {
        window.customViews[config.tab] = function(model) {
          return originalViewFn(model, dispatchFn);
        };
        console.log(`Registered legacy view for tab: ${config.tab}`);
      }
      
      // プラグインIDでビューを登録
      window.customViews[id] = function(model) {
        return originalViewFn(model, dispatchFn);
      };
      console.log(`Registered legacy view for plugin: ${id}`);
      
      // 特殊ケース: カウンター拡張の互換性対応
      if (id === 'counter-extension') {
        window.customViews['counter-extensions'] = function(model) {
          return originalViewFn(model, dispatchFn);
        };
        console.log("Registered legacy view for counter-extensions");
      }
    }
    
    // アップデートハンドラーの登録
    Object.keys(config).forEach(key => {
      if (typeof config[key] === 'function' && key !== 'view' && key !== 'init') {
        window.customUpdates[key] = config[key];
        console.log(`Registered legacy update handler: ${key}`);
      }
    });
    
    // タブの登録
    if (config.tab && !window.customTabs.includes(config.tab)) {
      window.customTabs.push(config.tab);
      console.log(`Registered legacy tab: ${config.tab}`);
    }
  }
  
  /**
   * プラグイン定義を作成する関数
   * @private
   */
  function createPluginDefinition(id, config, dispatchFn) {
    const pluginDefinition = {
      definition: {
        id: id,
        name: config.name || id,
        version: config.version || "1.0.0",
        dependencies: config.dependencies || [],
        compatibility: config.compatibility || "1.0"
      },
      views: {},
      updateHandlers: {},
      commandHandlers: {},
      tabs: config.tab ? [config.tab] : []
    };
    
    // ビューの登録 - dispatch関数をラップして渡す
    if (config.view) {
      const originalViewFn = config.view;
      
      // タブが指定されている場合はそのIDでビューを登録
      if (config.tab) {
        pluginDefinition.views[config.tab] = function(model) {
          return originalViewFn(model, dispatchFn);
        };
      }
      
      // ID自体でもビューを登録
      pluginDefinition.views[id] = function(model) {
        return originalViewFn(model, dispatchFn);
      };
      
      // 特殊ケース: カウンター拡張の互換性対応
      if (id === 'counter-extension') {
        pluginDefinition.views['counter-extensions'] = function(model) {
          return originalViewFn(model, dispatchFn);
        };
      }
    }
    
    // アクションハンドラーの登録
    Object.keys(config).forEach(key => {
      if (typeof config[key] === 'function' && key !== 'view' && key !== 'init') {
        pluginDefinition.updateHandlers[key] = config[key];
      }
    });
    
    // コマンドハンドラーの登録（もしあれば）
    if (config.commands) {
      Object.keys(config.commands).forEach(cmdKey => {
        if (typeof config.commands[cmdKey] === 'function') {
          pluginDefinition.commandHandlers[cmdKey] = config.commands[cmdKey];
        }
      });
    }
    
    return pluginDefinition;
  }
  
  /**
   * F#側にプラグインを登録する関数
   * @private
   */
  function registerWithFSharp(pluginDefinition) {
    if (window.registerFSharpPlugin) {
      try {
        window.registerFSharpPlugin(pluginDefinition);
        console.log(`Plugin ${pluginDefinition.definition.id} successfully registered with F#`);
        return true;
      } catch (error) {
        console.error(`Error registering plugin with F#: ${error.message}`);
        return false;
      }
    } else {
      console.warn('F# plugin registration function not available');
      return false;
    }
  }
  
  /**
   * シンプルなプラグイン登録関数
   * @param {string} id - プラグインID
   * @param {Object} config - プラグイン設定
   * @returns {Object} 登録されたプラグイン
   */
  window.plugin = function(id, config) {
    // 重複登録チェック
    if (window._pluginRegistry[id]) {
      console.log(`Plugin ${id} already registered, skipping`);
      return window._pluginRegistry[id];
    }
    
    console.log(`Registering plugin: ${id}`);
    
    // dispatch関数を作成
    const dispatchFn = _createDispatchFunction();
    
    // プラグイン定義を作成
    const pluginDefinition = createPluginDefinition(id, config, dispatchFn);
    
    // F#側に登録
    registerWithFSharp(pluginDefinition);
    
    // レガシー互換性を確保
    ensureLegacyCompatibility(id, config, dispatchFn);
    
    // プラグイン固有の初期化処理があれば実行
    if (config.init && typeof config.init === 'function') {
      try {
        // 初期化関数にもdispatchを渡す
        config.init(dispatchFn);
        console.log(`Plugin ${id} successfully initialized`);
      } catch (error) {
        console.error(`Error during plugin ${id} initialization: ${error.message}`);
      }
    }
    
    // レジストリに追加
    window._pluginRegistry[id] = pluginDefinition;
    
    return pluginDefinition;
  };
  
  /**
   * レガシープラグインの登録をサポートするAPI
   */
  window.AppPlugins = {
    register: function(plugin) {
      console.log("Legacy plugin registration called");
      
      // F#側に登録
      if (window.registerFSharpPlugin) {
        return window.registerFSharpPlugin(plugin);
      }
      return false;
    },
    
    dispatch: function(type, payload) {
      // 内部のディスパッチ関数を使用
      const dispatchFn = _createDispatchFunction();
      return dispatchFn(type, payload);
    },
    
    createBuilder: function(id, name, version) {
      console.log("Legacy builder creation called");
      return new PluginBuilder(id, name, version);
    }
  };

  /**
   * 後方互換性のためのプラグインビルダークラス
   */
  class PluginBuilder {
    constructor(id, name, version) {
      this.definition = {
        id: id || "unknown-plugin",
        name: name || "Unknown Plugin",
        version: version || "1.0.0",
        dependencies: [],
        compatibility: "1.0"
      };
      
      this.views = {};
      this.updateHandlers = {};
      this.commandHandlers = {};
      this.tabs = [];
      this.initFunction = null;
    }
    
    withDependencies(dependencies) {
      if (Array.isArray(dependencies)) {
        this.definition.dependencies = dependencies;
      }
      return this;
    }
    
    withCompatibility(compatibility) {
      if (typeof compatibility === 'string') {
        this.definition.compatibility = compatibility;
      }
      return this;
    }
    
    addView(id, viewFn) {
      if (typeof id === 'string' && typeof viewFn === 'function') {
        // ビュー関数をラップして、dispatchを渡す
        this.views[id] = function(model) {
          const dispatchFn = _createDispatchFunction();
          return viewFn(model, dispatchFn);
        };
      }
      return this;
    }
    
    addUpdateHandler(messageType, handlerFn) {
      if (typeof messageType === 'string' && typeof handlerFn === 'function') {
        this.updateHandlers[messageType] = handlerFn;
      }
      return this;
    }
    
    addCommandHandler(commandType, handlerFn) {
      if (typeof commandType === 'string' && typeof handlerFn === 'function') {
        this.commandHandlers[commandType] = handlerFn;
      }
      return this;
    }
    
    addTab(tabId) {
      if (typeof tabId === 'string' && !this.tabs.includes(tabId)) {
        this.tabs.push(tabId);
      }
      return this;
    }
    
    withInitFunction(initFn) {
      if (typeof initFn === 'function') {
        this.initFunction = initFn;
      }
      return this;
    }
    
    register() {
      const plugin = {
        definition: this.definition,
        views: this.views,
        updateHandlers: this.updateHandlers,
        commandHandlers: this.commandHandlers,
        tabs: this.tabs,
        init: this.initFunction
      };
      
      // レガシープラグインを登録
      const result = window.AppPlugins.register(plugin);
      
      // レガシー互換性を確保
      const id = this.definition.id;
      const dispatchFn = _createDispatchFunction();
      
      // ビューのレガシーサポート
      Object.keys(this.views).forEach(viewKey => {
        window.customViews = window.customViews || {};
        window.customViews[viewKey] = this.views[viewKey];
        console.log(`Registered legacy view for ${viewKey} from builder`);
      });
      
      // アップデートハンドラーのレガシーサポート
      Object.keys(this.updateHandlers).forEach(updateKey => {
        window.customUpdates = window.customUpdates || {};
        window.customUpdates[updateKey] = this.updateHandlers[updateKey];
        console.log(`Registered legacy update handler for ${updateKey} from builder`);
      });
      
      // タブのレガシーサポート
      this.tabs.forEach(tab => {
        window.customTabs = window.customTabs || [];
        if (!window.customTabs.includes(tab)) {
          window.customTabs.push(tab);
          console.log(`Registered legacy tab ${tab} from builder`);
        }
      });
      
      // 初期化関数の実行
      if (this.initFunction) {
        try {
          this.initFunction(dispatchFn);
          console.log(`Plugin ${id} initialized via builder`);
        } catch (error) {
          console.error(`Error during plugin ${id} initialization via builder: ${error.message}`);
        }
      }
      
      return result;
    }
  }

  // F#側からdispatch関数を設定するためのグローバル関数を公開
  window._setFSharpDispatch = _setFSharpDispatch;

  console.log("Plugin framework initialized");
} else {
  console.log("Plugin framework already defined");
}