// ts2fable 0.7.1
module rec FontPicker

open Fable.Core
open Fable.Core.JS

type Font =
    abstract family: string
    abstract files: obj option

type FontList = obj

[<ImportDefault("font-picker")>]
let FontPicker: FontPickerStatic = jsNative

type FontPickerOptions =
    abstract limit: int with get, set

[<AllowNullLiteral>]
type FontPicker =
    abstract getFonts: unit -> FontList
    abstract addFont: fontFamily:string * ?index:float -> unit
    abstract removeFont: fontFamily:string -> unit
    abstract getActiveFont: unit -> Font
    abstract setActiveFont: fontFamily:string -> unit
    abstract setOnChange: onChange:(Font -> unit) -> unit



[<AllowNullLiteral>]
type FontPickerStatic =
    [<Emit "new $0($1...)">]
    abstract Create: apiKey:string
                     * defaultFamily:string
                     * options:FontPickerOptions
                     * ?onChange:(Font -> unit)
                     -> FontPicker

type FontFace =
    abstract load: unit -> Promise<FontFace>
    abstract family: string

[<Emit("new FontFace($0...)")>]
let createFontFace (family: string, source: string): FontFace = jsNative
