import { useState, useRef, useEffect } from 'react';
import mapboxgl from 'mapbox-gl';
import MapboxGeocoder from '@mapbox/mapbox-gl-geocoder';
import './App.css';

function App() {
  const mapContainer = useRef<HTMLDivElement>(null);
  const [lng, setLng] = useState(-74.006);
  const [lat, setLat] = useState(40.71);
  const [zoom, setZoom] = useState(14);

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
                'line-color': 'red',
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
                'fill-color': '#ffffff'
            }
        }
    ]
    }
    });
    map.on("move", () => {
      setLng(map.getCenter().lng);
      setLat(map.getCenter().lat);
      setZoom(map.getZoom());
    });
    map.addControl(
      new MapboxGeocoder({
        accessToken: mapboxgl.accessToken,
        mapboxgl: map,
        marker: false
      })
    );
    
    // We only want this to run once. React docs say that empty deps array means only run effect hook once.
    // eslint-disable-next-line 
  }, []);

  return (
    <div ref={mapContainer} className="mapContainer">
      
    </div>
  );
}

export default App;
