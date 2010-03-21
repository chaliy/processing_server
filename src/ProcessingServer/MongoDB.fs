module MongoDB
open Shared
open MongoDB.Driver
open System.Xml.Linq

type Context =    
    abstract member Item : string -> MongoDB.Driver.Database with get    
    inherit System.IDisposable

let connect() =
    let mongo = new Mongo()    
    if not (mongo.Connect()) then
        failwith "Cannot connect"
 
    { new Context with
        member this.get_Item(name) = mongo.[name]
        member this.Dispose() = ignore (mongo.Disconnect()) }

let rec doc (inp) =

    let prep (x : obj) =
        match x with
        | :? ((string * obj) seq) as xx -> doc(xx) :> obj
        | :? XElement as xx -> xx.ToString() :> obj        
        | x -> x

    let addend (d:Document) (k, v) = d.Append(k, prep(v))
    let document = Document()
    
    inp |> Seq.fold addend document

let values (doc : Document) =
    doc.Keys 
    |> Seq.cast<string>
    |> Seq.toList
    |> List.map(fun k -> (k, doc.[k]))

type MongoDB.Driver.Document with       
      member x.GetString key = x.[key] :?> string
      member x.GetID key = match x.[key] with
                           | :? ID as x -> x      
                           | :? MongoDB.Driver.Oid as x -> x.ToString()
                           | _ -> failwith "Cannot get ID from given value"
      member x.GetData key = (x.[key] :?> Document) |> values
      member x.GetXml key = match x.[key] with
                            | :? string as x -> XElement.Parse(x)
                            | _ -> failwith "Cannot get XML from given value"
      member x.GetStringList key = match x.[key] with
                                   | :? (string array) as x -> x |> Array.toList
                                   | null -> []
                                   | _ -> failwith "Cannot get string list from given value"