type objectPropertyGetter = {
    "get": () => float
}

@bs.scope("Object") @bs.val external defineObjectProperty: ('a, string, objectPropertyGetter) => () = "defineProperty"

let devicePixelRatio = 300.0 /. 96.0

defineObjectProperty(Webapi.Dom.window, "devicePixelRatio", {
    "get": () => devicePixelRatio
})


@bs.val external document: 'a = "document"
@bs.val external window: 'a = "window"
@bs.val external setTimeout: (unit => unit, int) => float = "setTimeout"
@bs.val external localStorage: 'a = "localStorage"
@bs.send external getContext: ('a, string) => 'a = "getContext"
@bs.send external createElement: ('a, string) => 'a = "createElement"
@bs.send external getImageData: ('a, int, int, int, int) => 'a = "getImageData"
@bs.send external fillText: ('a, string, float, float) => () = "fillText"
@bs.send external toDataURL: 'a => string = "toDataURL"
@bs.send external appendChild: ('a, 'a) => () = "appendChild"

@bs.val external roundRect: ('a, int, int, int, int, int, bool, bool) => () = "roundRect"

let saveImage = () => {
    let dataURL: string = document["getElementsByTagName"]("canvas")["0"]["toDataURL"]()
    let a = document["createElement"]("a")
    a["href"] = dataURL
    a["download"] = "image.png"
    let _ = document["body"]["appendChild"](a)

    let _ = Js.Global.setTimeout(() => {
        let _  = document["body"]["removeChild"](a)
        Webapi.Url.revokeObjectURL(dataURL)

    }, 0)

    let _ = a["click"]()

    let _ = Webapi.Dom.window->Webapi.Dom.Window.open_(~url=dataURL, ~name="_blank")
}

switch (Webapi.Dom.Document.getElementById("save", Webapi.Dom.document)) {
    | None => raise(Not_found)
    | Some(save) => Webapi.Dom.EventTarget.addClickEventListener((_) => saveImage(), Webapi.Dom.Element.asEventTarget(save))
}

open Mapbox


@bs.scope("localStorage") @bs.val external savedBoundingBox: Js.Nullable.t<string> = "boundingbox"
@bs.scope("JSON") @bs.val external parseJsonBoundingBox: string => boundingbox = "parse"

let bounds = switch Js.Nullable.toOption(savedBoundingBox) {
    | None => ((0.0, 0.0), (0.0, 0.0))
    | Some(bounds) => parseJsonBoundingBox(bounds)
}

mapboxgl["accessToken"] = "pk.eyJ1IjoibXd1ZGthIiwiYSI6ImNraXhva29veDBtd3Mycm0wMTVtMmx4dXoifQ._3QauG82dcJHW7pNWU4aoA"

let map = newMap({
    container: "map",
    style: "mapbox://styles/mwudka/ckipad3rn3slk17p1rkdxmjdh/draft",
    zoom: 13.0,
    preserveDrawingBuffer: true,
    bounds: bounds
})

map->addControl(newGeocoder({
    accessToken: mapboxgl["accessToken"],
    mapboxgl: mapboxgl
}))

let generateTextImage = text => {
    let canvasEl = document["createElement"]("canvas")
    canvasEl["width"] = 1000
    canvasEl["height"] = 1000
    let context = canvasEl->getContext("2d")
    context["font"] = "30px serif";
    context["textBaseline"] = "middle"
    let metrics = context["measureText"](text)
    let height = 20 + metrics["actualBoundingBoxAscent"] + metrics["actualBoundingBoxDescent"]
    let radius = 20

    context["fillStyle"] = "white"
    roundRect(context, 0, 0, metrics["width"] + radius * 2, height, radius, true, false)

    context["fillStyle"] = "black"
    context["fillText"](text, radius, metrics["actualBoundingBoxAscent"] + radius / 2)


    context->getImageData(0, 0, metrics["width"] + radius * 2, height)
}

let nextImageId = ref(0)

map->on("load", () => {
    map->addSource("places", {
        "type": "geojson",
        "data": {
            "type": "FeatureCollection",
            "features": []
        }
    })

    map->addLayer({
        "id": "poi-labels",
        "type": "symbol",
        "source": "places",
        "layout": {
            // "text-field": ["get", "description"],
            // "text-justify": "auto",
            // "text-rotate": ["get", "rotation"],
            "icon-rotate": ["get", "rotation"],
            "icon-size": ["get", "icon-size"],
            // "text-offset": ["get", "offsetEM"],
            // "icon-offset": ["get", "offsetPX"],
            // "icon-text-fit": "both",
            "icon-image": ["get", "text-image"],
            "icon-allow-overlap": true,
            // "text-allow-overlap": true
        },
        "paint": {
            "text-color": "#000000",
        }
    })

    let createdFeatures = []

    let createFeature = (name: string, coordinates: lngLat) => {

        let imageId = nextImageId.contents
        nextImageId := imageId + 1

        let newImageName = j`image-$imageId`

        map->addImage(newImageName, generateTextImage(name))

        let newFeature = {
            "type": "Feature",
            "properties": {
                "description": name,
                "text-image": newImageName,
                "rotation": 0,
                "offset": 10,
                "offsetEM": [10, 10],
                "offsetPX": [0, 0],
                "icon-size": 1.0 /. devicePixelRatio
            },
            "geometry": {
                "type": "Point",
                "coordinates": coordinates
            }
        }
        let _ = createdFeatures->Js.Array2.push(newFeature)

        map->getSource("places")->setData({
            "type": "FeatureCollection",
            "features": createdFeatures
        })
    }

    map->onClick("click", e => {
        let _ = map
            ->querySourceFeatures("composite", {sourceLayer: "poi_label"})
            ->Js.Array2.map(poi => {
                // if poi.properties.name === "Ramona Gardens Park" {
                    createFeature(poi.properties.name, poi.geometry.coordinates)
                // }
            })
        

        let feature = map
            ->queryRenderedFeatures(e.point)
            ->Js.Array2.filter(f => f.\"type" === "Feature")
            ->Js.Array2.shift

        switch feature {
        | None => ()
        | Some(feature) => createFeature(feature.properties.name, e.lngLat->toArray)
        }
    })

Webapi.Dom.window|>Webapi.Dom.Window.addUnloadEventListener((_) => {
    localStorage["boundingbox"] = Js.Json.stringifyAny(map->getBounds->boundingBoxToArray)
})
})