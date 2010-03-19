open Shared
open Model
open Storage
open Processing
open Services
open Handlers
open System
open System.Threading

// *******************************
// *** Wire-up processing part ***
// *******************************

let tracing = Tracing()
let storage = TaskStorage(tracing)
let handlerCatalog = HandlerCatalog()
let handlers = handlerCatalog.ResolveAll()
let processingAgent = ProcessingAgent(storage, handlers, tracing)

// First ping, process all stuff and so on...
processingAgent.Start()

// *******************************
// ***  Wire-up listening part ***
// *******************************

let servceAgent = ServiceAgent();
servceAgent.Posted.Add(storage.Post)
// Notifiy ptocessor about potential tasks.
servceAgent.Posted.Add(fun _ -> processingAgent.Ping())
servceAgent.Start()

// **********************
// *** And Console UI ***
// **********************

while true do
    let inp = System.Console.ReadLine()
    match inp with
    | "Dump" -> storage.Dump()
    | "Ping" -> processingAgent.Ping()
    | "Clean" -> storage.Clean()
    | x ->
        let id = Guid.NewGuid().ToString()
        let data = System.Xml.Linq.XElement.Parse("<root />")
        storage.Post({ ID = id                       
                       Data = data } )
        processingAgent.Ping()