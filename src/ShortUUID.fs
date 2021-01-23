module ShortUUID
open Fable.Core
open Fable.Core.JS

type ShortUUIDExports =
    abstract generate: unit -> string

let [<ImportDefault("short-uuid")>] shortUUID: ShortUUIDExports = jsNative
