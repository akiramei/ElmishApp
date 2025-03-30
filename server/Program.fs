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


// ---------------------------------
// Models
// ---------------------------------

type Message = { Text: string }

type User =
    { Id: int64
      Username: string
      Email: string }

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
                    select (user.Id, user.Username, user.Email)
            }

        return
            result
            |> Seq.tryHead
            |> Option.map (fun (id, username, email) ->
                { Id = id
                  Username = username
                  Email = email })
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
                              routef "/users/%i" getUserByIdHandler ]
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
