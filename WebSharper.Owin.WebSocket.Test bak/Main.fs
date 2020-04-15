// $begin{copyright}
//
// This file is part of WebSharper
//
// Copyright (c) 2008-2018 IntelliFactory
//
// Licensed under the Apache License, Version 2.0 (the "License"); you
// may not use this file except in compliance with the License.  You may
// obtain a copy of the License at
//
//     http://www.apache.org/licenses/LICENSE-2.0
//
// Unless required by applicable law or agreed to in writing, software
// distributed under the License is distributed on an "AS IS" BASIS,
// WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or
// implied.  See the License for the specific language governing
// permissions and limitations under the License.
//
// $end{copyright}
namespace WebSharper.Owin.WebSocket.Test

open WebSharper.Html.Server
open WebSharper
open WebSharper.Sitelets

type Action =
    | Home

module Skin =
    open System.Web

    type Page =
        {
            Title : string
            Body : list<Element>
        }

    let MainTemplate =
        Content.Template<Page>("~/Main.html")
            .With("title", fun x -> x.Title)
            .With("body", fun x -> x.Body)

    let WithTemplate title body =
        Content.WithTemplate MainTemplate
            {
                Title = title
                Body = body
            }

module Site =


    let HomePage ep ctx =
        Skin.WithTemplate "HomePage"
            [
                Div [ClientSide <@ Client.WS ep @>]
            ]

    let MainSitelet ep =
        Sitelet.Sum [
            Sitelet.Content "/" Home (HomePage ep)
        ]

module SelfHostedServer =

    open global.Owin
    open Microsoft.Owin.Hosting
    open Microsoft.Owin.StaticFiles
    open Microsoft.Owin.FileSystems
    open WebSharper.Owin
    open WebSharper.Owin.WebSocket

    [<EntryPoint>]
    let Main args =
        match args with
        | [| rootDirectory; url |] ->
            use server = WebApp.Start(url, fun appB ->
                let ep = Endpoint.Create(url, "/ws", JsonEncoding.Readable)
                let rootDirectory =
                    System.IO.Path.Combine(
                        System.IO.Directory.GetCurrentDirectory(),
                        rootDirectory)
                // Put WebSocket before StaticFiles for the #4 regression test
                appB.UseWebSharper(
                        WebSharperOptions(
                            ServerRootDirectory = rootDirectory,
                            Sitelet = Some (Site.MainSitelet ep),
                            Debug = true))
                    .UseWebSocket(ep, Server.Start ep, maxMessageSize = 1000)
                    .UseStaticFiles(
                        StaticFileOptions(
                            FileSystem = PhysicalFileSystem(rootDirectory)))
                |> ignore)
            stdout.WriteLine("Serving {0}", url)
            
            stdin.ReadLine() |> ignore
            0
        | _ ->
            eprintfn "Usage: WebSharper.WebSockets.Owin.Test ROOT_DIRECTORY URL"
            1
