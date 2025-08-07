import React, { useState } from 'react';
import { CreatePlayerPerformanceRequest, FootballPlayer } from '../../services/adminService';

interface PerformanceImportExportProps {
  players: FootballPlayer[];
  onImport: (performances: CreatePlayerPerformanceRequest[]) => void;
  onExport: (performances: any[]) => void;
  existingPerformances?: any[];
}

export const PerformanceImportExport: React.FC<PerformanceImportExportProps> = ({
  players,
  onImport,
  onExport,
  existingPerformances = []
}) => {
  const [importText, setImportText] = useState('');
  const [importFormat, setImportFormat] = useState<'csv' | 'json'>('csv');
  const [showImport, setShowImport] = useState(false);

  const handleCSVImport = () => {
    try {
      const lines = importText.trim().split('\n');
      const performances: CreatePlayerPerformanceRequest[] = [];
      
      // Skip header line if it exists
      const dataLines = lines[0].toLowerCase().includes('player') ? lines.slice(1) : lines;
      
      for (const line of dataLines) {
        const parts = line.split(',').map(p => p.trim());
        
        if (parts.length < 8) continue;
        
        // Find player by name
        const playerName = parts[0];
        const player = players.find(p => 
          p.name.toLowerCase().includes(playerName.toLowerCase()) ||
          playerName.toLowerCase().includes(p.name.toLowerCase())
        );
        
        if (!player) continue;
        
        performances.push({
          playerId: player.id,
          goals: parseInt(parts[1]) || 0,
          penaltyGoals: parseInt(parts[2]) || 0,
          penaltiesMissed: parseInt(parts[3]) || 0,
          ownGoals: parseInt(parts[4]) || 0,
          assists: parseInt(parts[5]) || 0,
          yellowCards: parseInt(parts[6]) || 0,
          redCards: parseInt(parts[7]) || 0,
          played: parts.length > 8 ? parts[8].toLowerCase() === 'true' || parts[8] === '1' : true
        });
      }
      
      onImport(performances);
      setImportText('');
      setShowImport(false);
    } catch (err) {
      console.error('CSV import error:', err);
      alert('Error parsing CSV data. Please check the format.');
    }
  };

  const handleJSONImport = () => {
    try {
      const data = JSON.parse(importText);
      const performances: CreatePlayerPerformanceRequest[] = [];
      
      const dataArray = Array.isArray(data) ? data : [data];
      
      for (const item of dataArray) {
        const player = players.find(p => 
          p.id === item.playerId || 
          p.name.toLowerCase().includes(item.playerName?.toLowerCase()) ||
          item.playerName?.toLowerCase().includes(p.name.toLowerCase())
        );
        
        if (!player) continue;
        
        performances.push({
          playerId: player.id,
          goals: item.goals || 0,
          penaltyGoals: item.penaltyGoals || 0,
          penaltiesMissed: item.penaltiesMissed || 0,
          ownGoals: item.ownGoals || 0,
          assists: item.assists || 0,
          yellowCards: item.yellowCards || 0,
          redCards: item.redCards || 0,
          played: item.played !== false
        });
      }
      
      onImport(performances);
      setImportText('');
      setShowImport(false);
    } catch (err) {
      console.error('JSON import error:', err);
      alert('Error parsing JSON data. Please check the format.');
    }
  };

  const handleExportCSV = () => {
    const csvData = [
      'Player Name,Goals,Penalty Goals,Own Goals,Assists,Yellow Cards,Red Cards,Rating,Played,Points'
    ];
    
    existingPerformances.forEach(perf => {
      csvData.push([
        perf.playerName,
        perf.goals,
        perf.penaltyGoals,
        perf.ownGoals,
        perf.assists,
        perf.yellowCards,
        perf.redCards,
        perf.rating,
        perf.played,
        perf.points
      ].join(','));
    });
    
    const blob = new Blob([csvData.join('\n')], { type: 'text/csv' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'match_performances.csv';
    a.click();
    URL.revokeObjectURL(url);
  };

  const handleExportJSON = () => {
    const jsonData = existingPerformances.map(perf => ({
      playerId: perf.playerId,
      playerName: perf.playerName,
      goals: perf.goals,
      penaltyGoals: perf.penaltyGoals,
      ownGoals: perf.ownGoals,
      assists: perf.assists,
      yellowCards: perf.yellowCards,
      redCards: perf.redCards,
      rating: perf.rating,
      played: perf.played,
      points: perf.points
    }));
    
    const blob = new Blob([JSON.stringify(jsonData, null, 2)], { type: 'application/json' });
    const url = URL.createObjectURL(blob);
    const a = document.createElement('a');
    a.href = url;
    a.download = 'match_performances.json';
    a.click();
    URL.revokeObjectURL(url);
  };

  const csvTemplate = `Player Name,Goals,Penalty Goals,Own Goals,Assists,Yellow Cards,Red Cards,Rating,Played
Messi,2,1,0,1,0,0,8.5,true
Ronaldo,1,0,0,0,1,0,7.0,true
De Bruyne,0,0,0,2,0,0,7.5,true`;

  const jsonTemplate = `[
  {
    "playerName": "Messi",
    "goals": 2,
    "penaltyGoals": 1,
    "ownGoals": 0,
    "assists": 1,
    "yellowCards": 0,
    "redCards": 0,
    "rating": 8.5,
    "played": true
  }
]`;

  return (
    <div className="card">
      <div className="card-header">
        <h6 className="mb-0">
          <i className="fas fa-exchange-alt me-2"></i>
          Import/Export Performance Data
        </h6>
      </div>
      <div className="card-body">
        <div className="row">
          <div className="col-md-6">
            <h6>Export Current Performances</h6>
            <p className="text-muted small">Download existing performance data</p>
            <div className="btn-group w-100 mb-3">
              <button 
                className="btn btn-outline-success btn-sm"
                onClick={handleExportCSV}
                disabled={existingPerformances.length === 0}
              >
                <i className="fas fa-download me-1"></i>
                Export CSV
              </button>
              <button 
                className="btn btn-outline-info btn-sm"
                onClick={handleExportJSON}
                disabled={existingPerformances.length === 0}
              >
                <i className="fas fa-download me-1"></i>
                Export JSON
              </button>
            </div>
          </div>
          <div className="col-md-6">
            <h6>Import Performance Data</h6>
            <p className="text-muted small">Upload performance data from file</p>
            <button 
              className="btn btn-outline-primary btn-sm w-100"
              onClick={() => setShowImport(!showImport)}
            >
              <i className="fas fa-upload me-1"></i>
              {showImport ? 'Hide Import' : 'Show Import'}
            </button>
          </div>
        </div>

        {showImport && (
          <div className="mt-4">
            <div className="row mb-3">
              <div className="col-md-6">
                <label className="form-label">Import Format</label>
                <select 
                  className="form-select form-select-sm"
                  value={importFormat}
                  onChange={(e) => setImportFormat(e.target.value as 'csv' | 'json')}
                >
                  <option value="csv">CSV Format</option>
                  <option value="json">JSON Format</option>
                </select>
              </div>
              <div className="col-md-6">
                <label className="form-label">Actions</label>
                <div className="btn-group w-100">
                  <button 
                    className="btn btn-sm btn-success"
                    onClick={importFormat === 'csv' ? handleCSVImport : handleJSONImport}
                    disabled={!importText.trim()}
                  >
                    <i className="fas fa-upload me-1"></i>
                    Import Data
                  </button>
                  <button 
                    className="btn btn-sm btn-outline-secondary"
                    onClick={() => setImportText(importFormat === 'csv' ? csvTemplate : jsonTemplate)}
                  >
                    <i className="fas fa-file-code me-1"></i>
                    Load Template
                  </button>
                </div>
              </div>
            </div>

            <div className="mb-3">
              <label className="form-label">
                {importFormat === 'csv' ? 'CSV Data' : 'JSON Data'}
                <small className="text-muted ms-2">
                  ({importFormat === 'csv' ? 'Comma-separated values' : 'JSON array format'})
                </small>
              </label>
              <textarea
                className="form-control font-monospace"
                rows={8}
                value={importText}
                onChange={(e) => setImportText(e.target.value)}
                placeholder={`Paste your ${importFormat.toUpperCase()} data here or click "Load Template" for an example`}
              />
            </div>

            <div className="alert alert-info">
              <h6><i className="fas fa-info-circle me-2"></i>Import Tips:</h6>
              <ul className="mb-0">
                <li><strong>Player Matching:</strong> Players are matched by name (partial matches work)</li>
                <li><strong>CSV Format:</strong> Player Name, Goals, Penalty Goals, Own Goals, Assists, Yellow Cards, Red Cards, Rating, Played</li>
                <li><strong>JSON Format:</strong> Array of performance objects with player names or IDs</li>
                <li><strong>Default Values:</strong> Missing fields default to 0 (or 6.0 for rating, true for played)</li>
              </ul>
            </div>
          </div>
        )}
      </div>
    </div>
  );
};
