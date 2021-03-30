const fs = require('fs');

function foo() {
    const iconNames = require('@mapbox/maki/layouts/all.json')['all']
    const icons = iconNames.reduce((icons, iconName) => {
        icons[iconName] = fs.readFileSync(require.resolve(`@mapbox/maki/icons/${iconName}-15.svg`), 'utf8')
        return icons
    }, {})
    return `export default ${JSON.stringify(icons)}`
}

exports.default = foo