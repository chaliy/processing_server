module ProcessingServer.Client

open ProcessingServer.Contract

open System.IO
open System.Xml
open System.Xml.Linq
open System.Runtime.Serialization
open System.ServiceModel

module ServiceUtils =

    let Execute<'a, 'r> (url : string) (block : 'a -> 'r) =
        use factory = new ChannelFactory<'a>(WSHttpBinding())
        let channel = factory.CreateChannel(EndpointAddress(url))    
        block(channel)
                   

type TaskProcessingClient() =    
    
    let serialize msg = 
        use mem =  new MemoryStream()
        let ser = new DataContractSerializer(msg.GetType())
        ser.WriteObject(mem, msg)
        mem.Position <- int64 0
        XElement.Load(new XmlTextReader(mem))

    let post(msg, (tags : string array)) =
        let id = System.Guid.NewGuid().ToString()
        let task = Task()
        task.ID <- id
        task.Data <- serialize msg
        task.Tags <- tags |> Array.toList

        ServiceUtils.Execute<TaskProcessing, unit> 
            "http://localhost:1066/TaskProcessing/"
            (fun channel -> channel.Post(task))

        id
        
    member x.Post = post

type TaskProcessingStatsClient() =
    
    let queryOveralStats (tags : string array) =        
        let spec = OveralStatsQuerySpec()
        spec.Tags <- tags |> Array.toList

        ServiceUtils.Execute<TaskProcessingStats, OveralStats> 
            "http://localhost:1066/TaskProcessingStats/"
            (fun channel -> channel.QueryOveralStats(spec))
                    
    member x.QueryOveralStats = queryOveralStats

// ********************
// ***  Public API  ***
// ********************
let Post(msg, [<System.ParamArray>] tags) = 
    TaskProcessingClient().Post(msg, tags)

let QueryOveralStats([<System.ParamArray>] tags) = 
    TaskProcessingStatsClient().QueryOveralStats(tags)