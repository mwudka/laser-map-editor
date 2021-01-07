module rec App.URL
open Fable.Core
let [<Emit("URL")>] URL: URLStatic = jsNative

type URLStatic =
    abstract revokeObjectURL: string -> unit