module ``Services module Specification``

open FsSpec
open System
open System.Collections.Generic
open System.ServiceModel
open System.Threading
        
open Shared
open Services

let createClient() =
    let factory = new ChannelFactory<TaskProcessing>(new WSHttpBinding())
    let address = new EndpointAddress("http://localhost:1066/")
    factory.CreateChannel(address);


module ``Describe service agent`` =
    
    let agent = new ServiceAgent()
    agent.Start()

    // TODO Move this to FsSpec
    type Waiter<'T>() =
        let mutable result = None
        let resEvent = new ManualResetEvent(false)
        let sync = new obj()
                
        member x.Result() : 'T =
                match result with
                | Some res -> res
                | None -> failwith "Result not ready"

        member x.RegisterResult (res:'T) =                
                    lock sync (fun () -> 
                        result <- Some(res)
                        resEvent.Set () |> ignore )

        member x.Wait() = resEvent.WaitOne() |> ignore

    
    let ``should listen for tasks`` = spec {
        
        let waiter  = Waiter<Model.Task>()

        // TODO replace let x with use x when FsSpec will support this
        let x = agent.Posted.Subscribe(waiter.RegisterResult)        
        let client = createClient()

        let task = new Task()
        task.ID <- "NewID"
        task.Handler <- "Example"
        task.Data <- new List<DataItem>()
        client.Post(task)

        waiter.Wait()

        waiter.Result().should_not_be_null
        waiter.Result().ID.should_be_equal_to("NewID")
                
        x.Dispose()
    }    