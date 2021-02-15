import { LngLat } from "mapbox-gl";
import { SVG } from '@svgdotjs/svg.js';

export default function Exporter({map}: {map: mapboxgl.Map}) {

    function exportMap() {

        const svg = SVG().size("100%", "100%");

        function exportLineString(coordinates: GeoJSON.Position[]) {
            let svgPathString = coordinates.map((c, idx: Number) => {
                const projected = map.project(c as [number, number])
                const command = idx === 0 ? 'M' : 'L'
                return `${command} ${projected.x} ${projected.y}`
            }).join(' ');

            svg.path(svgPathString).fill('#ff000000').stroke('#000000');
        }

        map?.queryRenderedFeatures().forEach(feature => {
            switch(feature.geometry.type) {
                case "LineString":
                    exportLineString(feature.geometry.coordinates);
                    break;   
                case "MultiLineString":
                    feature.geometry.coordinates.forEach(exportLineString);
                    break;    
                case "Point":
                    // TODO: Add support for exporting points
                    break;
                default:
                    console.log('Unsupported geometry', feature.geometry);
            }
        });

        const blob = new Blob([svg.svg()], {type: 'image/svg+xml'});
        const el = document.createElement('a');
        el.href = URL.createObjectURL(blob);
        el.download = "map.svg";
        el.click();
        URL.revokeObjectURL(el.href);
    }

    return <button onClick={exportMap}>Export</button>
}