module Processing

open Shared
open Model
open Storage
open ProcessingServer.Handling

open System
open System.Threading
open System.Xml.Linq
open System.Diagnostics

type Executable = System.Threading.Tasks.Task

type ProcessingAgent(storage : TaskStorage,
                     handlers : ITaskHandler list,
                     tracing : Tracing) =
    
    let mutable runnig = 0
    let mutable stopping = false
    
    let storageLock = obj()
    let statsLock = obj()        
    let sync = SyncContext.Current()
    let pingTrottler = new Trottler(TimeSpan.FromMilliseconds(500.0))
    let all = System.Collections.Generic.List<Executable>()

    let trace msg =
        tracing.Trace msg

    let createContext (t : Task) = 
        { new IHandlerContext with
            member x.Data = t.Data
            member x.Trace(msg) = trace(msg) }

    let handleFailure id ex =
        trace (sprintf "ProcessingAgent: Task failed %s" id)
        lock statsLock (fun () -> runnig <- runnig - 1 ) // Stats
        storage.MarkFailed id ex
        pingTrottler.Ping()
        

    let resolveHandler task ctx = 
        let applicable = handlers 
                         |> List.filter(fun h -> h.CanHandle(ctx)) 

        match applicable.Length with
        | 0 -> 
            handleFailure task.ID (System.Exception("Handler was not found.")) 
            None  
        | 1 -> Some(applicable |> Seq.head)
        | _ -> 
            trace (sprintf "ProcessingAgent: More then one handler found for task %s. Executing first." task.ID)
            Some(applicable |> Seq.head)
    
    let wrap task =                        
        let ctx = createContext task
        let handler = resolveHandler task ctx        

        match handler with
        | Some h ->
                Some(new Executable(new System.Action(fun () -> 

                        let id = task.ID                                    
                        trace (sprintf "ProcessingAgent: Task started %s" id)
                        lock statsLock (fun () -> runnig <- runnig + 1 ) // Stats
                        storage.MarkStarted id
                        let sw = new Stopwatch()
                        sw.Start()
                                        
                        try
                            h.Handle(ctx)
                            
                            sw.Stop() |> ignore
                            trace (sprintf "ProcessingAgent: Task success %s (Time: %i ms)" id sw.ElapsedMilliseconds)
                            lock statsLock (fun () -> runnig <- runnig - 1 ) // Stats
                            storage.MarkSuccess id
                            pingTrottler.Ping()
                        with
                        | x -> handleFailure task.ID x                            
                    ), System.Threading.Tasks.TaskCreationOptions.PreferFairness )  )
        | None -> None        

    let pickTasks() =
        trace "ProcessingAgent: Ping"
        if (runnig < 10 && (not stopping)) then
            let tasks = lock storageLock (fun () -> storage.Pick(10))
            trace (sprintf "ProcessingAgent: Tasks recieved %i" tasks.Length)            
                  
            tasks 
            |> Seq.map(wrap)            
            |> Seq.iter(fun t -> match t with
                                 | Some t -> 
                                            all.Add(t)
                                            t.Start() 
                                 | None -> ())

    let start() =
        pingTrottler.Pushed.Add(fun _ -> pickTasks())
        pingTrottler.Ping()

    let stop() =
        trace "ProcessingAgent: Stop"        
        stopping <- true        
        Executable.WaitAll(all.ToArray())
        trace "ProcessingAgent: Stoped!"
        

    member x.Ping = pingTrottler.Ping
    member x.Start = start    
    member x.Stop = stop