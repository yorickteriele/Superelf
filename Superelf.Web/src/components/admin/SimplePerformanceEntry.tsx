import React, { useState, useEffect, useCallback } from 'react';
import { 
  adminService, 
  PlayerPerformance,
  CreatePlayerPerformanceRequest,
  FootballPlayer,
  Match
} from '../../services/adminService';

export interface SimplePerformanceEntryProps {
  match: Match;
  onError: (message: string) => void;
  onSuccess: (message: string) => void;
  onClose: () => void;
}

interface PlayerPerformanceEntry {
  player: FootballPlayer;
  performance: CreatePlayerPerformanceRequest;
  calculatedPoints: number;
  existingPerformance?: PlayerPerformance;
}

export const SimplePerformanceEntry: React.FC<SimplePerformanceEntryProps> = ({ 
  match, 
  onError, 
  onSuccess, 
  onClose 
}) => {
  const [showFloatingButton, setShowFloatingButton] = useState(false);

  // Show/hide floating button based on scroll position
  useEffect(() => {
    const handleScroll = () => {
      if (window.scrollY > 300) {
        setShowFloatingButton(true);
      } else {
        setShowFloatingButton(false);
      }
    };
    
    window.addEventListener('scroll', handleScroll);
    return () => window.removeEventListener('scroll', handleScroll);
  }, []);
  
  // Custom styles for mobile optimization
  const styles = {
    inputStyles: {
      padding: '0.25rem 0.5rem',
      fontSize: '0.875rem',
    } as React.CSSProperties,
    numberInput: {
      textAlign: 'center',
      minWidth: '45px',
      maxWidth: '100%'
    } as React.CSSProperties,
    mobileScroll: {
      WebkitOverflowScrolling: 'touch' as any,
      overscrollBehavior: 'contain',
      maxHeight: 'calc(100vh - 200px)',
    } as React.CSSProperties,
    stickyNavbar: {
      position: 'sticky' as 'sticky',
      top: '0',
      zIndex: 100,
      marginBottom: '1rem',
    } as React.CSSProperties
  };
  const [loading, setLoading] = useState(false);
  const [players, setPlayers] = useState<FootballPlayer[]>([]);
  const [performances, setPerformances] = useState<PlayerPerformance[]>([]);
  const [playerEntries, setPlayerEntries] = useState<PlayerPerformanceEntry[]>([]);

  // Calculate fantasy points
  const calculateFantasyPoints = useCallback((perf: CreatePlayerPerformanceRequest): number => {
    if (!perf.played) return 0;
    
    let points = 2; // Base points for playing
    points += perf.goals * 6; // 6 points per goal
    points += perf.penaltyGoals * 4; // 4 additional points for penalty goals
    points -= perf.penaltiesMissed * 2; // -2 points for missed penalties
    points -= perf.ownGoals * 2; // -2 points for own goals
    points += perf.assists * 4; // 4 points per assist
    points -= perf.yellowCards * 1; // -1 point per yellow card
    points -= perf.redCards * 3; // -3 points per red card
    
    return Math.max(0, points);
  }, []);

  const loadData = useCallback(async () => {
    setLoading(true);
    try {
      const [performancesData, playersData] = await Promise.all([
        adminService.getMatchPerformances(match.id),
        adminService.getAllPlayers({ pageSize: 1000 })
      ]);
      
      setPerformances(performancesData);
      setPlayers(playersData.items);
      
      // Create entries for all players
      const entries: PlayerPerformanceEntry[] = playersData.items.map(player => {
        const existingPerf = performancesData.find(p => p.playerId === player.id);
        
        const performance: CreatePlayerPerformanceRequest = {
          playerId: player.id,
          goals: existingPerf?.goals || 0,
          penaltyGoals: existingPerf?.penaltyGoals || 0,
          penaltiesMissed: existingPerf?.penaltiesMissed || 0,
          ownGoals: existingPerf?.ownGoals || 0,
          assists: existingPerf?.assists || 0,
          yellowCards: existingPerf?.yellowCards || 0,
          redCards: existingPerf?.redCards || 0,
          played: existingPerf?.played || false
        };
        
        return {
          player,
          performance,
          calculatedPoints: calculateFantasyPoints(performance),
          existingPerformance: existingPerf
        };
      });
      
      setPlayerEntries(entries);
    } catch (err: any) {
      onError(err.message || 'Failed to load data');
    } finally {
      setLoading(false);
    }
  }, [match.id, onError, calculateFantasyPoints]);

  useEffect(() => {
    loadData();
  }, [loadData]);

  const updatePlayerEntry = (playerId: string, field: keyof CreatePlayerPerformanceRequest, value: any) => {
    setPlayerEntries(prev => 
      prev.map(entry => {
        if (entry.player.id === playerId) {
          const updatedPerformance = { ...entry.performance, [field]: value };
          return {
            ...entry,
            performance: updatedPerformance,
            calculatedPoints: calculateFantasyPoints(updatedPerformance)
          };
        }
        return entry;
      })
    );
  };

  const handleSaveAll = async () => {
    const entriesToSave = playerEntries.filter(entry => 
      entry.performance.played && !entry.existingPerformance
    );
    
    if (entriesToSave.length === 0) {
      onError('No new performances to save');
      return;
    }

    setLoading(true);
    try {
      // Use bulk API
      const performances = entriesToSave.map(entry => entry.performance);
      const result = await adminService.addBulkPlayerPerformances(match.id, performances);
      onSuccess(`Saved ${entriesToSave.length} performances: ${result}`);
      await loadData(); // Reload data
    } catch (err: any) {
      // Fallback to individual saves
      let successCount = 0;
      for (const entry of entriesToSave) {
        try {
          await adminService.addPlayerPerformance(match.id, entry.performance);
          successCount++;
        } catch (error) {
          console.error('Failed to save performance:', error);
        }
      }
      
      if (successCount > 0) {
        onSuccess(`Saved ${successCount} of ${entriesToSave.length} performances`);
        await loadData();
      } else {
        onError('Failed to save any performances');
      }
    } finally {
      setLoading(false);
    }
  };

  // Group players by team (assuming we can determine team by club or other means)
  const homeTeamPlayers = playerEntries.filter(entry => 
    entry.player.club === match.homeTeam || !entry.player.club
  );
  const awayTeamPlayers = playerEntries.filter(entry => 
    entry.player.club === match.awayTeam && entry.player.club
  );
  const otherPlayers = playerEntries.filter(entry => 
    entry.player.club !== match.homeTeam && 
    entry.player.club !== match.awayTeam && 
    entry.player.club
  );

  const allGroupedPlayers = [
    { title: match.homeTeam, players: homeTeamPlayers },
    { title: match.awayTeam, players: awayTeamPlayers },
    { title: 'Other Players', players: otherPlayers }
  ].filter(group => group.players.length > 0);

  if (loading && playerEntries.length === 0) {
    return (
      <div className="modal show d-block" tabIndex={-1}>
        <div className="modal-dialog modal-fullscreen">
          <div className="modal-content">
            <div className="modal-header">
              <h5 className="modal-title">Loading...</h5>
            </div>
            <div className="modal-body">
              <div className="d-flex justify-content-center">
                <div className="spinner-border" role="status">
                  <span className="visually-hidden">Loading...</span>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>
    );
  }

  return (
    <div className="modal show d-block position-fixed top-0 start-0 w-100 h-100" tabIndex={-1} style={{zIndex: 1050, background: 'rgba(0,0,0,0.5)'}}>
      <div className="modal-dialog modal-dialog-centered modal-fullscreen-sm-down modal-xl mx-auto my-4" style={{maxWidth: '95%', maxHeight: '95vh'}}>
        <div className="modal-content">
          <div className="modal-header bg-primary text-white">
            <h5 className="modal-title">
              <i className="fas fa-futbol me-2"></i>
              Performance Entry - {match.homeTeam} vs {match.awayTeam}
            </h5>
            <button type="button" className="btn-close btn-close-white" onClick={onClose}></button>
          </div>
          <div className="modal-body p-0 p-sm-3" style={{...styles.mobileScroll, overflowY: 'auto'}}>
            <nav className="nav nav-pills nav-justified bg-light p-2 rounded sticky-top mb-3" style={styles.stickyNavbar}>
              {allGroupedPlayers.map((group, index) => (
                <a 
                  key={index} 
                  className="nav-link" 
                  href={`#team-${index}`}
                  onClick={(e) => {
                    e.preventDefault();
                    document.getElementById(`team-${index}`)?.scrollIntoView({ behavior: 'smooth' });
                  }}
                >
                  {group.title} ({group.players.length})
                </a>
              ))}
            </nav>
            <div className="alert alert-info py-2 mb-3 d-flex align-items-center">
              <i className="fas fa-info-circle me-2 fa-lg"></i>
              <div className="small">
                Enter performance data for players. Only those marked "Played" will be saved.
                <div className="mt-1 d-flex">
                  <span className="badge bg-success me-2">■</span> Existing data
                  <span className="badge bg-info mx-2">■</span> New data
                </div>
              </div>
            </div>

            {allGroupedPlayers.map((group, groupIndex) => (
              <div id={`team-${groupIndex}`} key={groupIndex} className="mb-4">
                <div className="card mb-3 shadow-sm">
                  <div className="card-header bg-primary text-white">
                    <h5 className="mb-0">
                      <i className="fas fa-users me-2"></i>
                      {group.title} ({group.players.length} players)
                    </h5>
                  </div>
                </div>
                
                <div className="table-responsive rounded shadow-sm">
                  <table className="table table-sm table-hover">
                    <thead className="table-dark sticky-top">
                      <tr>
                        <th style={{ minWidth: '180px' }}>Player</th>
                        <th style={{ width: '60px' }}>Goals</th>
                        <th style={{ width: '70px' }}>Pen G</th>
                        <th style={{ width: '70px' }}>Miss P</th>
                        <th style={{ width: '60px' }}>Own G</th>
                        <th style={{ width: '60px' }}>Assists</th>
                        <th style={{ width: '60px' }}>Yellow</th>
                        <th style={{ width: '60px' }}>Red</th>
                        <th style={{ width: '70px' }}>Played</th>
                        <th style={{ width: '60px' }}>Points</th>
                      </tr>
                    </thead>
                    <tbody>
                      {group.players.map(entry => (
                        <tr 
                          key={entry.player.id}
                          className={entry.existingPerformance ? 'table-success' : (entry.performance.played ? 'table-info' : '')}
                        >
                          <td className="ps-2">
                            <div className="d-flex flex-column">
                              <strong className="text-nowrap text-truncate" style={{ maxWidth: '180px' }}>
                                {entry.player.name}
                              </strong>
                              <small className="text-muted text-nowrap">
                                {entry.player.position} • {entry.player.club || 'No Club'}
                              </small>
                            </div>
                          </td>
                          <td className="text-center">
                            <input
                              type="number"
                              className="form-control form-control-sm"
                              min="0"
                              style={styles.numberInput}
                              value={entry.performance.goals}
                              onChange={(e) => updatePlayerEntry(entry.player.id, 'goals', parseInt(e.target.value) || 0)}
                              disabled={!!entry.existingPerformance}
                            />
                          </td>
                          <td className="text-center">
                            <input
                              type="number"
                              className="form-control form-control-sm"
                              min="0"
                              style={styles.numberInput}
                              value={entry.performance.penaltyGoals}
                              onChange={(e) => updatePlayerEntry(entry.player.id, 'penaltyGoals', parseInt(e.target.value) || 0)}
                              disabled={!!entry.existingPerformance}
                            />
                          </td>
                          <td className="text-center">
                            <input
                              type="number"
                              className="form-control form-control-sm"
                              min="0"
                              style={styles.numberInput}
                              value={entry.performance.penaltiesMissed}
                              onChange={(e) => updatePlayerEntry(entry.player.id, 'penaltiesMissed', parseInt(e.target.value) || 0)}
                              disabled={!!entry.existingPerformance}
                            />
                          </td>
                          <td className="text-center">
                            <input
                              type="number"
                              className="form-control form-control-sm"
                              min="0"
                              style={styles.numberInput}
                              value={entry.performance.ownGoals}
                              onChange={(e) => updatePlayerEntry(entry.player.id, 'ownGoals', parseInt(e.target.value) || 0)}
                              disabled={!!entry.existingPerformance}
                            />
                          </td>
                          <td className="text-center">
                            <input
                              type="number"
                              className="form-control form-control-sm"
                              min="0"
                              style={styles.numberInput}
                              value={entry.performance.assists}
                              onChange={(e) => updatePlayerEntry(entry.player.id, 'assists', parseInt(e.target.value) || 0)}
                              disabled={!!entry.existingPerformance}
                            />
                          </td>
                          <td className="text-center">
                            <input
                              type="number"
                              className="form-control form-control-sm"
                              min="0"
                              max="2"
                              style={styles.numberInput}
                              value={entry.performance.yellowCards}
                              onChange={(e) => updatePlayerEntry(entry.player.id, 'yellowCards', parseInt(e.target.value) || 0)}
                              disabled={!!entry.existingPerformance}
                            />
                          </td>
                          <td className="text-center">
                            <input
                              type="number"
                              className="form-control form-control-sm"
                              min="0"
                              max="1"
                              style={styles.numberInput}
                              value={entry.performance.redCards}
                              onChange={(e) => updatePlayerEntry(entry.player.id, 'redCards', parseInt(e.target.value) || 0)}
                              disabled={!!entry.existingPerformance}
                            />
                          </td>
                          <td className="text-center">
                            <div className="form-check d-flex justify-content-center">
                              <input
                                className="form-check-input"
                                type="checkbox"
                                checked={entry.performance.played}
                                onChange={(e) => updatePlayerEntry(entry.player.id, 'played', e.target.checked)}
                                disabled={!!entry.existingPerformance}
                                style={{ width: '20px', height: '20px' }}
                              />
                            </div>
                          </td>
                          <td className="text-center">
                            <span className={`badge ${entry.performance.played ? 'bg-primary' : 'bg-secondary'}`}>
                              {entry.calculatedPoints}
                            </span>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                  </table>
                </div>
              </div>
            ))}
            
            {/* Floating quick save button for mobile */}
            {showFloatingButton && (
              <div 
                className="position-fixed d-md-none" 
                style={{
                  bottom: '80px', 
                  right: '20px', 
                  zIndex: 1060
                }}
              >
                <button 
                  className="btn btn-success btn-lg rounded-circle shadow"
                  style={{ width: '60px', height: '60px' }} 
                  onClick={handleSaveAll}
                  disabled={loading}
                >
                  {loading ? (
                    <span className="spinner-border spinner-border-sm"></span>
                  ) : (
                    <i className="fas fa-save"></i>
                  )}
                </button>
              </div>
            )}
          </div>
          <div className="modal-footer d-flex justify-content-between sticky-bottom bg-light shadow-lg">
            <button 
              className="btn btn-secondary"
              onClick={onClose}
            >
              <i className="fas fa-times me-1"></i> Close
            </button>
            <button 
              className="btn btn-success"
              onClick={handleSaveAll}
              disabled={loading}
            >
              {loading ? (
                <span className="spinner-border spinner-border-sm me-1"></span>
              ) : (
                <i className="fas fa-save me-1"></i>
              )}
              Save All
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};
