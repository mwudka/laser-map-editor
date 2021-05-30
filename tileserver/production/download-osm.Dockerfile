FROM debian:buster

RUN apt-get update && apt-get install -y \
    aria2

ENTRYPOINT ["/bin/bash", "-c", "cd /osm-download && aria2c --seed-time=0 https://planet.openstreetmap.org/pbf/planet-latest.osm.pbf.torrent && mv *.pbf planet-latest.osm.pbf && wget https://planet.openstreetmap.org/pbf/planet-latest.osm.pbf.md5 && md5sum -c planet-latest.pbf.md5"]


