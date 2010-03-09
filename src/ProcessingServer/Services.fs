module Services

open System
open System.Runtime.Serialization
open System.Collections.Generic
open System.ServiceModel

open Shared
open Model

// *************
// *** Model ***
// *************

[<DataContract(Namespace="urn:org:mir:processing-v1.0")>]
type DataItem() =
    [<DataMember(Name="Key", IsRequired = true, Order = 0)>]
    let mutable key : string = ""
    [<DataMember(Name="Value", IsRequired = true, Order = 1)>]
    let mutable arg_value : string = ""
    
    member public x.Key with get() = key
    member public x.Value with get() = arg_value    

[<DataContract(Namespace="urn:org:mir:processing-v1.0")>]
type Task() =  
    [<DataMember(Name = "ID", IsRequired = true, Order = 0)>]  
    let mutable id : string = ""
    [<DataMember(Name = "Handler", IsRequired = true, Order = 1)>]
    let mutable handler : string = ""
    [<DataMember(Name = "Data", IsRequired = true, Order = 2)>]
    let mutable data : List<DataItem> = List<DataItem>()

    member public x.ID with get() = id
    member public x.Handler with get() = handler
    member public x.Data with get() = data

[<ServiceContract(Namespace = "urn:org:mir:processing-v1.0")>]
type TaskProcessing =    
    [<OperationContract(IsOneWay = true)>]
    abstract member Post : t : Task -> unit

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
          Handler = t.Handler
          Data = t.Data 
                 |> Seq.map(fun x -> (x.Key, x.Value :> obj))
                 |> Seq.toList }    
        
            
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