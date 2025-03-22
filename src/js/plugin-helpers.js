// plugin-helpers.js - フレームワーク強化版

/**
 * F#/Elmishプラグイン開発のためのシンプルなAPIを提供するライブラリ
 * 初期化や互換性の処理をフレームワーク側で一元管理
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
   * レガシー互換性を確保する関数
   * @private
   */
  function ensureLegacyCompatibility(id, config) {
    console.log(`Setting up legacy compatibility for plugin: ${id}`);
    
    // ビューの登録
    if (config.view) {
      // タブIDでビューを登録
      if (config.tab) {
        window.customViews[config.tab] = config.view;
        console.log(`Registered legacy view for tab: ${config.tab}`);
      }
      
      // プラグインIDでビューを登録
      window.customViews[id] = config.view;
      console.log(`Registered legacy view for plugin: ${id}`);
      
      // 特殊ケース: カウンター拡張の互換性対応
      if (id === 'counter-extension') {
        window.customViews['counter-extensions'] = config.view;
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
  function createPluginDefinition(id, config) {
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
    
    // ビューの登録
    if (config.view) {
      // タブが指定されている場合はそのIDでビューを登録
      if (config.tab) {
        pluginDefinition.views[config.tab] = config.view;
      }
      
      // ID自体でもビューを登録
      pluginDefinition.views[id] = config.view;
      
      // 特殊ケース: カウンター拡張の互換性対応
      if (id === 'counter-extension') {
        pluginDefinition.views['counter-extensions'] = config.view;
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
    
    // プラグイン定義を作成
    const pluginDefinition = createPluginDefinition(id, config);
    
    // F#側に登録
    registerWithFSharp(pluginDefinition);
    
    // レガシー互換性を確保
    ensureLegacyCompatibility(id, config);
    
    // プラグイン固有の初期化処理があれば実行
    if (config.init && typeof config.init === 'function') {
      try {
        config.init();
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
   * グローバルなディスパッチ関数
   * @param {string} type - メッセージタイプ
   * @param {Object} payload - メッセージのペイロード
   */
  window.dispatch = function(type, payload = {}) {
    if (window.appDispatch) {
      console.log(`Dispatching: ${type}`, payload);
      window.appDispatch([type, payload]);
    } else {
      console.error('Dispatch function not available');
    }
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
      window.dispatch(type, payload);
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
        this.views[id] = viewFn;
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
          this.initFunction();
          console.log(`Plugin ${id} initialized via builder`);
        } catch (error) {
          console.error(`Error during plugin ${id} initialization via builder: ${error.message}`);
        }
      }
      
      return result;
    }
  }

  console.log("Plugin framework initialized");
} else {
  console.log("Plugin framework already defined");
}