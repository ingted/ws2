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
namespace WebSharper.Owin.WebSocket

open System.Runtime.CompilerServices

/// Which JSON encoding to use when sending messages through the websocket.
[<RequireQualifiedAccess>]
type JsonEncoding =
    /// Verbose encoding that includes extra type information.
    /// This is the same encoding used by WebSharper RPC.
    | Typed
    /// Readable and external API-friendly encoding that drops
    /// some information regarding subtypes.
    /// This is the same encoding used by WebSharper Sitelets
    /// and by the WebSharper.Json.Serialize family of functions.
    | Readable
    /// Use the given server-side and client-side JSON encoding providers.

type private Context = WebSharper.Web.Context

/// A WebSockets endpoint.
[<Sealed>]
type Endpoint<'S2C, 'C2S> =

    /// Create a websockets endpoint for a given base URL and path.
    /// Call this on the server side and pass it down to the client.
    static member Create
        : url: string
        * route: string
        * ?encoding: JsonEncoding
        -> Endpoint<'S2C, 'C2S>

    /// Create a websockets endpoint for a given Owin app and path.
    /// Call this on the server side and pass it down to the client.
    /// This overload only works when the site is hosted on ASP.NET using Microsoft.Owin.Host.SystemWeb.
    static member Create
        : Owin.IAppBuilder
        * route: string
        * ?encoding: JsonEncoding
        -> Endpoint<'S2C, 'C2S>

    /// Create a websockets endpoint for a given full URL.
    /// Call this to connect to an external websocket from the client.
    static member CreateRemote
        : url : string
        * ?encoding: JsonEncoding
        -> Endpoint<'S2C, 'C2S>

/// WebSocket server.
module Server =
    open global.Owin.WebSocket

    /// Messages received by the server.
    type Message<'C2S> =
        | Message of 'C2S
        | Error of exn
        | Close

    /// A client to which you can post messages.
    [<Class>]
    type WebSocketClient<'S2C, 'C2S> =
        member JsonProvider : WebSharper.Core.Json.Provider
        member Connection : WebSocketConnection
        member Context : Context
        member PostAsync : 'S2C -> Async<unit>
        member Post : 'S2C -> unit

    type Agent<'S2C, 'C2S> = WebSocketClient<'S2C, 'C2S> -> Async<Message<'C2S> -> unit>

    type StatefulAgent<'S2C, 'C2S, 'State> = WebSocketClient<'S2C, 'C2S> -> Async<'State * ('State -> Message<'C2S> -> Async<'State>)>

    /// Messages received by the server supporting custom server-side messages.
    [<RequireQualifiedAccess>]
    type CustomMessage<'C2S, 'Custom> =
        | Message of 'C2S
        | Custom of 'Custom
        | Error of exn
        | Close

    /// The agent of a WebSocket server that can receive custom server-side messages.
    [<Class>]
    type CustomWebSocketAgent<'S2C, 'C2S, 'Custom> =
        member Client : WebSocketClient<'S2C, 'C2S>
        member PostCustom : 'Custom -> unit

    type CustomAgent<'S2C, 'C2S, 'Custom, 'State> =
        CustomWebSocketAgent<'S2C, 'C2S, 'Custom> -> Async<'State * ('State -> CustomMessage<'C2S, 'Custom> -> Async<'State>)>

/// WebSocket client.
module Client =

    /// Messages received by the client.
    type Message<'S2C> =
        | Message of 'S2C
        | Error
        | Open
        | Close

    /// A server to which you can post messages.
    [<Class>]
    type WebSocketServer<'S2C, 'C2S> =
        member Connection : WebSharper.JavaScript.WebSocket
        member Post : 'C2S -> unit

    type Agent<'S2C, 'C2S> = WebSocketServer<'S2C, 'C2S> -> Async<Message<'S2C> -> unit>

    type StatefulAgent<'S2C, 'C2S, 'State> = WebSocketServer<'S2C, 'C2S> -> Async<'State * ('State -> Message<'S2C> -> Async<'State>)>

    /// Connect to a websocket server.
    val FromWebSocket : ws: WebSharper.JavaScript.WebSocket -> agent: Agent<'S2C, 'C2S> -> JsonEncoding -> Async<WebSocketServer<'S2C, 'C2S>>

    /// Connect to a websocket server.
    val FromWebSocketStateful : ws: WebSharper.JavaScript.WebSocket -> agent: StatefulAgent<'S2C, 'C2S, 'State> -> JsonEncoding -> Async<WebSocketServer<'S2C, 'C2S>>

    /// Connect to a websocket server.
    val Connect : endpoint: Endpoint<'S2C, 'C2S> -> agent: Agent<'S2C, 'C2S> -> Async<WebSocketServer<'S2C, 'C2S>>

    /// Connect to a websocket server.
    val ConnectStateful : endpoint: Endpoint<'S2C, 'C2S> -> agent: StatefulAgent<'S2C, 'C2S, 'State> -> Async<WebSocketServer<'S2C, 'C2S>>

type private Env = System.Collections.Generic.IDictionary<string, obj>
type private AppFunc = System.Func<Env, System.Threading.Tasks.Task>
type private MidFunc = System.Func<AppFunc, AppFunc>

/// The OWIN middleware for WebSharper WebSockets.
/// Must be located after the WebSharper Sitelets or Remoting middleware in the pipeline.
type WebSharperWebSocketMiddleware<'S2C, 'C2S> =

    new : next: AppFunc
        * endpoint: Endpoint<'S2C, 'C2S>
        * agent: Server.Agent<'S2C, 'C2S>
        * ?maxMessageSize: int
        * ?onAuth: (Env -> bool)
        -> WebSharperWebSocketMiddleware<'S2C, 'C2S>

    static member AsMidFunc
        : endpoint: Endpoint<'S2C, 'C2S>
        * agent: Server.Agent<'S2C, 'C2S>
        * ?maxMessageSize: int
        * ?onAuth: (Env -> bool)
        -> MidFunc

    static member Stateful<'State>
        : next: AppFunc
        * endpoint: Endpoint<'S2C, 'C2S>
        * agent: Server.StatefulAgent<'S2C, 'C2S, 'State>
        * ?maxMessageSize: int
        * ?onAuth: (Env -> bool)
        -> WebSharperWebSocketMiddleware<'S2C, 'C2S>

    static member AsMidFunc<'State>
        : endpoint: Endpoint<'S2C, 'C2S>
        * agent: Server.StatefulAgent<'S2C, 'C2S, 'State>
        * ?maxMessageSize: int
        * ?onAuth: (Env -> bool)
        -> MidFunc

    static member Custom<'Custom, 'State>
        : next: AppFunc
        * endpoint: Endpoint<'S2C, 'C2S>
        * agent: Server.CustomAgent<'S2C, 'C2S, 'Custom, 'State>
        * ?maxMessageSize: int
        * ?onAuth: (Env -> bool)
        -> WebSharperWebSocketMiddleware<'S2C, 'C2S>

    static member AsMidFunc<'Custom, 'State>
        : endpoint: Endpoint<'S2C, 'C2S>
        * agent: Server.CustomAgent<'S2C, 'C2S, 'Custom, 'State>
        * ?maxMessageSize: int
        * ?onAuth: (Env -> bool)
        -> MidFunc

[<Extension; Sealed>]
type Extensions =

    [<Extension>]
    static member UseWebSocket
        : this: Owin.IAppBuilder
        * endpoint: Endpoint<'S2C, 'C2S>
        * agent: Server.Agent<'S2C, 'C2S>
        * ?maxMessageSize: int
        * ?onAuth: (Env -> bool)
        -> Owin.IAppBuilder

    [<Extension>]
    static member UseWebSocket
        : this: Owin.IAppBuilder
        * endpoint: Endpoint<'S2C, 'C2S>
        * agent: Server.StatefulAgent<'S2C, 'C2S, 'State>
        * ?maxMessageSize: int
        * ?onAuth: (Env -> bool)
        -> Owin.IAppBuilder

    [<Extension>]
    static member UseWebSocket
        : this: Owin.IAppBuilder
        * endpoint: Endpoint<'S2C, 'C2S>
        * agent: Server.CustomAgent<'S2C, 'C2S, 'Custom, 'State>
        * ?maxMessageSize: int
        * ?onAuth: (Env -> bool)
        -> Owin.IAppBuilder
