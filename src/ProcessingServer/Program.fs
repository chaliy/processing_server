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

let storage = TaskStorage()
let handlerCatalog = HandlerCatalog()
let processingAgent = ProcessingAgent(storage, handlerCatalog.ResolveAll())

storage.Dump()

// Write some debug info
processingAgent.Trace.Add(printfn "%s")

// Store processing status
processingAgent.Started.Add(storage.MarkStarted)
processingAgent.Success.Add(storage.MarkSuccess)
processingAgent.Failed.Add(fun (id, ex) -> storage.MarkFailed id ex)

// Ping agent may be we have new tasks
// This should be encapsulated into processing agent
processingAgent.Success
|> Event.merge(processingAgent.Failed  |> Event.map fst )
|> Event.add(fun _ -> processingAgent.Ping())

// First ping, process all stuff and so on...
processingAgent.Ping()

// *******************************
// ***  Wire-up listening part ***
// *******************************

let servceAgent = ServiceAgent();
servceAgent.Posted.Add(storage.Post)
servceAgent.Posted.Add(fun _ -> processingAgent.Ping())
servceAgent.Start()

// **********************
// *** And Console UI ***
// **********************

while true do
    let inp = System.Console.ReadLine()
    match inp with
    | "Dump" -> storage.Dump()
    | "Ping" -> processingAgent.Ping()
    | "Clean" -> storage.Clean()
    | x ->
        let id = Guid.NewGuid().ToString()
        let data = System.Xml.Linq.XElement.Parse("<root />")
        storage.Post({ ID = id                       
                       Data = data } )
        processingAgent.Ping()