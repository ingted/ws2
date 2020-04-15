// Learn more about F# at http://fsharp.org

open System

[<EntryPoint>]
let main argv =
    let d1 = IO.Directory.GetCurrentDirectory()
    let d2 = System.Reflection.Assembly.GetExecutingAssembly()
    printfn "%s" d1
    printfn "%s" d2.Location
    0 // return an integer exit code
