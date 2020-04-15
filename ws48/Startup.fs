namespace testFrom0

open System



//open Microsoft.AspNetCore
//open Microsoft.AspNetCore.Builder
//open Microsoft.AspNetCore.Hosting
//open Microsoft.AspNetCore.Http
//open Microsoft.Extensions.Configuration
//open Microsoft.Extensions.DependencyInjection
//open Microsoft.Extensions.Hosting
//open WebSharper.AspNetCore


//type Startup() =

//    member this.ConfigureServices(services: IServiceCollection) =
//        services.AddSitelet(Site.Main)
//            .AddAuthentication("WebSharper")
//            .AddCookie("WebSharper", fun options -> ())
//        |> ignore

//    member this.Configure(app: IApplicationBuilder, env: IWebHostEnvironment) =
//        if env.IsDevelopment() then app.UseDeveloperExceptionPage() |> ignore

//        app.UseAuthentication()
//            .UseStaticFiles()
//            .UseWebSharper()
//            .Run(fun context ->
//                context.Response.StatusCode <- 404
//                context.Response.WriteAsync("Page not found"))
module Program =
    open WebSharper.Html.Server
    open Suave
    open WebSharper
    open WebSharper.Sitelets
    open WebSharper.Suave    
    open global.Owin
    open Microsoft.Owin.Hosting
    open Microsoft.Owin.StaticFiles
    open Microsoft.Owin.FileSystems
    open WebSharper.Owin
    open WebSharper.Owin.WebSocket


    //open Suave
    open Microsoft.Owin.Extensions
    //let BuildWebHost args =
    //    WebHost
    //        .CreateDefaultBuilder(args)
    //        .UseStartup<Startup>()
    //        .Build()

    [<EntryPoint>]
    let main args =
        //let url = 
        //    let b = Suave.Web.defaultConfig.bindings |> List.item 0
        //    b.ToString()
        //BuildWebHost(args).Run()
        //use server = WebApp.Start(url, fun appB ->
        //    let ep = Endpoint.Create(url, "/ws", JsonEncoding.Readable)
        //    let rootDirectory =
        //        System.IO.Path.Combine(
        //            System.IO.Directory.GetCurrentDirectory(),
        //            rootDirectory)
        //    // Put WebSocket before StaticFiles for the #4 regression test
        //    appB.UseWebSharper(
        //            WebSharperOptions(
        //                ServerRootDirectory = rootDirectory,
        //                Sitelet = Some (Site.MainSitelet ep),
        //                Debug = true))
        //        .UseWebSocket(ep, Server.Start ep, maxMessageSize = 1000)
        //        .UseStaticFiles(
        //            StaticFileOptions(
        //                FileSystem = PhysicalFileSystem(rootDirectory)))
        //    |> ignore)
        let rootDirectory = @"C:\Users\anibal\Downloads\owin.websocket\WebSharper.Owin.WebSocket.Test\bin"
        match [| rootDirectory |] with
        //| _ ->
        //    let url = "http://localhost:8080/"
        //    let ep = Endpoint.Create(url, "/WS", JsonEncoding.Readable)
        //    startWebServer defaultConfig (WebSharperAdapter.ToWebPart (Site.Main ep))
        //    0
        | [| rootDirectory |] ->
            //let url = 
            //    let b = Suave.Web.defaultConfig.bindings |> List.item 0
            //    b.ToString()
            use server = 
                let url = "http://*:8080/"
                let url2 = "http://10.28.112.112:8080/"
                let url20 = "http://0.0.0.0:8080/"
                let ep = Endpoint.Create(url2, "/WS", JsonEncoding.Readable)
                let ep2 = Endpoint.Create(url2, "/WS2", JsonEncoding.Readable)
                let ep00 = Endpoint.Create(url20, "/WS", JsonEncoding.Readable)
                let ep20 = Endpoint.Create(url20, "/WS2", JsonEncoding.Readable)
                WebApp.Start(url, fun appB ->
                    let server1 = Server.Start 1
                    
                    let rootDirectory =
                        System.IO.Path.Combine(
                            System.IO.Directory.GetCurrentDirectory(),
                            rootDirectory)
                    // Put WebSocket before StaticFiles for the #4 regression test
                    appB.UseWebSharper(
                            WebSharperOptions(
                                ServerRootDirectory = rootDirectory,
                                Sitelet = Some (Site.Main ep ep2),
                                Debug = true))
                        .UseWebSocket(ep20, Server.Start 2, maxMessageSize = 1000)
                        .UseWebSocket(ep00, server1, maxMessageSize = 1000)                        
                        .UseStaticFiles(
                            StaticFileOptions(
                                FileSystem = PhysicalFileSystem(rootDirectory)))
                    |> ignore)
            stdout.WriteLine("Serving {0}", "http://localhost:8080")
            
            stdin.ReadLine() |> ignore
            0
        | _ ->
            eprintfn "Usage: WebSharper.WebSockets.Owin.Test ROOT_DIRECTORY URL"
            1



        //startWebServer defaultConfig (WebSharperAdapter.ToWebPart Site.Main)
        //0
