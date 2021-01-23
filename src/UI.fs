module App.UI

open App.Geo
open Browser.Types
open Fable.Core
open Fable.Import
open Mapbox.Mapboxgl
open FSharp.Data.UnitSystems.SI.UnitNames
open JsInterop
open FontPicker


let document = Browser.Dom.document
let console = Browser.Dom.console

let createElement (tagName: string): 'T = !! document.createElement (tagName)

let getFirstElementByTagName (tagName: string): 'T =
    !!document.getElementsByTagName(tagName).[0]

let potentialPOIButton (clickedLocation: LngLat) createFeature (feature: MapboxGeoJSONFeature) =
    let link: HTMLAnchorElement = createElement ("a")

    let newFeatureLocation =
        defaultArg (extractPoint (feature)) clickedLocation

    link.onclick <- fun _ -> createFeature newFeatureLocation feature
    link.setAttribute ("href", "#")
    link.addEventListener ("click", (fun e -> e.preventDefault ()))
    link.innerText <- extractName feature
    let li = document.createElement ("li")
    li.appendChild (link) |> ignore
    li

let poiSelector createFeature
                (clickedLocation: LngLat)
                (nearbyFeatures: seq<MapboxGeoJSONFeature>)
                (clickedFeatures: seq<MapboxGeoJSONFeature>)
                =
    let ul = document.createElement ("ul")
    ul.className <- "poi-selector"

    let distanceToClickedLocation feature =
        match extractPoint feature with
        | Some (ll) -> clickedLocation.distanceTo (ll)
        | None -> 0.0<meter>

    Seq.concat ([ clickedFeatures; nearbyFeatures ])
    |> Seq.sortBy distanceToClickedLocation
    |> Seq.map (potentialPOIButton clickedLocation createFeature)
    |> Seq.iter (fun f -> ul.appendChild f |> ignore)

    ul

let labelElement string input =
    let labelEl: HTMLLabelElement = createElement ("label")
    labelEl.innerText <- string
    labelEl.appendChild (input) |> ignore
    labelEl

let poiEditor (container: HTMLElement)
              (feature: LaserEditorFeature)
              (performUpdate: (string -> string -> int -> FontInfo option -> int -> unit))
              (deleteFeature: (LaserEditorFeature -> unit))
              =
    let form: HTMLFormElement = createElement ("form")
    container.appendChild (form) |> ignore

    let textEl: HTMLInputElement = createElement ("input")
    textEl.name <- "text-content"
    textEl.value <- feature.properties.textContent

    form.appendChild (labelElement "Text" textEl)
    |> ignore

    let rotationEl: HTMLInputElement = createElement ("input")
    rotationEl.name <- "rotation"
    rotationEl.value <- string (feature.properties.rotation)
    rotationEl.``type`` <- "number"
    rotationEl.min <- "0"
    rotationEl.max <- "360"

    form.appendChild (labelElement "Rotation" rotationEl)
    |> ignore

    let fontFaceEl: HTMLDivElement = createElement ("div")
    fontFaceEl.id <- "font-picker"

    form.appendChild (labelElement "Font Face" fontFaceEl)
    |> ignore

    let fontSizeEl: HTMLInputElement = createElement ("input")
    fontSizeEl.name <- "fontSize"
    fontSizeEl.value <- string (feature.properties.fontSize)
    fontSizeEl.``type`` <- "number"
    fontSizeEl.min <- "1"
    fontSizeEl.max <- "70"

    form.appendChild (labelElement "Font Size" fontSizeEl)
    |> ignore

    let submit: HTMLInputElement = createElement ("input")
    submit.``type`` <- "submit"
    form.appendChild (submit) |> ignore

    let delete: HTMLButtonElement = createElement ("button")
    delete.innerText <- "Delete"

    delete.addEventListener
        ("click",
         (fun e ->
             e.preventDefault ()
             deleteFeature (feature)))

    form.appendChild (delete) |> ignore

    let fontPicker =
        FontPicker.Create
            (Config.GOOGLE_FONTS_API_KEY,
             feature.properties.font
             |> Option.map (fun f -> f.Family)
             |> Option.defaultValue ("Open Sans"),
             jsOptions<FontPickerOptions> (fun o -> o.limit <- 250))

    feature.properties.font
    |> Option.map (fun f -> fontPicker.setActiveFont f.Family)
    |> ignore

    form.onsubmit <-
        (fun (e: Event) ->
            e.preventDefault ()
            let activeFont = fontPicker.getActiveFont ()
            console.log ("selected font", activeFont)

            let fontURL: string option =
                activeFont.files
                |> Option.map (fun f -> f?regular)

            let fontInfo: JS.Promise<FontInfo option> =
                match fontURL with
                | None -> JS.Constructors.Promise.resolve (None)
                | Some (fontURL) ->
                    let fontSource = sprintf "url(%s)" fontURL

                    createFontFace(activeFont.family, fontSource)
                        .load()
                        .``then``(function
                        | f ->
                            document?fonts?add (f)

                            Some
                                ({ Family = activeFont.family
                                   URL = fontURL }))

            fontInfo.``then`` (fun p ->
                performUpdate feature.properties.id textEl.value (int rotationEl.value) p (int fontSizeEl.value))
            |> ignore

            )
