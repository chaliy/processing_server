module Services

open System
open System.Collections.Generic
open System.ServiceModel
open System.Xml.Linq

open Shared
open Model

open ProcessingServer.Contract


// **********************
// *** Implementation ***
// **********************

[<ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)>]
type TaskProcessingWrapper(service : TaskProcessing) =
    interface TaskProcessing with
        member gms.Post(t) = service.Post(t)

type ServiceAgent() =

    let posted = new Event<Model.Task>()
    let sync = SyncContext.Current()

    let convert (t : Task) = 
        { ID = t.ID
          Data = t.Data
          Tags = t.Tags }    
        
            
    let service = { new TaskProcessing with
                        member gms.Post(t) = sync.Raise posted (convert t) }  

    let baseAddress = Uri("http://localhost:1066/")
    let host =         
        let host = new ServiceHost(new TaskProcessingWrapper(service), baseAddress)
        host.AddServiceEndpoint(typeof<TaskProcessing>,
            new WSHttpBinding(SecurityMode.Message), "") |> ignore
        host
        
    member x.Start() = host.Open()
    member x.Posted = posted.Publish