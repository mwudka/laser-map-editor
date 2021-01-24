# Laser Map Editor

This crude tool allows creating map images suitable for laser engraving onto wood. After launching the app:

1. Use the search box to find an address near the area you want to engrave
1. Resize the browser window and zoom/pan to fit the area you want to engrave
1. Click on any points of interest to add text markers to the map
1. When you are satisified, click the "Save" button to save your screen as an image
1. If you need to edit the image later, you can drag and drop it back onto the app to reopen it

## Development

### Requirements

* [dotnet SDK](https://www.microsoft.com/net/download/core) 3.0 or higher
* [node.js](https://nodejs.org) with [npm](https://www.npmjs.com/)
* An F# editor like Visual Studio, Visual Studio Code with [Ionide](http://ionide.io/) or [JetBrains Rider](https://www.jetbrains.com/rider/).

### Building and running the app

* Install JS dependencies: `npm install`
* Linux/OSX  
    * Configure environment variables: `envchain --set laser-map-editor GOOGLE_FONTS_API_KEY MAPBOX_API_KEY`  
    * Start: `envchain laser-map-editor npm start`
* Windows
    * Create a .env file in the root directory with `GOOGLE_FONTS_API_KEY` and `MAPBOX_API_KEY`
    * Start: `npm start`
    
After the first compilation is finished, in your browser open: http://localhost:8080. **Note**: if another program
is using port 8080, it will start on the first available port after 8080.


## Production deploy

GitHub Actions handle deploying to production automatically