module App.PNG

open Fable.Core
open Fable.Core.JS
open JsInterop

type PNGChunk =
    abstract name: string with get, set
    abstract data: Uint8Array with get, set

[<ImportDefault("png-chunks-extract")>]
let extract(buffer: U2<seq<uint8>, Uint8Array>): seq<PNGChunk> = jsNative

[<ImportDefault("png-chunks-encode")>]
let encode(chunks: seq<PNGChunk>): seq<uint8> = jsNative

type TextEncoder =
    abstract encode: string -> Uint8Array
type TextDecoder =
    abstract decode: Uint8Array -> string

[<Emit("new TextDecoder($0)")>]
let createTextDecoder (encoding: string): TextDecoder = jsNative

[<Emit("new TextEncoder($0)")>]
let createTextEncoder (encoding: string): TextEncoder = jsNative


[<Emit("new Uint8Array($0)")>]
let createUint8Array (arg: U2<ArrayBuffer, int>): Uint8Array = jsNative


let text(key: string, value: string): PNGChunk =
    // TODO: Validate key len <= 79
    // TODO: Validate key doesn't contain null bytes
    
    let textEncoder = createTextEncoder("utf-8")
    let key = textEncoder.encode(key)// key.ToCharArray() |> Seq.map (uint8) |> Seq.toArray
    let value = textEncoder.encode(value)//value.ToCharArray() |> Seq.map (uint8) |> Seq.toArray
    
    
    let data: uint8 [] = [|
        // keyword, appended above
        uint8(0) // null after keyword
        uint8(0) // compression flag, 0=uncompressed
        uint8(0) // compression method
        // language tag (omitted)
        uint8(0) // null after language tag
        // translated keyword (omitted)
        uint8(0) // null after translated keyword
        // text, appended below        
    |]
    
    let finalArray = createUint8Array(!^(key.length + data.Length + value.length))
    finalArray.set(key) |> ignore
    finalArray.set(data, key.length) |> ignore
    finalArray.set(value, key.length + data.Length) |> ignore
    
    jsOptions<PNGChunk>(fun o ->
        o.name <- "iTXt"
        o.data <- finalArray
        )

