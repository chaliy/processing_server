﻿module Shared

open System.Threading
 
type ID = string
type Agent<'T> = MailboxProcessor<'T>

[<AutoOpen>]
module SyncContextHelpers =
    let internal current() = 
        match SynchronizationContext.Current with 
        | null -> new SynchronizationContext()
        | ctxt -> ctxt

    let raise (event: Event<_>) args =
        current().Post((fun _ -> event.Trigger args), state = null) 

//type SynchronizationContext with 
//
//    /// A standard helper extension method to raise an event on the GUI thread
//    member syncContext.RaiseEvent (event: Event<_>) args = 
//        //let mutable syncContext : SynchronizationContext = null
//        syncContext.Post((fun _ -> event.Trigger args), state = null)
//
// 
//    /// A standard helper extension method to capture the current synchronization context.
//    /// If none is present, use a context that executes work in the thread pool.
//    static member CaptureCurrent () = 
//        match SynchronizationContext.Current with 
//        | null -> new SynchronizationContext()
//        | ctxt -> ctxt


let v k d = (k, d :> obj)