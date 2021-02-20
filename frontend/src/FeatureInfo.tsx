export default function FeatureInfo({features}: {features: mapboxgl.MapboxGeoJSONFeature[]}) {


    function formatFeature(feature: mapboxgl.MapboxGeoJSONFeature): string {
        return `id=${feature.id}: ${JSON.stringify(feature.properties, undefined, 2)}`;
    }

    return <pre>{features.map(formatFeature).join('\n\n')}</pre>
}