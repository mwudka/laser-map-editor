# Fable Minimal App

This is a small Fable app project so you can easily get started and add your own code easily in it.

## Requirements

* [dotnet SDK](https://www.microsoft.com/net/download/core) 3.0 or higher
* [node.js](https://nodejs.org) with [npm](https://www.npmjs.com/)
* An F# editor like Visual Studio, Visual Studio Code with [Ionide](http://ionide.io/) or [JetBrains Rider](https://www.jetbrains.com/rider/).

## Building and running the app

* Install JS dependencies: `npm install`
* Configure environment variables: `envchain --set laser-map-editor GOOGLE_FONTS_API_KEY MAPBOX_API_KEY`  
* Install F# dependencies: `envchain laser-map-editor npm start`
* After the first compilation is finished, in your browser open: http://localhost:8080/#mapbox-token=pk.XXX.YYY

Any modification you do to the F# code will be reflected in the web page after saving.

## Production deploy

* Build production bundle: `npm run build`