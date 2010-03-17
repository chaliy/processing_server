namespace ProcessingServer.Contract

type HandlerContext = {    
    Data : System.Xml.Linq.XElement
}

type ITaskHandler =
    abstract member CanHandle : HandlerContext -> bool
    abstract member Handle : HandlerContext -> unit   