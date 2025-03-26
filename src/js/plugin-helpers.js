// plugin-helpers.js - improved version with unified args pattern and robust handling

/**
 * F#/Elmishプラグイン開発のためのシンプルなAPIを提供するライブラリ
 * Elmishパターンに合わせてdispatchを引数として渡す形式に変更
 * プラグイン固有の状態管理を改善
 * view関数とupdate関数に統一されたargsパターンを適用
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
    return function (msgType, payload = {}) {
      if (_fsharpDispatch) {
        console.log(`Dispatching: ${msgType}`, payload);
        // 重要：F#側は配列形式 [msgType, payload] を期待している
        _fsharpDispatch([msgType, payload]);
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

  /**
   * 関数の引数を標準化する共通関数
   * @private
   */
  function normalizeArgs(args, fnType) {
    // すでにオブジェクトの場合はそのまま使用
    if (typeof args === 'object' && args !== null) {
      // 必要なプロパティが揃っているか確認
      const normalizedArgs = { ...args };
      
      // 古いプロパティ名を新しいプロパティ名に変換
      if (fnType === 'update' && args.type !== undefined && args.msgType === undefined) {
        normalizedArgs.msgType = args.type;
      }
      
      return normalizedArgs;
    }
    
    // 単一の引数の場合（古い形式）
    return fnType === 'view' 
      ? { model: args }
      : {}; // updateの場合は空オブジェクト
  }

  /**
   * 関数をラップして引数形式の互換性を提供する共通関数
   * @private
   */
  function wrapFunction(id, originalFn, fnType, dispatchFn) {
    return function() {
      // 単一オブジェクト形式と複数引数形式の処理
      if (arguments.length > 1 || typeof arguments[0] !== 'object' || arguments[0] === null) {
        console.warn(`Plugin ${id}: Deprecated ${fnType} function call detected. Please update to use args object.`);
        
        // 引数を適切なオブジェクトに変換
        let argsObj;
        if (fnType === 'view') {
          // view関数: (model) -> {model, dispatch}
          const model = arguments[0];
          argsObj = {
            model: model,
            dispatch: dispatchFn
          };
        } else if (fnType === 'update') {
          // update関数: (msgType, payload, model) -> {msgType, payload, model}
          const msgType = arguments[0];
          const payload = arguments.length > 1 ? arguments[1] : null;
          const model = arguments.length > 2 ? arguments[2] : null;
          argsObj = {
            msgType: msgType,
            payload: payload,
            model: model
          };
        }
        
        return originalFn(argsObj);
      }
      
      // 新形式: args形式で呼び出し
      const normalizedArgs = normalizeArgs(arguments[0], fnType);
      
      // dispatchを追加（view関数の場合）
      if (fnType === 'view' && !normalizedArgs.dispatch) {
        normalizedArgs.dispatch = dispatchFn;
      }
      
      return originalFn(normalizedArgs);
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
      updateHandlers: {},
      updateFunction: null, // 関数型アプローチ用
      commandHandlers: {},
      tabs: config.tab ? [config.tab] : [],
    };

    // ビューの登録 - 共通関数を使用してラップ
    if (config.view) {
      const wrappedViewFn = wrapFunction(id, config.view, 'view', dispatchFn);
      
      if (config.tab) {
        pluginDefinition.views[config.tab] = wrappedViewFn;
      }
      
      pluginDefinition.views[id] = wrappedViewFn;
    }

    // 関数型アプローチ対応：update関数を共通関数でラップ
    if (config.update && typeof config.update === "function") {
      // 共通関数を使用してラップ
      const wrappedUpdateFn = wrapFunction(id, config.update, 'update', dispatchFn);
      
      // 統一update関数を登録
      pluginDefinition.updateFunction = wrappedUpdateFn;

      // 統一更新ハンドラーを登録
      pluginDefinition.updateHandlers["__unified_update__"] = function (payload, model) {
        // payload形式は [msgType, actualPayload]
        if (Array.isArray(payload) && payload.length >= 2) {
          const msgType = payload[0];
          const actualPayload = payload[1];

          console.log(`Calling unified update with message: ${msgType}`);

          // argsオブジェクトを作成して呼び出し
          return wrappedUpdateFn({
            msgType: msgType,
            payload: actualPayload,
            model: model
          });
        }
        return model;
      };
    }
    // 従来のスタイルも引き続きサポート
    else {
      // 各メッセージハンドラーを登録
      Object.keys(config).forEach((key) => {
        if (
          typeof config[key] === "function" &&
          key !== "view" &&
          key !== "init"
        ) {
          // オリジナルのハンドラー関数
          const originalHandler = config[key];

          // ラップされたハンドラー関数
          pluginDefinition.updateHandlers[key] = function (payload, model) {
            // オリジナルのハンドラーを呼び出し - argsオブジェクトを使用
            const updatedModel = originalHandler({
              msgType: key,
              payload: payload,
              model: model
            });

            // 状態の変更がない場合は元のモデルを返す
            if (!updatedModel || updatedModel === model) return model;

            return updatedModel;
          };
        }
      });
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

  console.log("Plugin framework initialized with unified args pattern");
} else {
  console.log("Plugin framework already defined");

  // すでに定義されている場合も状態管理ヘルパーを追加
  if (!window.plugin.getState) {
    window.plugin.getState = function (pluginId, model) {
      if (!model || !model.CustomState) return {};
      return model.CustomState[pluginId] || {};
    };
  }

  if (!window.plugin.setState) {
    window.plugin.setState = function (pluginId, newState, model) {
      const currentPluginState = window.plugin.getState(pluginId, model);

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
    };
  }
}