import { mapStyleRules, StyleDef } from './StyleEditor'
import mapboxgl, { AnyLayer, ExpressionName } from 'mapbox-gl'

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
  const universalLayerProps = {
    source: 'postgis-tiles',
    // ST_AsMVT() uses 'default' as layer name
    'source-layer': 'default',
    minzoom: 14,
    maxzoom: 22,
  }

  function andNoStyleMatches(filter: [ExpressionName, ...any]) {
    const matchesAnyStyleRule = ['any', ...style.rules.map(rule => rule.filter.compileFilter())]
    return ['all', 
      ['!', matchesAnyStyleRule],
      filter
    ]
  }

  console.log(andNoStyleMatches(['has', 'highway']))

  const ghostLayers: AnyLayer[] = [
    {
      type: "line",
      id: "ghost-highways",
      paint: {
        "line-color": wrapInHoverDetection('#C0C0C0'),
        "line-width": 3,
      },
      filter: andNoStyleMatches(['has', 'highway']),
      ...universalLayerProps
    },
    {
      type: "fill",
      id: "ghost-buildings",
      paint: {
        "fill-color": wrapInHoverDetection('#E0E0E0'),
      },
      filter: andNoStyleMatches(['has', 'building']),
      ...universalLayerProps
    },
    {
      type: "fill",
      id: "ghost-nature-fill",
      paint: {
        "fill-color": wrapInHoverDetection('#D0D0D0'),
      },
      filter: andNoStyleMatches(['any', 
          ['has', 'natural'],
          ['has', 'leisure']
      ]),
      ...universalLayerProps
    },
    {
      type: "line",
      id: "ghost-nature-line",
      paint: {
        "line-color": wrapInHoverDetection('#CCCCCC'),
      },
      filter: andNoStyleMatches(['any', 
          ['has', 'natural'],
          ['has', 'leisure']
      ]),
      ...universalLayerProps
    }
  ]


  return {
    version: 8,
    sources: {
      'postgis-tiles': {
        type: 'vector',
        // tiles: ['http://localhost:8082/{z}/{x}/{y}'],
        tiles: ['http://mushu:8082/{z}/{x}/{y}'],
      },
    },
    layers: ghostLayers.concat(mapStyleRules(style, (rule, filter) => {
      const sharedLayerProps = {
        filter,
        ...universalLayerProps
      }

      let ret: AnyLayer[] = []

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
    })),
  }
}
