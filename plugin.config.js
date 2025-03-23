import { defineConfig } from "vite";
import react from "@vitejs/plugin-react";
import path from "path";
import fs from "fs";

// プラグインディレクトリ内のJSXファイルを自動的に検出
const pluginsDir = path.resolve(__dirname, "src/plugins");
const entries = {};

// ディレクトリが存在するか確認
if (fs.existsSync(pluginsDir)) {
  // ディレクトリ内のJSXファイルを検索
  fs.readdirSync(pluginsDir).forEach((file) => {
    if (file.endsWith(".jsx")) {
      // ファイル名から拡張子を除去してエントリーポイント名として使用
      const name = file.replace(".jsx", "");
      entries[name] = path.resolve(pluginsDir, file);
    }
  });
}

// プラグインビルド用の設定
export default defineConfig({
  plugins: [react()],
  build: {
    outDir: "./src",
    emptyOutDir: false, // 既存のファイルを削除しない
    lib: {
      formats: ["iife"],
      entry: entries,
      name: "Plugin",
    },
    rollupOptions: {
      external: ["react", "react-dom"],
      output: {
        entryFileNames: "js/[name].js",
        globals: {
          react: "React",
          "react-dom": "ReactDOM",
        },
      },
    },
  },
});
