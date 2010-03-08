module Model

open Shared

type Task = {
    ID : ID
    Data : (string * obj) list
    Handler : string
}
