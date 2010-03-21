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

[<DataContract(Namespace="urn:org:mir:processing-v1.0")>]
type OveralStats() =      
    let mutable completed : int = 0
    let mutable runnig : int = 0
    let mutable pending : int = 0
    let mutable failed : int = 0        

    [<DataMember(Name = "Completed", IsRequired = true, Order = 0)>]
    member x.Completed with get() = completed and set(v) = completed <- v
    [<DataMember(Name = "Runnig", IsRequired = true, Order = 1)>]
    member x.Runnig with get() = runnig and set(v) = runnig <- v
    [<DataMember(Name = "Pending", IsRequired = true, Order = 2)>]
    member x.Pending with get() = pending and set(v) = pending <- v
    [<DataMember(Name = "Failed", IsRequired = true, Order = 3)>]
    member x.Failed with get() = failed and set(v) = failed <- v

[<DataContract(Namespace="urn:org:mir:processing-v1.0")>]
type OveralStatsQuerySpec() =              
    let mutable tags : string list = []
    
    [<DataMember(Name = "Tags", IsRequired = false, Order = 0)>]
    member x.Tags with get() = tags and set(v) = tags <- v

[<ServiceContract(Namespace = "urn:org:mir:processing-v1.0")>]
type TaskProcessingStats =        
    [<OperationContract>]
    abstract member QueryOveralStats : spec:OveralStatsQuerySpec -> OveralStats