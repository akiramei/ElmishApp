// plugin-helpers.js

/**
 * F#/Elmishプラグインの開発を容易にするヘルパーライブラリ
 * 
 * このライブラリを使うと、グローバルオブジェクトを直接操作することなく
 * プラグインを作成することができます。
 */

// グローバル変数がすでに存在する場合は再定義しない
if (typeof window.AppPlugins === 'undefined') {
    console.log("Initializing AppPlugins helper");
    
    const AppPlugins = {
        /**
         * プラグインを登録します
         * @param {Object} plugin - プラグイン定義オブジェクト
         * @returns {boolean} 登録が成功したかどうか
         */
        register: function(plugin) {
            console.log("Registering plugin:", plugin.definition.id);
            
            // F#側のプラグイン登録関数を呼び出す
            if (window.registerFSharpPlugin) {
                return window.registerFSharpPlugin(plugin);
            } else {
                console.error("F# plugin registration function not available");
                return false;
            }
        },
        
        /**
         * メッセージをディスパッチします
         * @param {string} type - メッセージタイプ
         * @param {Object} payload - メッセージのペイロード
         */
        dispatch: function(type, payload) {
            console.log("Dispatching message:", type, payload);
            
            if (window.appDispatch) {
                window.appDispatch([type, payload]);
            } else {
                console.error("F# dispatch function not available");
            }
        },
        
        /**
         * プラグインビルダーを作成します
         * @param {string} id - プラグインID
         * @param {string} name - プラグイン名
         * @param {string} version - プラグインバージョン
         * @returns {Object} プラグインビルダーオブジェクト
         */
        createBuilder: function(id, name, version) {
            return new PluginBuilder(id, name, version);
        }
    };

    /**
     * プラグインを構築するためのビルダークラス
     */
    class PluginBuilder {
        /**
         * @param {string} id - プラグインID
         * @param {string} name - プラグイン名
         * @param {string} version - プラグインバージョン
         */
        constructor(id, name, version) {
            this.definition = {
                id: id,
                name: name,
                version: version,
                dependencies: [],
                compatibility: "1.0"
            };
            
            this.views = {};
            this.updateHandlers = {};
            this.commandHandlers = {};
            this.tabs = [];
            this.initFunction = null;
        }
        
        /**
         * プラグインの依存関係を設定します
         * @param {Array<string>} dependencies - 依存するプラグインのID配列
         * @returns {PluginBuilder} このビルダーインスタンス
         */
        withDependencies(dependencies) {
            this.definition.dependencies = dependencies;
            return this;
        }
        
        /**
         * プラグインの互換性バージョンを設定します
         * @param {string} compatibility - 互換性バージョン
         * @returns {PluginBuilder} このビルダーインスタンス
         */
        withCompatibility(compatibility) {
            this.definition.compatibility = compatibility;
            return this;
        }
        
        /**
         * カスタムビューを追加します
         * @param {string} id - ビューID
         * @param {Function} viewFn - modelを受け取りReactElementを返す関数
         * @returns {PluginBuilder} このビルダーインスタンス
         */
        addView(id, viewFn) {
            this.views[id] = viewFn;
            return this;
        }
        
        /**
         * 更新ハンドラーを追加します
         * @param {string} messageType - メッセージタイプ
         * @param {Function} handlerFn - (payload, model)を受け取り更新されたモデルを返す関数
         * @returns {PluginBuilder} このビルダーインスタンス
         */
        addUpdateHandler(messageType, handlerFn) {
            this.updateHandlers[messageType] = handlerFn;
            return this;
        }
        
        /**
         * コマンドハンドラーを追加します
         * @param {string} commandType - コマンドタイプ
         * @param {Function} handlerFn - payloadを受け取り副作用を実行する関数
         * @returns {PluginBuilder} このビルダーインスタンス
         */
        addCommandHandler(commandType, handlerFn) {
            this.commandHandlers[commandType] = handlerFn;
            return this;
        }
        
        /**
         * カスタムタブを追加します
         * @param {string} tabId - タブID
         * @returns {PluginBuilder} このビルダーインスタンス
         */
        addTab(tabId) {
            if (!this.tabs.includes(tabId)) {
                this.tabs.push(tabId);
            }
            return this;
        }
        
        /**
         * 初期化関数を設定します
         * @param {Function} initFn - プラグイン初期化時に実行される関数
         * @returns {PluginBuilder} このビルダーインスタンス
         */
        withInitFunction(initFn) {
            this.initFunction = initFn;
            return this;
        }
        
        /**
         * プラグインを構築して登録します
         * @returns {boolean} 登録が成功したかどうか
         */
        register() {
            const plugin = {
                definition: this.definition,
                views: this.views,
                updateHandlers: this.updateHandlers,
                commandHandlers: this.commandHandlers,
                tabs: this.tabs,
                init: this.initFunction
            };
            
            return AppPlugins.register(plugin);
        }
    }

    // グローバルに公開
    window.AppPlugins = AppPlugins;
    console.log("AppPlugins helper initialized and exposed globally");
} else {
    console.log("AppPlugins already defined, skipping initialization");
}

// ブラウザ環境ではexportを使わない (ESモジュールとしても使えるようにコメントアウト)
// export default AppPlugins;