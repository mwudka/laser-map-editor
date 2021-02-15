import { useState, useRef, useEffect } from 'react';
import mapboxgl, { MapboxGeoJSONFeature } from 'mapbox-gl';
import MapboxGeocoder from '@mapbox/mapbox-gl-geocoder';
import './App.css';
import Exporter from './Exporter';

function App() {
  const mapContainer = useRef<HTMLDivElement>(null);
  const [stateMap, setStateMap]  = useState<mapboxgl.Map>();
  const [lng, setLng] = useState(-120.61054884999999);
  const [lat, setLat] = useState(35.125492550000004);
  const [zoom, setZoom] = useState(16);
  const [hoveredFeature, setHoveredFeature] = useState<MapboxGeoJSONFeature|null>(null);

  useEffect(() => {
    console.log('Creating mapboxgl map');
    const map = new mapboxgl.Map({
      container: mapContainer.current!,
      center: [lng, lat],
      zoom: zoom,
      minZoom: 14,
      style: {
        'version': 8,
        'sources': {
            'postgis-tiles': {
                'type': 'vector',
                'tiles': [
                    "http://localhost:8082/{z}/{x}/{y}"
                ]
            }

        },
        'layers': [{
            'id': 'postgis-tiles-layer',
            'type': 'line',
            'source': 'postgis-tiles',
            // ST_AsMVT() uses 'default' as layer name
            'source-layer': 'default', 
            'filter': 
                ["has", "highway"]
            ,
            'minzoom': 14,
            'maxzoom': 22,
            'paint': {
                'line-opacity': 0.7,
                'line-color': ['case',
                  ['boolean', ['feature-state', 'hover'], false],
                  'red',
                  'green'
              ],
                'line-width': 1
            }
        },
        {
            'id': 'postgis-tiles-layer2',
            'type': 'fill',
            'source': 'postgis-tiles',
            // ST_AsMVT() uses 'default' as layer name
            'source-layer': 'default', 
            'filter': 
                ["has", "building"]
            ,
            'minzoom': 14,
            'maxzoom': 22,
            'paint': {
                'fill-color': '#999999'
            }
        }
    ]
    }
    });

    var hoveredStateId:number|string|null;
    map.on("move", () => {
      setLng(map.getCenter().lng);
      setLat(map.getCenter().lat);
      setZoom(map.getZoom());
    });
    map.on("mousemove", "postgis-tiles-layer", e => {
      if (e.features!.length > 0) {
        if (hoveredStateId) {
          map.setFeatureState({source: 'postgis-tiles', sourceLayer: 'default', id: hoveredStateId}, {hover: false});
        }
        let newHoveredStateId = e.features![0].id!;
        hoveredStateId = newHoveredStateId;
        setHoveredFeature(e.features![0]);
        map.setFeatureState({source: 'postgis-tiles', sourceLayer: 'default', id: newHoveredStateId}, {hover: true});
      } else {
        setHoveredFeature(null);
      }
    });
    map.on("mouseleave", "postgis-tiles-layer", () => {
      if (hoveredStateId) {
        map.setFeatureState({source: 'postgis-tiles', sourceLayer: 'default', id: hoveredStateId}, {hover: false});
      }
      setHoveredFeature(null);
      hoveredStateId = null;
    });
    map.addControl(
      new MapboxGeocoder({
        accessToken: mapboxgl.accessToken,
        mapboxgl: map,
        marker: false
      })
    );

    setStateMap(map);
    
    // We only want this to run once. React docs say that empty deps array means only run effect hook once.
    // eslint-disable-next-line 
  }, []);

  return (
    <div>
      {stateMap && <Exporter map={stateMap}></Exporter>}
      <pre>{JSON.stringify(hoveredFeature)}</pre>
      
      <div ref={mapContainer} className="mapContainer">
      
      </div>
    </div>
  );
}

export default App;
