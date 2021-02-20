import { StyleDef } from './StyleEditor'
import mapboxgl from 'mapbox-gl'

function compileLineLayer(
  id: string,
  filter: any[],
  paint: mapboxgl.LinePaint
): mapboxgl.LineLayer {
  return {
    id,
    filter,
    paint,
    type: 'line',
    source: 'postgis-tiles',
    // ST_AsMVT() uses 'default' as layer name
    'source-layer': 'default',
    minzoom: 14,
    maxzoom: 22,
  }
}

function compileFillLayer(
  id: string,
  filter: any[],
  paint: mapboxgl.FillPaint
): mapboxgl.FillLayer {
  return {
    id,
    filter,
    paint,
    type: 'fill',
    source: 'postgis-tiles',
    // ST_AsMVT() uses 'default' as layer name
    'source-layer': 'default',
    minzoom: 14,
    maxzoom: 22,
  }
}

export default function compileMapboxStyle(style: StyleDef): mapboxgl.Style {
  let id = 0
  let nextId = () => `postgis-tiles-layer-${id++}`
  return {
    version: 8,
    sources: {
      'postgis-tiles': {
        type: 'vector',
        tiles: ['http://localhost:8082/{z}/{x}/{y}'],
      },
    },
    layers: [
      compileLineLayer(nextId(), ['has', 'highway'], {
        'line-color': style.highwayColor,
        'line-width': style.highwayWidth,
      }),
      compileFillLayer(nextId(), ['has', 'building'], {
        'fill-color': style.buildingColor,
      }),
      compileFillLayer(nextId(), ['==', 'park', ['get', 'leisure']], {
        'fill-color': style.parkColor,
      }),
      compileFillLayer(nextId(), ['==', 'beach', ['get', 'natural']], {
        'fill-color': style.beachColor,
      }),
      compileFillLayer(nextId(), ['==', 'water', ['get', 'natural']], {
        'fill-color': style.waterColor,
      }),
    ],
  }
}
