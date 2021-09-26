package main

import (
	"context"
	"embed"
	"fmt"
	"github.com/joho/godotenv"
	"log"
	"math"
	"net/http"
	"os"
	"strconv"
	"time"

	"github.com/jackc/pgx/v4/pgxpool"
	_ "github.com/lib/pq"

	"github.com/go-chi/chi/v5"
	"github.com/go-chi/chi/v5/middleware"

	"github.com/foomo/simplecert"
	"github.com/foomo/tlsconfig"
)

var pool *pgxpool.Pool

func Tile(w http.ResponseWriter, r *http.Request) {
	// TODO: Handle non-int requests
	z, _ := strconv.Atoi(chi.URLParam(r, "z"))
	x, _ := strconv.Atoi(chi.URLParam(r, "x"))
	y, _ := strconv.Atoi(chi.URLParam(r, "y"))

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
	xmin := worldMercMin + tileMercSize*float64(x)
	xmax := worldMercMin + tileMercSize*(float64(x)+1)
	ymin := worldMercMax - tileMercSize*(float64(y)+1)
	ymax := worldMercMax - tileMercSize*float64(y)

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
               SELECT
				ST_AsMVTGeom(ST_Transform(t.geom, 3857), bounds.b2d, buffer=>4096) AS geom,
                        hstore_to_jsonb(tags) as tags,
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

//go:embed frontend/build
var frontend embed.FS

// This example demonstrates how spin up a custom HTTPS webserver for production deployment.
// It shows how to configure and start your service in a way that the certificate can be automatically renewed via the TLS challenge, before it expires.
// For this to succeed, we need to temporarily free port 443 (on which your service is running) and complete the challenge.
// Once the challenge has been completed the service will be restarted via the DidRenewCertificate hook.
// Requests to port 80 will always be redirected to the TLS secured version of your site.
func main() {

	var err error
	if fileExists(".env") {
		if err = godotenv.Load(".env"); err != nil {
			log.Fatal(err)
		}
	}

	if fileExists(".env.development.local") {
		if err = godotenv.Load(".env.development.local"); err != nil {
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

	r := chi.NewRouter()
	r.Use(middleware.Logger)

	r.Get("/api/v1/tile/{z}/{x}/{y}", Tile)

	var staticFS = http.FS(frontend)
	fs := http.FileServer(staticFS)

	r.Get("/*", func(writer http.ResponseWriter, request *http.Request) {
		request.URL.Path = "/frontend/build/" + request.URL.Path

		fs.ServeHTTP(writer, request)
	})

	var (
		// the structure that handles reloading the certificate
		certReloader *simplecert.CertReloader
		numRenews    int
		ctx, cancel  = context.WithCancel(context.Background())

		// init strict tlsConfig (this will enforce the use of modern TLS configurations)
		// you could use a less strict configuration if you have a customer facing web application that has visitors with old browsers
		tlsConf = tlsconfig.NewServerTLSConfig(tlsconfig.TLSModeServerStrict)

		// a simple constructor for a http.Server with our Handler
		makeServer = func() *http.Server {
			return &http.Server{
				Addr:      ":8443",
				Handler:   r,
				TLSConfig: tlsConf,
			}
		}

		// init server
		srv = makeServer()

		// init simplecert configuration
		cfg = simplecert.Default
	)

	// configure
	cfg.Domains = []string{"lasographer.com", "www.lasographer.com"}
	cfg.CacheDir = "./letsencrypt"
	cfg.SSLEmail = "mwudka@gmail.com"
	cfg.DNSProvider = "route53"

	// this function will be called just before certificate renewal starts and is used to gracefully stop the service
	// (we need to temporarily free port 443 in order to complete the TLS challenge)
	cfg.WillRenewCertificate = func() {
		// stop server
		cancel()
	}

	// this function will be called after the certificate has been renewed, and is used to restart your service.
	cfg.DidRenewCertificate = func() {

		numRenews++

		// restart server: both context and server instance need to be recreated!
		ctx, cancel = context.WithCancel(context.Background())
		srv = makeServer()

		// force reload the updated cert from disk
		certReloader.ReloadNow()

		// here we go again
		go serve(ctx, srv)
	}

	cfg.HTTPAddress = ":8082"
	cfg.TLSAddress = ":8443"

	// init simplecert configuration
	// this will block initially until the certificate has been obtained for the first time.
	// on subsequent runs, simplecert will load the certificate from the cache directory on disk.
	certReloader, err = simplecert.Init(cfg, func() {
		os.Exit(0)
	})
	if err != nil {
		log.Fatal("simplecert init failed: ", err)
	}

	// redirect HTTP to HTTPS
	log.Println("starting HTTP Listener on Port 80")
	go http.ListenAndServe(":8082", http.HandlerFunc(simplecert.Redirect))

	// enable hot reload
	tlsConf.GetCertificate = certReloader.GetCertificateFunc()

	// start serving
	log.Println("will serve at: https://" + cfg.Domains[0])
	serve(ctx, srv)

	fmt.Println("waiting forever")
	<-make(chan bool)
}

func serve(ctx context.Context, srv *http.Server) {

	// lets go
	go func() {
		if err := srv.ListenAndServeTLS("", ""); err != nil && err != http.ErrServerClosed {
			log.Fatalf("listen: %+s\n", err)
		}
	}()

	log.Printf("server started")
	<-ctx.Done()
	log.Printf("server stopped")

	ctxShutDown, cancel := context.WithTimeout(context.Background(), 5*time.Second)
	defer func() {
		cancel()
	}()

	err := srv.Shutdown(ctxShutDown)
	if err == http.ErrServerClosed {
		log.Printf("server exited properly")
	} else if err != nil {
		log.Printf("server encountered an error on exit: %+s\n", err)
	}
}
