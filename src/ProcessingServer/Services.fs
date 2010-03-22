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

    let queryOverallStats spec =        
        let stats = storage.OverallStats()
        let res = new OverallStats()
        res.Pending <- stats.Pending
        res.Running <- stats.Running
        res.Completed <- stats.Completed
        res.Failed <- stats.Failed
        res

    interface TaskProcessingStats with
        member gms.QueryOverallStats(spec) = queryOverallStats(spec)

type StatsServiceAgent(storage : TaskStorage) =

    let service = TaskProcessingStatsService(storage) 

    let host = ServiceUtils.CreateHost<TaskProcessingStats>("TaskProcessingStats", service)
        
    member x.Start() = host.Open()    