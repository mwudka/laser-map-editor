import './StyleEditor.css';

export interface StyleDef {
    highwayColor: string,
    highwayWidth: number,
    buildingColor: string,
    parkColor: string,
    beachColor: string,
    waterColor: string,
}

interface StyleEditorProps {
    style: StyleDef,
    onStyleChange: (style: StyleDef) => void
}

export default function StyleEditor({style, onStyleChange}: StyleEditorProps) {
    return <table className="styleEditor">
        <tr>
            <td>Highway</td>
            <td><input type="color" value={style.highwayColor} onChange={e => onStyleChange({...style, highwayColor: e.target.value})}/></td>
            <td><input type="range" value={style.highwayWidth} onChange={e => onStyleChange({...style, highwayWidth: parseInt(e.target.value, 10)})} min={1} max={10}/></td>
        </tr>        
        <tr>
            <td>Building</td>
            <td><input type="color" value={style.buildingColor} onChange={e => onStyleChange({...style, buildingColor: e.target.value})}/></td>
        </tr>        
        <tr>
            <td>Park</td>
            <td><input type="color" value={style.parkColor} onChange={e => onStyleChange({...style, parkColor: e.target.value})}/></td>
        </tr>        
        <tr>
            <td>Beach</td>
            <td><input type="color" value={style.beachColor} onChange={e => onStyleChange({...style, beachColor: e.target.value})}/></td>
        </tr>        
        <tr>
            <td>Water</td>
            <td><input type="color" value={style.waterColor} onChange={e => onStyleChange({...style, waterColor: e.target.value})}/></td>
        </tr>        
        

    </table>
};