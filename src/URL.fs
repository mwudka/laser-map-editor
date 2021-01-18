module rec App.URL
open Browser
open Fable.Core
let [<Emit("URL")>] URL: URLStatic = jsNative

type URLStatic =
    abstract revokeObjectURL: string -> unit
    
    abstract createObjectURL: obj -> string