import { SVG, Container as SVGContainer, Dom } from '@svgdotjs/svg.js'
import { Expression } from './expression'
import { mapStyleRules, StyleDef } from './StyleEditor'
/* eslint import/no-webpack-loader-syntax: off */
import layouts from '!!./makiLoader.js!./makiLoader.js'
import Modal from 'react-modal';
import { useState } from 'react';
import bboxClip from '@turf/bbox-clip'
import simplify from '@turf/simplify'
import area from '@turf/area'


Modal.setAppElement('#root');

export default function Exporter({
  map,
  style,
}: {
  map: mapboxgl.Map,
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
            let pathString = featureToLineString(coordinates).trim();
            if (!pathString) {
              return;
            }

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

          // TODO: Add support for Point/MultiPoint/GeometryCollection types
          if (feature.geometry.type !== 'Point' && feature.geometry.type !== 'MultiPoint' && feature.geometry.type !== 'GeometryCollection') {
            const bounds = map.getBounds();

            const clippedGeometry = bboxClip(feature.geometry, [bounds.getWest(), bounds.getSouth(), bounds.getEast(), bounds.getNorth()])
            
            switch(clippedGeometry.geometry.type) {
              case 'LineString':
                renderFeature(group, clippedGeometry.geometry.coordinates)
                break;
              case 'Polygon':
              case 'MultiLineString':
                const compoundGeometryGroup = group.group()
                clippedGeometry.geometry.coordinates.forEach(c => renderFeature(compoundGeometryGroup, c))
                break;
              case 'MultiPolygon':
                const multiPolygonGroup = group.group()
                clippedGeometry.geometry.coordinates.forEach((polygon) =>
                  polygon.forEach(c => renderFeature(multiPolygonGroup, c))
                )
                break

              default:
                // TODO: Add support for other geometry types
                console.log('Unsupported geometry', clippedGeometry.geometry)
            }
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

  const [modalIsOpen, setModalIsOpen] = useState(false);

  function closeModal() {
    setModalIsOpen(true);
  }

  function openModal() {
    setModalIsOpen(true);
  }

  function finishExport() {
    exportMap();
    setModalIsOpen(false);
  }

  return <span>
    <Modal 
      isOpen={modalIsOpen}
      onRequestClose={closeModal}
      style={{
        content: {
          top: '50%',
          left: '50%',
          right: 'auto',
          bottom: 'auto',
          marginRight: '-50%',
          transform: 'translate(-50%, -50%)',
          textAlign: 'center',
        },        
      }}
      contentLabel="Export Info"
      >
        <div style={{width: '300px'}}>
          <h1>Important Attribution Notice</h1>
          <p style={{textAlign: 'left'}}>
            Lasographer's geographic data is provided by OpenStreetMap. This community-run 
            project generously provides free, high quality map data. All they ask in return is that
            users of the data credit it appropriately. This means including text
            on the finished product that credits OpenStreetMap and links to the license
            page. For more information on how to comply with this requirement, refer
            to the <a href="https://wiki.osmfoundation.org/wiki/Licence/Attribution_Guidelines#Artwork.2C_household_goods.2C_and_clothing" target="_blank" rel="noreferrer">OSM attribution guidelines</a>.
          </p>
          <p>
            Sample attribution text:
          </p>
          <p style={{
            backgroundColor: '#999999'
          }}>
            © OpenStreetMap contributors<br/>
            openstreetmap.org/copyright
          </p>
          <button onClick={finishExport}>Export Design. I will add the attribution text.</button>
        </div>
    </Modal>
    <button onClick={openModal}>Export</button>
  </span>
}
