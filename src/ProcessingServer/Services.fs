module Services

open System
open System.Collections.Generic
open System.ServiceModel
open System.Xml.Linq

open Shared
open Model
open Storage

open ProcessingServer.Contract


// **********************
// *** Implementation ***
// **********************

module ServiceUtils =
    let baseAddress = Uri("http://localhost:1066/")
    let CreateHost<'a>(url:string, impl:'a) =
        let host = new ServiceHost(impl, baseAddress)
        host.AddServiceEndpoint(typeof<'a>, new WSHttpBinding(SecurityMode.Message), url) |> ignore
        host

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

    let host = ServiceUtils.CreateHost("TaskProcessing", TaskProcessingWrapper(service))
        
    member x.Start() = host.Open()
    member x.Posted = posted.Publish

[<ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)>]
type TaskProcessingStatsWrapper(service : TaskProcessingStats) =
    interface TaskProcessingStats with
        member gms.QueryOveralStats(spec) = service.QueryOveralStats(spec)

type StatsServiceAgent(storage : TaskStorage) =

    let queryOveralStats spec =
        let stats = new OveralStats()
        stats.Runnig <- 1
        stats.Completed <- 100
        stats
    
    let service = { new TaskProcessingStats with
                        member gms.QueryOveralStats(spec) = queryOveralStats spec }      

    let host = ServiceUtils.CreateHost("TaskProcessingStats", TaskProcessingStatsWrapper(service))
        
    member x.Start() = host.Open()    