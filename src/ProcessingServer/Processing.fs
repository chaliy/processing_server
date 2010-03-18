module Processing

open Shared
open Model
open Storage
open ProcessingServer.Handling

open System
open System.Threading
open System.Xml.Linq

type ProcessingAgent(storage : TaskStorage,
                      handlers : ITaskHandler list) =
    
    let mutable runnig = 0

    let trace = new Event<string>()
    let started = new Event<ID>()
    let success = new Event<ID>()
    let failed = new Event<ID * System.Exception>()    
    let sync = SyncContext.Current()

    let raiseTrace msg =
        sync.Raise trace msg

    let raiseStarted id = 
        raiseTrace (sprintf "ProcessingAgent: Task started %s" id)
        runnig <- runnig + 1
        sync.Raise started id

    let raiseSuccess id =         
        raiseTrace (sprintf "ProcessingAgent: Task success %s" id)
        runnig <- runnig - 1
        sync.Raise success id        

    let raiseFailed id ex = 
        raiseTrace (sprintf "ProcessingAgent: Task failed %s" id)
        runnig <- runnig - 1
        sync.Raise failed (id, ex)           

    let createContext (t : Task) = { Data = t.Data }
    
    let wrap task =                        
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

    let pickTasks() =
        if (runnig < 10) then
            let tasks = storage.Pick2(10)
            raiseTrace (sprintf "ProcessingAgent: Tasks recieved %i" tasks.Length)
                  
            tasks
            |> Seq.map(wrap)
            |> Seq.iter(fun t -> t.Start())        

    member x.Ping = pickTasks
    member x.Started = started.Publish
    member x.Success = success.Publish
    member x.Failed = failed.Publish
    member x.Trace = trace.Publish