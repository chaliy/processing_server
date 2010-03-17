module Processing

open Shared
open Model
open Storage
open ProcessingServer.Contract

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


type ProcessingAgent(handlers : ITaskHandler list) =    
       
    let started = new Event<ID>()
    let success = new Event<ID>()
    let failed = new Event<ID * System.Exception>()
    let sync = SyncContext.Current()

//    let resolveHadler t = (fun (data : XElement) ->                             
//                            printfn "Executing: %s" t.ID
//                            data |> Seq.iter(fun (k, v) -> printfn "Data: %s - %s" k (v.ToString()))                            
//                            Thread.Sleep(1000); 
//                            ())

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
