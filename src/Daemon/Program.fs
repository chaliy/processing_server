open Shared
open Model
open Storage
open Processing
open Services
open Handlers
open System
open System.Threading

// *******************************
// *** Wire-up processing part ***
// *******************************

let tracing = Tracing()
let storage = TaskStorage(tracing)
let handlerCatalog = HandlerCatalog()
let handlers = handlerCatalog.ResolveAll()
let processingAgent = ProcessingAgent(storage, handlers, tracing)

// First ping, process all stuff and so on...
processingAgent.Start()

// *******************************
// ***  Wire-up listening part ***
// *******************************

let servceAgent = ServiceAgent();
servceAgent.Posted.Add(storage.Post)
// Notifiy ptocessor about potential tasks.
servceAgent.Posted.Add(fun _ -> processingAgent.Ping())
servceAgent.Start()

// **********************
// *** And Console UI ***
// **********************
let inloop = ref true

// Ctrl+C should stop daemon
Console.TreatControlCAsInput <- false
Console.CancelKeyPress.Add(fun e -> 
                            e.Cancel <- true
                            inloop := false
                            (* processingAgent.Stop() *) )

while inloop.Value do
    let inp = System.Console.ReadLine()
    match inp with
    | "Dump" -> storage.Dump()    
    | "Clean" -> storage.Clean()
    | "Ping" -> processingAgent.Ping()
    | "Stop" -> inloop := false
    | null -> ()
    | _ -> printfn "Invalid command, try again"

processingAgent.Stop()