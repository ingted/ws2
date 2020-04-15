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

module OwinServer =
    open System
    open global.Owin
    open Microsoft.Owin
    open Microsoft.Owin.Extensions
    open WebSharper.Owin
    open WebSharper.Owin.WebSocket

    type IAppBuilder with
        member x.RequireAspNetSession() =
            x.Use(fun ctx nxt ->
                let httpCtx = ctx.Get<System.Web.HttpContextBase>(typeof<System.Web.HttpContextBase>.FullName)
                httpCtx.SetSessionStateBehavior(Web.SessionState.SessionStateBehavior.Required)
                nxt.Invoke())
             .UseStageMarker(PipelineStage.MapHandler)

    [<Sealed>]
    type Startup() =
        member __.Configuration(builder: IAppBuilder) =
            WebSharper.Web.Remoting.DisableCsrfProtection()
            let path = AppDomain.CurrentDomain.SetupInformation.ApplicationBase
            let wsAddress = @"ws://localhost:38461/"
            let ep = Endpoint.Create(wsAddress, "/ws", JsonEncoding.Readable)
            let debug = System.Web.HttpContext.Current.IsDebuggingEnabled
            builder
                .RequireAspNetSession()
                .UseWebSharper(
                    WebSharperOptions(
                        Sitelet = Some (Site.MainSitelet ep),
                        ServerRootDirectory = path,
                        Debug = debug
                    )
                )
                .UseWebSocket(ep, Server.Start ep, maxMessageSize = 1000)
            |> ignore

    [<assembly:OwinStartup(typeof<Startup>)>]
    do ()
