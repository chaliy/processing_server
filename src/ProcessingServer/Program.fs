open Shared
open Model
open Storage
open Processing
open System
open System.Threading


let storage = TaskStorage()
let storageAgent = StorageAgent(storage)
let processingAgent = ProcessingAgent()

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

// First ping
storageAgent.Ping()

while true do
    let newTask = System.Console.ReadLine()
    storage.Post({ ID = "TASK1_ID"
                   Handler = "Example"
                   Data = [v "Input" newTask ] })