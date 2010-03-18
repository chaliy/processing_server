module Processing

open Shared
open Model
open Storage
open ProcessingServer.Handling

open System
open System.Threading
open System.Xml.Linq

type StorageAgent(storage : TaskStorage) =

    let taskReady = new Event<Task>()
    let sync = SyncContext.Current()

    let agent = 
        new Agent<_>(fun _ ->                            
                        printfn "StorageAgent: Ping recieved"    
                        let task = storage.Pick()
                        if task.IsSome then
                            task.Value |> sync.Raise taskReady )                            
    
    member x.Ping() = agent.Post()
    member x.Start() = agent.Start()
    member x.TaskReady = taskReady.Publish

type ProcessingAgent2(storage : TaskStorage,
                      handlers : ITaskHandler list) =

    let mutable runnig = 0

    let started = new Event<ID>()
    let success = new Event<ID>()
    let failed = new Event<ID * System.Exception>()
    let sync = SyncContext.Current()

    let raiseStarted id = 
        runnig <- runnig + 1
        sync.Raise started id

    let raiseSuccess id = 
        runnig <- runnig - 1
        sync.Raise success id

    let raiseFailed id ex = 
        runnig <- runnig - 1
        sync.Raise failed (id, ex)

    let createContext (t : Task) = { Data = t.Data }
    
    let wrap task = 
        printfn "ProcessingAgent: New task recieved"                     
        let ctx = createContext task
        let handler = handlers 
                      |> List.find(fun h -> h.CanHandle(ctx))

        thread (fun () -> 
                raiseStarted task.ID  
                try
                    handler.Handle(ctx)
                    raiseSuccess task.ID
                with
                | x -> raiseFailed task.ID x )


type ProcessingAgent(handlers : ITaskHandler list) =
       
    let started = new Event<ID>()
    let success = new Event<ID>()
    let failed = new Event<ID * System.Exception>()
    let sync = SyncContext.Current()

    let createContext (t : Task) = { Data = t.Data }
        
    let agent = 
        new Agent<Task>(fun task ->                                                               
                            printfn "ProcessingAgent: New task recieved"                     
                            let ctx = createContext task
                            let handler = handlers 
                                          |> List.find(fun h -> h.CanHandle(ctx))

                            sync.Raise started task.ID  
                            try
                                handler.Handle(ctx)
                                sync.Raise success task.ID
                            with
                            | x -> sync.Raise failed (task.ID, x) ) 
                                  

    member x.Post msg = agent.Post msg
    member x.Start() = agent.Start()
    member x.Started = started.Publish
    member x.Success = success.Publish
    member x.Failed = failed.Publish
