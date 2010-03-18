module Model

open Shared
open System.Xml.Linq

type Task = {
    ID : ID
    Data : XElement    
}
