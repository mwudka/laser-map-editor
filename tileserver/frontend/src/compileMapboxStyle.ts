import { mapStyleRules, StyleDef } from './StyleEditor'
import mapboxgl, { AnyLayer, ExpressionName } from 'mapbox-gl'

function wrapInHoverDetection(
  expression: string | mapboxgl.StyleFunction | mapboxgl.Expression | undefined,
  hoverValue: any = '#ff0000'
): mapboxgl.Expression {
  return [
    'case',
    ['boolean', ['feature-state', 'hover'], false],
    hoverValue,
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
    const matchesAnyStyleRule = [
      'any',
      ...style.rules.map((rule) => rule.filter.compileFilter()),
    ]
    return ['all', ['!', matchesAnyStyleRule], filter]
  }

  const ghostLayers: AnyLayer[] = [
    {
      type: 'line',
      id: 'ghost-highways',
      paint: {
        'line-color': wrapInHoverDetection('#C0C0C0'),
        'line-width': 3,
      },
      filter: andNoStyleMatches(['has', 'highway']),
      ...universalLayerProps,
    },
    {
      type: 'fill',
      id: 'ghost-buildings',
      paint: {
        'fill-color': wrapInHoverDetection('#E0E0E0'),
      },
      filter: andNoStyleMatches(['has', 'building']),
      ...universalLayerProps,
    },
    {
      type: 'fill',
      id: 'ghost-nature-fill',
      paint: {
        'fill-color': wrapInHoverDetection('#D0D0D0'),
      },
      filter: andNoStyleMatches([
        'any',
        ['has', 'natural'],
        ['has', 'leisure'],
      ]),
      ...universalLayerProps,
    },
    {
      type: 'line',
      id: 'ghost-nature-line',
      paint: {
        'line-color': wrapInHoverDetection('#CCCCCC'),
      },
      filter: andNoStyleMatches([
        'any',
        ['has', 'natural'],
        ['has', 'leisure'],
      ]),
      ...universalLayerProps,
    },
  ]

  const poiLayer: AnyLayer = {
    id: 'mapbox-pois',
    type: 'symbol',
    source: 'mapbox',
    'source-layer': 'poi_label',
    layout: {
      'text-field': ['coalesce', ['get', 'name_en'], ['get', 'name']],
      'icon-image': [
        'case',
        ['has', 'maki_beta'],
        ['image', ['concat', ['get', 'maki_beta'], '-15']],
        ['image', ['concat', ['get', 'maki'], '-15']],
      ],
      'icon-allow-overlap': false,
      'text-allow-overlap': false,
      'text-anchor': [
        'step',
        ['zoom'],
        ['step', ['get', 'sizerank'], 'center', 5, 'top'],
        17,
        ['step', ['get', 'sizerank'], 'center', 13, 'top'],
      ],
      'text-offset': [
        'step',
        ['zoom'],
        [
          'step',
          ['get', 'sizerank'],
          ['literal', [0, 0]],
          5,
          ['literal', [0, 0.75]],
        ],
        17,
        [
          'step',
          ['get', 'sizerank'],
          ['literal', [0, 0]],
          13,
          ['literal', [0, 0.75]],
        ],
      ],
    },
    paint: {
      'text-opacity': wrapInHoverDetection(
        [
          'case',
          [
            'any',
            ...style.savedPOIs.map((poi) => [
              '==',
              parseInt(poi.id, 10),
              ['id'],
            ]),
          ],
          1.0,
          0.5,
        ],
        1.0
      ),
      'text-color': 'black',
      'text-halo-width': 5,
      'text-halo-blur': 0.5,
      'text-halo-color': wrapInHoverDetection('white'),
    }
  }

  const customLayers = mapStyleRules(style, (rule, filter) => {
    const sharedLayerProps = {
      filter,
      ...universalLayerProps,
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
  })

  const layers = [...ghostLayers, ...customLayers, poiLayer]


  console.log('REACT_APP_GLYPHS', process.env.REACT_APP_GLYPHS)
  console.log('REACT_APP_SPRITE', process.env.REACT_APP_SPRITE)
  return {
    version: 8,
    sources: {
      'postgis-tiles': {
        type: 'vector',
        tiles: [`${window.location.origin}/api/v1/tile/{z}/{x}/{y}`],
      },
      mapbox: {
        url: 'mapbox://mapbox.mapbox-streets-v8',
        type: 'vector',
      },
    },
    glyphs: process.env.REACT_APP_GLYPHS,
    // TODO: Make this local icon sprites?
    sprite: process.env.REACT_APP_SPRITE,
    layers,
  }
}
