import './BuildInfo.css';

export default function Exporter() {
    
    return <span className="buildInfo">{process.env.REACT_APP_BUILD_INFO || 'dev-build'}</span>
}