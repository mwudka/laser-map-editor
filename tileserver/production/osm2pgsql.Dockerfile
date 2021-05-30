# DOCKER-VERSION 1.5.0
# VERSION 0.2
# Based on https://github.com/openfirmware/docker-osm2pgsql

FROM debian:buster
MAINTAINER James Badger <james@jamesbadger.ca>

ENV DEBIAN_FRONTEND noninteractive

RUN apt-get update && apt-get install -y \
    make \
    cmake \
    g++ \
    libboost-dev \
    libboost-system-dev \
    libboost-filesystem-dev \
    libexpat1-dev \
    zlib1g-dev \
    libbz2-dev \
    libpq-dev \
    libproj-dev \
    lua5.3 \
    liblua5.3-dev \
    pandoc \
    git-all

ENV HOME /root
ENV OSM2PGSQL_VERSION 1.4.2

RUN mkdir src &&\
    cd src &&\
    git clone --depth 1 --branch $OSM2PGSQL_VERSION https://github.com/openstreetmap/osm2pgsql.git &&\
    cd osm2pgsql &&\
    mkdir build &&\
    cd build &&\
    cmake .. &&\
    make &&\
    make install &&\
    cd /root &&\
    rm -rf src

ENTRYPOINT ["/bin/bash", "-l", "-c"]