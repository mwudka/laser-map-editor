import { useState, useRef, useEffect } from 'react'
import mapboxgl, { MapboxGeoJSONFeature } from 'mapbox-gl'
import MapboxGeocoder from '@mapbox/mapbox-gl-geocoder'
import SplitPane, { Pane } from 'react-split-pane'
import './App.css'
import './Resizer.css'
import Exporter from './Exporter'
import StyleEditor, { StyleDef } from './StyleEditor'
import compileMapboxStyle from './compileMapboxStyle'
import FeatureInfo from './FeatureInfo'

function App() {
  const mapContainer = useRef<HTMLDivElement>(null)
  const [stateMap, setStateMap] = useState<mapboxgl.Map>()
  const [lng, setLng] = useState(-120.61054884999999)
  const [lat, setLat] = useState(35.125492550000004)
  const [zoom, setZoom] = useState(16)
  const [hoveredFeatures, setHoveredFeatures] = useState<
    MapboxGeoJSONFeature[]
  >([])
  const [style, setStyle] = useState<StyleDef>({
    highwayColor: '#444444',
    highwayWidth: 3,
    buildingColor: '#999999',
    parkColor: '#00ff00',
    waterColor: '#0000ff',
    beachColor: '#ffff00',
  })

  useEffect(() => {
    console.log('Creating mapboxgl map')
    const map = new mapboxgl.Map({
      container: mapContainer.current!,
      center: [lng, lat],
      zoom: zoom,
      minZoom: 14,
      style: compileMapboxStyle(style),
      hash: true,
    })

    map.on('move', () => {
      setLng(map.getCenter().lng)
      setLat(map.getCenter().lat)
      setZoom(map.getZoom())
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

      hoveredFeatures = map.queryRenderedFeatures(e.point).slice(0, 1);
      hoveredFeatures.forEach((f) => setHoverState(f, true))
      map.getCanvas().style.cursor = hoveredFeatures.length > 0 ? 'pointer' : ''
      setHoveredFeatures(hoveredFeatures)
    })
    map.on('mouseout', (e) => {
      hoveredFeatures.forEach((f) => setHoverState(f, false))
      hoveredFeatures = []
      setHoveredFeatures(hoveredFeatures)
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

  function onStyleChange(style: StyleDef) {
    console.log('style changed', style)
    setStyle(style)
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
            <FeatureInfo features={hoveredFeatures}/>
          </Pane>
        </SplitPane>

        <div ref={mapContainer} className="mapContainer" />
      </SplitPane>
    </div>
  )
}

export default App
