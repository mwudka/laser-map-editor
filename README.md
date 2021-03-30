# Populating database

    sudo docker run --rm -it -v $PWD:/download openmaptiles/openmaptiles-tools download-osm planet -- -d /download

    sudo docker run --name osm -p 5432:5432 -v $PWD/pg_osm:/var/lib/postgresql/data -e POSTGRES_PASSWORD=Ab5gc9Hd7nojHSN3hHxaJw -e POSTGRES_DB=osm  -d postgis/postgis:12-3.1-alpine

    CREATE EXTENSION hstore;

    osmium tags-filter *.osm.pbf highway=residential highway=service highway=track highway=unclassified highway=footway highway=path highway=tertiary highway=secondary highway=primary highway=living_street highway=trunk highway=motorway highway=motorway_link highway=pedestrian highway=trunk_link highway=primary_link highway=secondary_link building boundary=administrative water waterway landuse natural -o filtered.osm.pbf -f pbf,add_metadata=false --verbose


    osm2pgsql --database=osm_planet --host=127.0.0.1 --user=postgres --password -C 0 --slim --flat-nodes=nodes --hstore --output=flex --style=osm2pgsql-config.lua --drop planet-*.osm.pbf

    osmium export --output-format=pg --output=osm-planet.pg --format-option=tags_type=hstore --verbose --config=osmium-export-config.json planet-*210118*.osm.pbf

    pv osm-planet.pg | psql -p 5432 -h localhost -U postgres osm -c "COPY osmdata FROM STDIN;"

    psql -p 5432 -h localhost -U postgres osm -c "CREATE INDEX ON osmdata USING gist (geom);"


    {
        "attributes": {
            "type":      false,
            "id":        false,
            "version":   false,
            "changeset": false,
            "timestamp": false,
            "uid":       false,
            "user":      false,
            "way_nodes": false
        },
        "format_options": {
        },
        "linear_tags":  true,
        "area_tags":    true,
        "exclude_tags": [],
        "include_tags": ["highway", "building", "boundary", "place", "water", "waterway"]
    }
