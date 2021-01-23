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

importAll "./style.scss"


let window = Browser.Dom.window
let document = Browser.Dom.document
let localStorageBoundingBoxKey = "boundingbox"

let mutable createdFeatures: LaserEditorFeature list = []

let hashParams = Fable.Import.Browser.URLSearchParams.Create(window.document.location.hash.Substring(1))
// To prevent abuse, the production mapbox token is configured to only work on the production site. Because of that
// configuration, it doesn't work when developing locally. To allow local development, this snippet checks for a
// mapbox-token param in the hash, and if present uses it to override the hard-coded mapbox token
mapboxgl.accessToken <- defaultArg (hashParams.get("mapbox-token")) "pk.eyJ1IjoibXd1ZGthIiwiYSI6ImNraXhva29veDBtd3Mycm0wMTVtMmx4dXoifQ._3QauG82dcJHW7pNWU4aoA"

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

type DocumentV1 = {
    BoundingBox: (float*float)*(float*float)
    Features: LaserEditorFeature []
}
    with static member Key = "laser-map-editor-doc-v1"

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

    let doc = {
        BoundingBox = map.getBounds().toArray()
        Features = createdFeatures |> Seq.toArray
    }

    let newChunk =
        App.PNG.text (DocumentV1.Key, JSON.stringify(doc))

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

let generateTextImage (feature: LaserEditorFeature) =
    let canvasEl: HTMLCanvasElement = App.UI.createElement ("canvas")
    canvasEl.width <- 1000.0
    canvasEl.height <- 1000.0
    let context2D = canvasEl.getContext_2d ()
    
    let family = match feature.properties.font with
                 | None -> "sans-serif"
                 | Some(font) -> font.Family
                
    
    context2D.font <- sprintf "%dpx %s" feature.properties.fontSize family
    context2D.textBaseline <- "middle"
    let metrics = context2D.measureText (feature.properties.textContent)

    let height =
        20.0
        + metrics?actualBoundingBoxAscent
        + metrics?actualBoundingBoxDescent

    context2D.fillStyle <- !^ "white"
    roundRect (context2D, 0.0, 0.0, metrics.width + 40.0, height, 20.0)

    context2D.fillStyle <- !^ "black"
    context2D.fillText (feature.properties.textContent, 20.0, metrics?actualBoundingBoxAscent + 10.0)
    context2D.getImageData (0.0, 0.0, metrics.width + 40.0, height)



let refreshMapSource _ =
    let source: GeoJSONSource = !! map.getSource ("places")

    let newData = jsOptions<GeoJSON.FeatureCollection<GeoJSON.Geometry>>(fun o ->
        o.``type`` <- "FeatureCollection"
        o.features <- Seq.toArray !!createdFeatures
        )
    source.setData (!^newData) |> ignore

let newDoc _ =
    createdFeatures
        |> Seq.map (fun feature -> feature.properties.id)
        |> Seq.iter (fun id -> map.removeImage(id) |> ignore)
    createdFeatures <- []
    refreshMapSource()

bodyEl.addEventListener
    ("drop",
     !!(fun (e: DragEvent) ->
         e.preventDefault ()
         e.stopPropagation ()
         
         // TODO: Handle multiple files; invalid files
         let firstFile = e.dataTransfer.files.item (0)
         let firstFileBlob = firstFile.slice ()

         // TODO: Handle exception from App.PNG.extract that happens on invalid png data
         let arrayBuffer: Promise<ArrayBuffer> = firstFileBlob?arrayBuffer ()
         
         arrayBuffer.``then`` (fun (arrayBuffer: ArrayBuffer) ->
             let parseTextChunk (chunk: App.PNG.PNGChunk) =
                 console.log(chunk)
                 if chunk.name <> "iTXt" then
                     None
                 else
                     let keyEndIndex = chunk.data.indexOf (uint8 (0))
                     let key =
                         App.PNG.createTextDecoder("utf-8")
                             .decode(chunk.data.slice (``end`` = keyEndIndex))

                     let valueStartIndex = chunk.data.lastIndexOf (uint8 (0))
                     let value =
                         App.PNG.createTextDecoder("utf-8")
                             .decode(chunk.data.slice (``begin`` = valueStartIndex + 1))

                     Some(key, value)

             let parsedDoc: DocumentV1 option =
                 let uint8Array = App.PNG.createUint8Array (!^arrayBuffer)

                 App.PNG.extract (!^uint8Array)
                 |> Seq.map parseTextChunk
                 |> Seq.find (fun chunk ->
                     match chunk with
                     | None -> false
                     | Some (key, _) -> key = DocumentV1.Key)
                 |> Option.map (fun (_, serializedDoc) -> !! JSON.parse (serializedDoc))

             match parsedDoc with
             | None -> console.log ("No serialized doc")
             | Some(parsedDoc) ->
                 console.log ("Parsed serialized doc", parsedDoc)

                 newDoc()
                 map.fitBounds(!^parsedDoc.BoundingBox) |> ignore
                 createdFeatures <- List.ofArray (parsedDoc.Features)
                 
                 let loadFontPromises = createdFeatures |> Seq.map(fun f ->
                     match f.properties.font with
                     | None -> Constructors.Promise.resolve()
                     | Some(font) -> FontPicker.createFontFace(font.Family, sprintf "url(%s)" font.URL).load().``then``(fun loadedFont ->
                         document?fonts?add(loadedFont)
                         )
                     )
                 let loadFontPromises = Seq.toArray loadFontPromises
                 
                 Constructors.Promise.all(loadFontPromises).``then``(fun _ ->
                     console.log("Loaded fonts")
                     createdFeatures |> Seq.iter (fun feature ->
                         let textImage = generateTextImage feature
                         let imageId = feature.properties.id
                         map.addImage (imageId, !^textImage) |> ignore
                     )
                     refreshMapSource()
                     ) |> ignore
                 )
         |> ignore))

map.on
    ("load",
     (fun () ->
         document
             .getElementById("save")
             .addEventListener("click", saveImage)
             
         document
            .getElementById("new")
            .addEventListener("click", newDoc)

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
                       ``icon-image`` = [| "get"; "id" |]
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

    let id = "poi-" + ShortUUID.shortUUID.generate()

    let newFeature =
        jsOptions<LaserEditorFeature> (fun o ->
            o.``type`` <- "Feature"

            o.geometry <-
                jsOptions<GeoJSON.Point> (fun o ->
                    o.``type`` <- "Point"
                    o.coordinates <- coordinates.toArray ())

            o.properties <-
                jsOptions<FeatureProperties> (fun o ->
                    o.id <- id
                    o.textContent <- featureName
                    o.iconSize <- 1.0 / devicePixelRatio
                    o.rotation <- 0
                    o.fontSize <- 30))

    map.addImage (id, !^(generateTextImage newFeature))
    |> ignore
    
    createdFeatures <- newFeature :: createdFeatures
    refreshMapSource ()

let updateFeature (id: string) (newText: string) (newRotation: int) (newFontInfo: FontInfo option) (newFontSize: int) =
    console.log("Applying changes to feature id", id)
    let updatedFeature =
        createdFeatures
        |> List.find (fun feature -> feature.properties.id = id)

    updatedFeature.properties.rotation <- newRotation
    updatedFeature.properties.textContent <- newText
    updatedFeature.properties.font <- newFontInfo

    let imageName = updatedFeature.properties.id
    map.removeImage (imageName) |> ignore

    map.addImage (imageName, !^(generateTextImage updatedFeature))
    |> ignore

    refreshMapSource ()
    
let deleteFeature (feature: LaserEditorFeature) =
    console.log("Deleting feature", feature)
    
    createdFeatures <- createdFeatures |> List.filter(fun f -> f.properties.id <> feature.properties.id)
    refreshMapSource()
    map.removeImage (feature.properties.id) |> ignore

let mutable movingPOIId: string option = None

let onMove =
    (fun (e: MapMouseEvent) ->
        match movingPOIId with
        | None -> console.warn("onMove with no movingPOIId")
        | Some(movingPOIId) ->
            let updatedFeature =
                createdFeatures
                |> List.find (fun feature -> feature.properties.id = movingPOIId)

            updatedFeature.geometry.coordinates <- e.lngLat.toArray()

            refreshMapSource ()
    )

let onUp =
    (fun (e: obj option) ->
        map.off ("mousemove", onMove) |> ignore
        movingPOIId <- None
    )

map.on
    ("mousedown",
     "poi-labels",
     (fun (e: MapLayerMouseEvent) ->
         let feature: LaserEditorFeature = !!e.features.Item("0")
         movingPOIId <- Some(feature.properties.id)

         e.preventDefault ()
         map.on ("mousemove", onMove) |> ignore
         map.once ("mouseup", onUp) |> ignore

         ))
|> ignore

map.on
    ("click",
     "poi-labels",
     (fun (clickEvent: MapLayerMouseEvent) ->
         clickEvent.originalEvent.cancelBubble <- true
         
         let feature: LaserEditorFeature = !!clickEvent.features.Item("0")
         let feature = createdFeatures |> List.find(fun f -> f.properties.id = feature.properties.id)
         

         let popup =
             mapboxgl
                 .Popup
                 .Create(jsOptions<PopupOptions> (fun opts -> opts.closeOnClick <- Some(true)))
                 .setLngLat(!^feature.geometry.coordinates)
                 .addTo(map)

         let updateFeatureAndRemove a b c d e =
             updateFeature a b c d e
             popup.remove () |> ignore
             
         let deleteFeatureAndRemove a =
             deleteFeature a
             popup.remove() |> ignore

         let poiEditorNode = document.createElement("div")
         
         popup.setDOMContent (poiEditorNode) |> ignore
         
         console.log(feature.properties.font)
         console.log(createdFeatures)
         
         App.UI.poiEditor poiEditorNode feature updateFeatureAndRemove deleteFeatureAndRemove

         ))
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
     (fun (clickEvent: MapMouseEvent) ->
         if not clickEvent.originalEvent.cancelBubble then
             handleMapClick clickEvent
         else
             console.log ("Map click already handled")
    )
)
|> ignore


window.addEventListener
    ("unload",
     (fun _ -> window.localStorage.setItem (localStorageBoundingBoxKey, JSON.stringify (map.getBounds().toArray()))))
