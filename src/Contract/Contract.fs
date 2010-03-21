namespace ProcessingServer.Contract

open System.Xml.Linq
open System.ServiceModel
open System.Runtime.Serialization

[<DataContract(Namespace="urn:org:mir:processing-v1.0")>]
type Task() =      
    let mutable id : string = ""    
    let mutable data : XElement = null
    let mutable tags : string list = []

    [<DataMember(Name = "ID", IsRequired = true, Order = 0)>]
    member x.ID with get() = id and set(v) = id <- v    
    [<DataMember(Name = "Data", IsRequired = false, Order = 1)>]
    member x.Data with get() = data and set(v) = data <- v
    [<DataMember(Name = "Tags", IsRequired = false, Order = 2)>]
    member x.Tags with get() = tags and set(v) = tags <- v

[<ServiceContract(Namespace = "urn:org:mir:processing-v1.0")>]
type TaskProcessing =    
    [<OperationContract(IsOneWay = true)>]
    abstract member Post : t:Task -> unit