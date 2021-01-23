module App.Config
open Fable.Core

[<Emit("process.env.GOOGLE_FONTS_API_KEY")>]
let GOOGLE_FONTS_API_KEY: string = jsNative

[<Emit("process.env.MAPBOX_API_KEY")>]
let MAPBOX_API_KEY: string = jsNative