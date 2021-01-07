module rec MapboxGeocoder
open Fable.Core
open Mapbox.Mapboxgl

type Function = System.Action

let Foo = "88"

let [<Import("*","@mapbox/mapbox-gl-geocoder")>] MapboxGeocoder: MapboxGeocoderStatic = jsNative

//
//type [<AllowNullLiteral>] IExports =
//    abstract MapboxGeocoder: MapboxGeocoderStatic

module MapboxGeocoder =

    type [<AllowNullLiteral>] LngLatLiteral =
        abstract lat: float with get, set
        abstract lng: float with get, set

    type Bbox =
        float * float * float * float

    type [<AllowNullLiteral>] Options =
        abstract accessToken: string option with get, set
        /// On geocoded result what zoom level should the map animate to when a bbox isn't found in the response. If a bbox is found the map will fit to the bbox. (optional, default 16)
        abstract zoom: float option with get, set
        /// Override the default placeholder attribute value. (optional, default "Search")
        abstract placeholder: string option with get, set
        /// If false, animating the map to a selected result is disabled. (optional, default true)
        abstract flyTo: bool option with get, set
        /// a proximity argument: this is a geographical point given as an object with latitude and longitude properties. Search results closer to this point will be given higher priority.
        abstract proximity: LngLatLiteral option with get, set
        /// a bounding box argument: this is a bounding box given as an array in the format [minX, minY, maxX, maxY]. Search results will be limited to the bounding box.
        abstract bbox: Bbox option with get, set
        /// a comma seperated list of types that filter results to match those specified. See https://www.mapbox.com/developers/api/geocoding/#filter-type for available types.
        abstract types: string option with get, set
        /// a comma separated list of country codes to limit results to specified country or countries.
        abstract country: string option with get, set
        /// Minimum number of characters to enter before results are shown. (optional, default 2)
        abstract minLength: float option with get, set
        /// Maximum number of results to show. (optional, default 5)
        abstract limit: float option with get, set
        abstract mapboxgl: IExports option with get, set

    type [<AllowNullLiteral>] Results =
        inherit GeoJSON.FeatureCollection<GeoJSON.Point>
        abstract attribution: string with get, set
        abstract query: ResizeArray<string> with get, set

    type [<AllowNullLiteral>] Result =
        inherit GeoJSON.Feature<GeoJSON.Point>
        abstract bbox: Bbox with get, set
        abstract center: ResizeArray<float> with get, set
        abstract place_name: string with get, set
        abstract place_type: ResizeArray<string> with get, set
        abstract relevance: float with get, set
        abstract text: string with get, set

type [<AllowNullLiteral>] MapboxGeocoder =
    inherit Mapbox.Mapboxgl.Control
    /// <summary>Set & query the input</summary>
    /// <param name="searchInput">location name or other search input</param>
    abstract query: searchInput: string -> MapboxGeocoder
    /// Set input
    abstract setInput: value: string -> MapboxGeocoder
    /// <summary>Set proximity</summary>
    /// <param name="proximity">The new options.proximity value. This is a geographical point given as an object with latitude and longitude properties.</param>
    abstract setProximity: proximity: MapboxGeocoder.LngLatLiteral -> MapboxGeocoder
    /// Get proximity
    abstract getProximity: unit -> MapboxGeocoder.LngLatLiteral
    /// Subscribe to events that happen within the plugin.
    [<Emit "$0.on('clear',$1)">] abstract on_clear: listener: (unit -> obj option) -> MapboxGeocoder
    [<Emit "$0.on('loading',$1)">] abstract on_loading: listener: (MapboxGeocoderOn_loading -> obj option) -> MapboxGeocoder
    [<Emit "$0.on('results',$1)">] abstract on_results: listener: (MapboxGeocoder.Results -> obj option) -> MapboxGeocoder
    [<Emit "$0.on('result',$1)">] abstract on_result: listener: (MapboxGeocoderOn_result -> obj option) -> MapboxGeocoder
    [<Emit "$0.on('error',$1)">] abstract on_error: listener: (MapboxGeocoderOn_error -> obj option) -> MapboxGeocoder
    abstract on: ``type``: string * listener: Function -> MapboxGeocoder
    abstract on: ``type``: string * layer: string * listener: Function -> MapboxGeocoder
    /// <summary>Remove an event</summary>
    /// <param name="type">Event name.</param>
    abstract off: ``type``: string * listener: Function -> MapboxGeocoder
    abstract off: ``type``: string * layer: string * listener: Function -> MapboxGeocoder

type [<AllowNullLiteral>] MapboxGeocoderStatic =
    [<EmitConstructor>] abstract Create: ?options: MapboxGeocoder.Options -> MapboxGeocoder

type [<AllowNullLiteral>] MapboxGeocoderOn_loading =
    abstract query: string with get, set

type [<AllowNullLiteral>] MapboxGeocoderOn_result =
    abstract result: MapboxGeocoder.Result with get, set

type [<AllowNullLiteral>] MapboxGeocoderOn_error =
    abstract error: obj option with get, set

