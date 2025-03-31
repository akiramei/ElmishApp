// products-tab.js - 製品一覧タブプラグイン（修正版）
(function () {
  // プラグインID定義
  const PLUGIN_ID = "products-tab";

  // 製品プラグイン固有のメッセージ
  const ProductsMsg = {
    SELECT_PRODUCT: "SelectProduct",
    SAVE_SCROLL_POSITION: "SaveScrollPosition",
  };

  // APIメッセージ
  const ApiMsg = {
    FETCH_PRODUCTS: "ApiMsg.FetchProducts",
    FETCH_PRODUCT: "ApiMsg.FetchProduct",
  };

  // 新しいプラグインAPIを使用
  plugin(PLUGIN_ID, {
    name: "Products Tab Plugin",
    version: "1.0.0",

    // タブとして追加
    tab: "products",

    // 初期化
    init: function (dispatch) {
      // タブが初めて読み込まれたときに製品データを取得
      dispatch(ApiMsg.FETCH_PRODUCTS);
    },

    // 更新関数
    update: function (args) {
      const messageType = args.messageType;
      const payload = args.payload;
      const model = args.model;

      switch (messageType) {
        case ProductsMsg.SELECT_PRODUCT:
          // 選択された製品のIDを保存
          return plugin.setState(
            PLUGIN_ID,
            {
              selectedProductId: payload.productId,
            },
            model
          );
        case ProductsMsg.SAVE_SCROLL_POSITION:
          // スクロール位置を保存（UIの状態を維持するため）
          return plugin.setState(
            PLUGIN_ID,
            {
              scrollPosition: payload.position,
            },
            model
          );
        default:
          return model;
      }
    },

    // ビュー実装
    view: function (args) {
      const model = args.model;
      const dispatch = args.dispatch;

      // プラグイン固有の状態を取得
      const pluginState = plugin.getState(PLUGIN_ID, model);

      // 製品データの取得状態を確認 - ApiData構造を解析
      let productsState = null;
      if (model.ApiData && model.ApiData.Products) {
        // 判別共用体を解析
        if (typeof model.ApiData.Products === "string") {
          // 'NotStarted' または 'Loading' の文字列の場合
          productsState = model.ApiData.Products;
        } else if (model.ApiData.Products.tag) {
          if (model.ApiData.Products.tag === 2) {
            productsState = {
              tag: "Success",
              fields: model.ApiData.Products.fields || [],
            };
          } else if (model.ApiData.Products.tag === 3) {
            productsState = {
              tag: "Failed",
              error: model.ApiData.Products.fields,
            };
          }
        }
      }

      // 製品データの状態に基づいて表示を変える
      const ProductsComponent = function () {
        // 選択された製品ID
        const selectedProductId = pluginState.selectedProductId;

        // React Hooks
        const [scrollRef, setScrollRef] = React.useState(null);

        // 製品選択ハンドラー
        const handleSelectProduct = function (productId) {
          dispatch([ProductsMsg.SELECT_PRODUCT, { productId }]);

          // 特定の製品を選択した場合、APIからその製品の詳細を取得
          if (productId) {
            dispatch([ApiMsg.FETCH_PRODUCT, productId]);
          }
        };

        // APIから製品を再取得
        const handleRefresh = function () {
          dispatch(ApiMsg.FETCH_PRODUCTS);
        };

        // スクロール位置を保存
        React.useEffect(() => {
          if (scrollRef) {
            const saveScroll = () => {
              dispatch([
                ProductsMsg.SAVE_SCROLL_POSITION,
                {
                  position: scrollRef.scrollTop,
                },
              ]);
            };

            scrollRef.addEventListener("scroll", saveScroll);
            return () => scrollRef.removeEventListener("scroll", saveScroll);
          }
        }, [scrollRef]);

        // 保存されたスクロール位置を復元
        React.useEffect(() => {
          if (scrollRef && pluginState.scrollPosition) {
            scrollRef.scrollTop = pluginState.scrollPosition;
          }
        }, [scrollRef, pluginState.scrollPosition]);

        // ローディング表示
        if (!productsState || productsState === "NotStarted") {
          return React.createElement("div", { className: "p-5 text-center" }, [
            React.createElement(
              "h1",
              { className: "text-2xl font-bold mb-4" },
              "製品一覧"
            ),
            React.createElement(
              "p",
              { className: "text-gray-500" },
              "データがロードされていません"
            ),
            React.createElement(
              "button",
              {
                className:
                  "mt-4 px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 transition-colors",
                onClick: handleRefresh,
              },
              "データを取得"
            ),
          ]);
        }

        // ロード中表示
        if (productsState === "Loading") {
          return React.createElement("div", { className: "p-5 text-center" }, [
            React.createElement(
              "h1",
              { className: "text-2xl font-bold mb-4" },
              "製品一覧"
            ),
            React.createElement(
              "p",
              { className: "text-gray-500" },
              "読み込み中..."
            ),
            React.createElement(
              "div",
              { className: "mt-4 flex justify-center" },
              React.createElement("div", {
                className:
                  "w-8 h-8 border-4 border-blue-500 border-t-transparent rounded-full animate-spin",
              })
            ),
          ]);
        }

        // エラー表示
        if (productsState.tag === "Failed") {
          return React.createElement("div", { className: "p-5 text-center" }, [
            React.createElement(
              "h1",
              { className: "text-2xl font-bold mb-4" },
              "製品一覧"
            ),
            React.createElement(
              "p",
              { className: "text-red-500" },
              "データの取得に失敗しました"
            ),
            React.createElement(
              "button",
              {
                className:
                  "mt-4 px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 transition-colors",
                onClick: handleRefresh,
              },
              "再読み込み"
            ),
          ]);
        }

        // 製品データ取得成功
        const products = productsState.fields || [];

        // 製品がない場合
        if (products.length === 0) {
          return React.createElement("div", { className: "p-5 text-center" }, [
            React.createElement(
              "h1",
              { className: "text-2xl font-bold mb-4" },
              "製品一覧"
            ),
            React.createElement(
              "p",
              { className: "text-gray-500" },
              "製品が見つかりません"
            ),
            React.createElement(
              "button",
              {
                className:
                  "mt-4 px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 transition-colors",
                onClick: handleRefresh,
              },
              "再読み込み"
            ),
          ]);
        }

        // 製品一覧表示
        return React.createElement(
          "div",
          {
            className: "p-5",
            ref: setScrollRef,
            style: { maxHeight: "100vh", overflowY: "auto" },
          },
          [
            React.createElement(
              "div",
              {
                className:
                  "flex justify-between items-center mb-6 sticky top-0 bg-white p-2 z-10",
              },
              [
                React.createElement(
                  "h1",
                  { className: "text-2xl font-bold" },
                  "製品一覧"
                ),
                React.createElement(
                  "button",
                  {
                    className:
                      "px-4 py-2 bg-blue-500 text-white rounded hover:bg-blue-600 transition-colors",
                    onClick: handleRefresh,
                  },
                  "更新"
                ),
              ]
            ),
            React.createElement(
              "div",
              {
                className:
                  "grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4",
              },
              products.map(function (product) {
                const isSelected =
                  selectedProductId && selectedProductId === product.Id;

                return React.createElement(
                  "div",
                  {
                    key: product.Id,
                    className:
                      "border rounded-lg p-4 cursor-pointer transition-all " +
                      (isSelected
                        ? "border-blue-500 bg-blue-50"
                        : "hover:border-gray-400"),
                    onClick: function () {
                      handleSelectProduct(product.Id);
                    },
                  },
                  [
                    React.createElement(
                      "h2",
                      { className: "text-lg font-bold mb-2" },
                      product.Name
                    ),
                    React.createElement(
                      "p",
                      { className: "text-sm text-gray-600 mb-2" },
                      product.Description || "説明なし"
                    ),
                    React.createElement(
                      "div",
                      { className: "flex justify-between items-center" },
                      [
                        React.createElement(
                          "span",
                          { className: "font-bold text-lg" },
                          "¥" + product.Price.toLocaleString()
                        ),
                        React.createElement(
                          "span",
                          {
                            className:
                              "text-sm px-2 py-1 rounded " +
                              (product.IsActive
                                ? "bg-green-100 text-green-800"
                                : "bg-gray-100 text-gray-800"),
                          },
                          product.IsActive ? "販売中" : "停止中"
                        ),
                      ]
                    ),
                    React.createElement(
                      "div",
                      { className: "mt-2 text-sm text-gray-500" },
                      [
                        React.createElement(
                          "span",
                          null,
                          "在庫: " + product.Stock + "点"
                        ),
                        React.createElement(
                          "span",
                          { className: "ml-3" },
                          "SKU: " + product.SKU
                        ),
                      ]
                    ),
                  ]
                );
              })
            ),

            // 選択された製品の詳細表示（選択されている場合）
            selectedProductId &&
              React.createElement(
                (function () {
                  // 選択された製品を探す
                  const selectedProduct = products.find(
                    (p) => p.Id === selectedProductId
                  );

                  // 選択された製品がリストにない場合、APIから取得した詳細データを探す
                  let productDetail = selectedProduct;
                  if (
                    model.ApiData &&
                    model.ApiData.SelectedProduct &&
                    model.ApiData.SelectedProduct.tag === "Success"
                  ) {
                    const apiProduct = model.ApiData.SelectedProduct.fields;
                    if (apiProduct && apiProduct.Id === selectedProductId) {
                      productDetail = apiProduct;
                    }
                  }

                  if (!productDetail) return null;

                  return React.createElement(
                    "div",
                    {
                      className: "mt-8 p-6 border rounded-lg bg-white",
                      key: "product-detail",
                    },
                    [
                      React.createElement(
                        "h2",
                        { className: "text-xl font-bold mb-4" },
                        "製品詳細"
                      ),
                      React.createElement(
                        "div",
                        { className: "grid grid-cols-1 md:grid-cols-2 gap-4" },
                        [
                          React.createElement(
                            "div",
                            { className: "space-y-2" },
                            [
                              React.createElement("p", null, [
                                React.createElement(
                                  "span",
                                  { className: "font-medium" },
                                  "製品ID: "
                                ),
                                React.createElement(
                                  "span",
                                  null,
                                  productDetail.Id
                                ),
                              ]),
                              React.createElement("p", null, [
                                React.createElement(
                                  "span",
                                  { className: "font-medium" },
                                  "名前: "
                                ),
                                React.createElement(
                                  "span",
                                  null,
                                  productDetail.Name
                                ),
                              ]),
                              React.createElement("p", null, [
                                React.createElement(
                                  "span",
                                  { className: "font-medium" },
                                  "価格: "
                                ),
                                React.createElement(
                                  "span",
                                  null,
                                  "¥" + productDetail.Price.toLocaleString()
                                ),
                              ]),
                              React.createElement("p", null, [
                                React.createElement(
                                  "span",
                                  { className: "font-medium" },
                                  "在庫: "
                                ),
                                React.createElement(
                                  "span",
                                  null,
                                  productDetail.Stock + "点"
                                ),
                              ]),
                            ]
                          ),
                          React.createElement(
                            "div",
                            { className: "space-y-2" },
                            [
                              React.createElement("p", null, [
                                React.createElement(
                                  "span",
                                  { className: "font-medium" },
                                  "SKU: "
                                ),
                                React.createElement(
                                  "span",
                                  null,
                                  productDetail.SKU
                                ),
                              ]),
                              React.createElement("p", null, [
                                React.createElement(
                                  "span",
                                  { className: "font-medium" },
                                  "カテゴリ: "
                                ),
                                React.createElement(
                                  "span",
                                  null,
                                  productDetail.Category || "未分類"
                                ),
                              ]),
                              React.createElement("p", null, [
                                React.createElement(
                                  "span",
                                  { className: "font-medium" },
                                  "状態: "
                                ),
                                React.createElement(
                                  "span",
                                  {
                                    className: productDetail.IsActive
                                      ? "text-green-600"
                                      : "text-gray-600",
                                  },
                                  productDetail.IsActive ? "販売中" : "停止中"
                                ),
                              ]),
                              React.createElement("p", null, [
                                React.createElement(
                                  "span",
                                  { className: "font-medium" },
                                  "登録日: "
                                ),
                                React.createElement(
                                  "span",
                                  null,
                                  new Date(
                                    productDetail.CreatedAt
                                  ).toLocaleDateString()
                                ),
                              ]),
                            ]
                          ),
                        ]
                      ),
                      React.createElement("div", { className: "mt-4" }, [
                        React.createElement(
                          "h3",
                          { className: "font-medium mb-2" },
                          "説明"
                        ),
                        React.createElement(
                          "p",
                          { className: "text-gray-700" },
                          productDetail.Description || "説明はありません。"
                        ),
                      ]),
                      React.createElement(
                        "button",
                        {
                          className:
                            "mt-6 px-4 py-2 bg-gray-200 text-gray-800 rounded hover:bg-gray-300 transition-colors",
                          onClick: function () {
                            handleSelectProduct(null);
                          },
                        },
                        "詳細を閉じる"
                      ),
                    ]
                  );
                })()
              ),
          ]
        );
      };

      // コンポーネントをレンダリング
      return React.createElement(ProductsComponent);
    },
  });
})();
