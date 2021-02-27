import { ExpressionName, FillPaint, LinePaint } from 'mapbox-gl'
import React from 'react'
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
      return this.propertyKey;
    }
  }
}

export class LineStyle {
  width: number
  color: string
  type: "line"|"fill" = "line"

  constructor(width: number, color: string) {
    this.width = width
    this.color = color
  }

  compileStyle(): LinePaint {
    return {'line-color': this.color, "line-width": this.width}
  }

  
}

export class FillStyle {
  color: string
  type: "line"|"fill" = "fill"

  constructor(color: string) {
    this.color = color
  }

  compileStyle(): FillPaint {
    return {"fill-color": this.color}
  }
}

export interface StyleRule {
  id: string
  filter: StyleFilter|IdStyleFilter
  style: LineStyle | FillStyle
}

export interface StyleDef {
  rules: StyleRule[]
}

function StyleRuleEditor({
  rule,
  onStyleChange,
  onRuleDelete,
}: {
  rule: StyleRule
  onStyleChange: () => void,
  onRuleDelete: (rule: StyleRule) => void
}) {
  let deleteCell = (
    <td>
      <button onClick={e => onRuleDelete(rule)}>X</button>
    </td>
  )
  let leftCell = (
    <td>
      {rule.filter.summary()}      
    </td>
  )
  if (rule.style instanceof FillStyle) {
    return (
      <tr>
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
      <tr>
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
}

interface StyleEditorProps {
  style: StyleDef
  onStyleChange: () => void,
  onRuleDelete: (rule: StyleRule) => void
}

export default function StyleEditor({
  style,
  onStyleChange,
  onRuleDelete,
}: StyleEditorProps) {
  return (
    <table className="styleEditor">
      <tbody>
        {style.rules.map((r, idx) => (
          <StyleRuleEditor key={idx} onStyleChange={onStyleChange} onRuleDelete={onRuleDelete} rule={r} />
        ))}
      </tbody>
    </table>
  )
}
