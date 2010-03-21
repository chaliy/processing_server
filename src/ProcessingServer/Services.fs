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
    let baseAddress = "http://localhost:1066/"
    let CreateHost<'a>(url:string, impl:'a) =
        let host = new ServiceHost(impl, Uri(baseAddress + url))
        host.AddServiceEndpoint(typeof<'a>, new WSHttpBinding(SecurityMode.Message), "") |> ignore
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

    let host = ServiceUtils.CreateHost<TaskProcessing>("TaskProcessing", TaskProcessingWrapper(service))
        
    member x.Start() = host.Open()
    member x.Posted = posted.Publish

[<ServiceBehavior(InstanceContextMode = InstanceContextMode.Single)>]
type TaskProcessingStatsService(storage : TaskStorage) =

    let queryOveralStats spec =
        let stats = new OveralStats()
        stats.Runnig <- 1
        stats.Completed <- 100
        stats

    interface TaskProcessingStats with
        member gms.QueryOveralStats(spec) = queryOveralStats(spec)

type StatsServiceAgent(storage : TaskStorage) =

    let service = TaskProcessingStatsService(storage) 

    let host = ServiceUtils.CreateHost<TaskProcessingStats>("TaskProcessingStats", service)
        
    member x.Start() = host.Open()    