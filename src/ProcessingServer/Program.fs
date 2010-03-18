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
let storageAgent = StorageAgent(storage)
let handlerCatalog = HandlerCatalog()
let processingAgent = ProcessingAgent(handlerCatalog.ResolveAll())

storage.Dump()

// Write some debug info
processingAgent.Started.Add(printfn "Started %s")
processingAgent.Success.Add(printfn "Success %s")
processingAgent.Failed.Add(fun (id, ex) -> printfn "Success %s - %s" id (ex.Message))

// Store processing status
processingAgent.Started.Add(storage.MarkStarted)
processingAgent.Success.Add(storage.MarkSuccess)
processingAgent.Failed.Add(fun (id, ex) -> storage.MarkFailed id ex)

// Ping storage agent may be we have new tasks
processingAgent.Success
|> Event.merge(processingAgent.Started)
|> Event.merge(processingAgent.Failed  |> Event.map fst )
|> Event.add(fun _ -> storageAgent.Ping())

// post to processing agent if any task available
storageAgent.TaskReady.Add(processingAgent.Post)

processingAgent.Start()
storageAgent.Start()

// First ping
storageAgent.Ping()

// *******************************
// ***  Wire-up listening part ***
// *******************************

let servceAgent = ServiceAgent();
servceAgent.Posted.Add(storage.Post)
servceAgent.Start()

// **********************
// *** And Console UI ***
// **********************

while true do
    let inp = System.Console.ReadLine()
    match inp with
    | "Dump" -> storage.Dump()
    | "Ping" -> storageAgent.Ping()
    | "Clean" -> storage.Clean()
    | x ->
        let id = Guid.NewGuid().ToString()
        let data = System.Xml.Linq.XElement.Parse("<root />")
        storage.Post({ ID = id                       
                       Data = data } )
        storageAgent.Ping()

(* 
    ProcessingAgent
    -- Should poll    
*)