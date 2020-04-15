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
    [<JavaScript>]
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
    [<JavaScript>]
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
    [<JavaScript>]
    let ttc<'T, 'S> urlStr =
        let encode, decode = getEncoding0 ()
        let endpoint = Endpoint<'T, 'S>.CreateRemote(url=urlStr)
        let socket = new WebSocket(endpoint.URI)
        let server = Client.WebSocketServer(socket, encode)
        server
    [<JavaScript>]
    let fsiCmd () =
        let rvInput = Var.Create ""
        let submit = Submitter.CreateOption rvInput.View
        let vReversed =
            submit.View.MapAsync(function
                | None -> async { return "" }
                | Some input -> //Server.fsiExecute input
                    let svr = ttc<string, string> "http://localhost:8080"
                    async {
                        svr.Post input
                        return "post done"
                    }
            )

        divAttr [] [
            Doc.InputArea [] rvInput
            Doc.Button "Send" [] submit.Trigger
            hrAttr [] []
            h4Attr [attr.``class`` "text-muted"] [text "The server responded:"]
            divAttr [attr.``class`` "jumbotron"] [h1Attr [] [textView vReversed]]
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

            while true do
                do! FSharp.Control.Async.Sleep 1000
                server.Post (Server.Req3 {name = {FirstName = "John"; LastName = "Doe"}; age = 42})
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
                            | Server.MessageFromServer_String x -> writen "Response1 %s (state: %i)" x state
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
            server.Post (Server.MessageFromClient "MYIP")
            //while true do
            //    do! FSharp.Control.Async.Sleep 1000
            //    server.Post (Server.Req3 {name = {FirstName = "John"; LastName = "Doe"}; age = 42})
                //do! FSharp.Control.Async.Sleep 1000
                //server.Post (Server.Request1 [| "HELLO" |])
                //do! FSharp.Control.Async.Sleep 1000
                //server.Post (Server.Request2 lotsOf123s)
        }
        |> FSharp.Control.Async.Start

        container

    