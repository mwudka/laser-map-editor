#!/usr/bin/env bash

set -eux


cd tileserver

(cd frontend && npm ci && npm run build)
GOOS=linux GOARCH=amd64 CGO_ENABLED=0 go build -o tileserver-linux-amd64
