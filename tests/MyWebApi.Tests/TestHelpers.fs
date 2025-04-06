module MyWebApi.Tests.TestHelpers

open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.TestHost
open Microsoft.Extensions.DependencyInjection
open Microsoft.Extensions.Hosting
open Microsoft.Extensions.Configuration
open Microsoft.AspNetCore.Cors.Infrastructure
open System.Net.Http
open System.Collections.Generic
open System.Threading.Tasks
open MyWebApi.App
open System.Text.Json
open System.IO
open Microsoft.AspNetCore.Http
open Models
open Giraffe
open Thoth.Json.Giraffe
open SqlKata.Compilers
open Microsoft.Data.Sqlite
open System
open SqlHydra.Query

/// テスト用のWebHostBuilderを作成
let testDbConnectionString = "Data Source=:memory:;Mode=Memory;Cache=Shared"
let connection = new SqliteConnection(testDbConnectionString)

let initializeTestDatabase () =
    if connection.State <> System.Data.ConnectionState.Open then
        connection.Open()

    let command = connection.CreateCommand()

    // テーブルが存在する場合は削除
    command.CommandText <-
        """
        DROP TABLE IF EXISTS Users;
        DROP TABLE IF EXISTS Products;
    """

    command.ExecuteNonQuery() |> ignore

    command.CommandText <-
        """
        CREATE TABLE IF NOT EXISTS Users (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Username TEXT NOT NULL UNIQUE,
            Email TEXT NOT NULL,
            PasswordHash TEXT NOT NULL,
            CreatedAt TEXT NOT NULL
        );

        CREATE TABLE IF NOT EXISTS Products (
            Id INTEGER PRIMARY KEY AUTOINCREMENT,
            Name TEXT NOT NULL,
            Description TEXT,
            Category TEXT,
            Price REAL NOT NULL,
            Stock INTEGER NOT NULL,
            SKU TEXT NOT NULL UNIQUE,
            IsActive INTEGER NOT NULL,
            CreatedAt TEXT NOT NULL,
            UpdatedAt TEXT,
            Public01 TEXT,
            Public02 TEXT,
            Public03 TEXT,
            Public04 TEXT,
            Public05 TEXT,
            Public06 TEXT,
            Public07 TEXT,
            Public08 TEXT,
            Public09 TEXT,
            Public10 TEXT
        );

        INSERT INTO Users (Username, Email, PasswordHash, CreatedAt)
        VALUES
        ('alice', 'alice@example.com', 'hash_alice', '2025-03-30T12:00:00Z'),
        ('bob',   'bob@example.com',   'hash_bob',   '2025-03-30T13:00:00Z');

        INSERT INTO Products (Name, Description, Category, Price, Stock, SKU, IsActive, CreatedAt)
        VALUES 
        ('製品001', 'これは製品001の説明です。', '周辺機器', 1768.24, 160, 'SKU-0001', 1, '2025-03-30T12:00:00Z'),
        ('製品002', 'これは製品002の説明です。', '周辺機器', 11183.8, 372, 'SKU-0002', 0, '2025-03-30T12:00:00Z'),
        ('製品003', 'これは製品003の説明です。', '周辺機器', 5640.58, 209, 'SKU-0003', 1, '2025-03-30T12:00:00Z');
    """

    command.ExecuteNonQuery() |> ignore
    MyWebApi.App.setSharedTestConnection connection

let createTestHostBuilder () =
    initializeTestDatabase ()
    Environment.SetEnvironmentVariable("ASPNETCORE_ENVIRONMENT", "Testing")

    WebHostBuilder()
        .UseStartup(fun _ ->
            { new IStartup with
                member _.ConfigureServices(services: IServiceCollection) =
                    services.AddCors() |> ignore
                    services.AddGiraffe() |> ignore
                    services.AddRouting() |> ignore
                    services.AddSingleton<Json.ISerializer>(ThothSerializer()) |> ignore

                    services.AddSingleton<IConfiguration>(
                        ConfigurationBuilder()
                            .AddInMemoryCollection(
                                [| KeyValuePair("ConnectionStrings:DefaultConnection", testDbConnectionString) |]
                            )
                            .Build()
                    )
                    |> ignore

                    services.BuildServiceProvider()

                member _.Configure(app: IApplicationBuilder) =
                    app.UseCors(fun builder ->
                        builder.WithOrigins("http://localhost:5173").AllowAnyMethod().AllowAnyHeader()
                        |> ignore)
                    |> ignore

                    app.UseRouting() |> ignore

                    app.UseEndpoints(fun endpoints ->
                        endpoints.MapGet(
                            "/greeting",
                            RequestDelegate(fun context ->
                                task {
                                    let message = { Text = "Hello Test, from Giraffe API!" }
                                    let json = JsonSerializer.Serialize(message)
                                    context.Response.ContentType <- "application/json"
                                    return! context.Response.WriteAsync(json)
                                }
                                :> Task)
                        )
                        |> ignore

                        endpoints.MapGet(
                            "/users",
                            RequestDelegate(fun context ->
                                task {
                                    let! users = fetchAllUsers ()
                                    let json = JsonSerializer.Serialize(users)
                                    context.Response.ContentType <- "application/json"
                                    return! context.Response.WriteAsync(json)
                                }
                                :> Task)
                        )
                        |> ignore

                        endpoints.MapGet(
                            "/products",
                            RequestDelegate(fun context ->
                                task {
                                    let! products = fetchAllProducts ()
                                    let json = JsonSerializer.Serialize(products)
                                    context.Response.ContentType <- "application/json"
                                    return! context.Response.WriteAsync(json)
                                }
                                :> Task)
                        )
                        |> ignore)
                    |> ignore })

/// テスト用のHttpClientを作成
let createTestClient () =
    let server = new TestServer(createTestHostBuilder ())
    server.CreateClient()

/// テスト用のデータベース接続文字列
let testConnectionString = testDbConnectionString

/// テスト用のデータベースコンテキストを作成
let createTestContext () =
    let config =
        ConfigurationBuilder()
            .AddInMemoryCollection([| KeyValuePair("ConnectionStrings:DefaultConnection", testDbConnectionString) |])
            .Build()

    let compiler = SqlKata.Compilers.SqlServerCompiler()
    new QueryContext(connection, compiler)
