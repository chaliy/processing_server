open Shared
open Model
open Storage
open Processing
open Services
open Handlers
open System
open System.Threading

// *******************************
// *** Wire-up app ***
// *******************************

let tracing = Tracing()
let storage = TaskStorage(tracing)


// **********************
// *** And Console UI ***
// **********************
let mutable inloop = true
while inloop do
    let inp = System.Console.ReadLine()
    match inp with
    | "Dump" -> storage.Dump()    
    | "Clean" -> storage.Clean()
    | _ -> printfn "Invalid command, try again"