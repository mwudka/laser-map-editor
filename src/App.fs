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

let mutable createdFeatures: LaserEditorFeature list = []
let mutable nextImageId = 0

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

[<Emit("new Blob($0, $1)")>]
let createBlob (bytes: seq<obj>, options: BlobPropertyBag): Blob = jsNative

[<Emit("new Uint8Array($0)")>]
let createUint8Array (arrayBuffer: ArrayBuffer): Uint8Array = jsNative

type TextEncoder =
    abstract decode: Uint8Array -> string

[<Emit("new TextDecoder($0)")>]
let createTextDecoder (encoding: string): TextEncoder = jsNative

let saveImage _ =
    let canvasEl: HTMLCanvasElement = App.UI.getFirstElementByTagName("canvas")

    let imageMimeType = "image/png"
    let dataURL = canvasEl.toDataURL (imageMimeType)
    let binaryString = window.atob (dataURL.Split(',').[1])
    URL.revokeObjectURL (dataURL)

    let chunks =
        App.PNG.extract
            (!^(binaryString.ToCharArray()
                |> Seq.map uint8
                |> Seq.toArray))
        |> Seq.toList

    // TODO: Extract into helper
    let rec insertSecondLast el list =
        match list with
        | [ head ] -> el :: head :: []
        | head :: tail -> head :: (insertSecondLast el tail)
        | [] -> []

    // TODO: Create a real class for this.
    // TODO: Include bounding box
    let serializedDoc =
        JSON.stringify (createdFeatures |> Seq.toArray)

    // TODO: Make constant for magic string
    let newChunk =
        App.PNG.text ("laser-map-editor-doc", serializedDoc)

    // The text chunk needs to live somewhere after header chunk but before the end chunk. Second-to-last is a convenient
    // spot for this
    let chunks =
        chunks |> insertSecondLast newChunk |> Seq.toArray

    let encoded = App.PNG.encode (chunks)

    let blob =
        createBlob ([ encoded ], jsOptions<BlobPropertyBag> (fun o -> o.``type`` <- imageMimeType))

    let dataURL = URL.createObjectURL (blob)

    let a: HTMLAnchorElement = App.UI.createElement ("a")
    a.href <- dataURL
    a.setAttribute ("download", "image.png")
    document.body.appendChild (a) |> ignore

    (setTimeout (fun _ ->
        document.body.removeChild (a) |> ignore
        URL.revokeObjectURL (dataURL)),
     0)
    |> ignore

    a.click ()

let bodyEl =
    document.getElementsByTagName("body").[0]

bodyEl.addEventListener
    ("dragover",
     (fun (e: Event) ->
         e.preventDefault ()
         e.stopPropagation ()))


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
    let canvasEl: HTMLCanvasElement = App.UI.createElement ("canvas")
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


let refreshMapSource =
    fun () ->
        let source: GeoJSONSource = !! map.getSource ("places")
        

        let newData = jsOptions<GeoJSON.FeatureCollection<GeoJSON.Geometry>>(fun o ->
            o.``type`` <- "FeatureCollection"
            o.features <- Seq.toArray !!createdFeatures
            )
        source.setData (!^newData) |> ignore


bodyEl.addEventListener
    ("drop",
     (fun (e: Event) ->
         e.preventDefault ()
         e.stopPropagation ()

         let e: DragEvent = !!e

         let firstFile = e.dataTransfer.files.item (0)
         let firstFileBlob = firstFile.slice ()

         // TODO: Handle exception from App.PNG.extract that happens on invalid png data
         let arrayBuffer: Promise<ArrayBuffer> = firstFileBlob?arrayBuffer ()

         arrayBuffer.``then`` (fun (arrayBuffer: ArrayBuffer) ->
             let parseTextChunk (chunk: App.PNG.PNGChunk) =
                 if chunk.name <> "tEXt" then
                     None
                 else
                     let delimiterIndex = chunk.data.indexOf (uint8 (0))

                     let key =
                         createTextDecoder("utf-8")
                             .decode(chunk.data.slice (``end`` = delimiterIndex))

                     let value =
                         createTextDecoder("utf-8")
                             .decode(chunk.data.slice (``begin`` = delimiterIndex + 1))

                     Some(key, value)

             let parsedDoc: LaserEditorFeature [] option =
                 let uint8Array = createUint8Array (arrayBuffer)

                 App.PNG.extract (!^uint8Array)
                 |> Seq.map parseTextChunk
                 |> Seq.find (fun chunk ->
                     match chunk with
                     | None -> false
                     | Some (key, _) -> key = "laser-map-editor-doc")
                 |> Option.map (fun (_, serializedDoc) -> !! JSON.parse (serializedDoc))

             if parsedDoc.IsNone then
                 console.log ("No serialized doc")
             else
                 createdFeatures <- List.ofArray (parsedDoc.Value)
                 console.log ("Parsed serialized doc", createdFeatures)

                 let largestUsedImageId =
                     createdFeatures
                     |> Seq.map (fun feature -> feature.properties.id)
                     |> Seq.max

                 nextImageId <- largestUsedImageId + 1

                 createdFeatures
                 |> Seq.iter (fun feature ->
                     let textImage =
                         generateTextImage (feature.properties.textContent)

                     let imageId: string = feature.properties.textImage

                     map.addImage (imageId, !^textImage) |> ignore)

                 refreshMapSource ())
         |> ignore))

map.on
    ("load",
     (fun () ->
         document
             .getElementById("save")
             .addEventListener("click", saveImage)

         map.addSource
             ("places",
              !^jsOptions<GeoJSONSourceRaw>(fun o ->
                  o.``type`` <- "geojson"
                  o.data <- Some(!^jsOptions<GeoJSON.FeatureCollection>(fun o ->
                      o.``type`` <- "FeatureCollection"
                      o.features <- [||]
                      ))
                  )
         )
         |> ignore

         let poiLabelsLayer =
             {| id = "poi-labels"
                ``type`` = "symbol"
                source = "places"
                layout =
                    {| ``icon-rotate`` = [| "get"; "rotation" |]
                       ``icon-size`` = [| "get"; "iconSize" |]
                       ``icon-image`` = [| "get"; "textImage" |]
                       ``icon-allow-overlap`` = true |}
                    |> toPlainJsObj
                paint = {| ``text-color`` = "#000000" |} |> toPlainJsObj |}

             |> toPlainJsObj

         map.addLayer (poiLabelsLayer) |> ignore

         ))
|> ignore

let isFeature: (MapboxGeoJSONFeature -> bool) =
    fun feature -> feature?``type`` = "Feature"

[<Global>]
let devicePixelRatio: float = jsNative

let createFeature (coordinates: LngLat) feature =
    let featureName = extractName feature

    let imageId = nextImageId
    nextImageId <- nextImageId + 1
    let newImageName = sprintf "image-%i" imageId

    map.addImage (newImageName, !^(generateTextImage featureName))
    |> ignore

    let newFeature =
        jsOptions<LaserEditorFeature> (fun o ->
            o.``type`` <- "Feature"

            o.geometry <-
                jsOptions<GeoJSON.Point> (fun o ->
                    o.``type`` <- "Point"
                    o.coordinates <- coordinates.toArray ())

            o.properties <-
                jsOptions<FeatureProperties> (fun o ->
                    o.id <- imageId
                    o.textContent <- featureName
                    o.textImage <- newImageName
                    o.iconSize <- 1.0 / devicePixelRatio
                    o.rotation <- 0))

    createdFeatures <- newFeature :: createdFeatures
    refreshMapSource ()

let updateFeature (id) (newText: string) (newRotation: int) =
    let updatedFeature =
        createdFeatures
        |> List.find (fun feature -> feature?properties?id = id)

    updatedFeature?properties?rotation <- newRotation

    let imageName = updatedFeature.properties.textImage
    map.removeImage (imageName) |> ignore

    map.addImage (imageName, !^(generateTextImage newText))
    |> ignore

    refreshMapSource ()

let mutable movingFeatureId: int = -1

let onMove =
    (fun (e: obj option) ->
        let e: MapMouseEvent = !!e

        let updatedFeature =
            createdFeatures
            |> List.find (fun feature -> feature.properties.id = movingFeatureId)

        updatedFeature?geometry?coordinates <- e.lngLat.toArray ()

        refreshMapSource ())

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
         let feature: LaserEditorFeature = !!clickEvent?features.Item("0")

         let popup =
             mapboxgl
                 .Popup
                 .Create(jsOptions<PopupOptions> (fun opts -> opts.closeOnClick <- Some(true)))
                 .setLngLat(!^feature.geometry.coordinates)
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
        !!(map.querySourceFeatures
            ("composite", jsOptions<QuerySourceFeaturesParameters> (fun o -> o.sourceLayer <- "poi_label")))

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
        createFeature a b
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
