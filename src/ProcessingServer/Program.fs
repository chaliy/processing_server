open Shared
open System.Threading

type Task = {
    ID : ID
    Handler : string
}

type TaskStorage() =

    static member TASK1_ID = "TASK1_ID"    

    member x.MarkStarted id = ()
    member x.MarkSuccess id = ()
    member x.MarkFailed id ex = ()
    member x.LastFew() = seq {
        yield { ID = TaskStorage.TASK1_ID 
                Handler = "Example" }
    }

type StorageAgent(storage : TaskStorage) =    
    let taskReady = new Event<Task>()

    let agent = 
        new Agent<_>(fun inbox ->
                            async {
                                let! _ = inbox.Receive()        
                                storage.LastFew()
                                |> Seq.iter(fun t -> raise taskReady t)
                            } )
    
    member x.Ping() = agent.Post()
    member x.TaskReady = taskReady.Publish


type ProcessingAgent() =    
       
    let started = new Event<ID>()
    let success = new Event<ID>()
    let failed = new Event<ID * System.Exception>()

    let resolveHadler t = (fun unit -> Thread.Sleep(1000); ())
    
    let agent = 
        new Agent<Task>(fun inbox ->
                            async {
                                while true do
                                    let! task = inbox.Receive()                                   
                                    let handler = resolveHadler task

                                    raise started task.ID                                    
                                    try                             
                                        handler()
                                        raise success task.ID
                                    with
                                    | x -> raise failed (task.ID, x)
                            } )    

    member x.Post msg = agent.Post msg
    member x.Start() = agent.Start()
    member x.Started = started.Publish
    member x.Success = success.Publish
    member x.Failed = failed.Publish



let storage = TaskStorage()
let storageAgent = StorageAgent(storage)
let processingAgent = ProcessingAgent()

// Wire up agents

// Write some debug
processingAgent.Started.Add(printfn "Started %s")
processingAgent.Success.Add(printfn "Success %s")

// Store processing status
processingAgent.Started.Add(storage.MarkStarted)
processingAgent.Success.Add(storage.MarkSuccess)
processingAgent.Failed.Add(fun (t, ex) -> storage.MarkFailed t ex)

// Ping storage agent may be we have new tasks
processingAgent.Success.Add(fun _ -> storageAgent.Ping())
processingAgent.Failed.Add(fun _ -> storageAgent.Ping())

// post to processing agent if any task available
storageAgent.TaskReady.Add(processingAgent.Post)

processingAgent.Start()

// First ping
storageAgent.Ping()

//processingAgent.Post { ID = "TASK1_ID"
//                       Handler = "Example" }
//
//processingAgent.Post { ID = "TASK2_ID"
//                       Handler = "Example #2" }

System.Console.ReadLine() |> ignore