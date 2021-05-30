# Local dev

## Running tileserver locally

Create `.env.development.local` with `TILESERVER_DB_CONNECTION_STRING`

## Local frontend dev

Create a `.env.development.local` file with `REACT_APP_MAPBOX_ACCESS_TOKEN`.

Run via `npm start`

# Production

The GitHub action builds the frontend code, builds the go server (which includes the frontend code), builds a container,
and pushes the result to ghcr.io/mwudka/laser-map-editor:latest.

A docker-compose file in `./production/` defines services for postgis, downloading the OSM planet file, loading it into 
postgis, and running the server.

## OSM import process

First, download the latest planet data:

    sudo docker-compose run download-osm

The downloaded PBF should appear in `osm-download`. Make sure there is exactly one PBF file in `osm-download`.

Second, import the new tiledata into PG. This takes ~12hrs. It's probably best to do in a screen session.

     sudo docker-compose run osm2pgsql

Finally, restart the tileserver:

    sudo docker-compose up -d tileserver
