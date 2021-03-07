import { mapStyleRules, StyleDef } from './StyleEditor'
import mapboxgl, { AnyLayer } from 'mapbox-gl'

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
        // tiles: ['http://localhost:8082/{z}/{x}/{y}'],
        tiles: ['http://mushu:8082/{z}/{x}/{y}'],
      },
    },
    // TODO: This needs to be reversed; things at the bottom of the list cover things at the top
    layers: mapStyleRules(style, (rule, filter) => {
      const sharedLayerProps = {
        filter,
        source: 'postgis-tiles',
        // ST_AsMVT() uses 'default' as layer name
        'source-layer': 'default',
        minzoom: 14,
        maxzoom: 22,
      }

      let ret: AnyLayer[] = []

      // TODO: Support fill and line style
      if (rule.fillStyle) {
        const paint = rule.fillStyle.compileStyle()
        // TODO: Don't assume fill-color is set
        paint['fill-color'] = wrapInHoverDetection(paint['fill-color'])
        ret.push({
          ...sharedLayerProps,
          id: `${rule.id}-fill`,
          paint,
          type: 'fill',
        })
      }

      if (rule.lineStyle) {
        const paint = rule.lineStyle.compileStyle()
        // TODO: Don't assume line-color is set
        paint['line-color'] = wrapInHoverDetection(paint['line-color'])
        ret.push({
          ...sharedLayerProps,
          id: `${rule.id}-line`,
          type: 'line',
          paint,
        })
      }

      // TODO: Should this be a warn? Or just an FYI?
      if (ret.length === 0) {
        console.warn(`Invalid rule style ${JSON.stringify(rule, null, 2)}`)
      }

      return ret
    }),
  }
}
