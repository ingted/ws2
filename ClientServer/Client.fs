﻿namespace testFrom0

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
    let filterResult = Var.Create ([||]:string[])
    [<JavaScript>]
    let filterKeyWord = Var.Create ""

    [<JavaScript>]
    let content = Var.Create ""

    [<JavaScript>]
    let Send2 (serverReceive : Endpoint<Server.S2CMessage, Server.C2SMessage>) =
        let container = 
            Doc.InputArea [attr.id "container"; attr.style "width: 880px"; attr.``class`` "input"; attr.rows "10"] content
        let writen fmt =
            Printf.ksprintf (fun s ->
                Var.Set filterResult ([|s + "\n"|] |> Array.append filterResult.Value)
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

    [<JavaScript>]
    let Send3 (uri:string) = 
        content.Value <- ""
        async {            
            let! c = Server.getPort uri               
            WebSharper.JQuery.JQuery.Of("#console").Remove().Ignore
            Doc.RunById "consoleWC" (Send2 c :> Doc)
        }

    [<JavaScript>]
    let fsiCmd () =
        let rvInput = Var.Create ""
        let rvHisCmd = Var.Create ([||]:string[])
        

        //let filterResultFlattened =  
        //    Var.Lens filterResult (fun arr -> 
        //                                let reg = new RegExp(filterKeyWord.Value)
        //                                arr
        //                                |> Array.filter (fun s ->
        //                                    reg.Test s
        //                                ) 
        //                                |> String.concat "," ) (fun n s -> 
        //        [|s|] |> Array.append n
        //    )

        let filterResultFlattened =  
            filterResult.Lens (
                fun arr -> 
                    let reg = new RegExp(filterKeyWord.Value)
                    arr
                    |> Array.filter (fun s ->
                        reg.Test s
                    ) 
                    |> String.concat "," 
            ) (
                fun n s -> 
                    [|s|] |> Array.append n
            )
        

        let nScript = Var.Create "named script"
        let webSocket2 = Var.Create "http://localhost:8080/"
        let curPos = Var.Create 0
        let submit = Submitter.CreateOption rvInput.View
        let hisCmd = Submitter.CreateOption rvHisCmd.View
        //let nxtCmd = Submitter.CreateOption rvHisCmd.View
        
        let vReversed =
            submit.View.MapAsync(function
                | None -> async { return "" }
                | Some input -> 
                                //let reg = new RegExp("23")
                                //Var.Set filterResult ([|input|]|>Array.filter (fun s -> reg.Test s))
                                rvHisCmd.Value <- Array.append rvHisCmd.Value [|input|]
                                curPos.Value <- curPos.Value + 1
                                Server.fsiExecute input
                    //let svr = ttc<string, string> "ws://localhost:8080/WS2"
                    //async {
                    //    //svr.Post input
                    //    return "post done" + input
                    //}
            )
        let getHisCmd =
            hisCmd.View.MapAsync(function
                | None -> Server.getHisCmds ()
                | Some v when v.Length = 0 -> Server.getHisCmds ()
                | Some v ->
                    async {return v}
            )

        //let getNextCmd =
        //    nxtCmd.View.MapAsync(function
        //        | None -> Server.getHisCmds ()
        //        | Some v -> async {return v}
        //    )
        let filterBox = 
            (Doc.Input [attr.id "fKW"; attr.style "width: 880px"; attr.``class`` "input"; attr.rows "1"] filterKeyWord).OnBlur(fun e1 e2 ->
                   WebSharper.JQuery.JQuery.Of("#filteredResult").Val(filterResultFlattened.Value).Ignore
            )
        let content =
            divAttr [] [
                divAttr [][
                    Doc.Button "Send" [] submit.Trigger
                    Doc.Button "Clear Console" [] (fun () -> 
                                                        //WebSharper.JQuery.JQuery.Of("#consoleWC")
                                                        WebSharper.JQuery.JQuery.Of("#console").Empty().Ignore)
                    Doc.Button "Clear Command" [] (fun () -> 
                                                        WebSharper.JQuery.JQuery.Of("#fsiCmd").Val("").Ignore)
                    Doc.Button "Last Command" [] (fun () -> 
                                                        async {
                                                            let! hc = Server.getHisCmd ()
                                                            WebSharper.JQuery.JQuery.Of("#fsiCmd").Val(hc).Ignore
                                                        } |> Async.Start
                                                        )
                    Doc.Button "Previous Command" [] (fun () -> 
                                                                //let curCmdStr = 
                                                                if rvHisCmd.Value.Length = 0 then 
                                                                    async {
                                                                        let! hcs = Server.getHisCmds ()
                                                                        rvHisCmd.Value <- hcs
                                                                        if rvHisCmd.Value.Length > 0 then
                                                                            curPos.Value <- rvHisCmd.Value.Length - 1
                                                                        rvInput.Value <- rvHisCmd.Value.[curPos.Value]
                                                                        }|> Async.Start
                                                                
                                                                else                                                                    
                                                                    if curPos.Value = 0 then //rvHisCmd.Value.Length - 1 then 
                                                                        curPos.Value <- rvHisCmd.Value.Length - 1
                                                                    else 
                                                                        curPos.Value <- curPos.Value - 1
                                                                    let ccs = rvHisCmd.Value.[curPos.Value]
                                                                    rvInput.Value <- ccs
                                                                hisCmd.Trigger ()
                                                            
                                                                //WebSharper.JQuery.JQuery.Of("#fsiCmd").Val(curCmdStr).Ignore

                                                                )
                    Doc.Button "Get Script" [] (fun () -> 
                        async {
                            let! ns = Server.getNamedScript (WebSharper.JQuery.JQuery.Of("#nScript").Val().ToString())
                            WebSharper.JQuery.JQuery.Of("#fsiCmd").Val(ns).Ignore
                        } |> Async.Start
                        )
                    Doc.Button "Save Script" [] (fun () -> 
                        async {
                            let! hc = Server.upsertNamedScript (WebSharper.JQuery.JQuery.Of("#nScript").Val().ToString()) (WebSharper.JQuery.JQuery.Of("#fsiCmd").Val().ToString())
                            WebSharper.JQuery.JQuery.Of("#fsiCmd").Val(hc).Ignore
                        } |> Async.Start
                        )
                    Doc.Button "List Script" [] (fun () -> 
                        async {
                            let! hc = Server.listNamedScripts ()
                            let s = hc |> Array.fold (fun str item -> if str <> "" then str + "\r\n" + item else item) ""
                            WebSharper.JQuery.JQuery.Of("#nScript").Val("").Ignore
                            WebSharper.JQuery.JQuery.Of("#fsiCmd").Val(s).Ignore
                        } |> Async.Start
                        )
                    Doc.Button "Clear Result Cache" [] (fun () -> 
                        filterResult.Value <- Array.empty
                        )
                    brAttr [][]
                    Doc.Button "ConnectTo" [] (fun () -> 
                        Send3 (WebSharper.JQuery.JQuery.Of("#webSocket2").Val().ToString()) |> Async.Start
                        )
                    brAttr [][]
                    Doc.InputArea [attr.id "webSocket2"; attr.style "width: 880px"; attr.``class`` "input"; attr.rows "1" ] webSocket2
                    
                    brAttr [][]
                    filterBox
                    Doc.InputArea [attr.id "nScript"; attr.style "width: 880px"; attr.``class`` "input"; attr.rows "1" ] nScript
                    Doc.InputArea [attr.id "fsiCmd"; attr.style "width: 880px"; attr.``class`` "input"; attr.rows "10"; attr.value "printfn \"orz\""] rvInput
                ]
                hrAttr [] []
                Doc.InputArea [attr.id "filteredResult"; attr.style "width: 880px"; attr.``class`` "input"; attr.rows "10"] filterResultFlattened
                h4Attr [attr.``class`` "text-muted"] [text "The server responded:"]            
                divAttr [(*attr.``class`` "jumbotron"*)] [h1Attr [] [textView vReversed]]
            
                //divAttr [] [h1Attr [] [textView (getHisCmd |> View.map (fun strArray ->     ))]]
            ]

        content

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
                Var.Set filterResult ([|s + "\n"|] |> Array.append filterResult.Value)
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

