module Shared

open System.Threading
open System.Threading.Tasks

let thread run = 
    new Task(new System.Action(fun () -> run()), TaskCreationOptions.PreferFairness)
 
type ID = string
type Agent<'a>(run : 'a -> unit) =
    let agent = new MailboxProcessor<'a>(fun inbox -> async { 
                                            while true do
                                                let! x = inbox.Receive()    
                                                run(x)
                                        })
   
    member x.Start() = agent.Start()
    member x.Post = agent.Post
    member x.QueueLength = agent.CurrentQueueLength
    
    interface System.IDisposable with
        member x.Dispose() = (agent :> System.IDisposable).Dispose()      

let StartAgent<'a> (run : 'a -> unit) = 
    let a = new Agent<'a>(run)
    a.Start()
    a

module SyncContext =
    let Current() = 
        match SynchronizationContext.Current with 
        | null -> new SynchronizationContext()
        | ctxt -> ctxt

type SynchronizationContext with 
    
    member sync.Raise (event: Event<_>) args =
        sync.Post((fun _ -> event.Trigger args), state = null)

let v k d = (k, d :> obj)

type Tracing() =
                   
    let agent = StartAgent(fun msg -> printfn "%s" msg )     

    member x.Trace(msg) = 
        
        agent.Post (msg
            + "; (th: " + Thread.CurrentThread.ManagedThreadId.ToString() + "" 
            + "; d: " + System.DateTime.UtcNow.ToShortTimeString() + ")")


type Trottler(interval) =
    let pushed = new Event<_>()
    let sync = SyncContext.Current()

    let mutable lastPush = System.DateTime.UtcNow
    let mutable pending = false    

    let stateLock = new obj()

    let now() = System.DateTime.UtcNow
        
    let push(now) =    
        lock stateLock (fun _ ->    
                            lastPush <- now
                            pending <- false )
        sync.Raise pushed now

    let checkPending() =
        let now = now()
        if pending && now - lastPush > interval then push(now)
    
    let schedulePush() =   
        let now = now()
        if now - lastPush > interval then push(now)            
        else lock stateLock (fun _ -> pending <- true )
             

    let timer = new System.Threading.Timer((fun _ -> checkPending()),
                     null, dueTime = System.TimeSpan.Zero, period = interval)

    member x.Ping = schedulePush
    member x.Pushed = pushed.Publish
                                