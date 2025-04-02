module MyWebApi.App

open System
open System.IO
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe
open Microsoft.AspNetCore.Http
open Microsoft.Data.Sqlite
open SqlHydra.Query
open MyWebApi.Data.SqlHydraGenerated
open Models
open Mappers
open App.Shared

// DB
let connectionString = "Data Source=./database/sample.db"

let getConnection () = new SqliteConnection(connectionString)

let openContext () =
    let compiler = SqlKata.Compilers.SqlServerCompiler()
    let conn = getConnection ()
    conn.Open()
    new QueryContext(conn, compiler)

let fetchAllUsers () =
    task {
        let! users =
            selectTask HydraReader.Read (Create openContext) {
                for user in main.Users do
                    select user
            }

        return users |> Seq.map toUserDto |> Seq.toList
    }

let fetchUserById (id: int64) =
    task {
        let! result =
            selectTask HydraReader.Read (Create openContext) {
                for user in main.Users do
                    where (user.Id = id)
                    select user
            }

        return result |> Seq.tryHead |> Option.map toUserDto
    }

// すべての製品を取得
let fetchAllProducts () =
    task {
        let! products =
            selectTask HydraReader.Read (Create openContext) {
                for product in main.Products do
                    select product
            }

        // データベースモデルをクライアント向けモデルに変換
        return products |> Seq.map toProductDto |> Seq.toList
    }

// 特定IDの製品を取得 (一覧表示用)
let fetchProductById (id: int64) =
    task {
        let! result =
            selectTask HydraReader.Read (Create openContext) {
                for product in main.Products do
                    where (product.Id = id)
                    select product
            }

        // 最初の結果を取得し、クライアント向けモデルに変換
        return result |> Seq.tryHead |> Option.map toProductDto
    }

// 特定IDの製品詳細を取得 (詳細表示用)
let fetchProductDetailById (id: int64) =
    task {
        let! result =
            selectTask HydraReader.Read (Create openContext) {
                for product in main.Products do
                    where (product.Id = id)
                    select product
            }

        // 最初の結果を取得し、詳細用モデルに変換
        return result |> Seq.tryHead |> Option.map toProductDetailDto
    }

// 製品を削除する
let deleteProduct (id: int64) =
    task {
        let! result =
            deleteTask (Create openContext) {
                for product in main.Products do
                    where (product.Id = id)
            }

        return result > 0
    }

// 製品を更新する
let updateProduct (id: int64) (updateDto: ProductUpdateDto) =
    task {
        // 現在の日時をISO 8601形式で取得
        let currentTimestamp = DateTime.UtcNow.ToString("o")

        // 更新処理を実行
        let! result =
            updateTask (Create openContext) {
                for p in main.Products do
                    set p.Name updateDto.Name
                    set p.Description updateDto.Description
                    set p.Category updateDto.Category
                    set p.Price updateDto.Price
                    set p.Stock (int64 updateDto.Stock)
                    set p.SKU updateDto.SKU
                    set p.IsActive (if updateDto.IsActive then 1L else 0L)
                    set p.UpdatedAt (Some currentTimestamp)
                    where (p.Id = id)
            }

        // 更新に成功した場合、更新後のデータを取得
        if result > 0 then
            return! fetchProductDetailById id
        else
            return None
    }

// ---------------------------------
// Web API
// ---------------------------------

// API ハンドラー
let getGreetingHandler (name: string) =
    let greetings = sprintf "Hello %s, from Giraffe API!" name
    let model = { Text = greetings }
    json model

// ユーザー一覧を取得
let getUsersHandler =
    fun next ctx ->
        task {
            let! users = fetchAllUsers ()
            return! json users next ctx
        }

// 特定のユーザーを取得
let getUserByIdHandler (userId: int) =
    fun next ctx ->
        task {
            let! userOpt = fetchUserById userId

            match userOpt with
            | Some u -> return! json u next ctx
            | None -> return! RequestErrors.NOT_FOUND "User not found" next ctx
        }

// 製品一覧を取得するハンドラー
let getProductsHandler =
    fun next ctx ->
        task {
            let! products = fetchAllProducts ()
            return! json products next ctx
        }

// 特定の製品を取得するハンドラー (一覧用)
let getProductByIdHandler (productId: int) =
    fun next ctx ->
        task {
            let! productOpt = fetchProductById (int64 productId)

            match productOpt with
            | Some product -> return! json product next ctx
            | None -> return! RequestErrors.NOT_FOUND "Product not found" next ctx
        }

// 特定の製品詳細を取得するハンドラー (詳細表示用)
let getProductDetailByIdHandler (productId: int) =
    fun next ctx ->
        task {
            let! productDetailOpt = fetchProductDetailById (int64 productId)

            match productDetailOpt with
            | Some productDetail -> return! json productDetail next ctx
            | None -> return! RequestErrors.NOT_FOUND "Product detail not found" next ctx
        }

// 製品を削除するハンドラー
let deleteProductHandler (productId: int) =
    fun next ctx ->
        task {
            try
                let! deleted = deleteProduct (int64 productId)

                if deleted then
                    let response =
                        { Success = true
                          Message = "Product deleted successfully" }

                    return! json response next ctx
                else
                    return! RequestErrors.NOT_FOUND "Product not found" next ctx
            with ex ->
                let errorMsg = sprintf "Error deleting product: %s" ex.Message
                return! ServerErrors.INTERNAL_ERROR errorMsg next ctx
        }

// 製品を更新するハンドラー
let updateProductHandler (productId: int) =
    fun next (ctx: HttpContext) ->
        task {
            try
                // リクエストボディから更新データを取得
                let! updateDto = ctx.BindModelAsync<ProductUpdateDto>()

                // 製品の更新処理を実行
                let! updatedProductOpt = updateProduct (int64 productId) updateDto

                match updatedProductOpt with
                | Some updatedProduct -> return! json updatedProduct next ctx
                | None -> return! RequestErrors.NOT_FOUND "Product not found or update failed" next ctx
            with ex ->
                let errorMsg = sprintf "Error updating product: %s" ex.Message
                return! ServerErrors.INTERNAL_ERROR errorMsg next ctx
        }

// API ルーティング
let webApp =
    choose
        [ subRoute
              "/api"
              (choose
                  [ GET
                    >=> choose
                            [ route "/greeting" >=> getGreetingHandler "world"
                              routef "/greeting/%s" getGreetingHandler
                              route "/users" >=> getUsersHandler
                              routef "/users/%i" getUserByIdHandler
                              route "/products" >=> getProductsHandler
                              routef "/products/%i" getProductByIdHandler
                              routef "/products/%i/detail" getProductDetailByIdHandler ]
                    // DELETE メソッドの追加
                    DELETE >=> choose [ routef "/products/%i" deleteProductHandler ]
                    // PUT メソッドの追加
                    PUT >=> choose [ routef "/products/%i" updateProductHandler ] ])
          RequestErrors.NOT_FOUND "Route not found" ]

// ---------------------------------
// Error handler
// ---------------------------------

let errorHandler (ex: Exception) (logger: ILogger) =
    logger.LogError(ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> json { Text = ex.Message }

// ---------------------------------
// Config and Main
// ---------------------------------

let configureCors (builder: CorsPolicyBuilder) =
    builder
        .WithOrigins(
            "http://localhost:5173", // Viteのデフォルトポート
            "http://localhost:5000",
            "https://localhost:5001"
        )
        .AllowAnyMethod()
        .AllowAnyHeader()
    |> ignore

let configureApp (app: IApplicationBuilder) =
    let env = app.ApplicationServices.GetService<IWebHostEnvironment>()

    (match env.IsDevelopment() with
     | true -> app.UseDeveloperExceptionPage()
     | false -> app.UseGiraffeErrorHandler(errorHandler).UseHttpsRedirection())
        .UseCors(configureCors)
        .UseStaticFiles()
        .UseGiraffe(webApp)

let configureServices (services: IServiceCollection) =
    services.AddCors() |> ignore
    services.AddGiraffe() |> ignore

    services.AddSingleton<Json.ISerializer>(Thoth.Json.Giraffe.ThothSerializer())
    |> ignore

let configureLogging (builder: ILoggingBuilder) =
    builder.AddConsole().AddDebug() |> ignore

[<EntryPoint>]
let main args =
    let contentRoot = Directory.GetCurrentDirectory()
    let webRoot = Path.Combine(contentRoot, "WebRoot")

    Host
        .CreateDefaultBuilder(args)
        .ConfigureWebHostDefaults(fun webHostBuilder ->
            webHostBuilder
                .UseContentRoot(contentRoot)
                .UseWebRoot(webRoot)
                .Configure(Action<IApplicationBuilder> configureApp)
                .ConfigureServices(configureServices)
                .ConfigureLogging(configureLogging)
            |> ignore)
        .Build()
        .Run()

    0
