module Storage

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
//    Data : (string * obj) list
//    Handler : string
//    Status : TaskStatus
//    IssuedDate : DateTime        
//    PickedDate : Option<DateTime>
//    StartedDate : Option<DateTime>
//    CompletedDate : Option<DateTime>
//    FailedDate : Option<DateTime>
//    FailedMessage : Option<string>
//}

type TaskStorage() =    

    let s = function
            | Pending -> "Pending"
            | Picked -> "Picked"
            | Started -> "Started"
            | Completed -> "Completed"
            | Failed -> "Failed" 
            
    let tasks (ctx : Context) = ctx.["ProcessingTasks"].["Tasks"]
    
    let dump() =
        printfn "TaskStorage : Dump"

        use ctx = connect()
        let tasks = ctx |> tasks            

        tasks.FindAll().Documents
        |> Seq.map(fun d -> d.ToString())
        |> Seq.iter(printfn "%s")

    let clean() =
        printfn "TaskStorage : Clean"

        use ctx = connect()        
        let tasks = ctx |> tasks
        tasks.Delete(new Document())                    

    
    let pick() : Option<Task> = 
        printfn "TaskStorage : Pick"
        
        // Enter global lock
        // Get most old pending task
        // Mark them as Prepared with date
        // Exit global lock

        use ctx = connect()
        let tasks = ctx |> tasks
        
        use res = tasks.Find(doc [v "Status" (s Pending)])
                       .Sort("IssuedDate")
                       .Limit(1)
                                                 
        let docs = res.Documents |> Seq.toList

        if docs.Length = 0 then
            printfn "TaskStorage : Nothing picked"
            None
        else                       
            printfn "TaskStorage : Something picked"
            let doc = docs.Head
            doc.["Status"] <- (s Picked)
            doc.["PickedDate"] <- DateTime.UtcNow
            tasks.Update(doc)

            Some({ ID = doc.GetID("_id")
                   Handler = doc.GetString("Handler")
                   Data = doc.GetXml("Data") })      

    let post t =
        printfn "TaskStorage : Post"
        use ctx = connect()
        let tasks = ctx |> tasks
        tasks.Insert(doc [v "_id" t.ID
                          v "Handler" t.Handler
                          v "Status" (s Pending)
                          v "IssuedDate" DateTime.UtcNow
                          v "Data" t.Data])

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