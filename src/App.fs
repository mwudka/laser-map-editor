module App

open Browser.Types
open Fable.Core
open Fable.Core.JS
open Mapbox
open Mapbox.Mapboxgl
open JsInterop
open App.URL

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


let extractName: (MapboxGeoJSONFeature -> string) = fun feature -> feature?properties?name

let isFeature: (MapboxGeoJSONFeature -> bool) =
    fun feature -> feature?``type`` = "Feature"

// From https://stackoverflow.com/questions/1255512/how-to-draw-a-rounded-rectangle-on-html-canvas
let roundRect (ctx: CanvasRenderingContext2D, x: float, y: float, width: float, height: float, radius: float) =
    ctx.beginPath()
    ctx.moveTo(x + radius, y)
    ctx.lineTo(x + width - radius, y);
    ctx.quadraticCurveTo(x + width, y, x + width, y + radius);
    ctx.lineTo(x + width, y + height - radius);
    ctx.quadraticCurveTo(x + width, y + height, x + width - radius, y + height);
    ctx.lineTo(x + radius, y + height);
    ctx.quadraticCurveTo(x, y + height, x, y + height - radius);
    ctx.lineTo(x, y + radius);
    ctx.quadraticCurveTo(x, y, x + radius, y);
    ctx.closePath()
    ctx.fill()
//let roundRect: (CanvasRenderingContext2D, int, int, int, int, int -> unit) =
//    fun context x y width height radius -> ctx.beginPath()

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
    roundRect(context2D, 0.0, 0.0, metrics.width + 40.0, height, 20.0)
    
    context2D.fillStyle <- !^ "black"
    context2D.fillText (text, 20.0, metrics?actualBoundingBoxAscent + 10.0)
    context2D.getImageData (0.0, 0.0, metrics.width + 40.0, height)

let mutable createdFeatures = []
let mutable nextImageId = 0

let [<Global>] devicePixelRatio: float = jsNative

let createFeature coordinates feature =
    let featureName = extractName feature

    let imageId = nextImageId
    nextImageId <- nextImageId + 1
    let newImageName = sprintf "image-%i" imageId

    map.addImage (newImageName, !^(generateTextImage featureName)) |> ignore

    let newFeature =
        {| ``type`` = "Feature"
           geometry =
               {| ``type`` = "Point"
                  coordinates = coordinates?toArray() |}
               |> toPlainJsObj
           properties =
               {|
                  ``text-image`` = newImageName
                  rotation = 0
                  ``icon-size`` = 1.0 / devicePixelRatio
                   |}
               |> toPlainJsObj |}
        |> toPlainJsObj


    createdFeatures <- newFeature :: createdFeatures
    let source: GeoJSONSource = !! map.getSource ("places")

    let newData: GeoJSON.FeatureCollection<GeoJSON.Geometry> =
        !!({| ``type`` = "FeatureCollection"
              features = Seq.toArray createdFeatures |}
           |> toPlainJsObj)

    console.log (source, newData)
    source.setData (!^newData)

map.on
    ("click",
     (fun (e: Option<obj>) ->
         let clickEvent: MapMouseEvent = !! Option.get (e)

         let nearbyFeatureNames =
             !!(map.querySourceFeatures ("composite", {| sourceLayer = "poi_label" |}))
             |> Seq.map extractName
             |> String.concat ", "

         mapboxgl
             .Popup
             .Create(jsOptions<PopupOptions> (fun opts -> opts.closeOnClick <- Some(false)))
             .setLngLat(!^clickEvent.lngLat)
             .setHTML(nearbyFeatureNames)
             .addTo(map)
         |> ignore

         let feature =
             map.queryRenderedFeatures (!^ !^clickEvent.point)
             |> Seq.filter isFeature
             |> Seq.tryHead

         match feature with
         | None -> ()
         | Some (feature) -> createFeature clickEvent.lngLat feature |> ignore

         ))
|> ignore


window.addEventListener
    ("unload",
     (fun _ -> window.localStorage.setItem (localStorageBoundingBoxKey, JSON.stringify (map.getBounds().toArray()))))

//open Browser.Dom
//
//// Mutable variable to count the number of times we clicked the button
//let mutable count = 0
//
//// Get a reference to our button and cast the Element to an HTMLButtonElement
//let myButton = document.querySelector(".my-button") :?> Browser.Types.HTMLButtonElement
//
//// Register our listener
//myButton.onclick <- fun _ ->
//    count <- count + 1
//    myButton.innerText <- sprintf "You clicked: %i time%s" count (if count = 1 then "" else "s")
