open Shared
open System.Threading

type Task = {
    ID : ID
    Handler : string
}

module TaskStorage =
    let TASK1_ID = "TASK1_ID"  
    let last() = 
        { ID = TASK1_ID 
          Handler = "Example" }

type ProcessingAgent() =    
       
    let started = new Event<ID>()
    let success = new Event<ID>()
    let failed = new Event<ID * System.Exception>()

    let resolveHadler t = fun unit -> Thread.Sleep(1000); ()

    let syncContext = SynchronizationContext.CaptureCurrent()  
    let raise event args = syncContext.RaiseEvent event args

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
    member x.Start () = agent.Start()
    member x.Started = started.Publish
    member x.Success = success.Publish

let agent = new ProcessingAgent()
agent.Started.Add(printfn "Started %s")
agent.Success.Add(printfn "Success %s")

agent.Start()

agent.Post { ID = "TASK1_ID"
             Handler = "Example" }

agent.Post { ID = "TASK2_ID"
             Handler = "Example #2" }

System.Console.ReadLine() |> ignore
