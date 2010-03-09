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
    let mutable key : string = ""    
    let mutable arg_value : string = ""
    
    [<DataMember(Name="Key", IsRequired = true, Order = 0)>]
    member x.Key with get() = key and set(v) = key <- v
    [<DataMember(Name="Value", IsRequired = true, Order = 1)>]
    member x.Value with get() = arg_value and set(v) = arg_value <- v   

[<DataContract(Namespace="urn:org:mir:processing-v1.0")>]
type Task() =      
    let mutable id : string = ""
    let mutable handler : string = ""    
    let mutable data : List<DataItem> = List<DataItem>()

    [<DataMember(Name = "ID", IsRequired = true, Order = 0)>]
    member x.ID with get() = id and set(v) = id <- v
    [<DataMember(Name = "Handler", IsRequired = true, Order = 1)>]
    member x.Handler with get() = handler and set(v) = handler <- v
    [<DataMember(Name = "Data", IsRequired = true, Order = 2)>]
    member x.Data with get() = data and set(v) = data <- v    

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