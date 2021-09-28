FROM node:14-alpine as node


COPY tileserver/frontend .
ARG BUILD_INFO=docker-build
ENV REACT_APP_BUILD_INFO ${BUILD_INFO}
RUN npm ci && npm run build

FROM golang:1.17 as go
WORKDIR /go/src/app
COPY tileserver .
COPY --from=node /build frontend/build
RUN GOOS=linux GOARCH=amd64 CGO_ENABLED=0 go build -o tileserver-linux-amd64


FROM alpine:latest as alpine

RUN apk add -U --no-cache ca-certificates

FROM scratch

COPY --from=alpine /etc/ssl/certs/ca-certificates.crt /etc/ssl/certs/

COPY --from=go /go/src/app/tileserver-linux-amd64 /tileserver-linux-amd64

ENTRYPOINT [ "/tileserver-linux-amd64" ]