module Processing

open Shared
open Model
open Storage
open System
open System.Threading

type StorageAgent(storage : TaskStorage) =    
    let taskReady = new Event<Task>()

    let agent = 
        new Agent<_>(fun inbox ->
                            async {
                                let! _ = inbox.Receive()        
                                let task = storage.Pick()
                                if task.IsSome then
                                    task.Value |> raise taskReady
                            } )
    
    member x.Ping() = agent.Post()
    member x.TaskReady = taskReady.Publish


type ProcessingAgent() =    
       
    let started = new Event<ID>()
    let success = new Event<ID>()
    let failed = new Event<ID * System.Exception>()

    let resolveHadler t = (fun (data : (string * obj) list) ->                             
                            printfn "Executing: %s" t.ID
                            data |> Seq.iter(fun (k, v) -> printfn "Data: %s - %s" k (v.ToString()))                            
                            Thread.Sleep(1000); 
                            ())
    
    let agent = 
        new Agent<Task>(fun inbox ->
                            async {
                                while true do
                                    let! task = inbox.Receive()                                   
                                    let handler = resolveHadler task

                                    raise started task.ID                         
                                    try                             
                                        handler(task.Data)
                                        raise success task.ID
                                    with
                                    | x -> raise failed (task.ID, x)
                            } )    

    member x.Post msg = agent.Post msg
    member x.Start() = agent.Start()
    member x.Started = started.Publish
    member x.Success = success.Publish
    member x.Failed = failed.Publish
