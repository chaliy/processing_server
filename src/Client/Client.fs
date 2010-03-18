module ProcessingServer.Client

open ProcessingServer.Contract

open System.IO
open System.Xml
open System.Xml.Linq
open System.Runtime.Serialization
open System.ServiceModel

type MessagePoster() =

    let factory = new ChannelFactory<TaskProcessing>(WSHttpBinding())
    let channel = factory.CreateChannel(EndpointAddress("http://localhost:1066/"))

    let serialize msg = 
        use mem =  new MemoryStream()
        let ser = new DataContractSerializer(msg.GetType())
        ser.WriteObject(mem, msg)
        mem.Position <- int64 0
        XElement.Load(new XmlTextReader(mem))

    let post msg =
        let id = System.Guid.NewGuid().ToString()
        let task = Task()
        task.ID <- id
        task.Data <- serialize msg
        channel.Post(task)
        id
        
    member x.Post = post

let Post msg = MessagePoster().Post(msg)

