
namespace WebSharper.Owin.WebSocket.Test

module Server =
    open WebSharper
    open WebSharper.Owin.WebSocket.Server

    type [<JavaScript; NamedUnionCases>]
        C2SMessage =
        | Request1 of str: string[]
        | Request2 of int: int[]
    
    and [<JavaScript; NamedUnionCases "type">]
        S2CMessage =
        | [<Name "int">] Response2 of value: int
        | [<Name "string">] Response1 of value: string

    let Start () : StatefulAgent<S2CMessage, C2SMessage, int> =
        /// print to debug output and stdout
        let dprintfn x =
            Printf.ksprintf (fun s ->
                System.Diagnostics.Debug.WriteLine s
                stdout.WriteLine s
            ) x

        fun client -> async {
            let clientIp = client.Connection.Context.Request.RemoteIpAddress
            return 0, fun state msg -> async {
                eprintfn "Received message #%i from %s" state clientIp
                match msg with
                | Message data -> 
                    match data with
                    | Request1 x -> do! client.PostAsync (Response1 x.[0])
                    | Request2 x -> do! client.PostAsync (Response2 x.[0])
                    return state + 1
                | Error exn -> 
                    dprintfn "Error in WebSocket server connected to %s: %s" clientIp exn.Message
                    do! client.PostAsync (Response1 ("Error: " + exn.Message))
                    return state
                | Close ->
                    eprintfn "Closed connection to %s" clientIp
                    return state
            }
        }
