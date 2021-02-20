package main

import (
	"context"
	"fmt"
	"github.com/joho/godotenv"
	"log"
	"math"
	"net/http"
	"os"
	"strconv"
	"time"

	"github.com/julienschmidt/httprouter"
	_ "github.com/lib/pq"
	"github.com/jackc/pgx/v4/pgxpool"

)

var pool *pgxpool.Pool

func Tile(w http.ResponseWriter, r *http.Request, ps httprouter.Params) {
	z, _ := strconv.Atoi(ps.ByName("z"))
	x, _ := strconv.Atoi(ps.ByName("x"))
	y, _ := strconv.Atoi(ps.ByName("y"))

	w.Header().Set("Access-Control-Allow-Origin", "*")

	fmt.Printf("Processing request z=%d x=%d y=%d\n", z, x, y)

	//# Width of world in EPSG:3857
	worldMercMax := 20037508.3427892
	worldMercMin := -1 * worldMercMax
	worldMercSize := worldMercMax - worldMercMin
	//# Width in tiles
	worldTileSize := math.Pow(2, float64(z))
	//# Tile width in EPSG:3857
	tileMercSize := worldMercSize / worldTileSize
	//# Calculate geographic bounds from tile coordinates
	//# XYZ tile coordinates are in "image space" so origin is
	//# top-left, not bottom right
	xmin := worldMercMin + tileMercSize * float64(x)
	xmax := worldMercMin + tileMercSize * (float64(x) + 1)
	ymin := worldMercMax - tileMercSize * (float64(y) + 1)
	ymax := worldMercMax - tileMercSize * float64(y)

	DensifyFactor := 4.0
	segSize := (xmax - xmin) / DensifyFactor

	boundsSql := fmt.Sprintf("ST_Segmentize(ST_MakeEnvelope(%f, %f, %f, %f, 3857),%f)", xmin, ymin, xmax, ymax, segSize)

	query := fmt.Sprintf(`
WITH
            bounds AS (
                SELECT %s AS geom,
                       %s::box2d AS b2d
            ),
            mvtgeom AS (
               SELECT ST_AsMVTGeom(ST_Transform(t.geom, 3857), bounds.b2d) AS geom,
                        hstore_to_jsonb(slice(tags, ARRAY['highway', 'building', 'leisure', 'natural'])) as tags,
						t.osm_id as id
                FROM osm_data t, bounds
                WHERE ST_Intersects(t.geom, ST_Transform(bounds.geom, 3857))
            )
            SELECT ST_AsMVT(mvtgeom.*, 'default', 4096, 'geom', 'id') FROM mvtgeom`, boundsSql, boundsSql)

	fmt.Printf("Query:\n%s\n", query)

	start := time.Now()
	row := pool.QueryRow(context.Background(), query)

	if row == nil {
		fmt.Printf("Error running query\n")
		w.WriteHeader(500)
		return
	}
	queryDuration := time.Since(start)

	var mvt []byte
	if err := row.Scan(&mvt); err != nil {
		fmt.Printf("Error scanning row %v\n", err)
		w.WriteHeader(500)
		return
	}
	queryScanDuration := time.Since(start)

	fmt.Printf("Query %s Query + scan %s\n", queryDuration, queryScanDuration)
	w.WriteHeader(200)
	w.Write(mvt)
}

func fileExists(path string) bool {
	_, err := os.Stat(path)
	return !os.IsNotExist(err)
}

func main() {
	var err error
	if fileExists(".env") {
		if err = godotenv.Load(".env"); err != nil {
			log.Fatal(err)
		}
	}

	if fileExists(".env.local") {
		if err = godotenv.Load(".env.local"); err != nil {
			log.Fatal(err)
		}
	}

	dbConnectionString := os.Getenv("TILESERVER_DB_CONNECTION_STRING")
	if "" == dbConnectionString {
		log.Fatal("No connection string set in env var TILESERVER_DB_CONNECTION_STRING")
	}

	pool, err = pgxpool.Connect(context.Background(), dbConnectionString)

	if err != nil {
		panic(err)
	}
	defer pool.Close()

	router := httprouter.New()
	router.GET("/:z/:x/:y", Tile)

	if err = http.ListenAndServe("0.0.0.0:8082", router); err != nil {
		panic(err)
	}





}
