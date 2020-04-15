namespace testFrom0

open System.Threading

module Server =
    open WebSharper
    open WebSharper.Owin.WebSocket.Server
    [<Rpc>]
    let DoSomething input =
        let R (s: string) = System.String(Array.rev(s.ToCharArray()))
        async {
            return R input
        }

    [<Rpc>]
    let fsiExecute (input:string) =
        async {
            return input
        }

    type Name = {
        [<Name "first-name">] FirstName: string
        LastName: string
    }

    type User = {
        name: Name
        age: int
    }

    type [<JavaScript; NamedUnionCases>]
        C2SMessage =
        | Request1 of str: string[]
        | Request2 of int: int[]
        | Req3 of User
        | MessageFromClient of cmd : string
    
    and [<JavaScript; NamedUnionCases "type">]
        S2CMessage =
        | [<Name "int">] Response2 of value: int
        | [<Name "string">] Response1 of value: string
        | [<Name "name">] Resp3 of value: Name
        | [<Name "msgStr">] MessageFromServer_String of value: string
    
    (*
        type StatefulAgent<'S2C, 'C2S, 'State> = 
            WebSocketServer<'S2C, 'C2S> -> 
                Async<'State * ('State -> 
                                    Message<'S2C> -> 
                                        Async<'State>)>
    *)
    
    let Start i : StatefulAgent<S2CMessage, C2SMessage, int> =
        
        /// print to debug output and stdout
        let dprintfn x =
            Printf.ksprintf (fun s ->
                System.Diagnostics.Debug.WriteLine s
                stdout.WriteLine s
            ) x
        let agt = 
            MailboxProcessor.Start(
                fun (agt:MailboxProcessor<string*Message<C2SMessage>*int*AsyncReplyChannel<S2CMessage option*int>>) -> 
                    let rec f () =
                        async {
                            let! (clientIp, msg, state, channel) = agt.Receive () 
                            if i = 1 
                            then Thread.Sleep 2000 
                            else Thread.Sleep 1000
                            match msg with
                            | Message data -> 
                                match data with
                                | Request1 x -> 
                                    channel.Reply <| (Some (Response1 x.[0]), state + 1)
                                | Request2 x -> 
                                    channel.Reply <| (Some (Response2 x.[0]), state + 1)
                                | Req3 x ->
                                    channel.Reply <| (Some (Resp3 x.name), state + 1)
                                | MessageFromClient cmd ->
                                    channel.Reply <| (Some (MessageFromServer_String <| cmd.Substring 5), state + 1)
                            | Error exn -> 
                                dprintfn "Error in WebSocket server connected to %s: %s" clientIp exn.Message
                                channel.Reply <| (Some (Response1 ("Error: " + exn.Message)), state)
                            | Close ->
                                eprintfn "Closed connection to %d %s" i clientIp
                                channel.Reply <| (None, state)
                            return! f ()
                        }
                    f ()
        )
        fun client -> async {
            let clientIp = client.Connection.Context.Request.RemoteIpAddress
            
            return 0, fun state msg -> async {
                eprintfn "%d Received message #%i from %s" i state clientIp
                let! (msg2client, state) = 
                    agt.PostAndAsyncReply(
                        fun channel ->
                            (clientIp, msg, state, channel)
                    )
                match msg2client with
                | Some m ->
                    do! client.PostAsync m
                | None -> ()
                return state
            }
        }
