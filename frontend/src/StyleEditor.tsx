import { ExpressionName, FillPaint, LinePaint } from 'mapbox-gl'
import React, { useRef } from 'react'
import './StyleEditor.css'
import { DndProvider, useDrop, useDrag, DropTargetMonitor, XYCoord } from 'react-dnd'
import { HTML5Backend } from 'react-dnd-html5-backend'
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

interface DragItem {
  index: number
  id: string
  type: string
}

const DragItemTypes = {
  STYLE_RULE: 'style_rule',
}

function StyleRuleEditor({
  ruleIndex,
  rule,
  onStyleChange,
  onRuleDelete,
  onRuleReorder,
}: {
  ruleIndex: number
  rule: StyleRule
  onStyleChange: () => void
  onRuleDelete: (rule: StyleRule) => void,
  onRuleReorder: (dragIndex: number, hoverIndex: number) => void
}) {

  const ref = useRef<HTMLTableRowElement>(null)
  const [{ handlerId }, drop] = useDrop({
    accept: DragItemTypes.STYLE_RULE,
    collect(monitor) {
      return {
        handlerId: monitor.getHandlerId(),
      }
    },
    hover(item: DragItem, monitor: DropTargetMonitor) {
      if (!ref.current) {
        return
      }
      const dragIndex = item.index
      const hoverIndex = ruleIndex

      // Don't replace items with themselves
      if (dragIndex === hoverIndex) {
        return
      }

      // Determine rectangle on screen
      const hoverBoundingRect = ref.current?.getBoundingClientRect()

      // Get vertical middle
      const hoverMiddleY =
        (hoverBoundingRect.bottom - hoverBoundingRect.top) / 2

      // Determine mouse position
      const clientOffset = monitor.getClientOffset()

      // Get pixels to the top
      const hoverClientY = (clientOffset as XYCoord).y - hoverBoundingRect.top

      // Only perform the move when the mouse has crossed half of the items height
      // When dragging downwards, only move when the cursor is below 50%
      // When dragging upwards, only move when the cursor is above 50%

      // Dragging downwards
      if (dragIndex < hoverIndex && hoverClientY < hoverMiddleY) {
        return
      }

      // Dragging upwards
      if (dragIndex > hoverIndex && hoverClientY > hoverMiddleY) {
        return
      }

      // Time to actually perform the action
      onRuleReorder(dragIndex, hoverIndex)

      // Note: we're mutating the monitor item here!
      // Generally it's better to avoid mutations,
      // but it's good here for the sake of performance
      // to avoid expensive index searches.
      item.index = hoverIndex
    },
  })

  const [{ isDragging }, drag] = useDrag({
    item: { type: DragItemTypes.STYLE_RULE, id: rule.id, index: ruleIndex },
    collect: (monitor: any) => ({
      isDragging: monitor.isDragging(),
    }),
  })

  const opacity = isDragging ? 0.5 : 1

  drag(drop(ref))


  let deleteCell = (
    <td>
      <button onClick={(e) => onRuleDelete(rule)}>X</button>
    </td>
  )

  let dragCell = (
    <td>
      <span ref={ref} className="grippy"/>
    </td>
  )

  let leftCell = <td>{rule.filter.summary()}</td>
  if (rule.style instanceof FillStyle) {
    return (
      <tr data-handler-id={handlerId} style={{opacity}}>
        {dragCell}
        {deleteCell}
        {leftCell}
        <td>
          <input
            type="color"
            value={rule.style.color}
            onChange={(e) => {
              rule.style.color = e.target.value
              onStyleChange()
            }}
          />
        </td>
      </tr>
    )
  }

  if (rule.style instanceof LineStyle) {
    return (
      <tr data-handler-id={handlerId} style={{opacity}}>
        {dragCell}
        {deleteCell}
        {leftCell}
        <td>
          <input
            type="color"
            value={rule.style.color}
            onChange={(e) => {
              rule.style.color = e.target.value
              onStyleChange()
            }}
          />
        </td>

        <td>
          <input
            type="range"
            value={rule.style.width}
            onChange={(e) => {
              ;(rule.style as LineStyle).width = parseInt(e.target.value, 10)
              onStyleChange()
            }}
            min={1}
            max={10}
          />
        </td>
      </tr>
    )
  }

  return <tr/>

  // throw new Error(`Unsupported rule style type ${JSON.stringify(rule.style)}`)
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
  return (
    <table className="styleEditor">
      <DndProvider backend={HTML5Backend}>
        <tbody>
          {style.rules.map((r, idx) => (
            <StyleRuleEditor
              key={idx}
              ruleIndex={idx}
              onStyleChange={onStyleChange}
              onRuleDelete={onRuleDelete}
              onRuleReorder={onRuleReorder}
              rule={r}
            />
          ))}
        </tbody>
      </DndProvider>
    </table>
  )
}
