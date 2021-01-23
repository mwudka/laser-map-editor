module App.Geo

open Fable.Core
open Mapbox.Mapboxgl
open JsInterop

[<Measure>]
type kilometer

let extractName: (MapboxGeoJSONFeature -> string) = fun feature -> feature?properties?name

type FontInfo = { Family: string; URL: string }

type FeatureProperties =
    abstract iconSize: float with get, set
    abstract rotation: int with get, set
    abstract id: string with get, set
    abstract textContent: string with get, set
    abstract fontSize: int with get, set
    abstract font: FontInfo option with get, set

type LaserEditorFeature = GeoJSON.Feature<GeoJSON.Point, FeatureProperties>


let extractPoint (feature: MapboxGeoJSONFeature) =
    let featureType: string = !!feature?geometry?``type``
    let coordinates: Option<float * float> = !!feature?geometry?coordinates

    if featureType = "Point" then
        coordinates
        |> Option.map Mapbox.mapboxgl.LngLat.Create
    else
        None
