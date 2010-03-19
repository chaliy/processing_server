namespace ProcessingServer.Handling

type IHandlerContext =
    abstract member Data : System.Xml.Linq.XElement
    abstract member Trace : string -> unit

type ITaskHandler =
    abstract member CanHandle : IHandlerContext -> bool
    abstract member Handle : IHandlerContext -> unit   