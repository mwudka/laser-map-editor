module App.UI

open Browser.Types
open Fable.Core
open Fable.Import
open Mapbox.Mapboxgl
open FSharp.Data.UnitSystems.SI.UnitNames
open JsInterop


let document = Browser.Dom.document
let console = Browser.Dom.console

let createElement (tagName: string): 'T =
    !!document.createElement(tagName)

let getFirstElementByTagName (tagName: string): 'T = !!document.getElementsByTagName(tagName).[0]

let potentialPOIButton (clickedLocation: LngLat) createFeature (feature: MapboxGeoJSONFeature) =
    let link: HTMLAnchorElement = createElement ("a")

    let newFeatureLocation =
        defaultArg (Geo.extractPoint (feature)) clickedLocation

    link.onclick <- fun _ -> createFeature newFeatureLocation feature
    link.setAttribute ("href", "#")
    link.innerText <- Geo.extractName feature
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
        match Geo.extractPoint feature with
        | Some (ll) -> clickedLocation.distanceTo (ll)
        | None -> 0.0<meter>

    Seq.concat ([ clickedFeatures; nearbyFeatures ])
    |> Seq.sortBy distanceToClickedLocation
    |> Seq.map (potentialPOIButton clickedLocation createFeature)
    |> Seq.iter (fun f -> ul.appendChild f |> ignore)

    ul

let poiEditor (feature: Geo.LaserEditorFeature) (performUpdate: (obj -> string -> int -> unit)) =
    let form: HTMLFormElement = createElement ("form")


    let textEl: HTMLInputElement = createElement ("input")
    textEl.name <- "text-content"
    textEl.value <- feature.properties.textContent
    form.appendChild (textEl) |> ignore

    let rotationEl: HTMLInputElement = createElement ("input")
    rotationEl.name <- "rotation"
    rotationEl.value <- string(feature.properties.rotation)
    rotationEl.``type`` <- "number"
    rotationEl.min <- "0"
    rotationEl.max <- "360"
    form.appendChild (rotationEl) |> ignore

    let submit: HTMLInputElement = createElement ("input")
    submit.``type`` <- "submit"
    form.appendChild (submit) |> ignore
    
    form.onsubmit <-
        (fun (e: Event) ->
            e.preventDefault ()
            performUpdate feature.properties.id textEl.value (int rotationEl.value))



    form
