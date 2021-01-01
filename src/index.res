type objectPropertyGetter = {
    "get": () => int
}

@bs.scope("Object") @bs.val external defineObjectProperty: ('a, string, objectPropertyGetter) => () = "defineProperty"

defineObjectProperty(Webapi.Dom.window, "devicePixelRatio", {
    "get": () => 300 / 96
})


@bs.val external document: 'a = "document"
@bs.val external window: 'a = "window"
@bs.val external setTimeout: (unit => unit, int) => float = "setTimeout"
@bs.val external localStorage: 'a = "localStorage"



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

%%raw(`
var width = 64; // The image will be 64 pixels square
var bytesPerPixel = 4; // Each pixel is represented by 4 bytes: red, green, blue, and alpha.
var data = new Uint8Array(width * width * bytesPerPixel);
 
for (var x = 0; x < width; x++) {
for (var y = 0; y < width; y++) {
var offset = (y * width + x) * bytesPerPixel;
data[offset + 0] = 255;
data[offset + 1] = 0;
data[offset + 2] = 0;
data[offset + 3] = 255; // alpha
}
}
var generatedImage = { width: width, height: width, data: data }
`)

@bs.val external generatedImage: 'a = "generatedImage"

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

map->on("load", () => {
    map->loadImage("https://docs.mapbox.com/mapbox-gl-js/assets/popup.png", (_, image) => {
        Js.log("image loaded")
        map->addImage("text-label-background", image, {
            stretchX: ((25, 55), (85, 115)),
            stretchY: [(25, 100)],
            content: (25, 25, 115, 100),
            pixelRatio: 2
        })
    })

    // map->addImage("text-label-background", generatedImage, {
    //     stretchX: ((10, 20), (30, 63)),
    //     stretchY: [(10, 20)],
    //     content: (20, 10, 30, 20),
    //     pixelRatio: 1
    // })

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
            "text-field": ["get", "description"],
            "text-justify": "auto",
            "text-rotate": ["get", "rotation"],
            "icon-rotate": ["get", "rotation"],
            "text-offset": ["get", "offsetEM"],
            "icon-offset": ["get", "offsetEM"],
            "icon-text-fit": "both",
            "icon-image": ["literal", "text-label-background"],
            "icon-allow-overlap": true,
            "text-allow-overlap": true
        },
        "paint": {
            "text-color": "#000000",
        }
    })

    let createdFeatures = []

    let createFeature = (name: string, coordinates: lngLat) => {
        let newFeature = {
            "type": "Feature",
            "properties": {
                "description": name
                "rotation": 0,
                "offsetEM": [0, 0]
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