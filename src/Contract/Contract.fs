namespace ProcessingServer.Contract

open System.Xml.Linq
open System.ServiceModel
open System.Runtime.Serialization

[<DataContract(Namespace="urn:org:mir:processing-v1.0")>]
type Task() =      
    let mutable id : string = ""    
    let mutable data : XElement = null

    [<DataMember(Name = "ID", IsRequired = true, Order = 0)>]
    member x.ID with get() = id and set(v) = id <- v    
    [<DataMember(Name = "Data", IsRequired = true, Order = 2)>]
    member x.Data with get() = data and set(v) = data <- v

[<ServiceContract(Namespace = "urn:org:mir:processing-v1.0")>]
type TaskProcessing =    
    [<OperationContract(IsOneWay = true)>]
    abstract member Post : t : Task -> unit