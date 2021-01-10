// ts2fable 0.8.0-build.557
module rec Mapbox
open System
open Fable.Core
open Fable.Core.JS
open Browser.Types
open FSharp.Data.UnitSystems.SI

[<Erase>] type KeyOf<'T> = Key of string
type Array<'T> = System.Collections.Generic.IList<'T>
type Error = System.Exception
type Function = System.Action

let [<Import("*","mapbox-gl")>] mapboxgl: Mapboxgl.IExports = jsNative

type [<AllowNullLiteral>] ImageBitmap =
    /// Returns the intrinsic height of the image, in CSS pixels.
    abstract height: float
    /// Returns the intrinsic width of the image, in CSS pixels.
    abstract width: float
    /// Releases imageBitmap's underlying bitmap data.
    abstract close: unit -> unit

module Mapboxgl =

    type [<AllowNullLiteral>] IExports =
        abstract accessToken: string with get, set
        abstract version: string
        abstract baseApiUrl: string
        abstract workerCount: float
        abstract maxParallelImageRequests: float
        abstract supported: ?options: SupportedOptions -> bool
        /// Clears browser storage used by this library. Using this method flushes the Mapbox tile cache that is managed by this library.
        /// Tiles may still be cached by the browser in some cases.
        abstract clearStorage: ?callback: (Error -> unit) -> unit
        abstract setRTLTextPlugin: pluginURL: string * callback: (Error -> unit) * ?deferred: bool -> unit
        abstract getRTLTextPluginStatus: unit -> PluginStatus
        /// Initializes resources like WebWorkers that can be shared across maps to lower load
        /// times in some situations. `mapboxgl.workerUrl` and `mapboxgl.workerCount`, if being
        /// used, must be set before `prewarm()` is called to have an effect.
        /// 
        /// By default, the lifecycle of these resources is managed automatically, and they are
        /// lazily initialized when a Map is first created. By invoking `prewarm()`, these
        /// resources will be created ahead of time, and will not be cleared when the last Map
        /// is removed from the page. This allows them to be re-used by new Map instances that
        /// are created later. They can be manually cleared by calling
        /// `mapboxgl.clearPrewarmedResources()`. This is only necessary if your web page remains
        /// active but stops using maps altogether.
        /// 
        /// This is primarily useful when using GL-JS maps in a single page app, wherein a user
        /// would navigate between various views that can cause Map instances to constantly be
        /// created and destroyed.
        abstract prewarm: unit -> unit
        /// Clears up resources that have previously been created by `mapboxgl.prewarm()`.
        /// Note that this is typically not necessary. You should only call this function
        /// if you expect the user of your app to not return to a Map view at any point
        /// in your application.
        abstract clearPrewarmedResources: unit -> unit
        abstract Map: MapStatic
        abstract FreeCameraOptions: FreeCameraOptionsStatic
        abstract BoxZoomHandler: BoxZoomHandlerStatic
        abstract ScrollZoomHandler: ScrollZoomHandlerStatic
        abstract DragPanHandler: DragPanHandlerStatic
        abstract DragRotateHandler: DragRotateHandlerStatic
        abstract KeyboardHandler: KeyboardHandlerStatic
        abstract DoubleClickZoomHandler: DoubleClickZoomHandlerStatic
        abstract TouchZoomRotateHandler: TouchZoomRotateHandlerStatic
        abstract TouchPitchHandler: TouchPitchHandlerStatic
        abstract Control: ControlStatic
        abstract NavigationControl: NavigationControlStatic
        abstract PositionOptions: PositionOptionsStatic
        abstract GeolocateControl: GeolocateControlStatic
        abstract AttributionControl: AttributionControlStatic
        abstract ScaleControl: ScaleControlStatic
        abstract FullscreenControl: FullscreenControlStatic
        abstract Popup: PopupStatic
        abstract GeoJSONSource: GeoJSONSourceStatic
        abstract VideoSource: VideoSourceStatic
        abstract ImageSource: ImageSourceStatic
        abstract CanvasSource: CanvasSourceStatic
        abstract LngLat: LngLatStatic
        abstract LngLatBounds: LngLatBoundsStatic
        abstract Point: PointStatic
        abstract MercatorCoordinate: MercatorCoordinateStatic
        abstract Marker: MarkerStatic
        abstract Evented: EventedStatic
        abstract MapboxEvent: MapboxEventStatic
        abstract MapMouseEvent: MapMouseEventStatic
        abstract MapTouchEvent: MapTouchEventStatic
        abstract MapWheelEvent: MapWheelEventStatic
        abstract ErrorEvent: ErrorEventStatic

    type [<AllowNullLiteral>] SupportedOptions =
        abstract failIfMajorPerformanceCaveat: bool option with get, set

    type [<StringEnum>] [<RequireQualifiedAccess>] PluginStatus =
        | Unavailable
        | Loading
        | Loaded
        | Error

    type LngLatLike =
        U6<float * float, LngLat, float, float, float, float>

    type LngLatBoundsLike =
        U4<LngLatBounds, LngLatLike * LngLatLike, float * float * float * float, LngLatLike>

    type PointLike =
        U2<Point, float * float>

    type Offset =
        U3<float, PointLike, PointLike>

    type [<StringEnum>] [<RequireQualifiedAccess>] ExpressionName =
        | Array
        | Boolean
        | Collator
        | Format
        | Literal
        | Number
        | Object
        | String
        | Image
        | [<CompiledName "to-boolean">] ToBoolean
        | [<CompiledName "to-color">] ToColor
        | [<CompiledName "to-number">] ToNumber
        | [<CompiledName "to-string">] ToString
        | Typeof
        | [<CompiledName "feature-state">] FeatureState
        | [<CompiledName "geometry-type">] GeometryType
        | Id
        | [<CompiledName "line-progress">] LineProgress
        | Properties
        | At
        | Get
        | Has
        | In
        | [<CompiledName "index-of">] IndexOf
        | Length
        | Slice
        | [<CompiledName "!">] Not
        | [<CompiledName "!=">] NotEqual
        | [<CompiledName "<">] LessThan
        | [<CompiledName "<=">] LessThanEqual
        | [<CompiledName "==">] Equal
        | [<CompiledName ">">] GreaterThan
        | [<CompiledName ">=">] GreaterThanEqual
        | All
        | Any
        | Case
        | Match
        | Coalesce
        | Interpolate
        | [<CompiledName "interpolate-hcl">] InterpolateHcl
        | [<CompiledName "interpolate-lab">] InterpolateLab
        | Step
        | Let
        | Var
        | Concat
        | Downcase
        | [<CompiledName "is-supported-script">] IsSupportedScript
        | [<CompiledName "resolved-locale">] ResolvedLocale
        | Upcase
        | Rgb
        | Rgba
        | [<CompiledName "-">] Minus
        | [<CompiledName "*">] Times
        | [<CompiledName "/">] DividedBy
        | [<CompiledName "%">] Mod
        | [<CompiledName "^">] Exponent
        | [<CompiledName "+">] Plus
        | Abs
        | Acos
        | Asin
        | Atan
        | Ceil
        | Cos
        | E
        | Floor
        | Ln
        | Ln2
        | Log10
        | Log2
        | Max
        | Min
        | Pi
        | Round
        | Sin
        | Sqrt
        | Tan
        | Zoom
        | [<CompiledName "heatmap-density">] HeatmapDensity

    type Expression =
        ExpressionName * obj
        
    

    type [<StringEnum>] [<RequireQualifiedAccess>] Anchor =
        | Center
        | Left
        | Right
        | Top
        | Bottom
        | [<CompiledName "top-left">] TopLeft
        | [<CompiledName "top-right">] TopRight
        | [<CompiledName "bottom-left">] BottomLeft
        | [<CompiledName "bottom-right">] BottomRight

    /// Map
    type [<AllowNullLiteral>] Map =
        inherit Evented
        abstract addControl: control: U2<Control, IControl> * ?position: MapAddControlPosition -> Map
        abstract removeControl: control: U2<Control, IControl> -> Map
        /// <summary>Checks if a control exists on the map.</summary>
        /// <param name="control">The {@link IControl} to check.</param>
        abstract hasControl: control: IControl -> bool
        abstract resize: ?eventData: EventData -> Map
        abstract getBounds: unit -> LngLatBounds
        abstract getMaxBounds: unit -> LngLatBounds option
        abstract setMaxBounds: ?lnglatbounds: LngLatBoundsLike -> Map
        abstract setMinZoom: ?minZoom: float -> Map
        abstract getMinZoom: unit -> float
        abstract setMaxZoom: ?maxZoom: float -> Map
        abstract getMaxZoom: unit -> float
        abstract setMinPitch: ?minPitch: float -> Map
        abstract getMinPitch: unit -> float
        abstract setMaxPitch: ?maxPitch: float -> Map
        abstract getMaxPitch: unit -> float
        abstract getRenderWorldCopies: unit -> bool
        abstract setRenderWorldCopies: ?renderWorldCopies: bool -> Map
        abstract project: lnglat: LngLatLike -> Mapboxgl.Point
        abstract unproject: point: PointLike -> Mapboxgl.LngLat
        abstract isMoving: unit -> bool
        abstract isZooming: unit -> bool
        abstract isRotating: unit -> bool
        /// <summary>Returns an array of GeoJSON Feature objects representing visible features that satisfy the query parameters.
        /// 
        /// The properties value of each returned feature object contains the properties of its source feature. For GeoJSON sources, only string and numeric property values are supported (i.e. null, Array, and Object values are not supported).
        /// 
        /// Each feature includes top-level layer, source, and sourceLayer properties. The layer property is an object representing the style layer to which the feature belongs. Layout and paint properties in this object contain values which are fully evaluated for the given zoom level and feature.
        /// 
        /// Only features that are currently rendered are included. Some features will not be included, like:
        /// 
        /// - Features from layers whose visibility property is "none".
        /// - Features from layers whose zoom range excludes the current zoom level.
        /// - Symbol features that have been hidden due to text or icon collision.
        /// 
        /// Features from all other layers are included, including features that may have no visible contribution to the rendered result; for example, because the layer's opacity or color alpha component is set to 0.
        /// 
        /// The topmost rendered feature appears first in the returned array, and subsequent features are sorted by descending z-order. Features that are rendered multiple times (due to wrapping across the antimeridian at low zoom levels) are returned only once (though subject to the following caveat).
        /// 
        /// Because features come from tiled vector data or GeoJSON data that is converted to tiles internally, feature geometries may be split or duplicated across tile boundaries and, as a result, features may appear multiple times in query results. For example, suppose there is a highway running through the bounding rectangle of a query. The results of the query will be those parts of the highway that lie within the map tiles covering the bounding rectangle, even if the highway extends into other tiles, and the portion of the highway within each map tile will be returned as a separate feature. Similarly, a point feature near a tile boundary may appear in multiple tiles due to tile buffering.</summary>
        /// <param name="pointOrBox">The geometry of the query region: either a single point or southwest and northeast points describing a bounding box. Omitting this parameter (i.e. calling Map#queryRenderedFeatures with zero arguments, or with only a  options argument) is equivalent to passing a bounding box encompassing the entire map viewport.</param>
        abstract queryRenderedFeatures: ?pointOrBox: U2<PointLike, PointLike * PointLike> * ?options: obj -> ResizeArray<MapboxGeoJSONFeature>
        /// <summary>Returns an array of GeoJSON Feature objects representing features within the specified vector tile or GeoJSON source that satisfy the query parameters.
        /// 
        /// In contrast to Map#queryRenderedFeatures, this function returns all features matching the query parameters, whether or not they are rendered by the current style (i.e. visible). The domain of the query includes all currently-loaded vector tiles and GeoJSON source tiles: this function does not check tiles outside the currently visible viewport.
        /// 
        /// Because features come from tiled vector data or GeoJSON data that is converted to tiles internally, feature geometries may be split or duplicated across tile boundaries and, as a result, features may appear multiple times in query results. For example, suppose there is a highway running through the bounding rectangle of a query. The results of the query will be those parts of the highway that lie within the map tiles covering the bounding rectangle, even if the highway extends into other tiles, and the portion of the highway within each map tile will be returned as a separate feature. Similarly, a point feature near a tile boundary may appear in multiple tiles due to tile buffering.</summary>
        /// <param name="sourceID">The ID of the vector tile or GeoJSON source to query.</param>
        abstract querySourceFeatures: sourceID: string * ?parameters: obj -> ResizeArray<MapboxGeoJSONFeature>
        abstract setStyle: style: U2<Mapboxgl.Style, string> * ?options: MapSetStyleOptions -> Map
        abstract getStyle: unit -> Mapboxgl.Style
        abstract isStyleLoaded: unit -> bool
        abstract addSource: id: string * source: AnySourceData -> Map
        abstract isSourceLoaded: id: string -> bool
        abstract areTilesLoaded: unit -> bool
        abstract removeSource: id: string -> Map
        abstract getSource: id: string -> AnySourceImpl
        abstract addImage: name: string * image: U5<HTMLImageElement, ArrayBufferView, MapAddImage, ImageData, ImageBitmap> * ?options: MapAddImageOptions -> Map
        abstract hasImage: name: string -> bool
        abstract removeImage: name: string -> Map
        abstract loadImage: url: string * callback: Function -> Map
        abstract listImages: unit -> ResizeArray<string>
        abstract addLayer: layer: Mapboxgl.AnyLayer * ?before: string -> Map
        abstract moveLayer: id: string * ?beforeId: string -> Map
        abstract removeLayer: id: string -> Map
        abstract getLayer: id: string -> Mapboxgl.AnyLayer
        abstract setFilter: layer: string * ?filter: U2<ResizeArray<obj option>, bool> * ?options: FilterOptions -> Map
        abstract setLayerZoomRange: layerId: string * minzoom: float * maxzoom: float -> Map
        abstract getFilter: layer: string -> ResizeArray<obj option>
        abstract setPaintProperty: layer: string * name: string * value: obj option * ?klass: string -> Map
        abstract getPaintProperty: layer: string * name: string -> obj option
        abstract setLayoutProperty: layer: string * name: string * value: obj option -> Map
        abstract getLayoutProperty: layer: string * name: string -> obj option
        abstract setLight: options: Mapboxgl.Light * ?lightOptions: obj -> Map
        abstract getLight: unit -> Mapboxgl.Light
        /// <summary>Sets the terrain property of the style.</summary>
        /// <param name="terrain">Terrain properties to set. Must conform to the [Mapbox Style Specification](https://www.mapbox.com/mapbox-gl-style-spec/#terrain).
        /// If `null` or `undefined` is provided, function removes terrain.</param>
        abstract setTerrain: ?terrain: TerrainSpecification -> Map
        abstract setFeatureState: feature: U2<FeatureIdentifier, Mapboxgl.MapboxGeoJSONFeature> * state: MapSetFeatureStateState -> unit
        abstract getFeatureState: feature: U2<FeatureIdentifier, Mapboxgl.MapboxGeoJSONFeature> -> MapGetFeatureStateReturn
        abstract removeFeatureState: target: U2<FeatureIdentifier, Mapboxgl.MapboxGeoJSONFeature> * ?key: string -> unit
        abstract getContainer: unit -> HTMLElement
        abstract getCanvasContainer: unit -> HTMLElement
        abstract getCanvas: unit -> HTMLCanvasElement
        abstract loaded: unit -> bool
        abstract remove: unit -> unit
        abstract triggerRepaint: unit -> unit
        abstract showTileBoundaries: bool with get, set
        abstract showCollisionBoxes: bool with get, set
        /// Gets and sets a Boolean indicating whether the map will visualize
        /// the padding offsets.
        abstract showPadding: bool with get, set
        abstract repaint: bool with get, set
        abstract getCenter: unit -> Mapboxgl.LngLat
        abstract setCenter: center: LngLatLike * ?eventData: Mapboxgl.EventData -> Map
        abstract panBy: offset: PointLike * ?options: Mapboxgl.AnimationOptions * ?eventData: Mapboxgl.EventData -> Map
        abstract panTo: lnglat: LngLatLike * ?options: Mapboxgl.AnimationOptions * ?eventdata: Mapboxgl.EventData -> Map
        abstract getZoom: unit -> float
        abstract setZoom: zoom: float * ?eventData: Mapboxgl.EventData -> Map
        abstract zoomTo: zoom: float * ?options: Mapboxgl.AnimationOptions * ?eventData: Mapboxgl.EventData -> Map
        abstract zoomIn: ?options: Mapboxgl.AnimationOptions * ?eventData: Mapboxgl.EventData -> Map
        abstract zoomOut: ?options: Mapboxgl.AnimationOptions * ?eventData: Mapboxgl.EventData -> Map
        abstract getBearing: unit -> float
        abstract setBearing: bearing: float * ?eventData: Mapboxgl.EventData -> Map
        /// Returns the current padding applied around the map viewport.
        abstract getPadding: unit -> PaddingOptions
        /// <summary>Sets the padding in pixels around the viewport.
        /// 
        /// Equivalent to `jumpTo({padding: padding})`.</summary>
        /// <param name="padding">The desired padding. Format: { left: number, right: number, top: number, bottom: number }</param>
        /// <param name="eventData">Additional properties to be added to event objects of events triggered by this method.</param>
        abstract setPadding: padding: PaddingOptions * ?eventData: EventData -> Map
        abstract rotateTo: bearing: float * ?options: Mapboxgl.AnimationOptions * ?eventData: EventData -> Map
        abstract resetNorth: ?options: Mapboxgl.AnimationOptions * ?eventData: Mapboxgl.EventData -> Map
        abstract resetNorthPitch: ?options: Mapboxgl.AnimationOptions * ?eventData: Mapboxgl.EventData -> Map
        abstract snapToNorth: ?options: Mapboxgl.AnimationOptions * ?eventData: Mapboxgl.EventData -> Map
        abstract getPitch: unit -> float
        abstract setPitch: pitch: float * ?eventData: EventData -> Map
        abstract cameraForBounds: bounds: LngLatBoundsLike * ?options: CameraForBoundsOptions -> CameraForBoundsResult option
        abstract fitBounds: bounds: LngLatBoundsLike * ?options: Mapboxgl.FitBoundsOptions * ?eventData: Mapboxgl.EventData -> Map
        abstract fitScreenCoordinates: p0: PointLike * p1: PointLike * bearing: float * ?options: obj * ?eventData: EventData -> Map
        abstract jumpTo: options: Mapboxgl.CameraOptions * ?eventData: Mapboxgl.EventData -> Map
        /// Returns position and orientation of the camera entity.
        abstract getFreeCameraOptions: unit -> FreeCameraOptions
        /// <summary>FreeCameraOptions provides more direct access to the underlying camera entity.
        /// For backwards compatibility the state set using this API must be representable with
        /// `CameraOptions` as well. Parameters are clamped into a valid range or discarded as invalid
        /// if the conversion to the pitch and bearing presentation is ambiguous. For example orientation
        /// can be invalid if it leads to the camera being upside down, the quaternion has zero length,
        /// or the pitch is over the maximum pitch limit.</summary>
        /// <param name="options">FreeCameraOptions object</param>
        /// <param name="eventData">Additional properties to be added to event objects of events triggered by this method.</param>
        abstract setFreeCameraOptions: options: FreeCameraOptions * ?eventData: Object -> Map
        abstract easeTo: options: Mapboxgl.EaseToOptions * ?eventData: Mapboxgl.EventData -> Map
        abstract flyTo: options: Mapboxgl.FlyToOptions * ?eventData: Mapboxgl.EventData -> Map
        abstract isEasing: unit -> bool
        abstract stop: unit -> Map
        abstract on: ``type``: KeyOf<MapLayerEventType> * layer: string * listener: (obj -> unit) -> Map
        abstract on: ``type``: KeyOf<MapEventType> * listener: (obj -> unit) -> Map
        abstract on: ``type``: string * listener: (obj option -> unit) -> Map
        abstract once: ``type``: KeyOf<MapLayerEventType> * layer: string * listener: (obj -> unit) -> Map
        abstract once: ``type``: KeyOf<MapEventType> * listener: (obj -> unit) -> Map
        abstract once: ``type``: string * listener: (obj option -> unit) -> Map
        abstract off: ``type``: KeyOf<MapLayerEventType> * layer: string * listener: (obj -> unit) -> Map
        abstract off: ``type``: KeyOf<MapEventType> * listener: (obj -> unit) -> Map
        abstract off: ``type``: string * listener: (obj option -> unit) -> Map
        abstract scrollZoom: ScrollZoomHandler with get, set
        abstract boxZoom: BoxZoomHandler with get, set
        abstract dragRotate: DragRotateHandler with get, set
        abstract dragPan: DragPanHandler with get, set
        abstract keyboard: KeyboardHandler with get, set
        abstract doubleClickZoom: DoubleClickZoomHandler with get, set
        abstract touchZoomRotate: TouchZoomRotateHandler with get, set
        abstract touchPitch: TouchPitchHandler with get, set

    type [<StringEnum>] [<RequireQualifiedAccess>] MapAddControlPosition =
        | [<CompiledName "top-right">] TopRight
        | [<CompiledName "top-left">] TopLeft
        | [<CompiledName "bottom-right">] BottomRight
        | [<CompiledName "bottom-left">] BottomLeft

    type [<AllowNullLiteral>] MapSetStyleOptions =
        abstract diff: bool option with get, set
        abstract localIdeographFontFamily: string option with get, set

    type [<AllowNullLiteral>] MapAddImageOptions =
        abstract pixelRatio: float option with get, set
        abstract sdf: bool option with get, set

    type [<AllowNullLiteral>] MapSetFeatureStateState =
        [<EmitIndexer>] abstract Item: key: string -> obj option with get, set

    type [<AllowNullLiteral>] MapGetFeatureStateReturn =
        [<EmitIndexer>] abstract Item: key: string -> obj option with get, set

    /// Map
    type [<AllowNullLiteral>] MapStatic =
        [<EmitConstructor>] abstract Create: ?options: MapboxOptions -> Map

    type [<AllowNullLiteral>] MapboxOptions =
        /// If true, the gl context will be created with MSA antialiasing, which can be useful for antialiasing custom layers.
        /// This is false by default as a performance optimization.
        abstract antialias: bool option with get, set
        /// If true, an attribution control will be added to the map.
        abstract attributionControl: bool option with get, set
        abstract bearing: float option with get, set
        /// Snap to north threshold in degrees.
        abstract bearingSnap: float option with get, set
        /// The initial bounds of the map. If bounds is specified, it overrides center and zoom constructor options.
        abstract bounds: LngLatBoundsLike option with get, set
        /// If true, enable the "box zoom" interaction (see BoxZoomHandler)
        abstract boxZoom: bool option with get, set
        /// initial map center
        abstract center: LngLatLike option with get, set
        /// The max number of pixels a user can shift the mouse pointer during a click for it to be
        /// considered a valid click (as opposed to a mouse drag).
        abstract clickTolerance: float option with get, set
        /// If `true`, Resource Timing API information will be collected for requests made by GeoJSON
        /// and Vector Tile web workers (this information is normally inaccessible from the main
        /// Javascript thread). Information will be returned in a `resourceTiming` property of
        /// relevant `data` events.
        abstract collectResourceTiming: bool option with get, set
        /// If `true`, symbols from multiple sources can collide with each other during collision
        /// detection. If `false`, collision detection is run separately for the symbols in each source.
        abstract crossSourceCollisions: bool option with get, set
        /// ID of the container element
        abstract container: U2<string, HTMLElement> with get, set
        /// String or strings to show in an AttributionControl.
        /// Only applicable if options.attributionControl is `true`.
        abstract customAttribution: U2<string, ResizeArray<string>> option with get, set
        /// If true, enable the "drag to pan" interaction (see DragPanHandler).
        abstract dragPan: bool option with get, set
        /// If true, enable the "drag to rotate" interaction (see DragRotateHandler).
        abstract dragRotate: bool option with get, set
        /// If true, enable the "double click to zoom" interaction (see DoubleClickZoomHandler).
        abstract doubleClickZoom: bool option with get, set
        /// If `true`, the map's position (zoom, center latitude, center longitude, bearing, and pitch) will be synced with the hash fragment of the page's URL.
        /// For example, `http://path/to/my/page.html#2.59/39.26/53.07/-24.1/60`.
        /// An additional string may optionally be provided to indicate a parameter-styled hash,
        /// e.g. http://path/to/my/page.html#map=2.59/39.26/53.07/-24.1/60&foo=bar, where foo
        /// is a custom parameter and bar is an arbitrary hash distinct from the map hash.
        abstract hash: U2<bool, string> option with get, set
        /// Controls the duration of the fade-in/fade-out animation for label collisions, in milliseconds.
        /// This setting affects all symbol layers. This setting does not affect the duration of runtime
        /// styling transitions or raster tile cross-fading.
        abstract fadeDuration: float option with get, set
        /// If true, map creation will fail if the implementation determines that the performance of the created WebGL context would be dramatically lower than expected.
        abstract failIfMajorPerformanceCaveat: bool option with get, set
        /// A fitBounds options object to use only when setting the bounds option.
        abstract fitBoundsOptions: FitBoundsOptions option with get, set
        /// If false, no mouse, touch, or keyboard listeners are attached to the map, so it will not respond to input
        abstract interactive: bool option with get, set
        /// If true, enable keyboard shortcuts (see KeyboardHandler).
        abstract keyboard: bool option with get, set
        /// A patch to apply to the default localization table for UI strings, e.g. control tooltips.
        /// The `locale` object maps namespaced UI string IDs to translated strings in the target language;
        /// see `src/ui/default_locale.js` for an example with all supported string IDs.
        /// The object may specify all UI strings (thereby adding support for a new translation) or
        /// only a subset of strings (thereby patching the default translation table).
        abstract locale: MapboxOptionsLocale option with get, set
        /// If specified, defines a CSS font-family for locally overriding generation of glyphs in the
        /// 'CJK Unified Ideographs' and 'Hangul Syllables' ranges. In these ranges, font settings from
        /// the map's style will be ignored, except for font-weight keywords (light/regular/medium/bold).
        /// The purpose of this option is to avoid bandwidth-intensive glyph server requests.
        abstract localIdeographFontFamily: string option with get, set
        /// A string representing the position of the Mapbox wordmark on the map.
        abstract logoPosition: MapboxOptionsLogoPosition option with get, set
        /// If set, the map is constrained to the given bounds.
        abstract maxBounds: LngLatBoundsLike option with get, set
        /// Maximum pitch of the map.
        abstract maxPitch: float option with get, set
        /// Maximum zoom of the map.
        abstract maxZoom: float option with get, set
        /// Minimum pitch of the map.
        abstract minPitch: float option with get, set
        /// Minimum zoom of the map.
        abstract minZoom: float option with get, set
        /// If true, The maps canvas can be exported to a PNG using map.getCanvas().toDataURL();. This is false by default as a performance optimization.
        abstract preserveDrawingBuffer: bool option with get, set
        /// The initial pitch (tilt) of the map, measured in degrees away from the plane of the
        /// screen (0-60).
        abstract pitch: float option with get, set
        /// If `false`, the map's pitch (tilt) control with "drag to rotate" interaction will be disabled.
        abstract pitchWithRotate: bool option with get, set
        /// If `false`, the map won't attempt to re-request tiles once they expire per their HTTP
        /// `cacheControl`/`expires` headers.
        abstract refreshExpiredTiles: bool option with get, set
        /// If `true`, multiple copies of the world will be rendered, when zoomed out.
        abstract renderWorldCopies: bool option with get, set
        /// If true, enable the "scroll to zoom" interaction
        abstract scrollZoom: bool option with get, set
        /// stylesheet location
        abstract style: U2<Mapboxgl.Style, string> option with get, set
        /// If  true, the map will automatically resize when the browser window resizes
        abstract trackResize: bool option with get, set
        /// A callback run before the Map makes a request for an external URL. The callback can be
        /// used to modify the url, set headers, or set the credentials property for cross-origin requests.
        abstract transformRequest: TransformRequestFunction option with get, set
        /// If true, enable the "pinch to rotate and zoom" interaction (see TouchZoomRotateHandler).
        abstract touchZoomRotate: bool option with get, set
        /// If true, the "drag to pitch" interaction is enabled
        abstract touchPitch: bool option with get, set
        /// Initial zoom level
        abstract zoom: float option with get, set
        /// The maximum number of tiles stored in the tile cache for a given source. If omitted, the
        /// cache will be dynamically sized based on the current viewport.
        abstract maxTileCacheSize: float option with get, set
        /// If specified, map will use this token instead of the one defined in mapboxgl.accessToken.
        abstract accessToken: string option with get, set

    type quat =
        ResizeArray<float>

    type vec3 =
        ResizeArray<float>

    /// Various options for accessing physical properties of the underlying camera entity.
    /// A direct access to these properties allows more flexible and precise controlling of the camera
    /// while also being fully compatible and interchangeable with CameraOptions. All fields are optional.
    /// See {@Link Camera#setFreeCameraOptions} and {@Link Camera#getFreeCameraOptions}
    type [<AllowNullLiteral>] FreeCameraOptions =
        abstract position: MercatorCoordinate option with get, set
        /// <summary>Helper function for setting orientation of the camera by defining a focus point
        /// on the map.</summary>
        /// <param name="location">Location of the focus point on the map</param>
        /// <param name="up">Up vector of the camera is required in certain scenarios where bearing can't be deduced
        /// from the viewing direction.</param>
        abstract lookAtPoint: location: LngLatLike * ?up: vec3 -> unit
        /// <summary>Helper function for setting the orientation of the camera as a pitch and a bearing.</summary>
        /// <param name="pitch">Pitch angle in degrees</param>
        /// <param name="bearing">Bearing angle in degrees</param>
        abstract setPitchBearing: pitch: float * bearing: float -> unit

    /// Various options for accessing physical properties of the underlying camera entity.
    /// A direct access to these properties allows more flexible and precise controlling of the camera
    /// while also being fully compatible and interchangeable with CameraOptions. All fields are optional.
    /// See {@Link Camera#setFreeCameraOptions} and {@Link Camera#getFreeCameraOptions}
    type [<AllowNullLiteral>] FreeCameraOptionsStatic =
        [<EmitConstructor>] abstract Create: ?position: MercatorCoordinate * ?orientation: quat -> FreeCameraOptions

    type [<StringEnum>] [<RequireQualifiedAccess>] ResourceType =
        | [<CompiledName "Unknown">] Unknown
        | [<CompiledName "Style">] Style
        | [<CompiledName "Source">] Source
        | [<CompiledName "Tile">] Tile
        | [<CompiledName "Glyphs">] Glyphs
        | [<CompiledName "SpriteImage">] SpriteImage
        | [<CompiledName "SpriteJSON">] SpriteJSON
        | [<CompiledName "Image">] Image

    type [<AllowNullLiteral>] RequestParameters =
        /// The URL to be requested.
        abstract url: string with get, set
        /// Use `'include'` to send cookies with cross-origin requests.
        abstract credentials: RequestParametersCredentials option with get, set
        /// The headers to be sent with the request.
        abstract headers: RequestParametersHeaders option with get, set
        abstract ``method``: RequestParametersMethod option with get, set
        abstract collectResourceTiming: bool option with get, set

    type [<AllowNullLiteral>] TransformRequestFunction =
        [<Emit "$0($1...)">] abstract Invoke: url: string * resourceType: ResourceType -> RequestParameters

    type [<AllowNullLiteral>] PaddingOptions =
        abstract top: float with get, set
        abstract bottom: float with get, set
        abstract left: float with get, set
        abstract right: float with get, set

    type [<AllowNullLiteral>] FeatureIdentifier =
        abstract id: U2<string, float> option with get, set
        abstract source: string with get, set
        abstract sourceLayer: string option with get, set

    /// BoxZoomHandler
    type [<AllowNullLiteral>] BoxZoomHandler =
        abstract isEnabled: unit -> bool
        abstract isActive: unit -> bool
        abstract enable: unit -> unit
        abstract disable: unit -> unit

    /// BoxZoomHandler
    type [<AllowNullLiteral>] BoxZoomHandlerStatic =
        [<EmitConstructor>] abstract Create: map: Mapboxgl.Map -> BoxZoomHandler

    /// ScrollZoomHandler
    type [<AllowNullLiteral>] ScrollZoomHandler =
        abstract isEnabled: unit -> bool
        abstract enable: unit -> unit
        abstract disable: unit -> unit
        abstract setZoomRate: zoomRate: float -> unit
        abstract setWheelZoomRate: wheelZoomRate: float -> unit

    /// ScrollZoomHandler
    type [<AllowNullLiteral>] ScrollZoomHandlerStatic =
        [<EmitConstructor>] abstract Create: map: Mapboxgl.Map -> ScrollZoomHandler

    /// DragPenHandler
    type [<AllowNullLiteral>] DragPanHandler =
        abstract isEnabled: unit -> bool
        abstract isActive: unit -> bool
        abstract enable: unit -> unit
        abstract disable: unit -> unit

    /// DragPenHandler
    type [<AllowNullLiteral>] DragPanHandlerStatic =
        [<EmitConstructor>] abstract Create: map: Mapboxgl.Map -> DragPanHandler

    /// DragRotateHandler
    type [<AllowNullLiteral>] DragRotateHandler =
        abstract isEnabled: unit -> bool
        abstract isActive: unit -> bool
        abstract enable: unit -> unit
        abstract disable: unit -> unit

    /// DragRotateHandler
    type [<AllowNullLiteral>] DragRotateHandlerStatic =
        [<EmitConstructor>] abstract Create: map: Mapboxgl.Map * ?options: DragRotateHandlerStaticOptions -> DragRotateHandler

    type [<AllowNullLiteral>] DragRotateHandlerStaticOptions =
        abstract bearingSnap: float option with get, set
        abstract pitchWithRotate: bool option with get, set

    /// KeyboardHandler
    type [<AllowNullLiteral>] KeyboardHandler =
        abstract isEnabled: unit -> bool
        abstract enable: unit -> unit
        abstract disable: unit -> unit
        /// Returns true if the handler is enabled and has detected the start of a
        /// zoom/rotate gesture.
        abstract isActive: unit -> bool
        /// Disables the "keyboard pan/rotate" interaction, leaving the
        /// "keyboard zoom" interaction enabled.
        abstract disableRotation: unit -> unit
        /// Enables the "keyboard pan/rotate" interaction.
        abstract enableRotation: unit -> unit

    /// KeyboardHandler
    type [<AllowNullLiteral>] KeyboardHandlerStatic =
        [<EmitConstructor>] abstract Create: map: Mapboxgl.Map -> KeyboardHandler

    /// DoubleClickZoomHandler
    type [<AllowNullLiteral>] DoubleClickZoomHandler =
        abstract isEnabled: unit -> bool
        abstract enable: unit -> unit
        abstract disable: unit -> unit

    /// DoubleClickZoomHandler
    type [<AllowNullLiteral>] DoubleClickZoomHandlerStatic =
        [<EmitConstructor>] abstract Create: map: Mapboxgl.Map -> DoubleClickZoomHandler

    /// TouchZoomRotateHandler
    type [<AllowNullLiteral>] TouchZoomRotateHandler =
        abstract isEnabled: unit -> bool
        abstract enable: unit -> unit
        abstract disable: unit -> unit
        abstract disableRotation: unit -> unit
        abstract enableRotation: unit -> unit

    /// TouchZoomRotateHandler
    type [<AllowNullLiteral>] TouchZoomRotateHandlerStatic =
        [<EmitConstructor>] abstract Create: map: Mapboxgl.Map -> TouchZoomRotateHandler

    type [<AllowNullLiteral>] TouchPitchHandler =
        abstract enable: unit -> unit
        abstract isActive: unit -> bool
        abstract isEnabled: unit -> bool
        abstract disable: unit -> unit

    type [<AllowNullLiteral>] TouchPitchHandlerStatic =
        [<EmitConstructor>] abstract Create: map: Mapboxgl.Map -> TouchPitchHandler

    type [<AllowNullLiteral>] IControl =
        abstract onAdd: map: Map -> HTMLElement
        abstract onRemove: map: Map -> unit
        abstract getDefaultPosition: (unit -> string) option with get, set

    /// Control
    type [<AllowNullLiteral>] Control =
        inherit Evented
        inherit IControl
        abstract onAdd: map: Map -> HTMLElement
        abstract onRemove: map: Map -> unit
        abstract getDefaultPosition: (unit -> string) option with get, set

    /// Control
    type [<AllowNullLiteral>] ControlStatic =
        [<EmitConstructor>] abstract Create: unit -> Control

    /// Navigation
    type [<AllowNullLiteral>] NavigationControl =
        inherit Control

    /// Navigation
    type [<AllowNullLiteral>] NavigationControlStatic =
        [<EmitConstructor>] abstract Create: ?options: NavigationControlStaticOptions -> NavigationControl

    type [<AllowNullLiteral>] NavigationControlStaticOptions =
        abstract showCompass: bool option with get, set
        abstract showZoom: bool option with get, set
        abstract visualizePitch: bool option with get, set

    type [<AllowNullLiteral>] PositionOptions =
        abstract enableHighAccuracy: bool option with get, set
        abstract timeout: float option with get, set
        abstract maximumAge: float option with get, set

    type [<AllowNullLiteral>] PositionOptionsStatic =
        [<EmitConstructor>] abstract Create: unit -> PositionOptions

    /// Geolocate
    type [<AllowNullLiteral>] GeolocateControl =
        inherit Control
        abstract trigger: unit -> bool

    /// Geolocate
    type [<AllowNullLiteral>] GeolocateControlStatic =
        [<EmitConstructor>] abstract Create: ?options: GeolocateControlStaticOptions -> GeolocateControl

    type [<AllowNullLiteral>] GeolocateControlStaticOptions =
        abstract positionOptions: PositionOptions option with get, set
        abstract fitBoundsOptions: FitBoundsOptions option with get, set
        abstract trackUserLocation: bool option with get, set
        abstract showAccuracyCircle: bool option with get, set
        abstract showUserLocation: bool option with get, set

    /// Attribution
    type [<AllowNullLiteral>] AttributionControl =
        inherit Control

    /// Attribution
    type [<AllowNullLiteral>] AttributionControlStatic =
        [<EmitConstructor>] abstract Create: ?options: AttributionControlStaticOptions -> AttributionControl

    type [<AllowNullLiteral>] AttributionControlStaticOptions =
        abstract compact: bool option with get, set
        abstract customAttribution: U2<string, ResizeArray<string>> option with get, set

    /// Scale
    type [<AllowNullLiteral>] ScaleControl =
        inherit Control
        abstract setUnit: unit: ScaleControlSetUnitUnit -> unit

    type [<StringEnum>] [<RequireQualifiedAccess>] ScaleControlSetUnitUnit =
        | Imperial
        | Metric
        | Nautical

    /// Scale
    type [<AllowNullLiteral>] ScaleControlStatic =
        [<EmitConstructor>] abstract Create: ?options: ScaleControlStaticOptions -> ScaleControl

    type [<AllowNullLiteral>] ScaleControlStaticOptions =
        abstract maxWidth: float option with get, set
        abstract unit: string option with get, set

    /// FullscreenControl
    type [<AllowNullLiteral>] FullscreenControl =
        inherit Control

    /// FullscreenControl
    type [<AllowNullLiteral>] FullscreenControlStatic =
        [<EmitConstructor>] abstract Create: ?options: FullscreenControlOptions -> FullscreenControl

    type [<AllowNullLiteral>] FullscreenControlOptions =
        /// A compatible DOM element which should be made full screen.
        /// By default, the map container element will be made full screen.
        abstract container: HTMLElement option with get, set

    /// Popup
    type [<AllowNullLiteral>] Popup =
        inherit Evented
        abstract addTo: map: Mapboxgl.Map -> Popup
        abstract isOpen: unit -> bool
        abstract remove: unit -> Popup
        abstract getLngLat: unit -> Mapboxgl.LngLat
        /// <summary>Sets the geographical location of the popup's anchor, and moves the popup to it. Replaces trackPointer() behavior.</summary>
        /// <param name="lnglat">The geographical location to set as the popup's anchor.</param>
        abstract setLngLat: lnglat: LngLatLike -> Popup
        /// Tracks the popup anchor to the cursor position, on screens with a pointer device (will be hidden on touchscreens). Replaces the setLngLat behavior.
        /// For most use cases, `closeOnClick` and `closeButton` should also be set to `false` here.
        abstract trackPointer: unit -> Popup
        /// Returns the `Popup`'s HTML element.
        abstract getElement: unit -> HTMLElement
        abstract setText: text: string -> Popup
        abstract setHTML: html: string -> Popup
        abstract setDOMContent: htmlNode: Node -> Popup
        abstract getMaxWidth: unit -> string
        abstract setMaxWidth: maxWidth: string -> Popup
        /// <summary>Adds a CSS class to the popup container element.</summary>
        /// <param name="className">Non-empty string with CSS class name to add to popup container</param>
        abstract addClassName: className: string -> unit
        /// <summary>Removes a CSS class from the popup container element.</summary>
        /// <param name="className">Non-empty string with CSS class name to remove from popup container</param>
        abstract removeClassName: className: string -> unit
        /// <summary>Sets the popup's offset.</summary>
        /// <param name="offset">Sets the popup's offset.</param>
        abstract setOffset: ?offset: Offset -> Popup
        /// <summary>Add or remove the given CSS class on the popup container, depending on whether the container currently has that class.</summary>
        /// <param name="className">Non-empty string with CSS class name to add/remove</param>
        abstract toggleClassName: className: string -> unit

    /// Popup
    type [<AllowNullLiteral>] PopupStatic =
        [<EmitConstructor>] abstract Create: ?options: Mapboxgl.PopupOptions -> Popup

    type [<AllowNullLiteral>] PopupOptions =
        abstract closeButton: bool option with get, set
        abstract closeOnClick: bool option with get, set
        abstract closeOnMove: bool option with get, set
        abstract focusAfterOpen: bool option with get, set
        abstract anchor: Anchor option with get, set
        abstract offset: Offset option with get, set
        abstract className: string option with get, set
        abstract maxWidth: string option with get, set

    type [<AllowNullLiteral>] Style =
        abstract bearing: float option with get, set
        abstract center: ResizeArray<float> option with get, set
        abstract glyphs: string option with get, set
        abstract layers: ResizeArray<AnyLayer> option with get, set
        abstract metadata: obj option with get, set
        abstract name: string option with get, set
        abstract pitch: float option with get, set
        abstract light: Light option with get, set
        abstract sources: Sources option with get, set
        abstract sprite: string option with get, set
        abstract transition: Transition option with get, set
        abstract version: float with get, set
        abstract zoom: float option with get, set

    type [<AllowNullLiteral>] Transition =
        abstract delay: float option with get, set
        abstract duration: float option with get, set

    type [<AllowNullLiteral>] Light =
        abstract anchor: LightAnchor option with get, set
        abstract position: ResizeArray<float> option with get, set
        abstract ``position-transition``: Transition option with get, set
        abstract color: string option with get, set
        abstract ``color-transition``: Transition option with get, set
        abstract intensity: float option with get, set
        abstract ``intensity-transition``: Transition option with get, set

    type [<AllowNullLiteral>] Sources =
        [<EmitIndexer>] abstract Item: sourceName: string -> AnySourceData with get, set

    type PromoteIdSpecification =
        U2<string, string>

    type AnySourceData =
        U7<GeoJSONSourceRaw, VideoSourceRaw, ImageSourceRaw, CanvasSourceRaw, VectorSource, RasterSource, RasterDemSource>

    type [<AllowNullLiteral>] VectorSourceImpl =
        inherit VectorSource
        /// <summary>Sets the source `tiles` property and re-renders the map.</summary>
        /// <param name="tiles">An array of one or more tile source URLs, as in the TileJSON spec.</param>
        abstract setTiles: tiles: ResizeArray<string> -> VectorSourceImpl
        /// <summary>Sets the source `url` property and re-renders the map.</summary>
        /// <param name="url">A URL to a TileJSON resource. Supported protocols are `http:`, `https:`, and `mapbox://<Tileset ID>`.</param>
        abstract setUrl: url: string -> VectorSourceImpl

    type AnySourceImpl =
        U7<GeoJSONSource, VideoSource, ImageSource, CanvasSource, VectorSourceImpl, RasterSource, RasterDemSource>

    type [<AllowNullLiteral>] Source =
        abstract ``type``: SourceType with get, set

    /// GeoJSONSource
    type [<AllowNullLiteral>] GeoJSONSourceRaw =
        inherit Source
        inherit GeoJSONSourceOptions
        abstract ``type``: string with get, set

    type [<AllowNullLiteral>] GeoJSONSource =
        inherit GeoJSONSourceRaw
        abstract ``type``: string with get, set
        abstract setData: data: U3<GeoJSON.Feature<GeoJSON.Geometry>, GeoJSON.FeatureCollection<GeoJSON.Geometry>, String> -> GeoJSONSource
        abstract getClusterExpansionZoom: clusterId: float * callback: (obj option -> float -> unit) -> GeoJSONSource
        abstract getClusterChildren: clusterId: float * callback: (obj option -> ResizeArray<GeoJSON.Feature<GeoJSON.Geometry>> -> unit) -> GeoJSONSource
        abstract getClusterLeaves: cluserId: float * limit: float * offset: float * callback: (obj option -> ResizeArray<GeoJSON.Feature<GeoJSON.Geometry>> -> unit) -> GeoJSONSource

    type [<AllowNullLiteral>] GeoJSONSourceStatic =
        [<EmitConstructor>] abstract Create: ?options: Mapboxgl.GeoJSONSourceOptions -> GeoJSONSource

    type [<AllowNullLiteral>] GeoJSONSourceOptions =
        abstract data: U3<GeoJSON.Feature<GeoJSON.Geometry>, GeoJSON.FeatureCollection<GeoJSON.Geometry>, string> option with get, set
        abstract maxzoom: float option with get, set
        abstract attribution: string option with get, set
        abstract buffer: float option with get, set
        abstract tolerance: float option with get, set
        abstract cluster: U2<float, bool> option with get, set
        abstract clusterRadius: float option with get, set
        abstract clusterMaxZoom: float option with get, set
        /// Minimum number of points necessary to form a cluster if clustering is enabled. Defaults to `2`.
        abstract clusterMinPoints: float option with get, set
        abstract clusterProperties: obj option with get, set
        abstract lineMetrics: bool option with get, set
        abstract generateId: bool option with get, set
        abstract promoteId: PromoteIdSpecification option with get, set
        abstract filter: obj option with get, set

    /// VideoSource
    type [<AllowNullLiteral>] VideoSourceRaw =
        inherit Source
        inherit VideoSourceOptions
        abstract ``type``: string with get, set

    type [<AllowNullLiteral>] VideoSource =
        inherit VideoSourceRaw
        abstract ``type``: string with get, set
        abstract getVideo: unit -> HTMLVideoElement
        abstract setCoordinates: coordinates: ResizeArray<ResizeArray<float>> -> VideoSource

    type [<AllowNullLiteral>] VideoSourceStatic =
        [<EmitConstructor>] abstract Create: ?options: Mapboxgl.VideoSourceOptions -> VideoSource

    type [<AllowNullLiteral>] VideoSourceOptions =
        abstract urls: ResizeArray<string> option with get, set
        abstract coordinates: ResizeArray<ResizeArray<float>> option with get, set

    /// ImageSource
    type [<AllowNullLiteral>] ImageSourceRaw =
        inherit Source
        inherit ImageSourceOptions
        abstract ``type``: string with get, set

    type [<AllowNullLiteral>] ImageSource =
        inherit ImageSourceRaw
        abstract ``type``: string with get, set
        abstract updateImage: options: ImageSourceOptions -> ImageSource
        abstract setCoordinates: coordinates: ResizeArray<ResizeArray<float>> -> ImageSource

    type [<AllowNullLiteral>] ImageSourceStatic =
        [<EmitConstructor>] abstract Create: ?options: Mapboxgl.ImageSourceOptions -> ImageSource

    type [<AllowNullLiteral>] ImageSourceOptions =
        abstract url: string option with get, set
        abstract coordinates: ResizeArray<ResizeArray<float>> option with get, set

    /// CanvasSource
    type [<AllowNullLiteral>] CanvasSourceRaw =
        inherit Source
        inherit CanvasSourceOptions
        abstract ``type``: string with get, set

    type [<AllowNullLiteral>] CanvasSource =
        inherit CanvasSourceRaw
        abstract ``type``: string with get, set
        abstract coordinates: ResizeArray<ResizeArray<float>> with get, set
        abstract canvas: U2<string, HTMLCanvasElement> with get, set
        abstract play: unit -> unit
        abstract pause: unit -> unit
        abstract getCanvas: unit -> HTMLCanvasElement
        abstract setCoordinates: coordinates: ResizeArray<ResizeArray<float>> -> CanvasSource

    type [<AllowNullLiteral>] CanvasSourceStatic =
        [<EmitConstructor>] abstract Create: unit -> CanvasSource

    type [<AllowNullLiteral>] CanvasSourceOptions =
        abstract coordinates: ResizeArray<ResizeArray<float>> with get, set
        abstract animate: bool option with get, set
        abstract canvas: U2<string, HTMLCanvasElement> with get, set

    type CameraFunctionSpecification<'T> =
        U4<string, Array<float * 'T>, string, Array<float * 'T>>

    type ExpressionSpecification =
        Array<obj>

    type PropertyValueSpecification<'T> =
        U3<'T, CameraFunctionSpecification<'T>, ExpressionSpecification>

    type [<AllowNullLiteral>] TerrainSpecification =
        abstract source: string with get, set
        abstract exaggeration: PropertyValueSpecification<float> option with get, set

    type [<AllowNullLiteral>] VectorSource =
        inherit Source
        abstract ``type``: string with get, set
        abstract url: string option with get, set
        abstract tiles: ResizeArray<string> option with get, set
        abstract bounds: ResizeArray<float> option with get, set
        abstract scheme: VectorSourceScheme option with get, set
        abstract minzoom: float option with get, set
        abstract maxzoom: float option with get, set
        abstract attribution: string option with get, set
        abstract promoteId: PromoteIdSpecification option with get, set

    type [<AllowNullLiteral>] RasterSource =
        inherit Source
        abstract ``type``: string with get, set
        abstract url: string option with get, set
        abstract tiles: ResizeArray<string> option with get, set
        abstract bounds: ResizeArray<float> option with get, set
        abstract minzoom: float option with get, set
        abstract maxzoom: float option with get, set
        abstract tileSize: float option with get, set
        abstract scheme: VectorSourceScheme option with get, set
        abstract attribution: string option with get, set

    type [<AllowNullLiteral>] RasterDemSource =
        inherit Source
        abstract ``type``: string with get, set
        abstract url: string option with get, set
        abstract tiles: ResizeArray<string> option with get, set
        abstract bounds: ResizeArray<float> option with get, set
        abstract minzoom: float option with get, set
        abstract maxzoom: float option with get, set
        abstract tileSize: float option with get, set
        abstract attribution: string option with get, set
        abstract encoding: RasterDemSourceEncoding option with get, set

    /// LngLat
    type [<AllowNullLiteral>] LngLat =
        abstract lng: float with get, set
        abstract lat: float with get, set
        /// Return a new LngLat object whose longitude is wrapped to the range (-180, 180).
        abstract wrap: unit -> Mapboxgl.LngLat
        /// Return a LngLat as an array
        abstract toArray: unit -> ResizeArray<float>
        /// Return a LngLat as a string
        abstract toString: unit -> string
        /// Returns the approximate distance between a pair of coordinates in meters
        /// Uses the Haversine Formula (from R.W. Sinnott, "Virtues of the Haversine", Sky and Telescope, vol. 68, no. 2, 1984, p. 159)
        abstract distanceTo: lngLat: LngLat -> float<UnitNames.meter>
        abstract toBounds: radius: float -> LngLatBounds

    /// LngLat
    type [<AllowNullLiteral>] LngLatStatic =
        [<EmitConstructor>] abstract Create: lng: float * lat: float -> LngLat
        abstract convert: input: LngLatLike -> Mapboxgl.LngLat

    /// LngLatBounds
    type [<AllowNullLiteral>] LngLatBounds =
        abstract sw: LngLatLike with get, set
        abstract ne: LngLatLike with get, set
        abstract setNorthEast: ne: LngLatLike -> LngLatBounds
        abstract setSouthWest: sw: LngLatLike -> LngLatBounds
        /// Check if the point is within the bounding box.
        abstract contains: lnglat: LngLatLike -> bool
        /// Extend the bounds to include a given LngLat or LngLatBounds.
        abstract extend: obj: U2<Mapboxgl.LngLatLike, Mapboxgl.LngLatBoundsLike> -> LngLatBounds
        /// Get the point equidistant from this box's corners
        abstract getCenter: unit -> Mapboxgl.LngLat
        /// Get southwest corner
        abstract getSouthWest: unit -> Mapboxgl.LngLat
        /// Get northeast corner
        abstract getNorthEast: unit -> Mapboxgl.LngLat
        /// Get northwest corner
        abstract getNorthWest: unit -> Mapboxgl.LngLat
        /// Get southeast corner
        abstract getSouthEast: unit -> Mapboxgl.LngLat
        /// Get west edge longitude
        abstract getWest: unit -> float
        /// Get south edge latitude
        abstract getSouth: unit -> float
        /// Get east edge longitude
        abstract getEast: unit -> float
        /// Get north edge latitude
        abstract getNorth: unit -> float
        /// Returns a LngLatBounds as an array
        abstract toArray: unit -> ResizeArray<ResizeArray<float>>
        /// Return a LngLatBounds as a string
        abstract toString: unit -> string
        /// Returns a boolean
        abstract isEmpty: unit -> bool

    /// LngLatBounds
    type [<AllowNullLiteral>] LngLatBoundsStatic =
        [<EmitConstructor>] abstract Create: ?boundsLike: U2<LngLatLike * LngLatLike, float * float * float * float> -> LngLatBounds
        [<EmitConstructor>] abstract Create: sw: LngLatLike * ne: LngLatLike -> LngLatBounds
        /// Convert an array to a LngLatBounds object, or return an existing LngLatBounds object unchanged.
        abstract convert: input: LngLatBoundsLike -> Mapboxgl.LngLatBounds

    /// Point
    type [<AllowNullLiteral>] Point =
        abstract x: float with get, set
        abstract y: float with get, set
        abstract clone: unit -> Point
        abstract add: p: Point -> Point
        abstract sub: p: Point -> Point
        abstract mult: k: float -> Point
        abstract div: k: float -> Point
        abstract rotate: a: float -> Point
        abstract matMult: m: float -> Point
        abstract unit: unit -> Point
        abstract perp: unit -> Point
        abstract round: unit -> Point
        abstract mag: unit -> float
        abstract equals: p: Point -> bool
        abstract dist: p: Point -> float
        abstract distSqr: p: Point -> float
        abstract angle: unit -> float
        abstract angleTo: p: Point -> float
        abstract angleWidth: p: Point -> float
        abstract angleWithSep: x: float * y: float -> float

    /// Point
    type [<AllowNullLiteral>] PointStatic =
        [<EmitConstructor>] abstract Create: x: float * y: float -> Point
        abstract convert: a: PointLike -> Point

    /// MercatorCoordinate
    type [<AllowNullLiteral>] MercatorCoordinate =
        /// The x component of the position.
        abstract x: float with get, set
        /// The y component of the position.
        abstract y: float with get, set
        /// The z component of the position.
        abstract z: float option with get, set
        /// Returns the altitude in meters of the coordinate.
        abstract toAltitude: unit -> float
        /// Returns the LngLat for the coordinate.
        abstract toLngLat: unit -> LngLat
        /// Returns the distance of 1 meter in MercatorCoordinate units at this latitude.
        /// 
        /// For coordinates in real world units using meters, this naturally provides the
        /// scale to transform into MercatorCoordinates.
        abstract meterInMercatorCoordinateUnits: unit -> float

    /// MercatorCoordinate
    type [<AllowNullLiteral>] MercatorCoordinateStatic =
        [<EmitConstructor>] abstract Create: x: float * y: float * ?z: float -> MercatorCoordinate
        /// Project a LngLat to a MercatorCoordinate.
        abstract fromLngLat: lngLatLike: LngLatLike * ?altitude: float -> MercatorCoordinate

    /// Marker
    type [<AllowNullLiteral>] Marker =
        inherit Evented
        abstract addTo: map: Map -> Marker
        abstract remove: unit -> Marker
        abstract getLngLat: unit -> LngLat
        abstract setLngLat: lngLat: LngLatLike -> Marker
        abstract getElement: unit -> HTMLElement
        abstract setPopup: ?popup: Popup -> Marker
        abstract getPopup: unit -> Popup
        abstract togglePopup: unit -> Marker
        abstract getOffset: unit -> PointLike
        abstract setOffset: offset: PointLike -> Marker
        abstract setDraggable: shouldBeDraggable: bool -> Marker
        abstract isDraggable: unit -> bool
        abstract getRotation: unit -> float
        abstract setRotation: rotation: float -> Marker
        abstract getRotationAlignment: unit -> Alignment
        abstract setRotationAlignment: alignment: Alignment -> Marker
        abstract getPitchAlignment: unit -> Alignment
        abstract setPitchAlignment: alignment: Alignment -> Marker

    /// Marker
    type [<AllowNullLiteral>] MarkerStatic =
        [<EmitConstructor>] abstract Create: ?options: Mapboxgl.MarkerOptions -> Marker
        [<EmitConstructor>] abstract Create: ?element: HTMLElement * ?options: Mapboxgl.MarkerOptions -> Marker

    type [<StringEnum>] [<RequireQualifiedAccess>] Alignment =
//        | Map
        | Viewport
        | Auto

    type [<AllowNullLiteral>] MarkerOptions =
        /// DOM element to use as a marker. The default is a light blue, droplet-shaped SVG marker
        abstract element: HTMLElement option with get, set
        /// The offset in pixels as a PointLike object to apply relative to the element's center. Negatives indicate left and up.
        abstract offset: PointLike option with get, set
        /// A string indicating the part of the Marker that should be positioned closest to the coordinate set via Marker.setLngLat.
        /// Options are `'center'`, `'top'`, `'bottom'`, `'left'`, `'right'`, `'top-left'`, `'top-right'`, `'bottom-left'`, and `'bottom-right'`.
        /// The default value os `'center'`
        abstract anchor: Anchor option with get, set
        /// The color to use for the default marker if options.element is not provided. The default is light blue (#3FB1CE).
        abstract color: string option with get, set
        /// A boolean indicating whether or not a marker is able to be dragged to a new position on the map. The default value is false
        abstract draggable: bool option with get, set
        /// The max number of pixels a user can shift the mouse pointer during a click on the marker for it to be considered a valid click
        /// (as opposed to a marker drag). The default (0) is to inherit map's clickTolerance.
        abstract clickTolerance: float option with get, set
        /// The rotation angle of the marker in degrees, relative to its `rotationAlignment` setting. A positive value will rotate the marker clockwise.
        /// The default value is 0.
        abstract rotation: float option with get, set
        /// `map` aligns the `Marker`'s rotation relative to the map, maintaining a bearing as the map rotates.
        /// `viewport` aligns the `Marker`'s rotation relative to the viewport, agnostic to map rotations.
        /// `auto` is equivalent to `viewport`.
        /// The default value is `auto`
        abstract rotationAlignment: Alignment option with get, set
        /// `map` aligns the `Marker` to the plane of the map.
        /// `viewport` aligns the `Marker` to the plane of the viewport.
        /// `auto` automatically matches the value of `rotationAlignment`.
        /// The default value is `auto`.
        abstract pitchAlignment: Alignment option with get, set
        /// The scale to use for the default marker if options.element is not provided.
        /// The default scale (1) corresponds to a height of `41px` and a width of `27px`.
        abstract scale: float option with get, set

    /// Evented
    type [<AllowNullLiteral>] Evented =
        abstract on: ``type``: string * listener: Function -> Evented
        abstract off: ?``type``: U2<string, obj option> * ?listener: Function -> Evented
        abstract once: ``type``: string * listener: Function -> Evented
        abstract fire: ``type``: string * ?properties: EventedFireProperties -> Evented

    type [<AllowNullLiteral>] EventedFireProperties =
        [<EmitIndexer>] abstract Item: key: string -> obj option with get, set

    /// Evented
    type [<AllowNullLiteral>] EventedStatic =
        [<EmitConstructor>] abstract Create: unit -> Evented

    /// StyleOptions
    type [<AllowNullLiteral>] StyleOptions =
        abstract transition: bool option with get, set

    type [<AllowNullLiteral>] MapboxGeoJSONFeature =
        interface end

    type [<AllowNullLiteral>] EventData =
        [<EmitIndexer>] abstract Item: key: string -> obj option with get, set

    type MapboxEvent =
        MapboxEvent<obj>

    type [<AllowNullLiteral>] MapboxEvent<'TOrig> =
        abstract ``type``: string with get, set
        abstract target: Map with get, set
        abstract originalEvent: 'TOrig with get, set

    type [<AllowNullLiteral>] MapboxEventStatic =
        [<EmitConstructor>] abstract Create: unit -> MapboxEvent<'TOrig>

    type [<AllowNullLiteral>] MapMouseEvent =
        inherit MapboxEvent<MouseEvent>
        abstract ``type``: MapMouseEventType with get, set
        abstract point: Point with get, set
        abstract lngLat: LngLat with get, set
        abstract preventDefault: unit -> unit
        abstract defaultPrevented: bool with get, set

    type [<AllowNullLiteral>] MapMouseEventStatic =
        [<EmitConstructor>] abstract Create: unit -> MapMouseEvent

    type [<AllowNullLiteral>] MapLayerMouseEvent =
        interface end

    type [<AllowNullLiteral>] MapTouchEvent =
        inherit MapboxEvent<TouchEvent>
        abstract ``type``: MapTouchEventType with get, set
        abstract point: Point with get, set
        abstract lngLat: LngLat with get, set
        abstract points: ResizeArray<Point> with get, set
        abstract lngLats: ResizeArray<LngLat> with get, set
        abstract preventDefault: unit -> unit
        abstract defaultPrevented: bool with get, set

    type [<AllowNullLiteral>] MapTouchEventStatic =
        [<EmitConstructor>] abstract Create: unit -> MapTouchEvent

    type [<AllowNullLiteral>] MapLayerTouchEvent =
        interface end

    type [<AllowNullLiteral>] MapWheelEvent =
        inherit MapboxEvent<WheelEvent>
        abstract ``type``: string with get, set
        abstract preventDefault: unit -> unit
        abstract defaultPrevented: bool with get, set

    type [<AllowNullLiteral>] MapWheelEventStatic =
        [<EmitConstructor>] abstract Create: unit -> MapWheelEvent

    type [<AllowNullLiteral>] MapBoxZoomEvent =
        inherit MapboxEvent<MouseEvent>
        abstract ``type``: MapBoxZoomEventType with get, set
        abstract boxZoomBounds: LngLatBounds with get, set

    type MapDataEvent =
        U2<MapSourceDataEvent, MapStyleDataEvent>

    type [<AllowNullLiteral>] MapStyleDataEvent =
        inherit MapboxEvent
        abstract dataType: string with get, set

    type [<AllowNullLiteral>] MapSourceDataEvent =
        inherit MapboxEvent
        abstract dataType: string with get, set
        abstract isSourceLoaded: bool with get, set
        abstract source: Source with get, set
        abstract sourceId: string with get, set
        abstract sourceDataType: MapSourceDataEventSourceDataType with get, set
        abstract tile: obj option with get, set
        abstract coord: Coordinate with get, set

    type [<AllowNullLiteral>] Coordinate =
        abstract canonical: CanonicalCoordinate with get, set
        abstract wrap: float with get, set
        abstract key: float with get, set

    type [<AllowNullLiteral>] CanonicalCoordinate =
        abstract x: float with get, set
        abstract y: float with get, set
        abstract z: float with get, set
        abstract key: float with get, set
        abstract equals: coord: CanonicalCoordinate -> bool

    type [<AllowNullLiteral>] MapContextEvent =
        inherit MapboxEvent<WebGLContextEvent>
        abstract ``type``: MapContextEventType with get, set

    type [<AllowNullLiteral>] ErrorEvent =
        inherit MapboxEvent
        abstract ``type``: string with get, set
        abstract error: Error with get, set

    type [<AllowNullLiteral>] ErrorEventStatic =
        [<EmitConstructor>] abstract Create: unit -> ErrorEvent

    /// FilterOptions
    type [<AllowNullLiteral>] FilterOptions =
        /// Whether to check if the filter conforms to the Mapbox GL Style Specification.
        /// Disabling validation is a performance optimization that should only be used
        /// if you have previously validated the values you will be passing to this function.
        abstract validate: bool option with get, set

    /// AnimationOptions
    type [<AllowNullLiteral>] AnimationOptions =
        /// Number in milliseconds
        abstract duration: float option with get, set
        /// A function taking a time in the range 0..1 and returning a number where 0 is the initial
        /// state and 1 is the final state.
        abstract easing: (float -> float) option with get, set
        /// point, origin of movement relative to map center
        abstract offset: PointLike option with get, set
        /// When set to false, no animation happens
        abstract animate: bool option with get, set
        /// If `true`, then the animation is considered essential and will not be affected by `prefers-reduced-motion`.
        /// Otherwise, the transition will happen instantly if the user has enabled the `reduced motion` accesibility feature in their operating system.
        abstract essential: bool option with get, set

    /// CameraOptions
    type [<AllowNullLiteral>] CameraOptions =
        /// Map center
        abstract center: LngLatLike option with get, set
        /// Map zoom level
        abstract zoom: float option with get, set
        /// Map rotation bearing in degrees counter-clockwise from north
        abstract bearing: float option with get, set
        /// Map angle in degrees at which the camera is looking at the ground
        abstract pitch: float option with get, set
        /// If zooming, the zoom center (defaults to map center)
        abstract around: LngLatLike option with get, set

    type [<AllowNullLiteral>] CameraForBoundsOptions =
        inherit CameraOptions
        abstract padding: U2<float, PaddingOptions> option with get, set
        abstract offset: PointLike option with get, set
        abstract maxZoom: float option with get, set

    type [<AllowNullLiteral>] CameraForBoundsResult =
        interface end

    /// FlyToOptions
    type [<AllowNullLiteral>] FlyToOptions =
        inherit AnimationOptions
        inherit CameraOptions
        abstract curve: float option with get, set
        abstract minZoom: float option with get, set
        abstract speed: float option with get, set
        abstract screenSpeed: float option with get, set
        abstract maxDuration: float option with get, set

    /// EaseToOptions
    type [<AllowNullLiteral>] EaseToOptions =
        inherit AnimationOptions
        inherit CameraOptions
        abstract delayEndEvents: float option with get, set

    type [<AllowNullLiteral>] FitBoundsOptions =
        inherit Mapboxgl.FlyToOptions
        abstract linear: bool option with get, set
        abstract padding: U2<float, Mapboxgl.PaddingOptions> option with get, set
        /// point, origin of movement relative to map center
        abstract offset: Mapboxgl.PointLike option with get, set
        abstract maxZoom: float option with get, set
        abstract maxDuration: float option with get, set

    type [<AllowNullLiteral>] MapEventType =
        abstract error: ErrorEvent with get, set
        abstract load: MapboxEvent with get, set
        abstract idle: MapboxEvent with get, set
        abstract remove: MapboxEvent with get, set
        abstract render: MapboxEvent with get, set
        abstract resize: MapboxEvent with get, set
        abstract webglcontextlost: MapContextEvent with get, set
        abstract webglcontextrestored: MapContextEvent with get, set
        abstract dataloading: MapDataEvent with get, set
        abstract data: MapDataEvent with get, set
        abstract tiledataloading: MapDataEvent with get, set
        abstract sourcedataloading: MapSourceDataEvent with get, set
        abstract styledataloading: MapStyleDataEvent with get, set
        abstract sourcedata: MapSourceDataEvent with get, set
        abstract styledata: MapStyleDataEvent with get, set
        abstract boxzoomcancel: MapBoxZoomEvent with get, set
        abstract boxzoomstart: MapBoxZoomEvent with get, set
        abstract boxzoomend: MapBoxZoomEvent with get, set
        abstract touchcancel: MapTouchEvent with get, set
        abstract touchmove: MapTouchEvent with get, set
        abstract touchend: MapTouchEvent with get, set
        abstract touchstart: MapTouchEvent with get, set
        abstract click: MapMouseEvent with get, set
        abstract contextmenu: MapMouseEvent with get, set
        abstract dblclick: MapMouseEvent with get, set
        abstract mousemove: MapMouseEvent with get, set
        abstract mouseup: MapMouseEvent with get, set
        abstract mousedown: MapMouseEvent with get, set
        abstract mouseout: MapMouseEvent with get, set
        abstract mouseover: MapMouseEvent with get, set
        abstract movestart: MapboxEvent<U3<MouseEvent, TouchEvent, WheelEvent> option> with get, set
        abstract move: MapboxEvent<U3<MouseEvent, TouchEvent, WheelEvent> option> with get, set
        abstract moveend: MapboxEvent<U3<MouseEvent, TouchEvent, WheelEvent> option> with get, set
        abstract zoomstart: MapboxEvent<U3<MouseEvent, TouchEvent, WheelEvent> option> with get, set
        abstract zoom: MapboxEvent<U3<MouseEvent, TouchEvent, WheelEvent> option> with get, set
        abstract zoomend: MapboxEvent<U3<MouseEvent, TouchEvent, WheelEvent> option> with get, set
        abstract rotatestart: MapboxEvent<U2<MouseEvent, TouchEvent> option> with get, set
        abstract rotate: MapboxEvent<U2<MouseEvent, TouchEvent> option> with get, set
        abstract rotateend: MapboxEvent<U2<MouseEvent, TouchEvent> option> with get, set
        abstract dragstart: MapboxEvent<U2<MouseEvent, TouchEvent> option> with get, set
        abstract drag: MapboxEvent<U2<MouseEvent, TouchEvent> option> with get, set
        abstract dragend: MapboxEvent<U2<MouseEvent, TouchEvent> option> with get, set
        abstract pitchstart: MapboxEvent<U2<MouseEvent, TouchEvent> option> with get, set
        abstract pitch: MapboxEvent<U2<MouseEvent, TouchEvent> option> with get, set
        abstract pitchend: MapboxEvent<U2<MouseEvent, TouchEvent> option> with get, set
        abstract wheel: MapWheelEvent with get, set

    type [<AllowNullLiteral>] MapLayerEventType =
        abstract click: MapLayerMouseEvent with get, set
        abstract dblclick: MapLayerMouseEvent with get, set
        abstract mousedown: MapLayerMouseEvent with get, set
        abstract mouseup: MapLayerMouseEvent with get, set
        abstract mousemove: MapLayerMouseEvent with get, set
        abstract mouseenter: MapLayerMouseEvent with get, set
        abstract mouseleave: MapLayerMouseEvent with get, set
        abstract mouseover: MapLayerMouseEvent with get, set
        abstract mouseout: MapLayerMouseEvent with get, set
        abstract contextmenu: MapLayerMouseEvent with get, set
        abstract touchstart: MapLayerTouchEvent with get, set
        abstract touchend: MapLayerTouchEvent with get, set
        abstract touchcancel: MapLayerTouchEvent with get, set

    type AnyLayout =
        obj

    type AnyPaint =
        obj

    type [<AllowNullLiteral>] Layer =
        abstract id: string with get, set
        abstract ``type``: string with get, set
        abstract metadata: obj option with get, set
        abstract ref: string option with get, set
        abstract source: U2<string, AnySourceData> option with get, set
        abstract ``source-layer``: string option with get, set
        abstract minzoom: float option with get, set
        abstract maxzoom: float option with get, set
        abstract interactive: bool option with get, set
        abstract filter: ResizeArray<obj option> option with get, set
        abstract layout: Layout option with get, set
        abstract paint: obj option with get, set

    type [<AllowNullLiteral>] BackgroundLayer =
        inherit Layer
        abstract ``type``: string with get, set
        abstract layout: BackgroundLayout option with get, set
        abstract paint: BackgroundPaint option with get, set

    type [<AllowNullLiteral>] CircleLayer =
        inherit Layer
        abstract ``type``: string with get, set
        abstract layout: CircleLayout option with get, set
        abstract paint: CirclePaint option with get, set

    type [<AllowNullLiteral>] FillExtrusionLayer =
        inherit Layer
        abstract ``type``: string with get, set
        abstract layout: FillExtrusionLayout option with get, set
        abstract paint: FillExtrusionPaint option with get, set

    type [<AllowNullLiteral>] FillLayer =
        inherit Layer
        abstract ``type``: string with get, set
        abstract layout: FillLayout option with get, set
        abstract paint: FillPaint option with get, set

    type [<AllowNullLiteral>] HeatmapLayer =
        inherit Layer
        abstract ``type``: string with get, set
        abstract layout: HeatmapLayout option with get, set
        abstract paint: HeatmapPaint option with get, set

    type [<AllowNullLiteral>] HillshadeLayer =
        inherit Layer
        abstract ``type``: string with get, set
        abstract layout: HillshadeLayout option with get, set
        abstract paint: HillshadePaint option with get, set

    type [<AllowNullLiteral>] LineLayer =
        inherit Layer
        abstract ``type``: string with get, set
        abstract layout: LineLayout option with get, set
        abstract paint: LinePaint option with get, set

    type [<AllowNullLiteral>] RasterLayer =
        inherit Layer
        abstract ``type``: string with get, set
        abstract layout: RasterLayout option with get, set
        abstract paint: RasterPaint option with get, set

    type [<AllowNullLiteral>] SymbolLayer =
        inherit Layer
        abstract ``type``: string with get, set
        abstract layout: SymbolLayout option with get, set
        abstract paint: SymbolPaint option with get, set

    type AnyLayer =
        obj

    type [<AllowNullLiteral>] CustomLayerInterface =
        /// A unique layer id.
        abstract id: string with get, set
        abstract ``type``: string with get, set
        abstract renderingMode: CustomLayerInterfaceRenderingMode option with get, set
        /// <summary>Optional method called when the layer has been removed from the Map with Map#removeLayer.
        /// This gives the layer a chance to clean up gl resources and event listeners.</summary>
        /// <param name="map">The Map this custom layer was just added to.</param>
        /// <param name="gl">The gl context for the map.</param>
        abstract onRemove: map: Mapboxgl.Map * gl: WebGLRenderingContext -> unit
        /// <summary>Optional method called when the layer has been added to the Map with Map#addLayer.
        /// This gives the layer a chance to initialize gl resources and register event listeners.</summary>
        /// <param name="map">The Map this custom layer was just added to.</param>
        /// <param name="gl">The gl context for the map.</param>
        abstract onAdd: map: Mapboxgl.Map * gl: WebGLRenderingContext -> unit
        /// <summary>Optional method called during a render frame to allow a layer to prepare resources
        /// or render into a texture.
        /// 
        /// The layer cannot make any assumptions about the current GL state and must bind a framebuffer
        /// before rendering.</summary>
        /// <param name="gl">The map's gl context.</param>
        /// <param name="matrix">The map's camera matrix. It projects spherical mercator coordinates to gl
        /// coordinates. The mercator coordinate  [0, 0] represents the top left corner of
        /// the mercator world and  [1, 1] represents the bottom right corner. When the
        /// renderingMode is  "3d" , the z coordinate is conformal. A box with identical
        /// x, y, and z lengths in mercator units would be rendered as a cube.
        /// MercatorCoordinate .fromLatLng can be used to project a  LngLat to a mercator
        /// coordinate.</param>
        abstract prerender: gl: WebGLRenderingContext * matrix: ResizeArray<float> -> unit
        /// <summary>Called during a render frame allowing the layer to draw into the GL context.
        /// 
        /// The layer can assume blending and depth state is set to allow the layer to properly blend
        /// and clip other layers. The layer cannot make any other assumptions about the current GL state.
        /// 
        /// If the layer needs to render to a texture, it should implement the prerender method to do this
        /// and only use the render method for drawing directly into the main framebuffer.
        /// 
        /// The blend function is set to gl.blendFunc(gl.ONE, gl.ONE_MINUS_SRC_ALPHA). This expects
        /// colors to be provided in premultiplied alpha form where the r, g and b values are already
        /// multiplied by the a value. If you are unable to provide colors in premultiplied form you may
        /// want to change the blend function to
        /// gl.blendFuncSeparate(gl.SRC_ALPHA, gl.ONE_MINUS_SRC_ALPHA, gl.ONE, gl.ONE_MINUS_SRC_ALPHA).</summary>
        /// <param name="gl">The map's gl context.</param>
        /// <param name="matrix">The map's camera matrix. It projects spherical mercator coordinates to gl
        /// coordinates. The mercator coordinate  [0, 0] represents the top left corner of
        /// the mercator world and  [1, 1] represents the bottom right corner. When the
        /// renderingMode is  "3d" , the z coordinate is conformal. A box with identical
        /// x, y, and z lengths in mercator units would be rendered as a cube.
        /// MercatorCoordinate .fromLatLng can be used to project a  LngLat to a mercator
        /// coordinate.</param>
        abstract render: gl: WebGLRenderingContext * matrix: ResizeArray<float> -> unit

    type [<AllowNullLiteral>] StyleFunction =
        abstract stops: ResizeArray<ResizeArray<obj option>> option with get, set
        abstract property: string option with get, set
        abstract ``base``: float option with get, set
        abstract ``type``: StyleFunctionType option with get, set
        abstract ``default``: obj option with get, set
        abstract colorSpace: StyleFunctionColorSpace option with get, set

    type [<StringEnum>] [<RequireQualifiedAccess>] Visibility =
        | Visible
        | None

    type [<AllowNullLiteral>] Layout =
        abstract visibility: Visibility option with get, set

    type [<AllowNullLiteral>] BackgroundLayout =
        inherit Layout

    type [<AllowNullLiteral>] BackgroundPaint =
        abstract ``background-color``: U2<string, Expression> option with get, set
        abstract ``background-color-transition``: Transition option with get, set
        abstract ``background-pattern``: string option with get, set
        abstract ``background-pattern-transition``: Transition option with get, set
        abstract ``background-opacity``: U2<float, Expression> option with get, set
        abstract ``background-opacity-transition``: Transition option with get, set

    type [<AllowNullLiteral>] FillLayout =
        inherit Layout
        abstract ``fill-sort-key``: U2<float, Expression> option with get, set

    type [<AllowNullLiteral>] FillPaint =
        abstract ``fill-antialias``: U2<bool, Expression> option with get, set
        abstract ``fill-opacity``: U3<float, StyleFunction, Expression> option with get, set
        abstract ``fill-opacity-transition``: Transition option with get, set
        abstract ``fill-color``: U3<string, StyleFunction, Expression> option with get, set
        abstract ``fill-color-transition``: Transition option with get, set
        abstract ``fill-outline-color``: U3<string, StyleFunction, Expression> option with get, set
        abstract ``fill-outline-color-transition``: Transition option with get, set
        abstract ``fill-translate``: ResizeArray<float> option with get, set
        abstract ``fill-translate-transition``: Transition option with get, set
        abstract ``fill-translate-anchor``: LightAnchor option with get, set
        abstract ``fill-pattern``: U2<string, Expression> option with get, set
        abstract ``fill-pattern-transition``: Transition option with get, set

    type [<AllowNullLiteral>] FillExtrusionLayout =
        inherit Layout

    type [<AllowNullLiteral>] FillExtrusionPaint =
        abstract ``fill-extrusion-opacity``: U2<float, Expression> option with get, set
        abstract ``fill-extrusion-opacity-transition``: Transition option with get, set
        abstract ``fill-extrusion-color``: U3<string, StyleFunction, Expression> option with get, set
        abstract ``fill-extrusion-color-transition``: Transition option with get, set
        abstract ``fill-extrusion-translate``: U2<ResizeArray<float>, Expression> option with get, set
        abstract ``fill-extrusion-translate-transition``: Transition option with get, set
        abstract ``fill-extrusion-translate-anchor``: LightAnchor option with get, set
        abstract ``fill-extrusion-pattern``: U2<string, Expression> option with get, set
        abstract ``fill-extrusion-pattern-transition``: Transition option with get, set
        abstract ``fill-extrusion-height``: U3<float, StyleFunction, Expression> option with get, set
        abstract ``fill-extrusion-height-transition``: Transition option with get, set
        abstract ``fill-extrusion-base``: U3<float, StyleFunction, Expression> option with get, set
        abstract ``fill-extrusion-base-transition``: Transition option with get, set
        abstract ``fill-extrusion-vertical-gradient``: bool option with get, set

    type [<AllowNullLiteral>] LineLayout =
        inherit Layout
        abstract ``line-cap``: LineLayoutLineCap option with get, set
        abstract ``line-join``: U2<Expression, string> option with get, set
        abstract ``line-miter-limit``: U2<float, Expression> option with get, set
        abstract ``line-round-limit``: U2<float, Expression> option with get, set
        abstract ``line-sort-key``: U2<float, Expression> option with get, set

    type [<AllowNullLiteral>] LinePaint =
        abstract ``line-opacity``: U3<float, StyleFunction, Expression> option with get, set
        abstract ``line-opacity-transition``: Transition option with get, set
        abstract ``line-color``: U3<string, StyleFunction, Expression> option with get, set
        abstract ``line-color-transition``: Transition option with get, set
        abstract ``line-translate``: U2<ResizeArray<float>, Expression> option with get, set
        abstract ``line-translate-transition``: Transition option with get, set
        abstract ``line-translate-anchor``: LightAnchor option with get, set
        abstract ``line-width``: U3<float, StyleFunction, Expression> option with get, set
        abstract ``line-width-transition``: Transition option with get, set
        abstract ``line-gap-width``: U3<float, StyleFunction, Expression> option with get, set
        abstract ``line-gap-width-transition``: Transition option with get, set
        abstract ``line-offset``: U3<float, StyleFunction, Expression> option with get, set
        abstract ``line-offset-transition``: Transition option with get, set
        abstract ``line-blur``: U3<float, StyleFunction, Expression> option with get, set
        abstract ``line-blur-transition``: Transition option with get, set
        abstract ``line-dasharray``: U2<ResizeArray<float>, Expression> option with get, set
        abstract ``line-dasharray-transition``: Transition option with get, set
        abstract ``line-pattern``: U2<string, Expression> option with get, set
        abstract ``line-pattern-transition``: Transition option with get, set
        abstract ``line-gradient``: Expression option with get, set

    type [<AllowNullLiteral>] SymbolLayout =
        inherit Layout
        abstract ``symbol-placement``: SymbolLayoutSymbolPlacement option with get, set
        abstract ``symbol-spacing``: U2<float, Expression> option with get, set
        abstract ``symbol-avoid-edges``: bool option with get, set
        abstract ``symbol-z-order``: SymbolLayoutSymbolZOrder option with get, set
        abstract ``icon-allow-overlap``: U3<bool, StyleFunction, Expression> option with get, set
        abstract ``icon-ignore-placement``: bool option with get, set
        abstract ``icon-optional``: bool option with get, set
        abstract ``icon-rotation-alignment``: SymbolLayoutIconRotationAlignment option with get, set
        abstract ``icon-size``: U3<float, StyleFunction, Expression> option with get, set
        abstract ``icon-text-fit``: SymbolLayoutIconTextFit option with get, set
        abstract ``icon-text-fit-padding``: U2<ResizeArray<float>, Expression> option with get, set
        abstract ``icon-image``: U3<string, StyleFunction, Expression> option with get, set
        abstract ``icon-rotate``: U3<float, StyleFunction, Expression> option with get, set
        abstract ``icon-padding``: U2<float, Expression> option with get, set
        abstract ``icon-keep-upright``: bool option with get, set
        abstract ``icon-offset``: U3<ResizeArray<float>, StyleFunction, Expression> option with get, set
        abstract ``icon-anchor``: U3<Anchor, StyleFunction, Expression> option with get, set
        abstract ``icon-pitch-alignment``: SymbolLayoutIconRotationAlignment option with get, set
        abstract ``text-pitch-alignment``: SymbolLayoutIconRotationAlignment option with get, set
        abstract ``text-rotation-alignment``: SymbolLayoutIconRotationAlignment option with get, set
        abstract ``text-field``: U3<string, StyleFunction, Expression> option with get, set
        abstract ``text-font``: U3<string, ResizeArray<string>, Expression> option with get, set
        abstract ``text-size``: U3<float, StyleFunction, Expression> option with get, set
        abstract ``text-max-width``: U3<float, StyleFunction, Expression> option with get, set
        abstract ``text-line-height``: U2<float, Expression> option with get, set
        abstract ``text-letter-spacing``: U2<float, Expression> option with get, set
        abstract ``text-justify``: U2<Expression, string> option with get, set
        abstract ``text-anchor``: U3<Anchor, StyleFunction, Expression> option with get, set
        abstract ``text-max-angle``: U2<float, Expression> option with get, set
        abstract ``text-rotate``: U3<float, StyleFunction, Expression> option with get, set
        abstract ``text-padding``: U2<float, Expression> option with get, set
        abstract ``text-keep-upright``: bool option with get, set
        abstract ``text-transform``: U3<StyleFunction, Expression, string> option with get, set
        abstract ``text-offset``: U2<ResizeArray<float>, Expression> option with get, set
        abstract ``text-allow-overlap``: bool option with get, set
        abstract ``text-ignore-placement``: bool option with get, set
        abstract ``text-optional``: bool option with get, set
        abstract ``text-radial-offset``: U2<float, Expression> option with get, set
        abstract ``text-variable-anchor``: ResizeArray<Anchor> option with get, set
        abstract ``text-writing-mode``: ResizeArray<SymbolLayoutTextWritingMode> option with get, set
        abstract ``symbol-sort-key``: U2<float, Expression> option with get, set

    type [<AllowNullLiteral>] SymbolPaint =
        abstract ``icon-opacity``: U3<float, StyleFunction, Expression> option with get, set
        abstract ``icon-opacity-transition``: Transition option with get, set
        abstract ``icon-color``: U3<string, StyleFunction, Expression> option with get, set
        abstract ``icon-color-transition``: Transition option with get, set
        abstract ``icon-halo-color``: U3<string, StyleFunction, Expression> option with get, set
        abstract ``icon-halo-color-transition``: Transition option with get, set
        abstract ``icon-halo-width``: U3<float, StyleFunction, Expression> option with get, set
        abstract ``icon-halo-width-transition``: Transition option with get, set
        abstract ``icon-halo-blur``: U3<float, StyleFunction, Expression> option with get, set
        abstract ``icon-halo-blur-transition``: Transition option with get, set
        abstract ``icon-translate``: U2<ResizeArray<float>, Expression> option with get, set
        abstract ``icon-translate-transition``: Transition option with get, set
        abstract ``icon-translate-anchor``: LightAnchor option with get, set
        abstract ``text-opacity``: U3<float, StyleFunction, Expression> option with get, set
        abstract ``text-opacity-transition``: Transition option with get, set
        abstract ``text-color``: U3<string, StyleFunction, Expression> option with get, set
        abstract ``text-color-transition``: Transition option with get, set
        abstract ``text-halo-color``: U3<string, StyleFunction, Expression> option with get, set
        abstract ``text-halo-color-transition``: Transition option with get, set
        abstract ``text-halo-width``: U3<float, StyleFunction, Expression> option with get, set
        abstract ``text-halo-width-transition``: Transition option with get, set
        abstract ``text-halo-blur``: U3<float, StyleFunction, Expression> option with get, set
        abstract ``text-halo-blur-transition``: Transition option with get, set
        abstract ``text-translate``: U2<ResizeArray<float>, Expression> option with get, set
        abstract ``text-translate-transition``: Transition option with get, set
        abstract ``text-translate-anchor``: LightAnchor option with get, set

    type [<AllowNullLiteral>] RasterLayout =
        inherit Layout

    type [<AllowNullLiteral>] RasterPaint =
        abstract ``raster-opacity``: U2<float, Expression> option with get, set
        abstract ``raster-opacity-transition``: Transition option with get, set
        abstract ``raster-hue-rotate``: U2<float, Expression> option with get, set
        abstract ``raster-hue-rotate-transition``: Transition option with get, set
        abstract ``raster-brightness-min``: U2<float, Expression> option with get, set
        abstract ``raster-brightness-min-transition``: Transition option with get, set
        abstract ``raster-brightness-max``: U2<float, Expression> option with get, set
        abstract ``raster-brightness-max-transition``: Transition option with get, set
        abstract ``raster-saturation``: U2<float, Expression> option with get, set
        abstract ``raster-saturation-transition``: Transition option with get, set
        abstract ``raster-contrast``: U2<float, Expression> option with get, set
        abstract ``raster-contrast-transition``: Transition option with get, set
        abstract ``raster-fade-duration``: U2<float, Expression> option with get, set
        abstract ``raster-resampling``: RasterPaintRasterResampling option with get, set

    type [<AllowNullLiteral>] CircleLayout =
        inherit Layout
        abstract ``circle-sort-key``: U2<float, Expression> option with get, set

    type [<AllowNullLiteral>] CirclePaint =
        abstract ``circle-radius``: U3<float, StyleFunction, Expression> option with get, set
        abstract ``circle-radius-transition``: Transition option with get, set
        abstract ``circle-color``: U3<string, StyleFunction, Expression> option with get, set
        abstract ``circle-color-transition``: Transition option with get, set
        abstract ``circle-blur``: U3<float, StyleFunction, Expression> option with get, set
        abstract ``circle-blur-transition``: Transition option with get, set
        abstract ``circle-opacity``: U3<float, StyleFunction, Expression> option with get, set
        abstract ``circle-opacity-transition``: Transition option with get, set
        abstract ``circle-translate``: U2<ResizeArray<float>, Expression> option with get, set
        abstract ``circle-translate-transition``: Transition option with get, set
        abstract ``circle-translate-anchor``: LightAnchor option with get, set
        abstract ``circle-pitch-scale``: LightAnchor option with get, set
        abstract ``circle-pitch-alignment``: LightAnchor option with get, set
        abstract ``circle-stroke-width``: U3<float, StyleFunction, Expression> option with get, set
        abstract ``circle-stroke-width-transition``: Transition option with get, set
        abstract ``circle-stroke-color``: U3<string, StyleFunction, Expression> option with get, set
        abstract ``circle-stroke-color-transition``: Transition option with get, set
        abstract ``circle-stroke-opacity``: U3<float, StyleFunction, Expression> option with get, set
        abstract ``circle-stroke-opacity-transition``: Transition option with get, set

    type [<AllowNullLiteral>] HeatmapLayout =
        inherit Layout

    type [<AllowNullLiteral>] HeatmapPaint =
        abstract ``heatmap-radius``: U3<float, StyleFunction, Expression> option with get, set
        abstract ``heatmap-radius-transition``: Transition option with get, set
        abstract ``heatmap-weight``: U3<float, StyleFunction, Expression> option with get, set
        abstract ``heatmap-intensity``: U3<float, StyleFunction, Expression> option with get, set
        abstract ``heatmap-intensity-transition``: Transition option with get, set
        abstract ``heatmap-color``: U3<string, StyleFunction, Expression> option with get, set
        abstract ``heatmap-opacity``: U3<float, StyleFunction, Expression> option with get, set
        abstract ``heatmap-opacity-transition``: Transition option with get, set

    type [<AllowNullLiteral>] HillshadeLayout =
        inherit Layout

    type [<AllowNullLiteral>] HillshadePaint =
        abstract ``hillshade-illumination-direction``: U2<float, Expression> option with get, set
        abstract ``hillshade-illumination-anchor``: LightAnchor option with get, set
        abstract ``hillshade-exaggeration``: U2<float, Expression> option with get, set
        abstract ``hillshade-exaggeration-transition``: Transition option with get, set
        abstract ``hillshade-shadow-color``: U2<string, Expression> option with get, set
        abstract ``hillshade-shadow-color-transition``: Transition option with get, set
        abstract ``hillshade-highlight-color``: U2<string, Expression> option with get, set
        abstract ``hillshade-highlight-color-transition``: Transition option with get, set
        abstract ``hillshade-accent-color``: U2<string, Expression> option with get, set
        abstract ``hillshade-accent-color-transition``: Transition option with get, set

    type [<AllowNullLiteral>] MapAddImage =
        abstract width: float with get, set
        abstract height: float with get, set
        abstract data: U2<Uint8Array, Uint8ClampedArray> with get, set

    type [<AllowNullLiteral>] MapboxOptionsLocale =
        [<EmitIndexer>] abstract Item: key: string -> string with get, set

    type [<StringEnum>] [<RequireQualifiedAccess>] MapboxOptionsLogoPosition =
        | [<CompiledName "top-left">] TopLeft
        | [<CompiledName "top-right">] TopRight
        | [<CompiledName "bottom-left">] BottomLeft
        | [<CompiledName "bottom-right">] BottomRight

    type [<StringEnum>] [<RequireQualifiedAccess>] RequestParametersCredentials =
        | [<CompiledName "same-origin">] SameOrigin
        | Include

    type [<AllowNullLiteral>] RequestParametersHeaders =
        [<EmitIndexer>] abstract Item: header: string -> obj option with get, set

    type [<StringEnum>] [<RequireQualifiedAccess>] RequestParametersMethod =
        | [<CompiledName "GET">] GET
        | [<CompiledName "POST">] POST
        | [<CompiledName "PUT">] PUT

    type [<StringEnum>] [<RequireQualifiedAccess>] LightAnchor =
//        | Map
        | Viewport

    type [<StringEnum>] [<RequireQualifiedAccess>] SourceType =
        | Vector
        | Raster
        | [<CompiledName "raster-dem">] RasterDem
        | Geojson
        | Image
        | Video
        | Canvas

    type [<StringEnum>] [<RequireQualifiedAccess>] VectorSourceScheme =
        | Xyz
        | Tms

    type [<StringEnum>] [<RequireQualifiedAccess>] RasterDemSourceEncoding =
        | Terrarium
        | Mapbox

    type [<StringEnum>] [<RequireQualifiedAccess>] MapMouseEventType =
        | Mousedown
        | Mouseup
        | Click
        | Dblclick
        | Mousemove
        | Mouseover
        | Mouseenter
        | Mouseleave
        | Mouseout
        | Contextmenu

    type [<StringEnum>] [<RequireQualifiedAccess>] MapTouchEventType =
        | Touchstart
        | Touchend
        | Touchcancel

    type [<StringEnum>] [<RequireQualifiedAccess>] MapBoxZoomEventType =
        | Boxzoomstart
        | Boxzoomend
        | Boxzoomcancel

    type [<StringEnum>] [<RequireQualifiedAccess>] MapSourceDataEventSourceDataType =
        | Metadata
        | Content

    type [<StringEnum>] [<RequireQualifiedAccess>] MapContextEventType =
        | Webglcontextlost
        | Webglcontextrestored

    type [<StringEnum>] [<RequireQualifiedAccess>] CustomLayerInterfaceRenderingMode =
        | [<CompiledName "2d">] N2d
        | [<CompiledName "3d">] N3d

    type [<StringEnum>] [<RequireQualifiedAccess>] StyleFunctionType =
        | Identity
        | Exponential
        | Interval
        | Categorical

    type [<StringEnum>] [<RequireQualifiedAccess>] StyleFunctionColorSpace =
        | Rgb
        | Lab
        | Hcl

    type [<StringEnum>] [<RequireQualifiedAccess>] LineLayoutLineCap =
        | Butt
        | Round
        | Square

    type [<StringEnum>] [<RequireQualifiedAccess>] SymbolLayoutSymbolPlacement =
        | Point
        | Line
        | [<CompiledName "line-center">] LineCenter

    type [<StringEnum>] [<RequireQualifiedAccess>] SymbolLayoutSymbolZOrder =
        | [<CompiledName "viewport-y">] ViewportY
        | Source

    type [<StringEnum>] [<RequireQualifiedAccess>] SymbolLayoutIconRotationAlignment =
//        | Map
        | Viewport
        | Auto

    type [<StringEnum>] [<RequireQualifiedAccess>] SymbolLayoutIconTextFit =
        | None
        | Both
        | Width
        | Height

    type [<StringEnum>] [<RequireQualifiedAccess>] SymbolLayoutTextWritingMode =
        | Horizontal
        | Vertical

    type [<StringEnum>] [<RequireQualifiedAccess>] RasterPaintRasterResampling =
        | Linear
        | Nearest
