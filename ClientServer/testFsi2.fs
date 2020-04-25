#if INTERACTIVE
#r "nuget: FSharp.Compiler.Service"
#r "nuget: Unquote"
#endif
open FSharp.Compiler.SourceCodeServices
open FSharp.Compiler.Interactive.Shell

open Swensen.Unquote.Extensions

open System
open System.IO
open System.Reflection
open System.Text

open System.Collections.Concurrent
//let cw = ConsoleTextWriter.defaultWriter
type ConsoleTextWriter (tw:TextWriter) as this = 
    inherit TextWriter ()
    let mutable enc = Encoding.Default
    let queue = new ConcurrentDictionary<int, ConcurrentQueue<char> * ConcurrentQueue<char>>()
    do 
        ConsoleTextWriter.saveDefaultWriter ()
    member val curWriter = tw with get, set
    override this.NewLine =
        Threading.Thread.CurrentThread.ManagedThreadId.ToString() + ": "
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
            this.curWriter.WriteLine s
        | _ ->
            //this.curWriter.WriteLine (sprintf "queueIt %A" value)
            preCharQ.Enqueue value
            allQ.Enqueue value
    override this.WriteLine(value:string) =
        let tid = Threading.Thread.CurrentThread.ManagedThreadId
        let s = sprintf "%d: %s" tid <| value.Replace("\r\n", "\r\n|")
        this.curWriter.WriteLine s

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
            )
    

(*
ConsoleTextWriter.saveDefaultWriter ()
ConsoleTextWriter.defaultWriter
*)
let tw = new ConsoleTextWriter(Console.Out)
Console.SetOut (System.IO.TextWriter.Synchronized tw)


printfn "ppp"
Console.WriteLine "ggg"






// Initialize output and input streams
let sbOut = new StringBuilder()
let sbErr = new StringBuilder()
let inStream = new StringReader("")
let outStream = new StringWriter(sbOut)
let errStream = new StringWriter(sbErr)

// Build command line arguments & start FSI session
let argv = [|"C:\\fsi.exe"|]
let allArgs = Array.append argv [| "--noframework"; "--langversion:preview" |]
    

let fsiConfig = FsiEvaluationSession.GetDefaultConfiguration()
//let fsiSession = FsiEvaluationSession.Create(fsiConfig, allArgs, inStream, tw, tw)
let fsiSession = FsiEvaluationSession.Create(fsiConfig, allArgs, inStream, outStream, errStream)

let evalExpression text =
    match fsiSession.EvalExpression(text) with
    | Some value -> 
        value.ReflectionValue
        //printfn "%A" v        
    | None -> failwith "Got no result!"

evalExpression """
    let a = 1
    printfn "%d" a
"""

((evalExpression """
    let a x = 1 + x
    a
""") :?> (int -> int)) 10



let evalExpressionTyped<'T> (text) =
    match fsiSession.EvalExpression(text) with
    | Some value -> 
        value.ReflectionValue |> unbox<'T>
    | None -> failwith "Got no result!"

evalExpressionTyped<unit> """
    let a = 1
    printfn "%d" a
"""
evalExpressionTyped<int> "a"

fsiSession.EvalInteraction "printfn \"bye\""

fsiSession.EvalInteraction """#r "nuget: Akka.Cluster" """

let akka = fsiSession.EvalInteractionNonThrowing """
    #r "nuget: Akka" 
    open Akka
"""


fsiSession.EvalInteraction "let a = 1"
fsiSession.EvalInteraction "fsi"

let eint = 
    fsiSession.EvalInteractionNonThrowing """
#if INTERACTIVE
printfn "123"
#r @"C:\Users\anibal\Downloads\owin.websocket\packages\HtmlAgilityPack.1.11.23\lib\Net45\HtmlAgilityPack.dll"
#endif
//open Swensen.Unquote.Extensions
"""
printfn "gg"

let fsiConfig2 = FsiEvaluationSession.GetDefaultConfiguration(fsiSession)
let fsiSession2 = FsiEvaluationSession.Create(fsiConfig2, allArgs, inStream, tw, tw)

seq [0..9]
|> Seq.map (fun _ ->
    async {
        printfn "dddd"
    }
)
|> Async.Parallel
|> Async.StartAsTask
