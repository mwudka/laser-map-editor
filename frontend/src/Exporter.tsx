import { SVG, Container as SVGContainer } from '@svgdotjs/svg.js';
import { StyleDef } from "./StyleEditor";

export default function Exporter({map, style}: {map: mapboxgl.Map, style: StyleDef}) {

    function exportMap() {

        const svg = SVG().size("100%", "100%");

        function exportLineString(parent: SVGContainer, properties: GeoJSON.GeoJsonProperties, coordinates: GeoJSON.Position[]) {
            let svgPathString = coordinates.map((c, idx: Number) => {
                const projected = map.project(c as [number, number])
                const command = idx === 0 ? 'M' : 'L'
                return `${command} ${projected.x} ${projected.y}`
            }).join(' ');


            if (properties!['building']) {
                svgPathString += ' Z';
            }

            const path = parent.path(svgPathString);
            
            if (properties!['building']) {
                path.fill(style.buildingColor);
            }
            if (properties!['highway']) {
                path.stroke({color: style.highwayColor, width: style.highwayWidth}).fill('none');
            }
            
        }

        const buildingsGroup = svg.group();
        const highwaysGroup = svg.group();

        map?.queryRenderedFeatures().forEach(feature => {
            let group: SVGContainer;
            if (feature.properties!["building"]) {
                group = buildingsGroup;
            } else if (feature.properties!["highway"]) {
                group = highwaysGroup;
            } else {
                return;
            }

            switch(feature.geometry.type) {
                case "LineString":
                    exportLineString(group, feature.properties, feature.geometry.coordinates);
                    break;   
                case "MultiLineString":
                    feature.geometry.coordinates.forEach(c => exportLineString(group, feature.properties, c));
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