import { useState, useRef, useEffect } from 'react'
import mapboxgl from 'mapbox-gl'
import MapboxGeocoder from '@mapbox/mapbox-gl-geocoder'
import './App.css'
import './Resizer.css'
import Exporter from './Exporter'
import BuildInfo from './BuildInfo'
import StyleEditor, { FillStyle, LineStyle, StyleDef, StyleFilter, StyleRule } from './StyleEditor'
import compileMapboxStyle from './compileMapboxStyle'
import ReactDOM from 'react-dom'
import StyleRuleCreator from './StyleRuleCreator'
import deepFreeze from './deep-freeze'
import produce from 'immer'
import _, { uniqueId } from 'lodash'

function filterProperties(properties: any): any {
  return _.omitBy(properties, (_, name: string) => {
    if (name.includes(':') && !name.startsWith('addr:')) {
      return true
    }
    if (name === name.toUpperCase()) {
      return true
    }
    const propertiesToRemove = [
      'website',
      'latitude',
      'longitude',
    ]
    return propertiesToRemove.includes(name.toLowerCase())
  })
}

function App() {
  const mapContainer = useRef<HTMLDivElement>(null)
  const [stateMap, setStateMap] = useState<mapboxgl.Map>()
  const [style, setStyle] = useState<StyleDef>(deepFreeze({
    revision: 'style-default',
    savedPOIs: [],
    rules: [
      {
        id: 'default-coastline-rule',
        filter: new StyleFilter('natural', 'coastline'),
        lineStyle: new LineStyle(2, '#111111', false),
      },
      {
        id: 'default-highway-rule',
        filter: new StyleFilter('highway'),
        lineStyle: new LineStyle(2, '#444444', false),
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
  }))
  console.log('rendering app', style.revision)

  const styleRef = useRef(style)
  const mapRef = useRef(stateMap)

  useEffect(() => {
    console.log('syncing style to styleRef', style.revision)
    styleRef.current = style
  }, [style])

  useEffect(() => {
    console.log('syncing map to mapRef')
    mapRef.current = stateMap
  }, [stateMap])



  useEffect(() => {
    console.log('Creating mapboxgl map')
    const map = new mapboxgl.Map({
      container: mapContainer.current!,
      minZoom: 14,
      style: compileMapboxStyle(style),
      hash: true,
      zoom: 15,
      center: {lat: 47.567995, lng: -122.274154}
    })

    map.showTileBoundaries = process.env.REACT_APP_SHOW_TILE_BOUNDARIES === 'true';
    console.log('map.showTileBoundarie', map.showTileBoundaries)
    
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

      if (hoveredFeatures.length > 0 && hoveredFeatures[0].source === "postgis-tiles") {
        tooltipPopup.setHTML('<pre>' + hoveredFeatures.map(feature => `id=${feature.id}: ${JSON.stringify(filterProperties(feature.properties), undefined, 2)}`).join('\n\n') + '</pre>')

        if (!tooltipPopup.isOpen()) {
          tooltipPopup.addTo(map);
        }
        tooltipPopup.setLngLat(e.lngLat);
      } else if (hoveredFeatures.length > 0 && hoveredFeatures[0].source === "mapbox") {
        const geometry = hoveredFeatures[0].geometry
        if (geometry.type !== "Point") {
          console.error('Non-point POI', hoveredFeatures[0])
          return
        }

        // TODO: This code is duplicated
        const existingPOIIndex = styleRef.current.savedPOIs.findIndex(poi => poi.id === hoveredFeatures[0].id?.toString())
        const poiExists = existingPOIIndex !== -1

        tooltipPopup.setHTML(poiExists ? 'Click to remove from map' : 'Click to add to map')
        if (!tooltipPopup.isOpen()) {
          tooltipPopup.addTo(map);
        } 
        tooltipPopup.setLngLat([geometry.coordinates[0], geometry.coordinates[1]])
      } else {
        if (tooltipPopup.isOpen()) {
          tooltipPopup.remove();
        }
      }

      
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

      if (clickedFeature.source === "postgis-tiles") {
        const el = document.createElement('div')

        // TODO: The state's style variable gets captured when the click event handler is installed, so it only
        // ever has access to the default style. It needs to somehow be able to get access to the current
        console.log('render StyleRuleCreator', styleRef.current.revision)
        ReactDOM.render(
          <StyleRuleCreator
            feature={clickedFeature}
            onRuleAdded={(rule) => {
              console.log('StyleRuleCreator onRuleAdded', styleRef.current.revision)
              onStyleChange(draftStyle => {
                draftStyle.revision = uniqueId('style-')
                draftStyle.rules.unshift(rule)
              })
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
      } else if (clickedFeature.source === "mapbox") {
        const geometry = clickedFeature.geometry
        if (geometry.type !== "Point") {
          console.warn('Unable to add POI with unsupported geometry', geometry)
          return
        }

        console.log('toggling POI', clickedFeature)

        onStyleChange(style => {
          const existingPOIIndex = styleRef.current.savedPOIs.findIndex(poi => poi.id === clickedFeature.id?.toString())
          const poiExists = existingPOIIndex !== -1

          if (poiExists) {
            style.savedPOIs.splice(existingPOIIndex, 1)
          } else {          
            style.savedPOIs.unshift({
              id: '' + clickedFeature.id!,
              // TODO: Evaluate expression to compute name based on "text-field" style
              text: clickedFeature.properties!["name_en"] || clickedFeature.properties!['name'] || '' + clickedFeature.id!,
              position: geometry.coordinates as [number, number],
              sprite: clickedFeature.properties!["maki_beta"] || clickedFeature.properties!["maki"]
              // TODO: Evaluate expression to get icon

            })
          }
        })
      }
    })

    map.addControl(
      new MapboxGeocoder({
        accessToken: mapboxgl.accessToken,
        mapboxgl: map,
        marker: false,
        flyTo: {
          animate: false,
        },
      })
    )

    setStateMap(map)
    
    return () => {
      console.log('Removing map')
      map.remove()
    }

    // We only want this to run once. React docs say that empty deps array means only run effect hook once.
    // eslint-disable-next-line
  }, [])

  function setNewStyle(style: StyleDef) {
    deepFreeze(style)
    const compiledStyle = compileMapboxStyle(style)
    console.log('new style set', style, JSON.parse(JSON.stringify(compiledStyle)))
    mapRef.current!.setStyle(compiledStyle, { diff: true })

    setStyle(style)
    console.log('setNewStyle', style.revision)
  }

  function onStyleChange(recipe: (style: StyleDef) => void) {
    setNewStyle(produce(styleRef.current, recipe))
  }

  function onRuleDelete(rule: StyleRule) {
    console.log('rule deleted', rule)
    setNewStyle(produce(styleRef.current, draftStyle => {
      const idx = draftStyle.rules.findIndex(r => r.id === rule.id)
      draftStyle.rules.splice(idx, 1)
    }))    
  }

  function onRuleReorder(dragIndex: number, hoverIndex: number) {
    console.log(`rule reordered from ${dragIndex} to ${hoverIndex}`)
    setNewStyle(produce(styleRef.current, draftStyle => {
      const draggedRule = draftStyle.rules[dragIndex]
      draftStyle.rules.splice(dragIndex, 1)
      draftStyle.rules.splice(hoverIndex, 0, draggedRule)
    }))
  }

  return (
    <div id="app">
      <div id="title">
        <h1>Lasographer</h1>
        
      </div>
      <div id="toolbar">        
        {stateMap && <Exporter map={stateMap} style={style}></Exporter>}
      </div>
      <div id="style-editor" className="hide-scrollbar">
        <StyleEditor style={style} onStyleChange={onStyleChange} onRuleDelete={onRuleDelete} onRuleReorder={onRuleReorder} />
      </div>
      <div ref={mapContainer} className="mapContainer" />
      <div id="about">
        <a target="_blank" href="https://icons8.com/icon/2311/laser-beam" rel="noreferrer">Favicon Laser Beam</a> icon by <a target="_blank" href="https://icons8.com" rel="noreferrer">Icons8</a>
        <br/>
        Map data © <a target="_blank" rel="noreferrer" href="https://openstreetmap.org/copyright">OpenStreetMap</a>
        <br/>
        <BuildInfo/>        
      </div>
    </div>
  )
}

export default App
