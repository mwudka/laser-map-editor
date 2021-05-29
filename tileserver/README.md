## Running tileserver locally

Create `.env.development.local` with `TILESERVER_DB_CONNECTION_STRING`

## Local frontend dev

Create a `.env.development.local` file with `REACT_APP_MAPBOX_ACCESS_TOKEN`.

Run via `npm start`

## Running in production

The GitHub action builds a docker container and publishes it at ghcr.io/mwudka/laser-map-editor.latest.

To run in production, do something like:

    docker run -e TILESERVER_DB_CONNECTION_STRING='postgres://postgres:PASSWORD@9ba89918617f:5432/osm_planet' --link 9ba89918617f -p 8082:8082 ghcr.io/mwudka/laser-map-editor:latest