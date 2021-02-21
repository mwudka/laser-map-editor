import React, { useState, useRef, useEffect } from 'react'
import mapboxgl, { MapboxGeoJSONFeature } from 'mapbox-gl'
import MapboxGeocoder from '@mapbox/mapbox-gl-geocoder'
import SplitPane, { Pane } from 'react-split-pane'
import './App.css'
import './Resizer.css'
import Exporter from './Exporter'
import StyleEditor, { FillStyle, LineStyle, StyleDef, StyleFilter } from './StyleEditor'
import compileMapboxStyle from './compileMapboxStyle'
import FeatureInfo from './FeatureInfo'
import ReactDOM from 'react-dom'
import StyleRuleCreator from './StyleRuleCreator'

function App() {
  const mapContainer = useRef<HTMLDivElement>(null)
  const [stateMap, setStateMap] = useState<mapboxgl.Map>()
  const [hoveredFeatures, setHoveredFeatures] = useState<
    MapboxGeoJSONFeature[]
  >([])
  const [style, setStyle] = useState<StyleDef>({
    rules: [
      {
        id: 'default-park-color',
        filter: new StyleFilter('leisure', 'park'),
        style: new FillStyle('#00ff00'),
      },
      {
        id: 'default-building-rule',
        filter: new StyleFilter('building'),
        style: new FillStyle('#999999'),
      },
      {
        id: 'default-beach-color',
        filter: new StyleFilter('natural', 'beach'),
        style: new FillStyle('#ffff00'),
      },
      {
        id: 'default-water-color',
        filter: new StyleFilter('natural', 'water'),
        style: new FillStyle('#0000ff')
      }, 
      {
        id: 'default-highway-rule',
        filter: new StyleFilter('highway'),
        style: new LineStyle(4, '#444444'),
      },     
    ],
  })

  useEffect(() => {
    console.log('Creating mapboxgl map')
    const map = new mapboxgl.Map({
      container: mapContainer.current!,
      minZoom: 14,
      style: compileMapboxStyle(style),
      hash: true,
    })

    let hoveredFeatures: mapboxgl.MapboxGeoJSONFeature[] = []
    function setHoverState(
      feature: mapboxgl.MapboxGeoJSONFeature,
      hover: boolean
    ) {
      map.setFeatureState(
        {
          source: feature.source,
          sourceLayer: feature.sourceLayer,
          id: feature.id,
        },
        { hover }
      )
    }
    map.on('mousemove', (e) => {
      hoveredFeatures.forEach((f) => setHoverState(f, false))

      hoveredFeatures = map.queryRenderedFeatures(e.point).slice(0, 1)
      hoveredFeatures.forEach((f) => setHoverState(f, true))
      map.getCanvas().style.cursor = hoveredFeatures.length > 0 ? 'pointer' : ''
      setHoveredFeatures(hoveredFeatures)
    })
    map.on('mouseout', (e) => {
      hoveredFeatures.forEach((f) => setHoverState(f, false))
      hoveredFeatures = []
      setHoveredFeatures(hoveredFeatures)
    })
    map.on('click', (e) => {
      let features = map.queryRenderedFeatures(e.point)
      if (features.length === 0) {
        return
      }
      let clickedFeature = features[0]

      const el = document.createElement('div')

      ReactDOM.render(
        <StyleRuleCreator
          feature={clickedFeature}
          onRuleAdded={(rule) => {
            style.rules.push(rule)
            onStyleChange()
            popup.remove()
          }}
        />,
        el
      )

      const popup = new mapboxgl.Popup({
        closeButton: true,
        closeOnClick: true,
        focusAfterOpen: true,
        maxWidth: '30rem',
      })
        .setDOMContent(el)
        .setLngLat(e.lngLat)
        .addTo(map)
    })

    map.addControl(
      new MapboxGeocoder({
        accessToken: mapboxgl.accessToken,
        mapboxgl: map,
        marker: false,
      })
    )

    setStateMap(map)

    map.showTileBoundaries = true

    return () => {
      console.log('Removing map')
      map.remove()
    }

    // We only want this to run once. React docs say that empty deps array means only run effect hook once.
    // eslint-disable-next-line
  }, [])

  function onStyleChange() {
    console.log(
      'style changed',
      style,
      JSON.parse(JSON.stringify(compileMapboxStyle(style)))
    )
    setStyle({ ...style })
    stateMap?.setStyle(compileMapboxStyle(style), { diff: true })
  }

  return (
    <div>
      {stateMap && <Exporter map={stateMap} style={style}></Exporter>}
      <SplitPane split="vertical" minSize={300}>
        <SplitPane split="horizontal" minSize={200}>
          <Pane className="pane">
            <StyleEditor style={style} onStyleChange={onStyleChange} />
          </Pane>
          <Pane className="pane">
            <FeatureInfo features={hoveredFeatures} />
          </Pane>
        </SplitPane>

        <div ref={mapContainer} className="mapContainer" />
      </SplitPane>
    </div>
  )
}

export default App
