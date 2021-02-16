import { Style } from 'mapbox-gl';
import { useState } from 'react';
import './StyleEditor.css';

export interface StyleDef {
    highwayColor: string,
    highwayWidth: number,
    buildingColor: string,
}

interface StyleEditorProps {
    onStyleChange: (style: StyleDef) => void
}

export default function StyleEditor({onStyleChange}: StyleEditorProps) {
    const [styleDef, setStyleDef] = useState({
        highwayColor: '#ff0000',
        highwayWidth: 3,
        buildingColor: '#00ff00',
    })

    function updateStyle(newStyle: StyleDef) {
        setStyleDef(newStyle)
        onStyleChange(newStyle)
    }

    return <table className="styleEditor">
        <tr>
            <td>Highway</td>
            <td><input type="color" value={styleDef.highwayColor} onChange={e => updateStyle({...styleDef, highwayColor: e.target.value})}/></td>
            <td><input type="range" value={styleDef.highwayWidth} onChange={e => updateStyle({...styleDef, highwayWidth: parseInt(e.target.value, 10)})} min={1} max={10}/></td>
        </tr>        
        <tr>
            <td>Building</td>
            <td><input type="color" value={styleDef.buildingColor} onChange={e => updateStyle({...styleDef, buildingColor: e.target.value})}/></td>
        </tr>        
        

    </table>
};