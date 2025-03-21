// fsharp-js-bridge.js
// F#とJavaScript間の相互運用をサポートするグローバルヘルパー関数

// グローバルな名前空間を作成
window.FSharpJsBridge = {
    // プラグイン更新関数を安全に呼び出す
    callUpdateHandler: function(updateFn, payload, model) {
        console.log("JS Bridge: Calling update handler with", { 
            updateFn: updateFn, 
            payload: payload, 
            model: model 
        });
        
        try {
            if (typeof updateFn === 'function') {
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
    
    // オブジェクトのデバッグ用関数
    logObject: function(label, obj) {
        console.log("JS Bridge: " + label, obj);
        return obj;
    }
};

console.log("F# JS Bridge initialized");