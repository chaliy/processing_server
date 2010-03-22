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

let args = System.Environment.GetCommandLineArgs()
let command = 
    if args.Length = 1 then
        printfn "And what do you want?"
        System.Console.ReadLine()
    else
        args.[1]

match command.ToLower() with
| "dump" -> storage.Dump()    
| "clean" -> storage.Clean()
//| "stats" -> 
//            let stats = ProcessingServer.Client.TaskProcessingStatsClient("http://localhost:1066").QueryOverallStats([||])
//            printfn "Running: %i; Pending: %i; Completed: %i; Failed: %i" 
//                stats.Running stats.Pending stats.Completed stats.Failed
| "stats" -> 
            let stats = storage.OverallStats([])
            printfn "Pending: %i; Running: %i; Completed: %i; Failed: %i" 
                stats.Pending stats.Running stats.Completed stats.Failed
| _ -> printfn "Invalid command, try next time!"