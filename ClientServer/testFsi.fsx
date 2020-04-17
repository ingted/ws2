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


type CompilerStatus =
    | Standby
    | Running
    | Failed of FSharpErrorInfo[]
    | Succeeded of string * FSharpErrorInfo[]

/// Cache the parse and check results for a given file.
type FileResults =
    {
        Parse: FSharpParseFileResults
        Check: FSharpCheckFileResults
    }

module FileResults =

    let OfRes (parseRes, checkRes) =
        {
            Parse = parseRes
            Check = checkRes
        }

/// The compiler's state.
type Compiler =
    {
        Checker: FSharpChecker
        Options: FSharpProjectOptions
        CheckResults: FSharpCheckProjectResults
        MainFile: FileResults
        Sequence: int
        Status: CompilerStatus
    }

module Compiler =

    /// Dummy project file path needed by the checker API. This file is never actually created.
    let projFile = "/tmp/out.fsproj"
    /// The input F# source file path.
    let inFile = "/tmp/Main.fs"
    /// The default output assembly file path.
    let outFile = "/tmp/out.exe"

    /// <summary>
    /// Create checker options.
    /// </summary>
    /// <param name="checker">The F# code checker.</param>
    /// <param name="outFile"></param>
    let Options (checker: FSharpChecker) (outFile: string) =
        checker.GetProjectOptionsFromCommandLineArgs(projFile, [|
            "--simpleresolution"
            "--optimize-"
            "--noframework"
            "--fullpaths"
            "--warn:3"
            "--target:exe"
            inFile
            // Necessary standard library
            "-r:/tmp/mscorlib.dll"
            "-r:/tmp/netstandard.dll"
            "-r:/tmp/System.dll"
            "-r:/tmp/System.Core.dll"
            "-r:/tmp/System.IO.dll"
            "-r:/tmp/System.Runtime.dll"
            // Additional libraries we want to make available
            "-r:/tmp/System.Net.Http.dll"
            "-r:/tmp/System.Threading.dll"
            "-r:/tmp/System.Threading.Tasks.dll"
            "-r:/tmp/FSharp.Data.dll"
            "-r:/tmp/System.Xml.Linq.dll"
            "-r:/tmp/System.Numerics.dll"
            "-r:/tmp/WebFsc.Env.dll"
            "-o:" + outFile
        |])

    /// <summary>
    /// Create a compiler instance.
    /// </summary>
    /// <param name="source">The initial contents of Main.fs</param>
    let Create source = async {
        let checker = FSharpChecker.Create(keepAssemblyContents = true)
        let options = Options checker outFile
        File.WriteAllText(inFile, source)
        let! checkRes = checker.ParseAndCheckProject(options)
        let! fileRes = checker.GetBackgroundCheckResultsForFileInProject(inFile, options)
        // The first compilation takes longer, so we run one during load
        let! _ = checker.Compile(checkRes.DependencyFiles)
        return {
            Checker = checker
            Options = options
            CheckResults = checkRes
            MainFile = FileResults.OfRes fileRes
            Sequence = 0
            Status = Standby
        }
    }

    /// <summary>
    /// Check whether compilation has failed.
    /// </summary>
    /// <param name="errors">The messages returned by the compiler</param>
    let IsFailure (errors: seq<FSharpErrorInfo>) =
        errors
        |> Seq.exists (fun (x: FSharpErrorInfo) -> x.Severity = FSharpErrorSeverity.Error)


open Compiler
let aa = 
    Create """
        let a = 1
        printfn "%d" a
    """
let bb = aa |> Async.RunSynchronously


open System
open System.IO
open System.Text

// Initialize output and input streams
let sbOut = new StringBuilder()
let sbErr = new StringBuilder()
let inStream = new StringReader("")
let outStream = new StringWriter(sbOut)
let errStream = new StringWriter(sbErr)

// Build command line arguments & start FSI session
let argv = [|"C:\\fsi.exe"|]
let allArgs = Array.append argv [||]
    

let fsiConfig = FsiEvaluationSession.GetDefaultConfiguration()
let fsiSession = FsiEvaluationSession.Create(fsiConfig, allArgs, inStream, outStream, errStream)

let evalExpression text =
    match fsiSession.EvalExpression(text) with
    | Some value -> 
        let v = value.ReflectionValue
        let t = value.ReflectionType
        printfn "%A" v        
        printfn "%A" <| v.GetType().FSharpName
    | None -> printfn "Got no result!"

evalExpression """
    let a = 1
    printfn "%d" a
"""
open System.Reflection
let ass = Assembly.GetAssembly typeof<Option<_>>
ass.GetExportedTypes()|> Array.map(fun i -> i.FullName)|>Array.sort |>Array.iter (fun i -> printfn "%s" i)
ass.GetModules()
ass.GetTypes()|> Array.map(fun i -> i.FullName)|>Array.sort |>Array.iter (fun i -> printfn "%s" i)
open Internal.Utilities.StructuredFormat
let a x = x + 1
Display.any_to_string (a, a.GetType())


let (|TFunc|_|) (typ: Type) =
    if typ.IsGenericType && typ.GetGenericTypeDefinition () = typeof<int->int>.GetGenericTypeDefinition () then
        match typ.GetGenericArguments() with
        | [|targ1; targ2|] -> Some (targ1, targ2)
        | _ -> None
    else
        None

let rec typeStr (typ: Type) =
    match typ with
    | TFunc (TFunc(_, _) as tfunc, t) -> sprintf "(%s) -> %s" (typeStr tfunc) (typeStr t)
    | TFunc (t1, t2) -> sprintf "%s -> %s" (typeStr t1) (typeStr t2)
    | typ when typ = typeof<int> -> "int"
    | typ when typ = typeof<string> -> "string"
    | typ when typ.IsGenericParameter -> sprintf "'%s" (string typ)
    | typ -> string typ


open Microsoft.FSharp.Reflection
let funString o =
    let rec loop nested t =
        if FSharpType.IsTuple t then
            FSharpType.GetTupleElements t
            |> Array.map (loop true)
            |> String.concat " * "
        elif FSharpType.IsFunction t then
            let fs = if nested then sprintf "(%s -> %s)" else sprintf "%s -> %s"
            let domain, range = FSharpType.GetFunctionElements t
            fs (loop true domain) (loop false range)
        else
            t.FullName
    loop false (o.GetType())

typeStr typeof<(string -> (string -> int) -> int) -> int>
// val it: string = "string -> (string -> int) -> int"
typeStr (typeof<int->int>.GetGenericTypeDefinition())



let a x = x + 1
typeStr (a.GetType())
funString typeStr
open FSharp.Compiler.Interactive.Shell
Utilities.colorPrintL

evalExpression """
    let a x = 1 + x
    a
"""



let evalExpressionTyped<'T> (text) =
    match fsiSession.EvalExpression(text) with
    | Some value -> value.ReflectionValue |> unbox<'T>
    | None -> failwith "Got no result!"

evalExpressionTyped<unit> """
    let a = 1
    printfn "%d" a
"""


fsiSession.EvalInteraction "printfn \"bye\""

module ConsoleUtil =
    open System
    open System.IO
    open System.Text
    //let sw = new StringWriter ()
    //Console.SetOut sw
    //printfn "printfn"
    //Console.WriteLine "Console"
    //sw.ToString ()

    type LockerObj (state: bool) = 
        member val state = state with get, set
    let mutable consoleMS = new MemoryStream ()
    let mutable ifConsoleStreamSwitchedFirstTime = new LockerObj(false)
    //let locker = new Object ()
    let switchConsoleMS () =
        lock ifConsoleStreamSwitchedFirstTime (fun () ->
            if ifConsoleStreamSwitchedFirstTime.state then ()
            else ifConsoleStreamSwitchedFirstTime.state <- true
            let newms = new MemoryStream ()
            let writer = new StreamWriter(newms)
            Console.Out.Flush ()
            Console.SetOut writer            
            let oldMs = consoleMS
            consoleMS <- newms
            //writer.Flush()
            //writer.WriteLine "123"            
            oldMs
            )
    //consoleMS.Length
    //consoleMS.Flush ()
    let getStdOut () =
        if ifConsoleStreamSwitchedFirstTime.state = false then 
            ignore <| switchConsoleMS ()
            ""
        else 
            let oldMs = switchConsoleMS ()
            //let newms = new MemoryStream ()
            //let writer = new StreamWriter(newms)
            ////let numBArr = Text.Encoding.Unicode.GetBytes "123"
            ////Text.Encoding.Unicode.GetString numBArr
            
            //newms.Position <- 0L
            ////numBArr |> Array.iter newms.WriteByte
            ////numBArr |> Array.iter writer.Write 
            
            //writer.WriteLine "123"
            //writer.WriteLine "12300"
            //writer.Flush()
            ////writer.Close()
            ////newms.Flush()
            //newms.Position <- 0L

            let sr = new StreamReader(oldMs)
            let l = int oldMs.Length
            let str =
                if l <= 2 then ""
                else
                    oldMs.Position <- 0L
                    let cArr:Char[] = Array.zeroCreate l
                    let charlen = sr.Read(cArr, 0, l)
            
                    //let bArr = newms.ToArray()
                    String.Join(null, cArr).Substring(0, charlen - 1 - 1)
            oldMs.Dispose ()
            str

open System
open System.IO
open System.Text
open System.Collections.Concurrent
type ConsoleTextWriter (tw:TextWriter) as this = 
    inherit TextWriter ()
    let mutable enc = Encoding.Default
    let queue = new ConcurrentDictionary<int, ConcurrentQueue<char> * ConcurrentQueue<char>>()
    do 
        ConsoleTextWriter.saveDefaultWriter ()
    member val curWriter = tw with get, set

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
        let s = sprintf "%d: %A" tid value
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
tw.curWriter

seq [0..9]
|> Seq.map (fun _ ->
    async {
        printfn "dddd"
    }
)
|> Async.Parallel
|> Async.StartAsTask

    /// <summary>
    /// Turn a file in the virtual filesystem into a browser download.
    /// </summary>
    /// <param name="path">The file's location in the virtual filesystem</param>
    let DownloadFile (path: string) =
        printfn "Downloading output..."
        try JS.Invoke<unit>("WebFsc.getCompiledFile", path)
        with exn -> eprintfn "%A" exn
        
    /// <summary>
    /// Set the HttpClient used by FSharp.Data and by user code.
    /// </summary>
    /// <param name="http"></param>
    let SetFSharpDataHttpClient http =
        // Set the FSharp.Data run time HttpClient
        FSharp.Data.Http.Client <- http
        // Set the FSharp.Data design time HttpClient
        let asm = System.Reflection.Assembly.LoadFrom("/tmp/FSharp.Data.DesignTime.dll")
        let ty = asm.GetType("FSharp.Data.Http")
        let prop = ty.GetProperty("Client", BindingFlags.Static ||| BindingFlags.Public)
        prop.GetSetMethod().Invoke(null, [|http|])
        |> ignore
        // Set the user run time HttpClient
        Env.SetHttp http

    let asyncMainTypeName = "Microsoft.FSharp.Core.unit -> \
                            Microsoft.FSharp.Control.Async<Microsoft.FSharp.Core.unit>"

    /// <summary>
    /// Check whether the code contains a function <c>Main.AsyncMain : unit -> Async&lt;unit&gt;</c>.
    /// </summary>
    /// <param name="checkRes">The compiler check results</param>
    let findAsyncMain (checkRes: FSharpCheckProjectResults) =
        match checkRes.AssemblySignature.FindEntityByPath ["Main"] with
        | Some m ->
            m.MembersFunctionsAndValues
            |> Seq.exists (fun v ->
                v.IsModuleValueOrMember &&
                v.LogicalName = "AsyncMain" &&
                v.FullType.Format(FSharpDisplayContext.Empty) = asyncMainTypeName
            )
        | None -> false

    /// <summary>
    /// Filter out "Main module of program is empty: nothing will happen when it is run"
    /// when the program has a function <c>Main.AsyncMain : unit -> Async&lt;unit&gt;</c>.
    /// </summary>
    /// <param name="checkRes">The compiler check results</param>
    /// <param name="errors">The parse and check messages</param>
    let filterNoMainMessage checkRes (errors: FSharpErrorInfo[]) =
        if findAsyncMain checkRes then
            errors |> Array.filter (fun m -> m.ErrorNumber <> 988)
        else
            errors

    /// The delayer for triggering type checking on user input.
    let checkDelay = Delayer(500)

open Compiler

type Compiler with

    /// <summary>
    /// Compile an assembly.
    /// </summary>
    /// <param name="source">The source of Main.fs.</param>
    /// <returns>The compiler in "Running" mode and the callback to complete the compilation</returns>
    member comp.Run(source: string) =
        { comp with Status = Running },
        fun () -> async {
            let start = DateTime.Now
            let outFile = sprintf "/tmp/out%i.exe" comp.Sequence
            File.WriteAllText(inFile, source)
            // We need to recompute the options because we're changing the out file
            let options = Compiler.Options comp.Checker outFile
            let! checkRes = comp.Checker.ParseAndCheckProject(options)
            if IsFailure checkRes.Errors then return { comp with Status = Failed checkRes.Errors } else
            let! errors, outCode = comp.Checker.Compile(checkRes)
            let finish = DateTime.Now
            printfn "Compiled in %A" (finish - start)
            let errors =
                Array.append checkRes.Errors errors
                |> filterNoMainMessage checkRes
            if IsFailure errors || outCode <> 0 then return { comp with Status = Failed errors } else
            return
                { comp with
                    Sequence = comp.Sequence + 1
                    Status = Succeeded (outFile, errors) }
        }

    /// <summary>
    /// Trigger code checking.
    /// Includes auto-delay, so can (and should) be called on every user input.
    /// </summary>
    /// <param name="source">The source of Main.fs</param>
    /// <param name="dispatch">The callback to dispatch the results</param>
    member comp.TriggerCheck(source: string, dispatch: Compiler * FSharpErrorInfo[] -> unit) =
        checkDelay.Trigger(async {
            let! parseRes, checkRes = comp.Checker.ParseAndCheckFileInProject(inFile, 0, source, comp.Options)
            let checkRes =
                match checkRes with
                | FSharpCheckFileAnswer.Succeeded res -> res
                | FSharpCheckFileAnswer.Aborted -> comp.MainFile.Check
            dispatch
                ({ comp with MainFile = FileResults.OfRes (parseRes, checkRes) },
                Array.append parseRes.Errors checkRes.Errors)
        })

    /// <summary>
    /// Get autocompletion items.
    /// </summary>
    /// <param name="line">The line where code has been input</param>
    /// <param name="col">The column where code has been input</param>
    /// <param name="lineText">The text of the line that has changed</param>
    member comp.Autocomplete(line: int, col: int, lineText: string) = async {
        let partialName = QuickParse.GetPartialLongNameEx(lineText, col)
        let! res = comp.MainFile.Check.GetDeclarationListInfo(Some comp.MainFile.Parse, line, lineText, partialName)
        return res.Items
    }

    /// The warnings and errors from the latest check.
    member comp.Messages =
        match comp.Status with
        | Standby | Running -> [||]
        | Succeeded(_, m) | Failed m -> m

    member comp.IsRunning =
        comp.Status = Running

    member comp.MarkAsFailedIfRunning() =
        match comp.Status with
        | Running -> { comp with Status = Failed [||] }
        | _ -> comp
