module App.UI

open Mapbox.Mapboxgl
open FSharp.Data.UnitSystems.SI.UnitNames

let document = Browser.Dom.document
let console = Browser.Dom.console

let potentialPOIButton (clickedLocation: LngLat) createFeature (feature: MapboxGeoJSONFeature) =
    let link = document.createElement ("a")

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
