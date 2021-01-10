module App.Geo

open Fable.Core
open Mapbox.Mapboxgl
open JsInterop

[<Measure>]
type kilometer

let extractName: (MapboxGeoJSONFeature -> string) = fun feature -> feature?properties?name

let extractPoint (feature: MapboxGeoJSONFeature) =
    let featureType: string = !!feature?geometry?``type``
    let coordinates: Option<float * float> = !!feature?geometry?coordinates

    if featureType = "Point" then
        coordinates
        |> Option.map Mapbox.mapboxgl.LngLat.Create
    else
        None