// plugin-helpers.js - 技術的負債を排除した新モデル構造対応版

/**
 * F#/Elmishプラグイン開発のためのシンプルなAPIを提供するライブラリ
 * 新しいモデル構造（ApiData, CounterState等）に対応
 */

// グローバル変数が既に存在する場合は再定義しない
if (typeof window.plugin === "undefined") {
  console.log("Initializing Plugin Framework");

  // プラグイン登録を追跡するためのレジストリ
  window._pluginRegistry = window._pluginRegistry || {};

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
    return function (type, payload = {}) {
      if (_fsharpDispatch) {
        console.log(`Dispatching: ${type}`, payload);
        // 重要：F#側は配列形式 [type, payload] を期待している
        _fsharpDispatch([type, payload]);
        return true;
      } else {
        console.error("F# dispatch function not available");
        return false;
      }
    };
  }

  /**
   * プラグイン固有の状態を取得する
   * @param {string} pluginId - プラグインID
   * @param {Object} model - F#モデル
   * @returns {Object} プラグイン固有の状態
   */
  function getPluginState(pluginId, model) {
    if (!model || !model.CustomState) return {};
    return model.CustomState[pluginId] || {};
  }

  /**
   * プラグイン固有の状態を更新する
   * @param {string} pluginId - プラグインID
   * @param {Object} newState - 新しい状態
   * @param {Object} model - 元のモデル
   * @returns {Object} 更新されたモデル
   */
  function setPluginState(pluginId, newState, model) {
    const currentPluginState = getPluginState(pluginId, model);

    return {
      ...model,
      CustomState: {
        ...(model.CustomState || {}),
        [pluginId]: {
          ...currentPluginState,
          ...newState,
        },
      },
    };
  }

  // プラグイン定義を作成する関数
  function createPluginDefinition(id, config, dispatchFn) {
    // プラグイン定義の基本構造を作成
    const pluginDefinition = {
      definition: {
        id: id,
        name: config.name || id,
        version: config.version || "1.0.0",
        dependencies: config.dependencies || [],
        compatibility: config.compatibility || "1.0",
      },
      views: {},
      updateFunction: null, // 関数型アプローチ用
      commandHandlers: {},
      tabs: config.tab ? [config.tab] : [],
    };

    // ビューの登録 - 新しい形式のみサポート
    if (config.view && typeof config.view === "function") {
      // タブビューを登録
      if (config.tab) {
        pluginDefinition.views[config.tab] = function (args) {
          return config.view(args);
        };
      }

      // プラグインIDでビューを登録
      pluginDefinition.views[id] = function (args) {
        return config.view(args);
      };
    }

    // 関数型アプローチ対応：update関数を使用
    if (config.update && typeof config.update === "function") {
      // オリジナルのupdate関数を保存
      const originalUpdateFn = config.update;

      // 改良されたupdate関数をラップ
      const wrappedUpdateFn = function (messageType, payload, model) {
        // オリジナルのupdate関数を呼び出し
        const args = {
          messageType: messageType,
          payload: payload,
          model: model
        };

        const updatedModel = originalUpdateFn(args);

        // 状態の変更がない場合は元のモデルを返す
        if (!updatedModel || updatedModel === model) return model;

        return updatedModel;
      };

      // 統一update関数を登録
      pluginDefinition.updateFunction = wrappedUpdateFn;
    }

    // コマンドハンドラーの登録
    if (config.commands) {
      Object.keys(config.commands).forEach((cmdKey) => {
        if (typeof config.commands[cmdKey] === "function") {
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
        console.log(
          `Plugin ${pluginDefinition.definition.id} successfully registered with F#`
        );
        return true;
      } catch (error) {
        console.error(`Error registering plugin with F#: ${error.message}`);
        return false;
      }
    } else {
      console.warn("F# plugin registration function not available");
      return false;
    }
  }

  /**
   * シンプルなプラグイン登録関数
   * @param {string} id - プラグインID
   * @param {Object} config - プラグイン設定
   * @returns {Object} 登録されたプラグイン
   */
  window.plugin = function (id, config) {
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

    // プラグイン固有の初期化処理があれば実行
    if (config.init && typeof config.init === "function") {
      try {
        // 初期化関数にもdispatchを渡す
        config.init(dispatchFn);
        console.log(`Plugin ${id} successfully initialized`);
      } catch (error) {
        console.error(
          `Error during plugin ${id} initialization: ${error.message}`
        );
      }
    }

    // レジストリに追加
    window._pluginRegistry[id] = pluginDefinition;

    return pluginDefinition;
  };

  // プラグイン状態管理のヘルパー関数
  window.plugin.getState = getPluginState;
  window.plugin.setState = setPluginState;

  // F#側からdispatch関数を設定するためのグローバル関数を公開
  window._setFSharpDispatch = _setFSharpDispatch;

  // ====== 新しいモデル構造へのアクセスヘルパー ======
  
  // ApiDataヘルパー
  window.plugin.api = {
    // 製品関連
    getProducts: function(model) {
      // モデル構造の各階層を詳細にチェック
      if (!model || !model.ApiData || !model.ApiData.ProductData) return [];
      const status = model.ApiData.ProductData.Status;
      // 比較の前に文字列変換を確保
      const statusStr = String(status).toLowerCase();
      return statusStr === "success" && Array.isArray(model.ApiData.ProductData.Products) 
        ? model.ApiData.ProductData.Products 
        : [];
    },
    
    getProductsStatus: function(model) {
      if (!model || !model.ApiData || !model.ApiData.ProductData) return 'notStarted';
      return String(model.ApiData.ProductData.Status).toLowerCase();
    },
    
    isProductsLoading: function(model) {
      // 文字列比較前の型チェック
      if (!model || !model.ApiData || !model.ApiData.ProductData) return false;
      const status = String(model.ApiData.ProductData.Status).toLowerCase();
      return status === "loading";
    },
    
    hasProductsError: function(model) {
      if (!model || !model.ApiData || !model.ApiData.ProductData) return false;
      const status = String(model.ApiData.ProductData.Status).toLowerCase();
      return status === "failed";
    },
    
    // ユーザー関連
    getUsers: function(model) {
      // モデル構造の各階層を詳細にチェック
      if (!model || !model.ApiData || !model.ApiData.UserData) return [];
      const status = model.ApiData.UserData.Status;
      // 比較の前に文字列変換を確保
      const statusStr = String(status).toLowerCase();
      return statusStr === "success" && Array.isArray(model.ApiData.UserData.Users) 
        ? model.ApiData.UserData.Users 
        : [];
    },
    
    getUsersStatus: function(model) {
      if (!model || !model.ApiData || !model.ApiData.UserData) return 'notStarted';
      return String(model.ApiData.UserData.Status).toLowerCase();
    },
    
    isUsersLoading: function(model) {
      if (!model || !model.ApiData || !model.ApiData.UserData) return false;
      const status = String(model.ApiData.UserData.Status).toLowerCase();
      return status === "loading";
    },
    
    hasUsersError: function(model) {
      if (!model || !model.ApiData || !model.ApiData.UserData) return false;
      const status = String(model.ApiData.UserData.Status).toLowerCase();
      return status === "failed";
    }
  };

  // カウンター関連ヘルパー
  window.plugin.counter = {
    getValue: function(model) {
      if (!model || !model.CounterState) return 0;
      return model.CounterState.Counter ?? 0;
    },
    
    // カウンター増減のショートカット - 統一された配列形式
    increment: function(dispatch) {
      dispatch(["CounterMsg", { type: "IncrementCounter" }]);
    },
    
    decrement: function(dispatch) {
      dispatch(["CounterMsg", { type: "DecrementCounter" }]);
    }
  };

  // ProductsState関連ヘルパー
  window.plugin.products = {
    getPageInfo: function(model) {
      if (!model || !model.ProductsState || !model.ProductsState.PageInfo) {
        return {
          CurrentPage: 1,
          PageSize: 10,
          TotalItems: 0,
          TotalPages: 1
        };
      }
      return model.ProductsState.PageInfo;
    },
    
    getSelectedIds: function(model) {
      if (!model || !model.ProductsState) return [];
      return Array.isArray(model.ProductsState.SelectedIds) 
        ? model.ProductsState.SelectedIds 
        : [];
    },
    
    isSelected: function(model, productId) {
      const selectedIds = this.getSelectedIds(model);
      return selectedIds.includes(productId);
    },
    
    // 製品関連アクションのディスパッチヘルパー - 統一された配列形式
    changePage: function(dispatch, page) {
      dispatch(["ProductsMsg", { type: "ChangePage", page: page }]);
    },
    
    changePageSize: function(dispatch, size) {
      dispatch(["ProductsMsg", { type: "ChangePageSize", size: size }]);
    },
    
    toggleSelection: function(dispatch, productId) {
      dispatch(["ProductsMsg", { type: "ToggleProductSelection", id: productId }]);
    },
    
    viewDetails: function(dispatch, productId) {
      dispatch(["ProductsMsg", { type: "ViewProductDetails", id: productId }]);
    }
  };

  console.log("Plugin framework initialized with new model structure support");
} else {
  console.log("Plugin framework already defined");
}