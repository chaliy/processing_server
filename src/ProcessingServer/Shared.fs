module Shared

open System.Threading
 
type ID = string
type Agent<'T>(run : 'T -> unit) =
    let agent = new MailboxProcessor<'T>(fun inbox -> async { 
                                            while true do
                                                let! x = inbox.Receive()    
                                                run(x)
                                        })

    member x.Start() = agent.Start()
    member x.Post = agent.Post
    
    interface System.IDisposable with
        member x.Dispose() = (agent :> System.IDisposable).Dispose()  

module SyncContext =
    let Current() = 
        match SynchronizationContext.Current with 
        | null -> new SynchronizationContext()
        | ctxt -> ctxt

type SynchronizationContext with 
    
    member sync.Raise (event: Event<_>) args = 
        //let mutable syncContext : SynchronizationContext = null
        sync.Post((fun _ -> event.Trigger args), state = null)

let v k d = (k, d :> obj)