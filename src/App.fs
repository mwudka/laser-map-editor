module App

open Browser.Types
open Fable.Core
open Fable.Core.JS
open Mapbox
open Mapbox.Mapboxgl
open JsInterop
open App.URL
open App.Geo
open Fable.Core.DynamicExtensions


open MapboxGeocoder

let window = Browser.Dom.window
let document = Browser.Dom.document
let localStorageBoundingBoxKey = "boundingbox"

mapboxgl.accessToken <- "pk.eyJ1IjoibXd1ZGthIiwiYSI6ImNraXhva29veDBtd3Mycm0wMTVtMmx4dXoifQ._3QauG82dcJHW7pNWU4aoA"

let map =
    mapboxgl.Map.Create
        (jsOptions<MapboxOptions> (fun opts ->
            opts.container <- !^ "map"
            opts.style <- Some(!^ "mapbox://styles/mwudka/ckipad3rn3slk17p1rkdxmjdh/draft")
            opts.zoom <- Some(13.0)
            opts.bounds <- Some(!! JSON.parse (window.localStorage.getItem (localStorageBoundingBoxKey)))
            opts.preserveDrawingBuffer <- Some(true)))


let geocoder =
    MapboxGeocoder.Create
        (jsOptions<MapboxGeocoder.Options> (fun opts ->
            opts.accessToken <- Some(mapboxgl.accessToken)
            opts.mapboxgl <- Some(mapboxgl)

            ))

map.addControl (!^geocoder) |> ignore

let saveImage _ =
    let canvasEl: HTMLCanvasElement =
        !!document.getElementsByTagName("canvas").[0]


    let dataURL = canvasEl.toDataURL ("image/png")
    let a: HTMLAnchorElement = !! document.createElement ("a")
    a.href <- dataURL
    a.setAttribute ("download", "image.png")
    document.body.appendChild (a) |> ignore

    (setTimeout (fun _ ->
        document.body.removeChild (a) |> ignore
        URL.revokeObjectURL (dataURL)),
     0)
    |> ignore

    a.click ()


map.on
    ("load",
     (fun () ->
         document
             .getElementById("save")
             .addEventListener("click", saveImage)

         map.addSource
             ("places",
              !!({| ``type`` = "geojson"
                    data =
                        {| ``type`` = "FeatureCollection"
                           features = [||] |} |}
                 |> toPlainJsObj))
         |> ignore

         let poiLabelsLayer =
             {| id = "poi-labels"
                ``type`` = "symbol"
                source = "places"
                layout =
                    {| ``icon-rotate`` = [| "get"; "rotation" |]
                       ``icon-size`` = [| "get"; "icon-size" |]
                       ``icon-image`` = [| "get"; "text-image" |]
                       ``icon-allow-overlap`` = true |}
                    |> toPlainJsObj
                paint = {| ``text-color`` = "#000000" |} |> toPlainJsObj |}

             |> toPlainJsObj

         map.addLayer (!!poiLabelsLayer) |> ignore

         ))
|> ignore

let isFeature: (MapboxGeoJSONFeature -> bool) =
    fun feature -> feature?``type`` = "Feature"

// From https://stackoverflow.com/questions/1255512/how-to-draw-a-rounded-rectangle-on-html-canvas
let roundRect (ctx: CanvasRenderingContext2D, x: float, y: float, width: float, height: float, radius: float) =
    ctx.beginPath ()
    ctx.moveTo (x + radius, y)
    ctx.lineTo (x + width - radius, y)
    ctx.quadraticCurveTo (x + width, y, x + width, y + radius)
    ctx.lineTo (x + width, y + height - radius)
    ctx.quadraticCurveTo (x + width, y + height, x + width - radius, y + height)
    ctx.lineTo (x + radius, y + height)
    ctx.quadraticCurveTo (x, y + height, x, y + height - radius)
    ctx.lineTo (x, y + radius)
    ctx.quadraticCurveTo (x, y, x + radius, y)
    ctx.closePath ()
    ctx.fill ()

let generateTextImage text =
    let canvasEl: HTMLCanvasElement = !! document.createElement ("canvas")
    canvasEl.width <- 1000.0
    canvasEl.height <- 1000.0
    let context2D = canvasEl.getContext_2d ()
    context2D.font <- "30px serif"
    context2D.textBaseline <- "middle"
    let metrics = context2D.measureText (text)

    let height =
        20.0
        + metrics?actualBoundingBoxAscent
        + metrics?actualBoundingBoxDescent

    context2D.fillStyle <- !^ "white"
    roundRect (context2D, 0.0, 0.0, metrics.width + 40.0, height, 20.0)

    context2D.fillStyle <- !^ "black"
    context2D.fillText (text, 20.0, metrics?actualBoundingBoxAscent + 10.0)
    context2D.getImageData (0.0, 0.0, metrics.width + 40.0, height)

let mutable createdFeatures = []
let mutable nextImageId = 0

[<Global>]
let devicePixelRatio: float = jsNative

let refreshMapSource =
    fun () ->
        let source: GeoJSONSource = !! map.getSource ("places")

        let newData: GeoJSON.FeatureCollection<GeoJSON.Geometry> =
            !!({| ``type`` = "FeatureCollection"
                  features = Seq.toArray createdFeatures |}
               |> toPlainJsObj)

        source.setData (!^newData) |> ignore

let createFeature (coordinates: LngLat) feature =
    let featureName = extractName feature

    let imageId = nextImageId
    nextImageId <- nextImageId + 1
    let newImageName = sprintf "image-%i" imageId

    map.addImage (newImageName, !^(generateTextImage featureName))
    |> ignore

    let newFeature =
        {| ``type`` = "Feature"
           geometry =
               {| ``type`` = "Point"
                  coordinates = coordinates.toArray () |}
               |> toPlainJsObj
           properties =
               {| id = imageId
                  ``text-content`` = featureName
                  ``text-image`` = newImageName
                  rotation = 0
                  ``icon-size`` = 1.0 / devicePixelRatio |}
               |> toPlainJsObj |}
        |> toPlainJsObj


    createdFeatures <- newFeature :: createdFeatures
    refreshMapSource ()

let updateFeature (id) (newText: string) (newRotation: int) =
    let updatedFeature =
        createdFeatures
        |> List.find (fun feature -> feature?properties?id = id)

    updatedFeature?properties?rotation <- newRotation

    let imageName = updatedFeature?properties?``text-image``
    map.removeImage (imageName) |> ignore

    map.addImage (imageName, !^(generateTextImage newText))
    |> ignore

    refreshMapSource ()

let mutable movingFeatureId: string = ""

let onMove =
    (fun (e: obj option) ->
        let e: MapMouseEvent = !!e
        
        let updatedFeature =
            createdFeatures
            |> List.find (fun feature -> feature?properties?id = movingFeatureId)
        updatedFeature?geometry?coordinates <- e.lngLat.toArray()

        refreshMapSource ()
        )

let onUp =
    (fun (e: obj option) -> map.off ("mousemove", onMove) |> ignore)

map.on
    ("mousedown",
     "poi-labels",
     (fun (e: obj) ->
         let e: MapMouseEvent = !!e
         movingFeatureId <- e?features.Item("0")?properties?id
         
         e.preventDefault ()
         map.on ("mousemove", onMove) |> ignore
         map.once ("mouseup", onUp) |> ignore

         ))
|> ignore

map.on
    ("click",
     "poi-labels",
     (fun (e: obj) ->
         let clickEvent: MapMouseEvent = !!e
         clickEvent.originalEvent.cancelBubble <- true
         let feature: GeoJSON.Feature<GeoJSON.Point, GeoJSON.GeoJsonProperties> = !!clickEvent?features.Item("0")

         let popup =
             mapboxgl
                 .Popup
                 .Create(jsOptions<PopupOptions> (fun opts -> opts.closeOnClick <- Some(true)))
                 .setLngLat(!!feature.geometry.coordinates)
                 .addTo(map)

         let updateFeatureAndRemove a b c =
             updateFeature a b c
             popup.remove () |> ignore

         let poiEditorNode =
             App.UI.poiEditor feature updateFeatureAndRemove

         popup.setDOMContent (poiEditorNode) |> ignore))
|> ignore



let handleMapClick (clickEvent: MapMouseEvent) =
    let nearbyFeatures: seq<MapboxGeoJSONFeature> =
        !!(map.querySourceFeatures ("composite", {| sourceLayer = "poi_label" |}))

    let clickedFeatures =
        map.queryRenderedFeatures (!^ !^clickEvent.point)
        |> Seq.filter isFeature

    let popup =
        mapboxgl
            .Popup
            .Create(jsOptions<PopupOptions> (fun opts -> opts.closeOnClick <- Some(true)))
            .setLngLat(!^clickEvent.lngLat)
            .addTo(map)

    let createFeatureAndRemoveFunction a b =
        createFeature a b |> ignore
        popup.remove ()

    let poiSelectorNode =
        App.UI.poiSelector (createFeatureAndRemoveFunction) clickEvent.lngLat nearbyFeatures clickedFeatures

    popup.setDOMContent (poiSelectorNode) |> ignore

map.on
    ("click",
     (fun (e: Option<obj>) ->
         let clickEvent: Option<MapMouseEvent> = !!e

         match clickEvent with
         | Some (clickEvent) when not clickEvent.originalEvent.cancelBubble -> handleMapClick clickEvent
         | _ -> console.log ("Map click already handled")))
|> ignore


window.addEventListener
    ("unload",
     (fun _ -> window.localStorage.setItem (localStorageBoundingBoxKey, JSON.stringify (map.getBounds().toArray()))))
