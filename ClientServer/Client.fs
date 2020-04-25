namespace testFrom0

open WebSharper
open WebSharper.JavaScript
open WebSharper.UI
open WebSharper.UI.Next.Client
open WebSharper.UI.Next.Html
open WebSharper.UI.Next

open WebSharper.Html.Client
open WebSharper.Owin.WebSocket


//open WebSharper.UI.Next.CSharp.Client.Html


[<JavaScript>]
module Client =
    open WebSharper.Owin.WebSocket.Client
    
    let Main () =
        let rvInput = Var.Create ""
        let submit = Submitter.CreateOption rvInput.View
        let vReversed =
            submit.View.MapAsync(function
                | None -> async { return "" }
                | Some input -> Server.DoSomething input
            )
        div [ text "This goes into #main." ]
        |> Doc.RunById "navbar"
        divAttr [] [
            Doc.Input [] rvInput
            Doc.Button "Send" [] submit.Trigger
            hrAttr [] []
            h4Attr [attr.``class`` "text-muted"] [text "The server responded:"]
            divAttr [attr.``class`` "jumbotron"] [h1Attr [] [textView vReversed]]
        ]

    let m2 () =
        let varTxt = Var.Create "orz"
        let vLength =
            varTxt.View
            |> View.Map String.length
            |> View.Map (fun l -> sprintf "You entered %i characters." l)
        //let varTxt2 = Var.Create ""
        let vWords =
            varTxt.View
            |> View.Map (fun s -> s.Split(' '))
            |> Doc.BindView (fun words ->
                words
                |> Array.map (fun w -> liAttr [] [text w] :> Doc)
                |> Doc.Concat
            )
        
        divAttr [] [
            divAttr [] [
                Doc.Input [] varTxt
                textView vLength
            ]
            divAttr [] [
                text "You entered the following words:"
                ulAttr [] [ vWords ]
            ]
        ]

    //let ttc<'T, 'S> () =
    //    let encode, decode = getEncoding encode decode jsonEncoding
    //    let flush = cacheSocket socket decode
    //    let endpoint = Endpoint<'T, 'S>.CreateRemote(url="")
    //    let socket = new WebSocket(endpoint.URI)
    //    let server = Client.WebSocketServer(socket, encode)
    //    server

    //[<JavaScript>]
    //let ttc<'T, 'S> urlStr =
    //    let encode, decode = getEncoding0 ()
    //    let endpoint = Endpoint<'T, 'S>.CreateRemote(url=urlStr)
    //    let socket = new WebSocket(endpoint.URI)
    //    let flush = cacheSocket socket decode
    //    let isOpen = flush agent.Post
    //    let server = Client.WebSocketServer(socket, encode)
    //    server
    [<JavaScript>]
    let fsiCmd () =
        let rvInput = Var.Create ""
        let submit = Submitter.CreateOption rvInput.View
        
        let vReversed =
            submit.View.MapAsync(function
                | None -> async { return "" }
                | Some input -> Server.fsiExecute input
                    //let svr = ttc<string, string> "ws://localhost:8080/WS2"
                    //async {
                    //    //svr.Post input
                    //    return "post done" + input
                    //}
            )

        divAttr [] [
            divAttr [][
                Doc.Button "Send" [] submit.Trigger
                Doc.Button "Clear Console" [] (fun () -> 
                                                    //WebSharper.JQuery.JQuery.Of("#consoleWC")
                                                    WebSharper.JQuery.JQuery.Of("#console").Empty().Ignore)
                brAttr [][]
                Doc.InputArea [attr.style "width: 800px"; attr.``class`` "input"; attr.rows "10"; attr.value "printfn \"orz\""] rvInput
            ]
            hrAttr [] []
            h4Attr [attr.``class`` "text-muted"] [text "The server responded:"]
            divAttr [(*attr.``class`` "jumbotron"*)] [h1Attr [] [textView vReversed]]
        ]


    [<JavaScript>]
    let WS (endpoint : Endpoint<Server.S2CMessage, Server.C2SMessage>) =
        let container = Pre []
        let writen fmt =
            Printf.ksprintf (fun s ->
                JS.Document.CreateTextNode(s + "\n")
                |> container.Dom.AppendChild
                |> ignore
            ) fmt
        async {
            do
                writen "Checking regression #4..."
                JQuery.JQuery.Ajax(
                    JQuery.AjaxSettings(
                        Url = "/ws.txt",
                        Method = JQuery.RequestType.GET,
                        Success = (fun x _ _ -> writen "%s" (x :?> _)),
                        Error = (fun _ _ e -> writen "KO: %s." e)
                    )
                ) |> ignore

            let! server =
                ConnectStateful endpoint <| fun server -> async {
                    return 0, fun state msg -> async {
                        match msg with
                        | Message data ->
                            match data with
                            | Server.Response1 x -> writen "Response1 %s (state: %i)" x state
                            | Server.Response2 x -> writen "Response2 %i (state: %i)" x state
                            | Server.Resp3 x -> writen "Resp3 %A" x
                            | Server.MessageFromServer_String x -> writen "MessageFromServer_String %A" x
                            return (state + 1)
                        | Close ->
                            writen "WebSocket connection closed."
                            return state
                        | Open ->
                            writen "WebSocket connection open."
                            return state
                        | Error ->
                            writen "WebSocket connection error!"
                            return state
                    }
                }
    
            //let lotsOfHellos = "HELLO" |> Array.create 1000
            //let lotsOf123s = 123 |> Array.create 1000
            //server.Post (Server.Req3 {name = {FirstName = "John"; LastName = "Doe"}; age = 42})
            let conn = server.Connection
            while true do
                do! FSharp.Control.Async.Sleep 1000
                conn.Send (Json.Serialize (Server.Req3 {name = {FirstName = "John00"; LastName = "Doe"}; age = 42}))
                //server.Post (Server.Req3 {name = {FirstName = "John"; LastName = "Doe"}; age = 42})
                //do! FSharp.Control.Async.Sleep 1000
                //server.Post (Server.Request1 [| "HELLO" |])
                //do! FSharp.Control.Async.Sleep 1000
                //server.Post (Server.Request2 lotsOf123s)
        }
        |> FSharp.Control.Async.Start

        container

    [<JavaScript>]
    let Send (serverReceive : Endpoint<Server.S2CMessage, Server.C2SMessage>) =
        //let encode, decode = getEncoding encode decode jsonEncoding
        //let flush = cacheSocket socket decode
        //let socket = new WebSocket(serverReceive.URI)
        //let server = WebSocketServer(serverReceive, encode)



        let container = Pre []
        let writen fmt =
            Printf.ksprintf (fun s ->
                JS.Document.CreateTextNode(s + "\n")
                |> container.Dom.AppendChild
                |> ignore
            ) fmt
        async {
            do
                ()

            let! server =
                ConnectStateful serverReceive <| fun server -> async {
                    return 0, fun state msg -> async {
                        match msg with
                        | Message data ->
                            match data with
                            | Server.MessageFromServer_String x -> writen "MessageFromServer_String %s \r\n(state: %i)" x state
                            | _ ->
                                writen "invalidMessage"
                            return (state + 1)
                        | Close ->
                            writen "WebSocket connection closed."
                            return state
                        | Open ->
                            writen "WebSocket connection open."
                            return state
                        | Error ->
                            writen "WebSocket connection error!"
                            return state
                    }
                }
    
            //let lotsOfHellos = "HELLO" |> Array.create 1000
            //let lotsOf123s = 123 |> Array.create 1000
            server.Post (Server.MessageFromClient "kickOff")
            //while true do
            //    do! FSharp.Control.Async.Sleep 1000
            //    server.Post (Server.Req3 {name = {FirstName = "John"; LastName = "Doe"}; age = 42})
                //do! FSharp.Control.Async.Sleep 1000
                //server.Post (Server.Request1 [| "HELLO" |])
                //do! FSharp.Control.Async.Sleep 1000
                //server.Post (Server.Request2 lotsOf123s)
        }
        |> FSharp.Control.Async.Start
        
        container.SetAttribute("id", "console")
        container

    