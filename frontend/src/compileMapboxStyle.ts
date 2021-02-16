import { StyleDef } from './StyleEditor';
import mapboxgl from 'mapbox-gl';

export default function compileMapboxStyle(style: StyleDef): mapboxgl.Style {
    return {
        version: 8,
        sources: {
          'postgis-tiles': {
            type: 'vector',
            tiles: ['http://localhost:8082/{z}/{x}/{y}'],
          },
        },
        layers: [
          {
            id: 'postgis-tiles-layer',
            type: 'line',
            source: 'postgis-tiles',
            // ST_AsMVT() uses 'default' as layer name
            'source-layer': 'default',
            filter: ['has', 'highway'],
            minzoom: 14,
            maxzoom: 22,
            paint: {
              'line-color': style.highwayColor,
              'line-width': style.highwayWidth,
            },
          },
          {
            id: 'postgis-tiles-layer2',
            type: 'fill',
            source: 'postgis-tiles',
            // ST_AsMVT() uses 'default' as layer name
            'source-layer': 'default',
            filter: ['has', 'building'],
            minzoom: 14,
            maxzoom: 22,
            paint: {
              'fill-color': style.buildingColor,
            },
          },
        ],
      }
}