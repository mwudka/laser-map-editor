import { useState, useRef, useEffect } from 'react'
import mapboxgl, { MapboxGeoJSONFeature } from 'mapbox-gl'
import MapboxGeocoder from '@mapbox/mapbox-gl-geocoder'
import SplitPane, { Pane } from 'react-split-pane'
import './App.css'
import './Resizer.css'
import Exporter from './Exporter'
import StyleEditor, { StyleDef } from './StyleEditor';
import compileMapboxStyle from './compileMapboxStyle';

function App() {
  const mapContainer = useRef<HTMLDivElement>(null)
  const [stateMap, setStateMap] = useState<mapboxgl.Map>()
  const [lng, setLng] = useState(-120.61054884999999)
  const [lat, setLat] = useState(35.125492550000004)
  const [zoom, setZoom] = useState(16)
  const [
    hoveredFeature,
    setHoveredFeature,
  ] = useState<MapboxGeoJSONFeature | null>(null)
  const [style, setStyle] = useState<StyleDef>({
    highwayColor: '#ff0000',
    highwayWidth: 3,
    buildingColor: '#999999',
  })

  useEffect(() => {
    console.log('Creating mapboxgl map')
    const map = new mapboxgl.Map({
      container: mapContainer.current!,
      center: [lng, lat],
      zoom: zoom,
      minZoom: 14,
      style: compileMapboxStyle(style),
    })

    var hoveredStateId: number | string | null
    map.on('move', () => {
      setLng(map.getCenter().lng)
      setLat(map.getCenter().lat)
      setZoom(map.getZoom())
    })
    map.on('mousemove', 'postgis-tiles-layer', (e) => {
      if (e.features!.length > 0) {
        if (hoveredStateId) {
          map.setFeatureState(
            {
              source: 'postgis-tiles',
              sourceLayer: 'default',
              id: hoveredStateId,
            },
            { hover: false }
          )
        }
        let newHoveredStateId = e.features![0].id!
        hoveredStateId = newHoveredStateId
        setHoveredFeature(e.features![0])
        map.setFeatureState(
          {
            source: 'postgis-tiles',
            sourceLayer: 'default',
            id: newHoveredStateId,
          },
          { hover: true }
        )
      } else {
        setHoveredFeature(null)
      }
    })
    map.on('mouseleave', 'postgis-tiles-layer', () => {
      if (hoveredStateId) {
        map.setFeatureState(
          {
            source: 'postgis-tiles',
            sourceLayer: 'default',
            id: hoveredStateId,
          },
          { hover: false }
        )
      }
      setHoveredFeature(null)
      hoveredStateId = null
    })
    map.addControl(
      new MapboxGeocoder({
        accessToken: mapboxgl.accessToken,
        mapboxgl: map,
        marker: false,
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

  function onStyleChange(style: StyleDef) {
    console.log('style changed', style);
    setStyle(style);
    stateMap?.setStyle(compileMapboxStyle(style), { diff: true })
  }

  return (
    <div>
      {stateMap && <Exporter map={stateMap} style={style}></Exporter>}
      <pre>{JSON.stringify(hoveredFeature)}</pre>

      <SplitPane split="vertical" minSize={200}>
        <Pane className="pane">
          <StyleEditor style={style} onStyleChange={onStyleChange} />
        </Pane>
        <div ref={mapContainer} className="mapContainer" />
      </SplitPane>
    </div>
  )
}

export default App
