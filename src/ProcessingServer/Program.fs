open Shared
open Model
open Storage
open Processing
open System
open System.Threading


let storage = TaskStorage()
let storageAgent = StorageAgent(storage)
let processingAgent = ProcessingAgent()

storage.Dump()

// Wire up agents

// Write some debug
processingAgent.Started.Add(printfn "Started %s")
processingAgent.Success.Add(printfn "Success %s")
processingAgent.Failed.Add(fun (id, ex) -> printfn "Success %s - %s" id (ex.Message))

// Store processing status
processingAgent.Started.Add(storage.MarkStarted)
processingAgent.Success.Add(storage.MarkSuccess)
processingAgent.Failed.Add(fun (id, ex) -> storage.MarkFailed id ex)

// Ping storage agent may be we have new tasks
processingAgent.Success
|> Observable.merge(processingAgent.Started)
|> Observable.merge(processingAgent.Failed 
                    |> Observable.map(fun (id, ex) -> id))
|> Observable.add(fun _ -> storageAgent.Ping())

// post to processing agent if any task available
storageAgent.TaskReady.Add(processingAgent.Post)

processingAgent.Start()
storageAgent.Start()

// First ping
storageAgent.Ping()

while true do
    let inp = System.Console.ReadLine()
    match inp with
    | "Dump" -> storage.Dump()
    | "Ping" -> storageAgent.Ping()
    | "Clean" -> storage.Clean()
    | x ->
        let id = Guid.NewGuid().ToString()
        storage.Post({ ID = id
                       Handler = "Example"
                       Data = [v "Input" inp ] } )
        storageAgent.Ping()