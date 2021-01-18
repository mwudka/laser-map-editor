// ts2fable 0.8.0-build.557
module rec GeoJSON
open Fable.Core

type Array<'T> = System.Collections.Generic.IList<'T>


type GeoJsonGeometryTypes =
    Geometry

type GeoJsonTypes =
    GeoJSON

type BBox =
    U2<float * float * float * float, float * float * float * float * float * float>

type Position =
    float*float

/// The base GeoJSON object.
/// https://tools.ietf.org/html/rfc7946#section-3
/// The GeoJSON specification also allows foreign members
/// (https://tools.ietf.org/html/rfc7946#section-6.1)
/// Developers should use "&" type in TypeScript or extend the interface
/// to add these foreign members.
type [<AllowNullLiteral>] GeoJsonObject =
    /// Specifies the type of GeoJSON object.
    abstract ``type``: GeoJsonTypes with get, set
    /// Bounding box of the coordinate range of the object's Geometries, Features, or Feature Collections.
    /// The value of the bbox member is an array of length 2*n where n is the number of dimensions
    /// represented in the contained geometries, with all axes of the most southwesterly point
    /// followed by all axes of the more northeasterly point.
    /// The axes order of a bbox follows the axes order of geometries.
    /// https://tools.ietf.org/html/rfc7946#section-5
    abstract bbox: BBox option with get, set

type GeoJSON =
    U3<Geometry, Feature, FeatureCollection>

type Geometry =
    U7<Point, MultiPoint, LineString, MultiLineString, Polygon, MultiPolygon, GeometryCollection>

type GeometryObject =
    Geometry

/// Point geometry object.
/// https://tools.ietf.org/html/rfc7946#section-3.1.2
type [<AllowNullLiteral>] Point =
    inherit GeoJsonObject
    /// Specifies the type of GeoJSON object.
    abstract ``type``: string with get, set
    abstract coordinates: Position with get, set

/// MultiPoint geometry object.
///   https://tools.ietf.org/html/rfc7946#section-3.1.3
type [<AllowNullLiteral>] MultiPoint =
    inherit GeoJsonObject
    /// Specifies the type of GeoJSON object.
    abstract ``type``: string with get, set
    abstract coordinates: ResizeArray<Position> with get, set

/// LineString geometry object.
/// https://tools.ietf.org/html/rfc7946#section-3.1.4
type [<AllowNullLiteral>] LineString =
    inherit GeoJsonObject
    /// Specifies the type of GeoJSON object.
    abstract ``type``: string with get, set
    abstract coordinates: ResizeArray<Position> with get, set

/// MultiLineString geometry object.
/// https://tools.ietf.org/html/rfc7946#section-3.1.5
type [<AllowNullLiteral>] MultiLineString =
    inherit GeoJsonObject
    /// Specifies the type of GeoJSON object.
    abstract ``type``: string with get, set
    abstract coordinates: ResizeArray<ResizeArray<Position>> with get, set

/// Polygon geometry object.
/// https://tools.ietf.org/html/rfc7946#section-3.1.6
type [<AllowNullLiteral>] Polygon =
    inherit GeoJsonObject
    /// Specifies the type of GeoJSON object.
    abstract ``type``: string with get, set
    abstract coordinates: ResizeArray<ResizeArray<Position>> with get, set

/// MultiPolygon geometry object.
/// https://tools.ietf.org/html/rfc7946#section-3.1.7
type [<AllowNullLiteral>] MultiPolygon =
    inherit GeoJsonObject
    /// Specifies the type of GeoJSON object.
    abstract ``type``: string with get, set
    abstract coordinates: ResizeArray<ResizeArray<ResizeArray<Position>>> with get, set

/// Geometry Collection
/// https://tools.ietf.org/html/rfc7946#section-3.1.8
type [<AllowNullLiteral>] GeometryCollection =
    inherit GeoJsonObject
    /// Specifies the type of GeoJSON object.
    abstract ``type``: string with get, set
    abstract geometries: ResizeArray<Geometry> with get, set

type GeoJsonProperties =
    obj option option

type Feature =
    Feature<Geometry, GeoJsonProperties>

type Feature<'G> =
    Feature<'G, GeoJsonProperties>

/// A feature object which contains a geometry and associated properties.
/// https://tools.ietf.org/html/rfc7946#section-3.2
type [<AllowNullLiteral>] Feature<'G, 'P> =
    inherit GeoJsonObject
    /// Specifies the type of GeoJSON object.
    abstract ``type``: string with get, set
    /// The feature's geometry
    abstract geometry: 'G with get, set
    /// A value that uniquely identifies this feature in a
    /// https://tools.ietf.org/html/rfc7946#section-3.2.
    abstract id: U2<string, float> option with get, set
    /// Properties associated with this feature.
    abstract properties: 'P with get, set

type FeatureCollection =
    FeatureCollection<Geometry, GeoJsonProperties>

type FeatureCollection<'G> =
    FeatureCollection<'G, GeoJsonProperties>

/// A collection of feature objects.
///   https://tools.ietf.org/html/rfc7946#section-3.3
type [<AllowNullLiteral>] FeatureCollection<'G, 'P> =
    inherit GeoJsonObject
    /// Specifies the type of GeoJSON object.
    abstract ``type``: string with get, set
    abstract features: Array<Feature<'G, 'P>> with get, set
