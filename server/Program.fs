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
open Microsoft.Data.Sqlite
open SqlHydra.Query
open MyWebApi.Data.SqlHydraGenerated
open Models
open Mappers

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

        return users
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
        return products |> Seq.map Mappers.toProductDto
    }

// 特定IDの製品を取得
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

// 特定の製品を取得するハンドラー
let getProductByIdHandler (productId: int) =
    fun next ctx ->
        task {
            let! productOpt = fetchProductById (int64 productId)

            match productOpt with
            | Some product -> return! json product next ctx
            | None -> return! RequestErrors.NOT_FOUND "Product not found" next ctx
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
                              routef "/products/%i" getProductByIdHandler ]
                    // POST, PUT, DELETE などの他のHTTPメソッドもここに追加できます
                    ])
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
