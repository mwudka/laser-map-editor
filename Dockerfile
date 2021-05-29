FROM scratch

COPY tileserver/tileserver-linux-amd64 /tileserver-linux-amd64

ENTRYPOINT [ "/tileserver-linux-amd64" ]