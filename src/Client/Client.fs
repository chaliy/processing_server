module ProcessingServer.Client

open ProcessingServer.Contract

open System.IO
open System.Xml
open System.Xml.Linq
open System.Runtime.Serialization
open System.ServiceModel

let Execute<'a, 'r> (server : string) url (block : 'a -> 'r) =                
    let builder = System.UriBuilder(server)
    builder.Path <- url    
    use factory = new ChannelFactory<'a>(WSHttpBinding())
    let channel = factory.CreateChannel(EndpointAddress(builder.Uri))    
    block(channel)        
                   
type TaskProcessingClient(server : string) =    
    
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

        Execute<TaskProcessing, _> 
            server "TaskProcessing/" (fun ch -> ch.Post(task))

        id
        
    member x.Post(msg, [<System.ParamArray>] tags) = post(msg, tags)

type TaskProcessingStatsClient(server : string) =
    
    let queryOverallStats (tags : string array) =        
        let spec = OverallStatsQuerySpec()
        spec.Tags <- tags |> Array.toList

        Execute<TaskProcessingStats, _> 
            server "TaskProcessingStats/" (fun ch -> ch.QueryOverallStats(spec))
                    
    member x.QueryOverallStats([<System.ParamArray>] tags) = queryOverallStats(tags)