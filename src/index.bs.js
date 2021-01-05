// Generated by ReScript, PLEASE EDIT WITH CARE


var devicePixelRatio = 300.0 / 96.0;

Object.defineProperty(window, "devicePixelRatio", {
      get: (function (param) {
          return devicePixelRatio;
        })
    });

function saveImage(param) {
  var dataURL = document.getElementsByTagName("canvas")[0].toDataURL();
  var a = document.createElement("a");
  a.href = dataURL;
  a.download = "image.png";
  document.body.appendChild(a);
  setTimeout((function (param) {
          document.body.removeChild(a);
          URL.revokeObjectURL(dataURL);
          
        }), 0);
  a.click();
  window.open(dataURL, "_blank", undefined);
  
}

var save = document.getElementById("save");

if (save == null) {
  throw {
        RE_EXN_ID: "Not_found",
        Error: new Error()
      };
}

save.addEventListener("click", (function (param) {
        return saveImage(undefined);
      }));

var bounds = localStorage.boundingbox;

var bounds$1 = (bounds == null) ? [
    [
      0.0,
      0.0
    ],
    [
      0.0,
      0.0
    ]
  ] : JSON.parse(bounds);

mapboxgl.accessToken = "pk.eyJ1IjoibXd1ZGthIiwiYSI6ImNraXhva29veDBtd3Mycm0wMTVtMmx4dXoifQ._3QauG82dcJHW7pNWU4aoA";

var map = new mapboxgl.Map({
      container: "map",
      style: "mapbox://styles/mwudka/ckipad3rn3slk17p1rkdxmjdh/draft",
      zoom: 13.0,
      preserveDrawingBuffer: true,
      bounds: bounds$1
    });

map.addControl(new MapboxGeocoder({
          accessToken: mapboxgl.accessToken,
          mapboxgl: mapboxgl
        }));

function generateTextImage(text) {
  var canvasEl = document.createElement("canvas");
  canvasEl.width = 1000;
  canvasEl.height = 1000;
  var context = canvasEl.getContext("2d");
  context.font = "30px serif";
  context.textBaseline = "middle";
  var metrics = context.measureText(text);
  var height = (20 + metrics.actualBoundingBoxAscent | 0) + metrics.actualBoundingBoxDescent | 0;
  context.fillStyle = "white";
  roundRect(context, 0, 0, metrics.width + 40 | 0, height, 20, true, false);
  context.fillStyle = "black";
  context.fillText(text, 20, metrics.actualBoundingBoxAscent + 10 | 0);
  return context.getImageData(0, 0, metrics.width + 40 | 0, height);
}

var nextImageId = {
  contents: 0
};

map.on("load", (function (param) {
        map.addSource("places", {
              type: "geojson",
              data: {
                type: "FeatureCollection",
                features: []
              }
            });
        map.addLayer({
              id: "poi-labels",
              type: "symbol",
              source: "places",
              layout: {
                "icon-rotate": [
                  "get",
                  "rotation"
                ],
                "icon-size": [
                  "get",
                  "icon-size"
                ],
                "icon-image": [
                  "get",
                  "text-image"
                ],
                "icon-allow-overlap": true
              },
              paint: {
                "text-color": "#000000"
              }
            });
        var createdFeatures = [];
        var createFeature = function (name, coordinates) {
          var imageId = nextImageId.contents;
          nextImageId.contents = imageId + 1 | 0;
          var newImageName = "image-" + imageId;
          map.addImage(newImageName, generateTextImage(name));
          var newFeature = {
            type: "Feature",
            properties: {
              description: name,
              "text-image": newImageName,
              rotation: 0,
              offset: 10,
              offsetEM: [
                10,
                10
              ],
              offsetPX: [
                0,
                0
              ],
              "icon-size": 1.0 / devicePixelRatio
            },
            geometry: {
              type: "Point",
              coordinates: coordinates
            }
          };
          createdFeatures.push(newFeature);
          map.getSource("places").setData({
                type: "FeatureCollection",
                features: createdFeatures
              });
          
        };
        map.on("click", (function (e) {
                map.querySourceFeatures("composite", {
                        sourceLayer: "poi_label"
                      }).map(function (poi) {
                      return createFeature(poi.properties.name, poi.geometry.coordinates);
                    });
                var feature = map.queryRenderedFeatures(e.point).filter(function (f) {
                        return f.type === "Feature";
                      }).shift();
                if (feature !== undefined) {
                  return createFeature(feature.properties.name, e.lngLat.toArray());
                }
                
              }));
        window.addEventListener("unload", (function (param) {
                localStorage.boundingbox = JSON.stringify(map.getBounds().toArray());
                
              }));
        
      }));

export {
  devicePixelRatio ,
  saveImage ,
  bounds$1 as bounds,
  map ,
  generateTextImage ,
  nextImageId ,
  
}
/*  Not a pure module */
