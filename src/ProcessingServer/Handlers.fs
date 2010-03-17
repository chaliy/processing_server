module Handlers

open Shared
open ProcessingServer.Contract
open System.Reflection
open System.ComponentModel.Composition.Hosting

type HandlerCatalog() =
    
    let resolveAll() = 
        let directoryCatalog = new DirectoryCatalog(".")
        let assemblyCatalog = new AssemblyCatalog(Assembly.GetExecutingAssembly())
        let aggregateCatalog = new AggregateCatalog()
        aggregateCatalog.Catalogs.Add(directoryCatalog)
        aggregateCatalog.Catalogs.Add(assemblyCatalog) 
        let container = new CompositionContainer(aggregateCatalog)
        container.GetExportedValues<ITaskHandler>()        

    member x.ResolveAll = resolveAll