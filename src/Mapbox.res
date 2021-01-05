
type geocoder
type geocoderConfig = {
    accessToken: string,
    mapboxgl: string
}
@bs.new external newGeocoder: geocoderConfig => geocoder = "MapboxGeocoder"

type lonlat = (float, float)
type boundingbox = (lonlat, lonlat)

type map
type mapConfig = {
    container: string,
    style: string,
    zoom: float,
    preserveDrawingBuffer: bool,
    bounds: boundingbox
}
type xy = {x: int, y: int}
type lngLat = (float, float)
type clickEvent = {
    point: xy,
    lngLat: lngLat
}
type featureProperties = {
    name: string
}
type geometry = {
    \"type": [#Point],
    coordinates: lngLat
}
type feature = {
    \"type": string,
    properties: featureProperties,
    geometry: geometry
}
type mapSource
type mapboxBoundingBox
@bs.new external newMap: mapConfig => map = "mapboxgl.Map"
@bs.send external addControl: (map, geocoder) => () = "addControl"
@bs.send external queryRenderedFeatures: (map, xy) => array<feature> = "queryRenderedFeatures"
@bs.send external on: (map, string, () => ()) => () = "on"
@bs.send external onClick: (map, string, (clickEvent) => ()) => () = "on"
@bs.send external addSource: (map, string, 'a) => () = "addSource"
@bs.send external getSource: (map, string) => mapSource = "getSource"
@bs.send external setData: (mapSource, 'a) => () = "setData"
@bs.send external toArray: (lngLat) => lngLat = "toArray"
@bs.send external addLayer: (map, 'a) => () = "addLayer"
@bs.send external loadImage: (map, string, ('a, 'a) => ()) => () = "loadImage"
type addImageOptions = {
    stretchX: ((int, int), (int, int)),
    stretchY: array<(int, int)>,
    content: (int, int, int, int),
    pixelRatio: int
}
@bs.send external addImageWithOptions: (map, string, 'a, addImageOptions) => () = "addImage"
@bs.send external addImage: (map, string, 'a) => () = "addImage"
@bs.send external getBounds: (map) => (mapboxBoundingBox) = "getBounds"
@bs.send external boundingBoxToArray: (mapboxBoundingBox) => array<float> = "toArray"

type querySourceFeaturesFilter = {
    sourceLayer: string
}
@bs.send external querySourceFeatures: (map, string, querySourceFeaturesFilter) => array<feature> = "querySourceFeatures"

@bs.val external mapboxgl: 'a = "mapboxgl"

@bs.send external getContext: ('a, string) => 'a = "getContext"