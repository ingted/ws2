namespace testFrom0

open System.Threading
open System.Reactive
open FSharp.Control.Reactive
open System.IO
open System.Text
//open FSharp.Compiler.SourceCodeServices
//open FSharp.Compiler.Interactive.Shell
open FSharp.Compiler
open FSharp.Compiler.AbstractIL
open FSharp.Compiler.Interactive.Shell
open FSharp.Compiler.Interactive.Shell.Settings
open Swensen.Unquote.Extensions
open System
open System.IO
open System.Reflection
open System.Text
open System.Collections.Concurrent
open System.Reactive.Subjects
open FSharp.Control.Reactive

type TwOrSub =
| TW of TextWriter
| SUB of ReplaySubject<string>
type ConsoleTextWriter (tos: TwOrSub) = 
    inherit TextWriter ()
    let mutable enc = Encoding.Default
    let queue = new ConcurrentDictionary<int, ConcurrentQueue<char> * ConcurrentQueue<char>>()
    let mutable cw = Unchecked.defaultof<string -> unit>
    do 
        ConsoleTextWriter.saveDefaultWriter ()
        cw <- 
            match tos with 
            | TW tw -> fun (str:string) -> tw.WriteLine str
            | SUB sub -> fun (str:string) -> sub.OnNext str
    member this.curWrite 
        with get () = cw
        and set value = cw <- value
                
                
    //override this.NewLine =
    //    Threading.Thread.CurrentThread.ManagedThreadId.ToString() + ": "
    override this.Write(value:char) =
        let tid = Threading.Thread.CurrentThread.ManagedThreadId
        let preCharQ, allQ =
            queue.GetOrAdd(
                tid
                , (ConcurrentQueue<char>(), ConcurrentQueue<char>())
            )
        let preChar = ref ' '
        if preCharQ.IsEmpty then () 
        else
            ignore <| preCharQ.TryDequeue(preChar)
            //this.curWriter.WriteLine (sprintf "dqIt %A %A" dq preChar)
        
        match preChar.Value, value with
        | ('\013', '\010') ->
            let aq = allQ.ToArray() |> Array.take (allQ.Count - 1)
            let qq = ref (ConcurrentQueue<char>(), ConcurrentQueue<char>())

            ignore <| queue.TryRemove(tid, qq)
            let str = String.Join(null, aq)
            let s = sprintf "%d: %s" tid str
            //this.curWriter.WriteLine s
            this.curWrite s
        | _ ->
            //this.curWriter.WriteLine (sprintf "queueIt %A" value)
            preCharQ.Enqueue value
            allQ.Enqueue value
    override this.WriteLine(value:string) =
        let tid = Threading.Thread.CurrentThread.ManagedThreadId
        let s = sprintf "%d: %s" tid <| value.Replace("\r\n", "\r\n|")
        //this.curWriter.WriteLine s
        this.curWrite s

    override this.Encoding 
        with get () = enc
    static member val locker = new Object() with get
    static member val ifSaveDefaultWritter = false with get, set
    static member val defaultWriter = Unchecked.defaultof<TextWriter> with get, set
    static member saveDefaultWriter () =
        lock ConsoleTextWriter.locker (fun () ->
            if ConsoleTextWriter.ifSaveDefaultWritter = false then
                ConsoleTextWriter.defaultWriter <- Console.Out
                ConsoleTextWriter.ifSaveDefaultWritter <- true
                //let a = 123
                //()
            )

module Server =
    open WebSharper
    open WebSharper.Owin.WebSocket.Server
    let sbOut = new StringBuilder()
    let sbErr = new StringBuilder()
    let inStream = new StringReader("")
    let outStream = new StringWriter(sbOut)
    let errStream = new StringWriter(sbErr)

    // Build command line arguments & start FSI session
    let argv = [|"C:\\fsi.exe"|]
    let allArgs = Array.append argv [| "--noframework"; "--langversion:preview" |]
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


    //type ConsoleTextWriter (tw:TextWriter) as this = 
    //    inherit TextWriter ()
    //    let mutable enc = Encoding.Default
    //    member val curWriter = tw with get, set
    //    member val executeWhenWriteLine = fun (str:string) -> () with get, set
    //    member val executeWhenWrite = fun (ch:char) -> () with get, set
    //    override this.Write(value:char) =
    //        //let s = sprintf "orz: %A" value
    //        //this.curWriter.WriteLine s
    //        this.executeWhenWrite value
    //    override this.WriteLine(value:string) =
    //        //let s = sprintf "orz: %A" value
    //        this.executeWhenWriteLine value
    //    override this.Encoding 
    //        with get () = enc

    [<Rpc>]
    let DoSomething input =
        let R (s: string) = System.String(Array.rev(s.ToCharArray()))
        async {
            return R input
        }
    let obs = Subject<string>.replay
    let obsOrig = Subject<string>.broadcast
    let tw = new ConsoleTextWriter(SUB obs)

    let ooOrig = 
        obsOrig
        |> Observable.observeOn System.Reactive.Concurrency.Scheduler.CurrentThread
        |> Observable.subscribeOn System.Reactive.Concurrency.Scheduler.CurrentThread
        |> Observable.subscribe (fun o ->
            //let tid = Threading.Thread.CurrentThread.ManagedThreadId.ToString()
            ConsoleTextWriter.defaultWriter.WriteLine o
        )

    Console.SetOut tw
    //let fsiConfigOld = FsiEvaluationSession.GetDefaultConfiguration()
    //let fsiSessionOld = FsiEvaluationSession.Create(fsiConfigOld, allArgs, inStream, tw, tw)

    open FsiSession

    let fsiSession = 
        FsiEvaluationSession.Create (fsiConfig, [|"fsi.exe"; "/langversion:preview"|], inStream, tw, tw, collectible=false, legacyReferenceResolver=legacyReferenceResolver)


    let locker = new Object()

    type FSCMD = string
    let fsiExecututor = 
        MailboxProcessor.Start(
            fun (agt:MailboxProcessor<FSCMD * AsyncReplyChannel<S2CMessage option>>) -> 
                let rec f () =
                    async {
                        let! (cmd, channel) = agt.Receive ()
                        //let rst, err = fsiSession.EvalInteractionNonThrowing cmd
                        let rst = fsiSession.EvalInteraction cmd
                        //match rst with
                        //| Choice1Of2 (Some value) ->
                        //    if value.ReflectionValue = null then channel.Reply <| None else
                        //    channel.Reply <| Some (MessageFromServer_String (value.ReflectionValue.ToString()))
                        //| Choice1Of2 None ->
                        //    channel.Reply <| None
                        //| Choice2Of2 exn ->
                        //    channel.Reply <| Some (MessageFromServer_String (exn.Message))
                        return! f ()
                    }
                f ()
    )

    [<Rpc>]
    let fsiExecute (input:string) =
        async {
            let reply = fsiExecututor.PostAndReply (fun (channel:AsyncReplyChannel<S2CMessage option>) -> input, channel)
            match reply with
            | Some (MessageFromServer_String v) -> return v
            | None -> return "noReturn"
            | _ -> return "invalidMsg"
            }

    

    
    (*
        type StatefulAgent<'S2C, 'C2S, 'State> = 
            WebSocketServer<'S2C, 'C2S> -> 
                Async<'State * ('State -> 
                                    Message<'S2C> -> 
                                        Async<'State>)>
    *)
    //let os wsproc =
    //    WebSharper.Owin.WebSocket.ProcessWebSocketConnection<int, int>(wsproc)
    //let sfAgt : StatefulAgent<int, int, string * Owin.WebSocket.WebSocketConnection> =
    //    fun client -> async {
    //        let conn = client.Connection
    //        let clientIp = client.Connection.Context.Request.RemoteIpAddress
    //        return ("initState", conn), fun state msg -> async {
    //            return ("stateString", conn)
    //        }
    //    }
    let mre = new ManualResetEvent (false)

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
                                    let ll = if cmd.Length <= 5 then cmd.Length else 5
                                    channel.Reply <| (Some (MessageFromServer_String <| cmd.Substring(0, ll)), state + 1)
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
                //let tid = Threading.Thread.CurrentThread.ManagedThreadId.ToString() + ":"
                let! (msg2client, state) = 
                    agt.PostAndAsyncReply(
                        fun channel ->
                            (clientIp, msg, state, channel)
                    )
                match msg2client with
                | Some m ->
                    do! client.PostAsync m
                    let loopPost (cmdResult:string) = 
                        //sync {
                            //let! m = 
                            //return! loopPost ()
                            //loopPost ()
                            if cmdResult.Substring(0, 2) = "1:" then
                                obsOrig.OnNext cmdResult
                            client.PostAsync (S2CMessage.MessageFromServer_String cmdResult)
                            |> Async.Start
                            //printfn "cpIt"
                        //}
                    //do! loopPost ()

                    use oo = 
                        obs
                        //|> Observable.filter (fun o ->
                        //    o.Substring(0, 2) <> tid
                        //)
                        |> Observable.observeOn System.Reactive.Concurrency.Scheduler.CurrentThread
                        |> Observable.subscribeOn System.Reactive.Concurrency.Scheduler.CurrentThread
                        |> Observable.subscribe loopPost
                    
                    mre.WaitOne ()|> ignore
                    printfn "jumpOut"
                | None -> ()
                return state * 10
            }
        }

    let wbSockIn i : StatefulAgent<S2CMessage, C2SMessage, int> =
        Console.SetOut (System.IO.TextWriter.Synchronized tw)
        let dprintfn x =
            Printf.ksprintf (fun s ->
                System.Diagnostics.Debug.WriteLine s
                stdout.WriteLine s
            ) x
        let fsiKickOffAgent = 
            MailboxProcessor.Start(
                fun (agt:MailboxProcessor<WebSocketClient<S2CMessage, C2SMessage> * Message<C2SMessage> * int * AsyncReplyChannel<S2CMessage option * int>>) -> 
                    let rec f () =
                        async {
                            let! (client, msg, state, channel) = agt.Receive () 
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
                                    let ll = if cmd.Length <= 5 then cmd.Length else 5
                                    channel.Reply <| (Some (MessageFromServer_String <| cmd.Substring(0, ll)), state + 1)
                            | Error exn -> 
                                dprintfn "Error in WebSocket server connected to %s: %s" client.Connection.Context.Request.RemoteIpAddress exn.Message
                                channel.Reply <| (Some (Response1 ("Error: " + exn.Message)), state)
                            | Close ->
                                eprintfn "Closed connection to %d %s" i client.Connection.Context.Request.RemoteIpAddress
                                channel.Reply <| (None, state)
                            return! f ()
                        }
                    f ()
        )
        fun client -> async {
            let clientIp = client.Connection.Context.Request.RemoteIpAddress
            
            return 0, fun state msg -> async {
                eprintfn "%d Received kickOff message #%i from %s" i state clientIp
                let! (msg2client, state) = 
                    fsiKickOffAgent.PostAndAsyncReply(
                        fun channel ->
                            (client, msg, state, channel)
                    )
                match msg2client with
                | Some m ->
                    do! client.PostAsync m
                    let loopPost cmdResult = 
                        //sync {
                            //let! m = 
                            //return! loopPost ()
                            //loopPost ()
                            client.PostAsync (S2CMessage.MessageFromServer_String cmdResult)
                            |> Async.Start
                            printfn "cpIt"
                        //}
                    //do! loopPost ()

                    use oo = 
                        obs
                        |> Observable.observeOn System.Reactive.Concurrency.Scheduler.CurrentThread
                        |> Observable.subscribeOn System.Reactive.Concurrency.Scheduler.CurrentThread
                        |> Observable.subscribe loopPost
                    
                    mre.WaitOne ()|> ignore
                    printfn "jumpOut"
                | None -> ()
                return state * 10
            }
        }
