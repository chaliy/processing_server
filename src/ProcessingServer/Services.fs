module Services

open System
open System.Runtime.Serialization
open System.Collections.Generic
open System.ServiceModel

open Shared
open Storage

(* Model *)

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
    abstract member Post : Task -> unit

(* Implementation *)
type TaskProcessingService() = 
    interface TaskProcessing with
        member gms.Post(t) = ()
        
type ServiceAgent(storage : TaskStorage) =
    
    let service = { new TaskProcessing with
                        member gms.Post(t) = () }    
    let init() =        
        let baseAddress = Uri("http://localhost:1066/")
        use host = new ServiceHost(service, baseAddress)
        host.AddServiceEndpoint(typeof<TaskProcessing>,
            new WSHttpBinding(SecurityMode.Message), "") |> ignore
      
        host.Open();

    member x.Start() = init()                                                    