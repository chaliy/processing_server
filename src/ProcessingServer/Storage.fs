﻿module Storage

open Shared
open Model

open System
open MongoDB
open MongoDB.Driver

type TaskStatus =
| Pending
| Picked
| Started
| Completed
| Failed

// For reference purposes
//type TaskRecord = {
//    ID : ID
//    Data : XElement
//    Tags : string list
//    Status : TaskStatus
//    IssuedDate : DateTime        
//    PickedDate : Option<DateTime>
//    StartedDate : Option<DateTime>
//    CompletedDate : Option<DateTime>
//    FailedDate : Option<DateTime>
//    FailedMessage : Option<string>
//}

type OverallStats = {
    Pending : int
    Running : int
    Completed : int
    Failed : int                
}

type TaskStorage(tracing : Tracing) =    

    let s = function
            | Pending -> "Pending"
            | Picked -> "Picked"
            | Started -> "Started"
            | Completed -> "Completed"
            | Failed -> "Failed" 
            
    let tasks (ctx : Context) = ctx.["ProcessingTasks"].["Tasks"]
    
    let dump() =
        tracing.Trace "TaskStorage : Dump"

        use ctx = connect()
        let tasks = ctx |> tasks

        tasks.FindAll().Documents
        |> Seq.map(fun d -> d.ToString())
        |> Seq.iter(tracing.Trace)

    let clean() =
        tracing.Trace "TaskStorage : Clean"

        use ctx = connect()        
        let tasks = ctx |> tasks
        tasks.Delete(new Document())                    

    let pick(limit) : Task list = 
        tracing.Trace "TaskStorage : Pick"
                
        use ctx = connect()
        let tasks = ctx |> tasks
        
        use res = tasks.Find(doc [v "Status" (s Pending)])
                       .Sort("IssuedDate")
                       .Limit(limit)
                                                 
        res.Documents 
        |> Seq.toList
        |> List.map(fun doc ->
                    // Update status
                    doc.["Status"] <- (s Picked)
                    doc.["PickedDate"] <- DateTime.UtcNow
                    tasks.Update(doc)

                    { ID = doc.GetID("_id")      
                      Data = doc.GetXml("Data")
                      Tags = doc.GetStringList("Tags") } )
    
    let post t =
        tracing.Trace "TaskStorage : Post"
        use ctx = connect()
        let tasks = ctx |> tasks
        tasks.Insert(doc [v "_id" t.ID                          
                          v "Status" (s Pending)
                          v "IssuedDate" DateTime.UtcNow
                          v "Data" t.Data
                          v "Tags" t.Tags ])

    let overallStats(tags:string list) =
        // TODO update this to use MongoDB group function
        use ctx = connect()     
        let tasks = ctx |> tasks        
        let map = 
            "function(){\n" +
            "   emit( this.Status , 1 );\n" +            
            "};";
        let reduce = 
            "function( key , values ){\n" +            
            "    var total = 0;\n" +
            "    for ( var i = 0; i < values.length; i++ )\n" +
            "        total += values[i];\n" +
            "    return total;\n" +
            "};";

        let mr = tasks.MapReduce()        
        mr.Map <- Code(map)
        mr.Reduce <- Code(reduce) 
        if tags.Length > 0 then       
            mr.Query <- doc [v "Tags" tags ]
        mr.Execute() |> ignore
        let rr = 
            mr.Documents |> Seq.toList

        let results = 
            mr.Documents
            |> Seq.map(fun d -> (d.GetString("_id"), d.GetInteger("value")))

        let resultFor statuses =
            results 
            |> Seq.filter(fun (st, value) -> statuses |> Seq.exists(fun x -> st = (s x)))
            |> Seq.sumBy(fun (st, value) -> value)

        {
            Pending = resultFor [Pending]
            Running = resultFor [Picked; Started]
            Completed = resultFor [Completed]
            Failed = resultFor [Failed]
        }        
        

    let byId id = 
        doc [v "_id" id]
                  
    let mark id upd =
        use ctx = connect()
        let tasks = ctx |> tasks
        let doc = tasks.FindOne(byId id)
        
        upd |> Seq.iter(fun (k, v) -> doc.[k] <- v)        
                
        tasks.Update(doc)

    member x.MarkStarted id = mark id [v "Status" (s Started)
                                       v "StartedDate" DateTime.UtcNow]

    member x.MarkSuccess id = mark id [v "Status" (s Completed)
                                       v "CompletedDate" DateTime.UtcNow]

    member x.MarkFailed id (ex : Exception) = mark id [v "Status" (s Failed)
                                                       v "FailedDate" DateTime.UtcNow
                                                       v "FailedMessage" ex.Message]
    
    member x.Pick = pick    
    member x.Post = post
    member x.Dump = dump
    member x.Clean = clean
    member x.OverallStats = overallStats