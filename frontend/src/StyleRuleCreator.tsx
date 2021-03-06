import { MapboxGeoJSONFeature } from 'mapbox-gl'
import { nanoid } from 'nanoid'
import { IdStyleFilter, LineStyle, StyleFilter, StyleRule } from './StyleEditor'

export default function StyleRuleCreator({
  feature,
  onRuleAdded,
}: {
  feature: MapboxGeoJSONFeature
  onRuleAdded: (rule: StyleRule) => void
}) {
  let buttons: JSX.Element[] = []

  buttons.push(
    <div key="this-object">
      <button
        onClick={(e) =>
          onRuleAdded({
            id: nanoid(),
            filter: new IdStyleFilter(feature.id! as number),
            lineStyle: new LineStyle(5, '#ff0000'),
          })
        }
      >
        ...this specific object
      </button>
    </div>
  )

  // TODO: Allow setting style for single object (via id)
  Object.keys(feature.properties!).forEach((propertyKey) => {
    const propertyValue: string = feature.properties![propertyKey]

    // TODO: New style should default to the current style
    buttons.push(
      <div key={`${propertyKey}-keypresence`}>
        <button
          onClick={(e) =>
            onRuleAdded({
              id: nanoid(),
              filter: new StyleFilter(propertyKey),
              lineStyle: new LineStyle(5, '#ff0000'),
            })
          }
        >{`...objects with key ${propertyKey}`}</button>
      </div>
    )
    buttons.push(
      <div key={`${propertyKey}-keyvalue`}>
        <button
          onClick={(e) =>
            onRuleAdded({
              id: nanoid(),
              filter: new StyleFilter(propertyKey, propertyValue),
              lineStyle: new LineStyle(5, '#ff0000'),
            })
          }
        >{`...objects ${propertyKey}=${propertyValue}`}</button>
      </div>
    )
  })

  return (
    <div>
      <h2>Add style rule for...</h2>
      {buttons}
    </div>
  )
}
