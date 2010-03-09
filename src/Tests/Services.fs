module ``Services module Specification``

open FsSpec
open System
open System.ServiceModel
        
open Shared
open Services

let createClient() =
    let factory = new ChannelFactory<TaskProcessing>(new WSHttpBinding())
    let address = new EndpointAddress("http://localhost:1066/")
    factory.CreateChannel(address);


module ``Describe service agent`` =
    
    let agent = new ServiceAgent()
    agent.Start()

    let ``should listen for tasks`` = spec {
        //use posted = agent.Posted.Subscribe()
        let client = createClient()
        let task = new Task()
//        task.ID <- "NewID"
//        task.Handler <- "Example"
//        task.Data <- new System.Collections.List<DataItem>()
        client.Post(task)
    }