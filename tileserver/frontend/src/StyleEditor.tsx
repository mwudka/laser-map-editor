import { ExpressionName, FillPaint, LinePaint, LngLatLike } from 'mapbox-gl'
import { DragDropContext, Draggable, Droppable, DropResult } from 'react-beautiful-dnd'
import titleize from 'titleize'
import './StyleEditor.css'
export class IdStyleFilter {
  id: number

  constructor(id: number) {
    this.id = id
  }

  compileFilter(): [ExpressionName, ...any] {
    return ['==', this.id, ['id']]
  }

  summary(): string {
    return `#${this.id}`
  }
}

export class StyleFilter {
  propertyKey: string
  propertyValue?: string

  constructor(propertyKey: string, propertyValue?: string) {
    this.propertyKey = propertyKey
    this.propertyValue = propertyValue
  }

  compileFilter(): [ExpressionName, ...any] {
    // TODO: Should probably be null/undefined check
    if (this.propertyValue) {
      if (this.propertyKey === '$id') {
        return ['==', parseInt(this.propertyValue, 10), ['id']]
      } else {
        return ['==', this.propertyValue, ['get', this.propertyKey]]
      }
    } else {
      return ['has', this.propertyKey]
    }
  }

  summary(): string {
    return titleize(this.propertyValue ? `${this.propertyKey} ${this.propertyValue}` : this.propertyKey)
  }
}

export class LineStyle {
  width: number
  color: string
  type: 'line' | 'fill' = 'line'
  dashed: boolean

  constructor(width: number, color: string, dashed: boolean) {
    this.width = width
    this.color = color
    this.dashed = dashed
  }

  compileStyle(): LinePaint {
    return { 'line-color': this.color, 'line-width': this.width, 'line-dasharray': this.dashed ? [2, 1] : [] }
  }
}

export class FillStyle {
  color: string
  type: 'line' | 'fill' = 'fill'

  constructor(color: string) {
    this.color = color
  }

  compileStyle(): FillPaint {
    return { 'fill-color': this.color }
  }
}

export interface StyleRule {
  id: string
  filter: StyleFilter | IdStyleFilter
  lineStyle?: LineStyle
  fillStyle?: FillStyle
}

export interface SavedPOI {
  id: string,
  position: LngLatLike,
  text: string,
  sprite: string,
}

export interface StyleDef {
  revision: string
  rules: StyleRule[],
  savedPOIs: SavedPOI[],
}

/**
 * Helper method that calls the given callback for each rule. The callback is given the rule
 * and a MapBox GL expression that returns true if the given rule matches AND no higher-priority
 * rules match.
 * @param style 
 * @param callback 
 */
export function mapStyleRules<T>(style: StyleDef, callback: (rule: StyleRule, filter: [ExpressionName, ...any[]]) => T[] | void): T[] {
  const reversedRules = [...style.rules].reverse()

  return reversedRules.flatMap((rule, idx) => {
    let filter = rule.filter.compileFilter()

    const higherPriorityFilters = reversedRules.slice(idx + 1, reversedRules.length).map(rule => rule.filter.compileFilter())
    if (higherPriorityFilters.length > 0) {
      filter = ['all', filter, ['!', ['any', ...higherPriorityFilters]]]
    }

    return callback(rule, filter) || []
  })
}


// TODO: There is probably a JS library to do this? Or maybe it can be on the object prototype somehow?
function ifPresent<T, R>(obj: T | undefined, callback: ((arg0: T) => R)) {
  if (obj) {
    return callback(obj)
  }
}

function StyleRuleEditor({
  ruleIndex,
  rule,
  onStyleChange,
  onRuleDelete,
}: {
  ruleIndex: number
  rule: StyleRule
  onStyleChange: (recipe: (style: StyleDef) => void) => void
  onRuleDelete: (rule: StyleRule) => void
}) {
  return <Draggable draggableId={rule.id} index={ruleIndex}>
    {provided => (
      <div className="styleEditorRow" ref={provided.innerRef} {...provided.draggableProps} {...provided.dragHandleProps}>
        <div className="handle" />

        <div className="title">
          {rule.filter.summary()}
          <button className="deleteButton" onClick={(e) => onRuleDelete(rule)}>delete</button>
        </div>

        <div className="fill-select">
          <label>
            <input type="checkbox" checked={!!rule.fillStyle} onChange={e => {
              onStyleChange((style: StyleDef) => {
                let idx = style.rules.findIndex(r => r.id === rule.id)
                if (e.target.checked) {
                  style.rules[idx].fillStyle = new FillStyle('#ff0000')
                } else {
                  style.rules[idx].fillStyle = undefined
                }
              })
            }} />
      Fill
      </label>
        </div>
        <div className="fill-color">
          {ifPresent(rule.fillStyle, fillStyle => <input
            type="color"
            value={fillStyle.color}
            onChange={(e) => {
              onStyleChange((style: StyleDef) => {
                let idx = style.rules.findIndex(r => r.id === rule.id)
                style.rules[idx].fillStyle = new FillStyle(e.target.value)
              })
            }}
          />
          )}
        </div>
        <div className="line-select">
          <label>
            <input type="checkbox" checked={!!rule.lineStyle} onChange={e => {
              onStyleChange((style: StyleDef) => {
                let idx = style.rules.findIndex(r => r.id === rule.id)
                if (e.target.checked) {
                  style.rules[idx].lineStyle = new LineStyle(3, '#ff0000', false)
                } else {
                  style.rules[idx].lineStyle = undefined
                }
              })
            }} />
      Line
      </label>
        </div>
        <div className="line-color">
          {ifPresent(rule.lineStyle, lineStyle => <input
            type="color"
            value={lineStyle.color}
            onChange={(e) => {
              onStyleChange((style: StyleDef) => {
                let idx = style.rules.findIndex(r => r.id === rule.id)
                style.rules[idx].lineStyle = new LineStyle(lineStyle.width, e.target.value, lineStyle.dashed)
              })
            }}
          />
          )}
        </div>
        <div className="line-width">
          {ifPresent(rule.lineStyle, lineStyle => <input
            type="range"
            value={lineStyle.width}
            onChange={(e) => {
              onStyleChange((style: StyleDef) => {
                let idx = style.rules.findIndex(r => r.id === rule.id)
                style.rules[idx].lineStyle = new LineStyle(parseInt(e.target.value, 10), lineStyle.color, lineStyle.dashed)
              })
            }}
            min={1}
            max={10}
          />
          )}
        </div>
        <div className="line-dash">
          {ifPresent(rule.lineStyle, lineStyle => <label>
            Dashed<input type="checkbox" checked={lineStyle.dashed} onChange={e => {
              onStyleChange((style: StyleDef) => {
                let idx = style.rules.findIndex(r => r.id === rule.id)
                style.rules[idx].lineStyle = new LineStyle(lineStyle.width, lineStyle.color, e.target.checked)
              })
            }} /></label>)}
        </div>
      </div>
    )}
  </Draggable>


}

interface StyleEditorProps {
  style: StyleDef
  onStyleChange: (recipe: (style: StyleDef) => void) => void
  onRuleDelete: (rule: StyleRule) => void
  onRuleReorder: (dragIndex: number, hoverIndex: number) => void
}

export default function StyleEditor({
  style,
  onStyleChange,
  onRuleDelete,
  onRuleReorder,
}: StyleEditorProps) {

  console.log('rendering StyleEditor')

  function onDragEnd(result: DropResult) {
    if (!result.destination) {
      console.log('onDragEnd no destination')
      return
    }

    if (result.destination.index === result.source.index) {
      console.log('onDragEnd no change')
      return
    }

    console.log('onDragEnd', result.source.index, result.destination.index)
    onRuleReorder(result.source.index, result.destination.index)
  }

  return (
    <DragDropContext onDragEnd={onDragEnd}>
      <Droppable droppableId="styleRules">
        {(provided) => (
          <div id="styleRulesList" {...provided.droppableProps} ref={provided.innerRef}>
            {style.rules.map((r, idx) => (
              <StyleRuleEditor rule={r} ruleIndex={idx} onStyleChange={onStyleChange} onRuleDelete={onRuleDelete} key={r.id} />
            ))}
            {provided.placeholder}
          </div>
        )}
      </Droppable>
    </DragDropContext>
  )
}
