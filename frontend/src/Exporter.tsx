import { SVG, Container as SVGContainer } from '@svgdotjs/svg.js'
import { Expression } from './expression'
import { FillStyle, LineStyle, StyleDef } from './StyleEditor'

export default function Exporter({
  map,
  style,
}: {
  map: mapboxgl.Map
  style: StyleDef
}) {
  function exportMap() {
    const svg = SVG().size('100%', '100%')

    function featureToLineString(coordinates: GeoJSON.Position[]): string {
      return coordinates
        .map((c, idx: Number) => {
          const projected = map.project(c as [number, number])
          const command = idx === 0 ? 'M' : 'L'
          return `${command} ${projected.x} ${projected.y}`
        })
        .join(' ')
    }

    style.rules.forEach((rule) => {
      const expression = Expression.parse(
        rule.filter.compileFilter(),
        'boolean'
      )
      const group = svg.group()
      map
        ?.queryRenderedFeatures()
        .filter((f) => expression.evaluate(f))
        .forEach((feature) => {
          function renderFeature(
            group: SVGContainer,
            coordinates: GeoJSON.Position[]
          ) {
            const pathString = featureToLineString(coordinates)
            if (rule.style instanceof LineStyle) {
              let path = group.path(pathString)
              path
                .stroke({ color: rule.style.color, width: rule.style.width })
                .fill('none')
            } else if (rule.style instanceof FillStyle) {
              let path = group.path(`${pathString} Z`)
              path.fill(rule.style.color)
            }
          }

          switch (feature.geometry.type) {
            case 'LineString':
              renderFeature(group, feature.geometry.coordinates)
              break
            case 'MultiLineString':
              const multilineStringGroup = group.group()
              feature.geometry.coordinates.forEach((c) =>
                renderFeature(multilineStringGroup, c)
              )
              break
            case 'Point':
              // TODO: Add support for exporting points
              break
            default:
              console.log('Unsupported geometry', feature.geometry)
          }
        })
    })

    const blob = new Blob([svg.svg()], { type: 'image/svg+xml' })
    const el = document.createElement('a')
    el.href = URL.createObjectURL(blob)
    el.download = 'map.svg'
    el.click()
    URL.revokeObjectURL(el.href)
  }

  return <button onClick={exportMap}>Export</button>
}
