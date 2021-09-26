FROM alpine:latest as alpine

RUN apk add -U --no-cache ca-certificates

FROM scratch

COPY --from=alpine /etc/ssl/certs/ca-certificates.crt /etc/ssl/certs/

COPY tileserver/tileserver-linux-amd64 /tileserver-linux-amd64

ENTRYPOINT [ "/tileserver-linux-amd64" ]