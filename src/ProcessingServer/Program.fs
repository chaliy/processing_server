open Shared
open System.Threading

type Task = {
    ID : ID
    Handler : string
}

type TaskStorage() =

    static member TASK1_ID = "TASK1_ID"    

    member x.LastFew = seq {
        yield { ID = TaskStorage.TASK1_ID 
                Handler = "Example" }
    }

type StorageAgent(taskStorage : TaskStorage) =    
    let taskReady = new Event<Task>()    
    
    member x.Start() = ()
    member x.TaskReady = taskReady.Publish


type ProcessingAgent() =    
       
    let started = new Event<ID>()
    let success = new Event<ID>()
    let failed = new Event<ID * System.Exception>()

    let resolveHadler t = fun unit -> Thread.Sleep(1000); ()
    
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


let storage = TaskStorage()
let storageAgent = StorageAgent(storage)
let processingAgent = ProcessingAgent()

// Wire up agents
processingAgent.Started.Add(printfn "Started %s")
processingAgent.Success.Add(printfn "Success %s")

storageAgent.TaskReady.Add(processingAgent.Post)


storageAgent.Start()
processingAgent.Start()

processingAgent.Post { ID = "TASK1_ID"
                       Handler = "Example" }

processingAgent.Post { ID = "TASK2_ID"
                       Handler = "Example #2" }

System.Console.ReadLine() |> ignore
