namespace ProcessingServer.Contract

type HandlerContext = {
    Batch : string
    Data : System.Xml.Linq.XElement
}

type ITaskHandler =
    abstract member CanHandle : HandlerContext -> bool
    abstract member Handle : HandlerContext -> unit   