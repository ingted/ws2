
namespace WebSharper.Owin.WebSocket.Test

open WebSharper
open WebSharper.JavaScript
open WebSharper.Html.Client
open WebSharper.Owin.WebSocket






[<JavaScript>]
module Client = //javascript run in the browser/js client
    open WebSharper.Owin.WebSocket.Client

    
    
    
    
    
    
    
    
    
    
    
    
    
    
    
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
            
            let lotsOfHellos = "HELLO" |> Array.create 1000
            let lotsOf123s = 123 |> Array.create 1000

            while true do
                do! Async.Sleep 1000
                server.Post (Server.Request1 [| "HELLO" |])
                do! Async.Sleep 1000
                server.Post (Server.Request2 lotsOf123s)
        }
        |> Async.Start

        container
