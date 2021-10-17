import './BuildInfo.css';

export default function Exporter() {
    
    return <span className="buildInfo">Version {process.env.REACT_APP_BUILD_INFO || 'dev-build'}</span>
}