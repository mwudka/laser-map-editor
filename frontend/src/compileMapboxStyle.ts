import { StyleDef } from './StyleEditor'
import mapboxgl, { FillPaint, LinePaint } from 'mapbox-gl'

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
        tiles: ['http://100.125.119.76:8082/{z}/{x}/{y}'],
      },
    },
    layers: style.rules.map((rule) => {
      let filter = rule.filter.compileFilter()

      // TODO: Move this logic into a central place so the Exporter can use it too
      // TODO: Should be undefined check?
      if (!rule.filter.propertyValue) {
        // There is no property value set, so this rule might be less specific than other rules. If so, we need to exclude other rules from this rule. This lets the user override general rules with more specific rules.
        /**
         * TODO: Right now this breaks down the following situation:
         * 1. Default rule for 'highway key present' exists
         * 2. User clicks on a residential street, and opts to create a new rule for the specific street. Now there is a new rule for name=streetname
         * Only the overridden rule should apply, but instead both appear. That's because this code only detects more specific rules by looking at matching keys. Maybe the more specific rule should be created with 'highway present' AND 'name=streetname', so that it's obvious that it's more specific?
         */
        const allRuleFilters = style.rules.map((r) => r.filter)
        const moreSpecificFilters = allRuleFilters
          .filter(
            (f) => f.propertyKey === rule.filter.propertyKey && f.propertyValue
          )
          .map((f) => f.compileFilter())
        if (moreSpecificFilters.length > 0) {
          filter = ['all', filter, ['!', ['any', ...moreSpecificFilters]]]
          console.log(filter)
        }
      }

      const sharedLayerProps = {
        id: rule.id,
        filter,
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
