module HtmlToFSharp_Web.App

open System
open System.IO
open System.Collections.Generic
open Microsoft.AspNetCore.Builder
open Microsoft.AspNetCore.Cors.Infrastructure
open Microsoft.AspNetCore.Hosting
open Microsoft.AspNetCore.Http
open Microsoft.Extensions.Logging
open Microsoft.Extensions.DependencyInjection
open Giraffe.HttpHandlers
open Giraffe.Middleware
open Giraffe
open FSharp.Data
open HtmlToFSharp.Engine

type TransformModel = JsonProvider<"""
{
    "content":"<div class=\"container\"><h1>Hello</h1></div"
}
""">

// ---------------------------------
// Web app
// ---------------------------------
let addGiraffeReferences (parsedHtml:string list) =
    let htmlFns = 
        parsedHtml
        |> fun res -> String.Join("\n", res)
        |> fun res -> res.Split([|"\n"|], StringSplitOptions.None) 
        |> Seq.map (sprintf "%s%s" doubleSpaces)
        |> fun res -> String.Join("\n", res)
    
    printfn "%s" htmlFns

    let openStatement = "open Giraffe.XmlViewEngine"
    let tempFn = "let myHtml ="
    
    String.Concat(
        openStatement, newLine, 
        newLine, 
        tempFn, newLine, 
        spaces, "[", newLine, 
        htmlFns, newLine, 
        spaces, "]"
    )
    
let transformHandler next (ctx:HttpContext) =
    task {
        let! body = ctx.ReadBodyFromRequest()
        let model = TransformModel.Parse(body)
        let giraffeCode = 
            parseHtmlString model.Content
            |> List.map (processRootHtmlNode)
            |> addGiraffeReferences
        
        return! ctx.WriteText giraffeCode
    }

let webApp =
    choose [
        GET >=>
            choose [
                route "/" >=> htmlFile "./index.html"
            ]
        POST >=>
            choose [
                route "/transform" >=> transformHandler
            ]
        setStatusCode 404 >=> text "Not Found" ]

// ---------------------------------
// Error handler
// ---------------------------------

let errorHandler (ex : Exception) (logger : ILogger) =
    logger.LogError(EventId(), ex, "An unhandled exception has occurred while executing the request.")
    clearResponse >=> setStatusCode 500 >=> text ex.Message

// ---------------------------------
// Config and Main
// ---------------------------------

let configureCors (builder : CorsPolicyBuilder) =
    builder.WithOrigins("http://localhost:8080").AllowAnyMethod().AllowAnyHeader() |> ignore

let configureApp (app : IApplicationBuilder) =
    app.UseCors(configureCors)
       .UseGiraffeErrorHandler(errorHandler)
       .UseStaticFiles()
       .UseGiraffe(webApp)

let configureServices (services : IServiceCollection) =
    let sp  = services.BuildServiceProvider()
    let env = sp.GetService<IHostingEnvironment>()
    services.AddCors() |> ignore

let configureLogging (builder : ILoggingBuilder) =
    let filter (l : LogLevel) = l.Equals LogLevel.Error
    builder.AddFilter(filter).AddConsole().AddDebug() |> ignore

[<EntryPoint>]
let main argv =
    let contentRoot = Directory.GetCurrentDirectory()
    let webRoot     = Path.Combine(contentRoot, "wwwroot")
    WebHostBuilder()
        .UseKestrel()
        .UseContentRoot(contentRoot)
        .UseIISIntegration()
        .UseWebRoot(webRoot)
        .Configure(Action<IApplicationBuilder> configureApp)
        .ConfigureServices(configureServices)
        .ConfigureLogging(configureLogging)
        .Build()
        .Run()
    0