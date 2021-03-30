import { SVG, Container as SVGContainer, Dom } from '@svgdotjs/svg.js'
import { Expression } from './expression'
import { mapStyleRules, StyleDef } from './StyleEditor'
/* eslint import/no-webpack-loader-syntax: off */
import layouts from '!!./makiLoader.js!./makiLoader.js'

export default function Exporter({
  map,
  style,
}: {
  map: mapboxgl.Map
  style: StyleDef
}) {
  function exportMap() {
    const svg = SVG().size('100%', '100%').attr('xmlns:inkscape', "http://www.inkscape.org/namespaces/inkscape")

    function featureToLineString(coordinates: GeoJSON.Position[]): string {
      return coordinates
        .map((c, idx: Number) => {
          const projected = map.project(c as [number, number])
          const command = idx === 0 ? 'M' : 'L'
          return `${command} ${projected.x} ${projected.y}`
        })
        .join(' ')
    }

    function inkscapeLayer<T extends Dom>(dom: T, layerName: string): T {
      const inkscapeNS = "http://www.inkscape.org/namespaces/inkscape"
      return dom.attr("inkscape:label", layerName, inkscapeNS).attr("inkscape:groupmode", "layer", inkscapeNS)
    }

    const featuresGroup = inkscapeLayer(svg.group(), "Features")

    mapStyleRules(style, (rule, filter) => {
      const expression = Expression.parse(
        filter,
        'boolean'
      )

      const group = inkscapeLayer(featuresGroup.group(), rule.filter.summary())
      map
        ?.queryRenderedFeatures()
        .filter((f) => expression.evaluate(f))
        .forEach((feature) => {
          function renderFeature(
            group: SVGContainer,
            coordinates: GeoJSON.Position[]
          ) {
            let pathString = featureToLineString(coordinates)

            // If filling, we need to close the path so that it renders properly
            if (rule.fillStyle) {
              pathString = `${pathString} Z`
            }
            const path = group.path(pathString)

            if (rule.fillStyle) {
              path.fill(rule.fillStyle.color)
            } else {
              path.fill('none')
            }

            if (rule.lineStyle) {
              path.stroke({ color: rule.lineStyle.color, width: rule.lineStyle.width, dasharray: rule.lineStyle.dashed ? "5,2.5" : undefined })
            }
          }

          switch (feature.geometry.type) {
            case 'LineString':
              renderFeature(group, feature.geometry.coordinates)
              break
            case 'Polygon':
            case 'MultiLineString':
              const compoundGeometryGroup = group.group()
              feature.geometry.coordinates.forEach((c) =>
                renderFeature(compoundGeometryGroup, c)
              )
              break
            case 'MultiPolygon':
              const multiPolygonGroup = group.group()
              feature.geometry.coordinates.forEach((polygon) =>
                polygon.forEach(c => renderFeature(multiPolygonGroup, c))
              )
              break
            case 'Point':
              // TODO: Add support for exporting points
              break
            default:
              // TODO: Add support for other geometry types
              console.log('Unsupported geometry', feature.geometry)
          }
        })
    })
    
    const poisGroup = inkscapeLayer(svg.group(), "POIs")

    style.savedPOIs.forEach(poi => {
      const pos = map.project(poi.position as [number, number])

      const currentPOIGroup = poisGroup.group()

      const text = currentPOIGroup.text(poi.text).stroke('#000000')
      // TODO: Positioning is slightly off
      text.cx(text.width() / 2)
      const spriteSVG = layouts[poi.sprite]
      // TODO: Don't assume it's defined
      const sprite = SVG().group().svg(spriteSVG, true)
      sprite.first().children().forEach(el => {
        currentPOIGroup.add(el.clone())
      })

      currentPOIGroup.cx(pos.x).cy(pos.y)

      
      poisGroup.add(currentPOIGroup)
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
