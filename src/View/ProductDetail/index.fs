// src/View/ProductDetail/index.fs
module App.View.ProductDetail.Index

open App.Types
open App.Shared

// 公開コンポーネントのエクスポート
let renderProductDetail = App.View.ProductDetail.RenderProductDetail
let renderProductEditForm = ProductEditForm.RenderProductEditForm

// タブの定義をエクスポート
type DetailTab = Components.Tabs.DetailTab
