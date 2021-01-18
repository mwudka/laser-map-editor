module ShortUUID
open Fable.Core
open Fable.Core.JS

type ShortUUIDExports =
    abstract generate: unit -> string

let [<ImportDefault("short-uuid")>] shortUUID: ShortUUIDExports = jsNative

//// ts2fable 0.7.1
//module rec ShortUUID
//open System
//open Fable.Core
//open Fable.Core.JS
//
//let [<Import("*","short-uuid")>] ``short-uuid``: Short_uuid.IExports = jsNative
//
//module Short_uuid =
//    let [<Import("shortUUID","short-uuid")>] shortUUID: ShortUUID.IExports = jsNative
//
//    type [<AllowNullLiteral>] IExports =
//        abstract shortUUID: ?alphabet: string * ?options: Options -> ShortUUID.Translator
//
//    type [<AllowNullLiteral>] Options =
//        abstract consistentLength: bool option with get, set
//
//    module ShortUUID =
//
//        type [<AllowNullLiteral>] IExports =
//            abstract constants: IExportsConstants
//            /// Generate a new regular UUID.
//            abstract uuid: unit -> string
//            /// Generate a base 58 short uuid
//            abstract generate: unit -> string
//
//        type [<AllowNullLiteral>] Translator =
//            /// The alphabet used for encoding UUIDs.
//            abstract alphabet: string with get, set
//            /// Generate a new short UUID using this translator's alphabet.
//            abstract ``new``: (unit -> string) with get, set
//            /// Generate a new short UUID using this translator's alphabet.
//            abstract generate: (unit -> string) with get, set
//            /// Generate a new regular UUID.
//            abstract uuid: unit -> string
//            /// short -> long
//            abstract toUUID: shortId: string -> string
//            /// long -> short
//            abstract fromUUID: regularUUID: string -> string
//
//        type [<AllowNullLiteral>] IExportsConstants =
//            abstract flickrBase58: string with get, set
//            abstract cookieBase90: string with get, set
