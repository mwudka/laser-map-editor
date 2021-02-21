import { StyleDef, StyleRule } from './StyleEditor'
import mapboxgl, { FillPaint, LinePaint } from 'mapbox-gl'
import StyleRuleCreator from './StyleRuleCreator'

function wrapInHoverDetection(
  expression: string | mapboxgl.StyleFunction | mapboxgl.Expression | undefined
): mapboxgl.Expression {
  return [
    'case',
    ['boolean', ['feature-state', 'hover'], false],
    '#ff0000',
    expression,
  ]
}

export default function compileMapboxStyle(style: StyleDef): mapboxgl.Style {
  return {
    version: 8,
    sources: {
      'postgis-tiles': {
        type: 'vector',
        tiles: ['http://localhost:8082/{z}/{x}/{y}'],
      },
    },
    layers: style.rules.map((rule) => {
      let filter = rule.filter.compileFilter()

      const sharedLayerProps = {
        id: rule.id,
        filter: rule.filter.compileFilter(),
        source: 'postgis-tiles',
        // ST_AsMVT() uses 'default' as layer name
        'source-layer': 'default',
        minzoom: 14,
        maxzoom: 22,
      }

      switch (rule.style.type) {
        case 'fill': {
          const paint = rule.style.compileStyle() as FillPaint
          // TODO: Don't assume fill-color is set
          paint['fill-color'] = wrapInHoverDetection(paint['fill-color'])
          return {
            ...sharedLayerProps,
            paint,
            type: 'fill',
          }
        }
        case 'line': {
          const paint = rule.style.compileStyle() as LinePaint
          // TODO: Don't assume line-color is set
          paint['line-color'] = wrapInHoverDetection(paint['line-color'])
          return {
            ...sharedLayerProps,
            type: 'line',
            paint,
          }
        }
      }
    }),
  }
}
