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

    [<JavaScript>]
    let protoCmd () =

        let jsI = Var.Create ""
        let protoI = Var.Create ".\\default.proto.encoded"
        let bidderI = Var.Create "http://192.168.101.12:18880/bid"
        let bidderR = Var.Create ""
        //async {
        //    WebSharper.JQuery.JQuery.Of("#protoId").Val(".\\default.proto.encoded").Ignore
        //    let! jStr = Server.p2j (WebSharper.JQuery.JQuery.Of("#protoId").Text())
        //    WebSharper.JQuery.JQuery.Of("#jsId").Val(jStr).Ignore
        //} |> Async.Start 
        let jsubmit = Submitter.CreateOption jsI.View
        let bidderSubmit = Submitter.CreateOption bidderI.View
        //let psubmit = Submitter.CreateOption protoI.View 
        
        let j2pExecution = 
            jsubmit.View.MapAsync(function
                        | None -> async { return "" }
                        | Some input -> 
                                       Server.j2p (WebSharper.JQuery.JQuery.Of("#jsId").Val().ToString()) (WebSharper.JQuery.JQuery.Of("#protoId").Val().ToString())
                    )
        //let bidderExecution = 
        //    bidderSubmit.View.MapAsync(function
        //                | None -> async { return "" }
        //                | Some input -> 
        //                               Server.postBidderDoubleClicker (WebSharper.JQuery.JQuery.Of("#bidderId").Val().ToString()) (WebSharper.JQuery.JQuery.Of("#protoId").Val().ToString())
        //            )


        divAttr [] [
            divAttr [][
                Doc.Button "js2Proto" [] jsubmit.Trigger
                Doc.Button "proto2js" [] (fun () ->
                                            async {
                                                let! jStr = Server.p2j (WebSharper.JQuery.JQuery.Of("#protoId").Val().ToString())
                                                WebSharper.JQuery.JQuery.Of("#jsId").Val(jStr).Ignore
                                            } |> Async.Start                                                    
                                            )
                Doc.Button "Clear js" [] (fun () -> 
                                                    //WebSharper.JQuery.JQuery.Of("#consoleWC")
                                                    WebSharper.JQuery.JQuery.Of("#jsId").Val("").Ignore)
                Doc.Button "Clear proto" [] (fun () -> 
                                                    WebSharper.JQuery.JQuery.Of("#protoId").Val("").Ignore)
                
                Doc.Button "Post proto" [] (fun () ->
                                                    bidderSubmit.Trigger ()
                                                    async {
                                                        let! jStr = Server.postBidderDoubleClicker (WebSharper.JQuery.JQuery.Of("#bidderId").Val().ToString()) (WebSharper.JQuery.JQuery.Of("#protoId").Val().ToString())
                                                        WebSharper.JQuery.JQuery.Of("#bidderRespId").Val(jStr).Ignore
                                                        } |> Async.Start  
                                                        )
                Doc.Button "Clear result" [] (fun () -> 
                    WebSharper.JQuery.JQuery.Of("#bidderRespId").Val("").Ignore)
                brAttr [][]
                Doc.InputArea [attr.id "jsId"; attr.style "width: 880px"; attr.``class`` "input"; attr.rows "10" ] jsI
                Doc.InputArea [attr.id "protoId"; attr.style "width: 880px"; attr.``class`` "input"; attr.rows "1" ] protoI
                Doc.InputArea [attr.id "bidderId"; attr.style "width: 880px"; attr.``class`` "input"; attr.rows "1" ] bidderI
                
            ]
            hrAttr [] []
            h4Attr [attr.``class`` "text-muted"] [text "Server execution:"]
            divAttr [(*attr.``class`` "jumbotron"*)] [h1Attr [] [textView j2pExecution]]
            h4Attr [attr.``class`` "text-muted"] [text "Bidder response:"]
            Doc.InputArea [attr.id "bidderRespId"; attr.style "width: 880px"; attr.``class`` "input"; attr.rows "10" ] bidderR
            //divAttr [(*attr.``class`` "jumbotron"*)] [Doc.InputArea [] [textView bidderExecution]]
            
            //divAttr [] [h1Attr [] [textView (getHisCmd |> View.map (fun strArray ->     ))]]
        ]

    [<JavaScript>]
    let fsiCmd () =
        let rvInput = Var.Create ""
        let rvHisCmd = Var.Create ([||]:string[])
        let curPos = Var.Create 0
        let submit = Submitter.CreateOption rvInput.View
        let hisCmd = Submitter.CreateOption rvHisCmd.View
        //let nxtCmd = Submitter.CreateOption rvHisCmd.View
        
        let vReversed =
            submit.View.MapAsync(function
                | None -> async { return "" }
                | Some input -> 
                                rvHisCmd.Value <- Array.append rvHisCmd.Value [|input|]
                                curPos.Value <- curPos.Value + 1
                                Server.fsiExecute input
            )
        let getHisCmd =
            hisCmd.View.MapAsync(function
                | None -> Server.getHisCmds ()
                | Some v when v.Length = 0 -> Server.getHisCmds ()
                | Some v ->
                    async {return v}
            )

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
                //Doc.Button "Next Command" [] nxtCmd.Trigger
                brAttr [][]
                Doc.InputArea [attr.id "fsiCmd"; attr.style "width: 880px"; attr.``class`` "input"; attr.rows "10"; attr.value "printfn \"orz\""] rvInput
            ]
            hrAttr [] []
            h4Attr [attr.``class`` "text-muted"] [text "The server responded:"]
            divAttr [(*attr.``class`` "jumbotron"*)] [h1Attr [] [textView vReversed]]
            
            //divAttr [] [h1Attr [] [textView (getHisCmd |> View.map (fun strArray ->     ))]]
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

    