import { ExpressionName, FillPaint, LinePaint } from 'mapbox-gl'
import { DragDropContext, Draggable, Droppable, DropResult } from 'react-beautiful-dnd'
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
    return `id=${this.id}`
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
    if (this.propertyValue) {
      return `${this.propertyKey}=${this.propertyValue}`
    } else {
      return this.propertyKey
    }
  }
}

export class LineStyle {
  width: number
  color: string
  type: 'line' | 'fill' = 'line'

  constructor(width: number, color: string) {
    this.width = width
    this.color = color
  }

  compileStyle(): LinePaint {
    return { 'line-color': this.color, 'line-width': this.width }
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
  style: LineStyle | FillStyle
}

export interface StyleDef {
  rules: StyleRule[]
}

/**
 * Helper method that calls the given callback for each rule. The callback is given the rule
 * and a MapBox GL expression that returns true if the given rule matches AND no higher-priority
 * rules match.
 * @param style 
 * @param callback 
 */
export function mapStyleRules<T>(style: StyleDef, callback: (rule: StyleRule, filter: [ExpressionName, ...any[]]) => T): T[] {
  return style.rules.map((rule, idx) => {
    let filter = rule.filter.compileFilter()

    const higherPriorityFilters = style.rules.slice(0, idx).map(rule => rule.filter.compileFilter())
    if (higherPriorityFilters.length > 0) {
      filter = ['all', filter, ['!', ['any', ...higherPriorityFilters]]]
    }

    return callback(rule, filter)
  });
}

function StyleRuleEditor({
  ruleIndex,
  rule,
  onStyleChange,
  onRuleDelete,
}: {
  ruleIndex: number
  rule: StyleRule
  onStyleChange: () => void
  onRuleDelete: (rule: StyleRule) => void,
}) {


  return <span>
    <button onClick={(e) => onRuleDelete(rule)}>X</button>
    {rule.filter.summary()}
    {rule.style instanceof FillStyle && <input
      type="color"
      value={rule.style.color}
      onChange={(e) => {
        rule.style.color = e.target.value
        onStyleChange()
      }}
    />}
    {rule.style instanceof LineStyle && <span>
      <input
        type="color"
        value={rule.style.color}
        onChange={(e) => {
          rule.style.color = e.target.value
          onStyleChange()
        }}
      />
      <input
        type="range"
        value={rule.style.width}
        onChange={(e) => {
          ; (rule.style as LineStyle).width = parseInt(e.target.value, 10)
          onStyleChange()
        }}
        min={1}
        max={10}
      />
    </span>}
  </span>

}

interface StyleEditorProps {
  style: StyleDef
  onStyleChange: () => void
  onRuleDelete: (rule: StyleRule) => void
  onRuleReorder: (dragIndex: number, hoverIndex: number) => void
}

export default function StyleEditor({
  style,
  onStyleChange,
  onRuleDelete,
  onRuleReorder,
}: StyleEditorProps) {

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
          <div {...provided.droppableProps} ref={provided.innerRef}>
            {style.rules.map((r, idx) => (
              <Draggable draggableId={r.id} index={idx} key={r.id}>
                {provided => (
                  <div ref={provided.innerRef} {...provided.draggableProps}>
                    <span {...provided.dragHandleProps} className="grippy" />
                    <StyleRuleEditor ruleIndex={idx} rule={r} onStyleChange={onStyleChange} onRuleDelete={onRuleDelete} />
                  </div>
                )}
              </Draggable>
            ))}
            {provided.placeholder}
          </div>
        )}
      </Droppable>
    </DragDropContext>
  )
}
