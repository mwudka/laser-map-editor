module App.PNG

open Fable.Core
open Fable.Core.JS

type PNGChunk =
    abstract name: string
    abstract data: Uint8Array

[<ImportDefault("png-chunks-extract")>]
let extract(buffer: U2<seq<uint8>, Uint8Array>): seq<PNGChunk> = jsNative

[<ImportDefault("png-chunks-encode")>]
let encode(chunks: seq<PNGChunk>): seq<uint8> = jsNative

[<Import("encode", from="png-chunk-text")>]
let text(key: string, value: string): PNGChunk = jsNative

