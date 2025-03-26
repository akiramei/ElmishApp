// fsharp-js-bridge.js - improved version with unified args pattern
// F#とJavaScript間の相互運用をサポートするグローバルヘルパー関数

// グローバルな名前空間を作成
window.FSharpJsBridge = {
  // プラグイン更新関数を安全に呼び出す（従来バージョン）
  callUpdateHandler: function (updateFn, payload, model) {
    console.log("JS Bridge: Calling update handler with", {
      updateFn: updateFn,
      payload: payload,
      model: model,
    });

    try {
      if (typeof updateFn === "function") {
        var result = updateFn(payload, model);
        console.log("JS Bridge: Update handler returned", result);
        return result || model;
      } else {
        console.error("JS Bridge: Not a function:", updateFn);
        return model;
      }
    } catch (error) {
      console.error("JS Bridge: Error calling update function:", error);
      return model;
    }
  },

  // 統一update関数を安全に呼び出す（関数型アプローチ用）- 複数引数バージョン
  callUnifiedUpdateHandler: function (updateFn, msgType, payload, model) {
    console.log("JS Bridge: Calling unified update handler with", {
      msgType: msgType,
      payload: payload,
    });

    try {
      if (typeof updateFn === "function") {
        var result = updateFn(msgType, payload, model);
        console.log("JS Bridge: Unified update returned", result);
        return result || model;
      } else {
        console.error("JS Bridge: Not a function:", updateFn);
        return model;
      }
    } catch (error) {
      console.error("JS Bridge: Error calling unified update function:", error);
      console.error(error.stack);
      return model;
    }
  },

  // args形式でのプラグイン関数呼び出し - 新しいパターン
  callFunctionWithArgs: function (fn, args) {
    console.log("JS Bridge: Calling function with args", {
      function: fn,
      args: args,
    });

    try {
      if (typeof fn === "function") {
        // 引数の標準化 - 古いプロパティ名も対応
        const normalizedArgs = { ...args };
        
        // type から msgType へのマッピング（後方互換性）
        if (args.messageType !== undefined && args.msgType === undefined) {
          normalizedArgs.msgType = args.messageType;
        } else if (args.type !== undefined && args.msgType === undefined) {
          normalizedArgs.msgType = args.type;
        }
        
        var result = fn(normalizedArgs);
        console.log("JS Bridge: Function returned", result);
        return result || args.model;
      } else {
        console.error("JS Bridge: Not a function:", fn);
        return args.model;
      }
    } catch (error) {
      console.error("JS Bridge: Error calling function with args:", error);
      console.error(error.stack);
      return args.model;
    }
  },

  // update関数を統一args形式で呼び出す - 新しいパターン
  callUpdateWithArgs: function (updateFn, msgType, payload, model) {
    console.log("JS Bridge: Calling update with args format", {
      msgType: msgType,
      payload: payload,
    });

    try {
      // argsオブジェクトを作成
      var args = {
        msgType: msgType,
        payload: payload,
        model: model
      };

      if (typeof updateFn === "function") {
        var result = updateFn(args);
        console.log("JS Bridge: Update with args returned", result);
        return result || model;
      } else {
        console.error("JS Bridge: Not a function:", updateFn);
        return model;
      }
    } catch (error) {
      console.error("JS Bridge: Error calling update with args:", error);
      console.error(error.stack);
      return model;
    }
  },

  // オブジェクトのデバッグ用関数
  logObject: function (label, obj) {
    console.log("JS Bridge: " + label, obj);
    return obj;
  },
};

console.log("F# JS Bridge initialized with args pattern support");