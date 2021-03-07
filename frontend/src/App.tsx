import React, { useState, useRef, useEffect } from 'react'
import mapboxgl from 'mapbox-gl'
import MapboxGeocoder from '@mapbox/mapbox-gl-geocoder'
import './App.css'
import './Resizer.css'
import Exporter from './Exporter'
import StyleEditor, { FillStyle, LineStyle, StyleDef, StyleFilter, StyleRule } from './StyleEditor'
import compileMapboxStyle from './compileMapboxStyle'
import ReactDOM from 'react-dom'
import StyleRuleCreator from './StyleRuleCreator'

function App() {
  const mapContainer = useRef<HTMLDivElement>(null)
  const [stateMap, setStateMap] = useState<mapboxgl.Map>()
  const [style, setStyle] = useState<StyleDef>({
    rules: [
      {
        id: 'default-highway-rule',
        filter: new StyleFilter('highway'),
        lineStyle: new LineStyle(2, '#444444'),
      },
      {
        id: 'default-building-rule',
        filter: new StyleFilter('building'),
        fillStyle: new FillStyle('#999999'),
      },
      {
        id: 'default-water-color',
        filter: new StyleFilter('natural', 'water'),
        fillStyle: new FillStyle('#0000ff')
      },
      {
        id: 'default-beach-color',
        filter: new StyleFilter('natural', 'beach'),
        fillStyle: new FillStyle('#ffff00'),
      },
      {
        id: 'default-park-color',
        filter: new StyleFilter('leisure', 'park'),
        fillStyle: new FillStyle('#00ff00'),
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

    const tooltipPopup = new mapboxgl.Popup({
      maxWidth: '30rem',
    })
      .addTo(map)

    map.on('mousemove', (e) => {
      hoveredFeatures.forEach((f) => setHoverState(f, false))

      hoveredFeatures = map.queryRenderedFeatures(e.point).slice(0, 1)
      hoveredFeatures.forEach((f) => setHoverState(f, true))
      map.getCanvas().style.cursor = hoveredFeatures.length > 0 ? 'pointer' : ''

      if (hoveredFeatures.length > 0) {
        tooltipPopup.setHTML('<pre>' + hoveredFeatures.map(feature => `id=${feature.id}: ${JSON.stringify(feature.properties, undefined, 2)}`).join('\n\n') + '</pre>')

        if (!tooltipPopup.isOpen()) {
          tooltipPopup.addTo(map);
        }
      } else {
        if (tooltipPopup.isOpen()) {
          tooltipPopup.remove();
        }
      }

      tooltipPopup.setLngLat(e.lngLat);
    })
    map.on('mouseout', (e) => {
      hoveredFeatures.forEach((f) => setHoverState(f, false))
      hoveredFeatures = []

      tooltipPopup.remove();
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
            style.rules.unshift(rule)
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

  function onRuleDelete(rule: StyleRule) {
    console.log('rule deleted', rule)
    style.rules = style.rules.filter(r => r.id !== rule.id)
    onStyleChange()
  }

  function onRuleReorder(dragIndex: number, hoverIndex: number) {
    console.log(`rule reordered from ${dragIndex} to ${hoverIndex}`)
    const draggedRule = style.rules[dragIndex]
    style.rules.splice(dragIndex, 1)
    style.rules.splice(hoverIndex, 0, draggedRule)
    onStyleChange()
  }

  return (
    <div id="app">
      <div id="toolbar">
        {stateMap && <Exporter map={stateMap} style={style}></Exporter>}
      </div>
      <div id="style-editor" className="hide-scrollbar">
        <StyleEditor style={style} onStyleChange={onStyleChange} onRuleDelete={onRuleDelete} onRuleReorder={onRuleReorder} />
      </div>
      <div ref={mapContainer} className="mapContainer" />
    </div>
  )
}

export default App
