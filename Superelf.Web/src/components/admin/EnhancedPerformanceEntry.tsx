import React, { useState, useEffect, useCallback, useMemo } from 'react';
import { 
  adminService, 
  PlayerPerformance,
  CreatePlayerPerformanceRequest,
  FootballPlayer,
  Match
} from '../../services/adminService';
import { PerformanceImportExport } from './PerformanceImportExport';

export interface EnhancedPerformanceEntryProps {
  match: Match;
  onError: (message: string) => void;
  onSuccess: (message: string) => void;
  onClose: () => void;
}

export interface PerformanceTemplate {
  name: string;
  description: string;
  performance: Partial<CreatePlayerPerformanceRequest>;
  icon: string;
}

export interface BulkPerformanceEntry {
  playerId: string;
  playerName: string;
  position: string;
  club: string;
  goals: number;
  penaltyGoals: number;
  penaltiesMissed: number;
  ownGoals: number;
  assists: number;
  yellowCards: number;
  redCards: number;
  played: boolean;
  calculatedPoints?: number;
}

const PERFORMANCE_TEMPLATES: PerformanceTemplate[] = [
  {
    name: 'Clean Sheet GK',
    description: 'Goalkeeper clean sheet',
    performance: { goals: 0, assists: 0, yellowCards: 0, redCards: 0, played: true, penaltiesMissed: 0 },
    icon: '🥅'
  },
  {
    name: 'Defender Clean Sheet',
    description: 'Defender with clean sheet',
    performance: { goals: 0, assists: 0, yellowCards: 0, redCards: 0, played: true, penaltiesMissed: 0 },
    icon: '🛡️'
  },
  {
    name: 'Goal + Assist',
    description: 'Scorer with assist',
    performance: { goals: 1, assists: 1, yellowCards: 0, redCards: 0, played: true, penaltiesMissed: 0 },
    icon: '⚽'
  },
  {
    name: 'Hat-trick Hero',
    description: 'Hat-trick scorer',
    performance: { goals: 3, assists: 0, yellowCards: 0, redCards: 0, played: true, penaltiesMissed: 0 },
    icon: '🎩'
  },
  {
    name: 'Booked Player',
    description: 'Player with yellow card',
    performance: { goals: 0, assists: 0, yellowCards: 1, redCards: 0, played: true, penaltiesMissed: 0 },
    icon: '🟨'
  },
  {
    name: 'Sent Off',
    description: 'Player with red card',
    performance: { goals: 0, assists: 0, yellowCards: 0, redCards: 1, played: true, penaltiesMissed: 0 },
    icon: '🟥'
  },
  {
    name: 'Substitute',
    description: 'Substitute appearance',
    performance: { goals: 0, assists: 0, yellowCards: 0, redCards: 0, played: true, penaltiesMissed: 0 },
    icon: '🔄'
  },
  {
    name: 'No Show',
    description: 'Player did not play',
    performance: { goals: 0, assists: 0, yellowCards: 0, redCards: 0, played: false, penaltiesMissed: 0 },
    icon: '❌'
  }
];

export const EnhancedPerformanceEntry: React.FC<EnhancedPerformanceEntryProps> = ({ 
  match, 
  onError, 
  onSuccess, 
  onClose 
}) => {
  const [loading, setLoading] = useState(false);
  const [activeTab, setActiveTab] = useState<'single' | 'bulk' | 'templates' | 'import'>('single');
  const [performances, setPerformances] = useState<PlayerPerformance[]>([]);
  const [players, setPlayers] = useState<FootballPlayer[]>([]);
  const [playerSearch, setPlayerSearch] = useState('');
  const [showPlayerDropdown, setShowPlayerDropdown] = useState(false);
  
  // Single entry state
  const [newPerformance, setNewPerformance] = useState<CreatePlayerPerformanceRequest>({
    playerId: '',
    goals: 0,
    penaltyGoals: 0,
    penaltiesMissed: 0,
    ownGoals: 0,
    assists: 0,
    yellowCards: 0,
    redCards: 0,
    played: true
  });

  // Bulk entry state
  const [bulkEntries, setBulkEntries] = useState<BulkPerformanceEntry[]>([]);
  const [selectedPlayers, setSelectedPlayers] = useState<string[]>([]);

  const loadData = useCallback(async () => {
    setLoading(true);
    try {
      const [performancesData, playersData] = await Promise.all([
        adminService.getMatchPerformances(match.id),
        adminService.getAllPlayers({ pageSize: 1000 })
      ]);
      setPerformances(performancesData);
      setPlayers(playersData.items);
    } catch (err: any) {
      onError(err.message || 'Failed to load data');
    } finally {
      setLoading(false);
    }
  }, [match.id, onError]);

  useEffect(() => {
    loadData();
  }, [loadData]);

  // Filter players based on search and availability
  const filteredPlayers = useMemo(() => {
    const existingPlayerIds = new Set(performances.map(p => p.playerId));
    return players
      .filter(player => !existingPlayerIds.has(player.id))
      .filter(player => 
        playerSearch === '' || 
        player.name.toLowerCase().includes(playerSearch.toLowerCase()) ||
        player.position.toLowerCase().includes(playerSearch.toLowerCase()) ||
        (player.club && player.club.toLowerCase().includes(playerSearch.toLowerCase()))
      )
      .slice(0, 10);
  }, [players, performances, playerSearch]);

  // Calculate fantasy points in real-time
  const calculateFantasyPoints = useCallback((perf: Partial<CreatePlayerPerformanceRequest>): number => {
    if (!perf.played) return 0;
    
    let points = 2; // Base points for playing
    points += (perf.goals || 0) * 6; // 6 points per goal
    points += (perf.penaltyGoals || 0) * 4; // 4 additional points for penalty goals
    points -= (perf.penaltiesMissed || 0) * 2; // -2 points for missed penalties
    points -= (perf.ownGoals || 0) * 2; // -2 points for own goals
    points += (perf.assists || 0) * 4; // 4 points per assist
    points -= (perf.yellowCards || 0) * 1; // -1 point per yellow card
    points -= (perf.redCards || 0) * 3; // -3 points per red card
    
    return Math.max(0, points);
  }, []);

  const handleSingleSubmit = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!newPerformance.playerId) return;

    setLoading(true);
    try {
      await adminService.addPlayerPerformance(match.id, newPerformance);
      onSuccess('Performance added successfully');
      setNewPerformance({
        playerId: '',
        goals: 0,
        penaltyGoals: 0,
        penaltiesMissed: 0,
        ownGoals: 0,
        assists: 0,
        yellowCards: 0,
        redCards: 0,
        played: true
      });
      setPlayerSearch('');
      await loadData();
    } catch (err: any) {
      onError(err.message || 'Failed to add performance');
    } finally {
      setLoading(false);
    }
  };

  const handleBulkSubmit = async () => {
    if (bulkEntries.length === 0) return;

    setLoading(true);
    try {
      const performances = bulkEntries.map(entry => ({
        playerId: entry.playerId,
        goals: entry.goals,
        penaltyGoals: entry.penaltyGoals,
        penaltiesMissed: entry.penaltiesMissed,
        ownGoals: entry.ownGoals,
        assists: entry.assists,
        yellowCards: entry.yellowCards,
        redCards: entry.redCards,
        played: entry.played
      }));

      // Use bulk API endpoint
      try {
        const result = await adminService.addBulkPlayerPerformances(match.id, performances);
        onSuccess(result);
        setBulkEntries([]);
        setSelectedPlayers([]);
        await loadData();
      } catch (bulkError) {
        // Fallback to individual API calls if bulk fails
        console.warn('Bulk API failed, falling back to individual calls:', bulkError);
        let successCount = 0;
        for (const performance of performances) {
          try {
            await adminService.addPlayerPerformance(match.id, performance);
            successCount++;
          } catch (err) {
            console.error('Failed to add performance:', err);
          }
        }
        onSuccess(`${successCount} performances added successfully (using individual calls)`);
        setBulkEntries([]);
        setSelectedPlayers([]);
        await loadData();
      }
    } catch (err: any) {
      onError(err.message || 'Failed to add bulk performances');
    } finally {
      setLoading(false);
    }
  };

  const handleImportPerformances = async (importedPerformances: CreatePlayerPerformanceRequest[]) => {
    if (importedPerformances.length === 0) {
      onError('No valid performances found in import data');
      return;
    }

    setLoading(true);
    try {
      // Use bulk API for imported data
      const result = await adminService.addBulkPlayerPerformances(match.id, importedPerformances);
      onSuccess(`Import successful: ${result}`);
      await loadData();
    } catch (err: any) {
      // Fallback to individual calls
      console.warn('Bulk import failed, falling back to individual calls:', err);
      let successCount = 0;
      for (const performance of importedPerformances) {
        try {
          await adminService.addPlayerPerformance(match.id, performance);
          successCount++;
        } catch (error) {
          console.error('Failed to import performance:', error);
        }
      }
      if (successCount > 0) {
        onSuccess(`Imported ${successCount} of ${importedPerformances.length} performances`);
        await loadData();
      } else {
        onError('Failed to import any performances');
      }
    } finally {
      setLoading(false);
    }
  };

  const applyTemplate = (template: PerformanceTemplate) => {
    if (activeTab === 'single') {
      setNewPerformance(prev => ({
        ...prev,
        ...template.performance
      }));
    } else if (activeTab === 'bulk' && selectedPlayers.length > 0) {
      setBulkEntries(prev => 
        prev.map(entry => 
          selectedPlayers.includes(entry.playerId) 
            ? { 
                ...entry, 
                ...template.performance,
                calculatedPoints: calculateFantasyPoints({ ...entry, ...template.performance })
              }
            : entry
        )
      );
    }
  };

  const addPlayerToBulk = (player: FootballPlayer) => {
    if (bulkEntries.some(entry => entry.playerId === player.id)) return;
    
    const newEntry: BulkPerformanceEntry = {
      playerId: player.id,
      playerName: player.name,
      position: player.position,
      club: player.club || '',
      goals: 0,
      penaltyGoals: 0,
      penaltiesMissed: 0,
      ownGoals: 0,
      assists: 0,
      yellowCards: 0,
      redCards: 0,
      played: true,
      calculatedPoints: calculateFantasyPoints({ played: true })
    };
    
    setBulkEntries(prev => [...prev, newEntry]);
  };

  const updateBulkEntry = (playerId: string, field: keyof BulkPerformanceEntry, value: any) => {
    setBulkEntries(prev => 
      prev.map(entry => 
        entry.playerId === playerId 
          ? { 
              ...entry, 
              [field]: value,
              calculatedPoints: calculateFantasyPoints({ ...entry, [field]: value })
            }
          : entry
      )
    );
  };

  const removeBulkEntry = (playerId: string) => {
    setBulkEntries(prev => prev.filter(entry => entry.playerId !== playerId));
    setSelectedPlayers(prev => prev.filter(id => id !== playerId));
  };

  const currentFantasyPoints = useMemo(() => 
    calculateFantasyPoints(newPerformance), [newPerformance, calculateFantasyPoints]);

  return (
    <div className="modal show d-block position-fixed top-0 start-0 w-100 h-100" tabIndex={-1} style={{zIndex: 1050, background: 'rgba(0,0,0,0.5)'}}>
      <div className="modal-dialog modal-dialog-centered modal-fullscreen-sm-down modal-xl mx-auto my-4" style={{maxWidth: '95%', maxHeight: '95vh'}}>
        <div className="modal-content bg-dark text-light">
          <div className="modal-header border-secondary" style={{background: '#212529'}}>
            <h5 className="modal-title">
              <i className="fas fa-chart-line me-2"></i>
              Performance Entry - {match.homeTeam} vs {match.awayTeam}
            </h5>
            <button type="button" className="btn-close btn-close-white" onClick={onClose}></button>
          </div>
          <div className="modal-body overflow-auto px-md-4 bg-dark" style={{maxHeight: 'calc(95vh - 140px)', overscrollBehavior: 'contain'}}>
            {/* Tab Navigation */}
            <ul className="nav border-bottom border-secondary mb-4 nav-justified nav-fill flex-nowrap overflow-auto bg-dark" role="tablist">
              <li className="nav-item" role="presentation">
                <button 
                  className={`nav-link bg-transparent ${activeTab === 'single' ? 'active border-bottom border-2 border-info text-info' : 'text-light'}`}
                  onClick={() => setActiveTab('single')}
                >
                  <i className="fas fa-user d-block d-sm-none"></i>
                  <span className="d-none d-sm-inline"><i className="fas fa-user me-1"></i>Single</span>
                </button>
              </li>
              <li className="nav-item" role="presentation">
                <button 
                  className={`nav-link bg-transparent ${activeTab === 'bulk' ? 'active border-bottom border-2 border-info text-info' : 'text-light'}`}
                  onClick={() => setActiveTab('bulk')}
                >
                  <i className="fas fa-users d-block d-sm-none"></i>
                  <span className="d-none d-sm-inline"><i className="fas fa-users me-1"></i>Bulk ({bulkEntries.length})</span>
                </button>
              </li>
              <li className="nav-item" role="presentation">
                <button 
                  className={`nav-link bg-transparent ${activeTab === 'templates' ? 'active border-bottom border-2 border-info text-info' : 'text-light'}`}
                  onClick={() => setActiveTab('templates')}
                >
                  <i className="fas fa-magic d-block d-sm-none"></i>
                  <span className="d-none d-sm-inline"><i className="fas fa-magic me-1"></i>Templates</span>
                </button>
              </li>
              <li className="nav-item" role="presentation">
                <button 
                  className={`nav-link bg-transparent ${activeTab === 'import' ? 'active border-bottom border-2 border-info text-info' : 'text-light'}`}
                  onClick={() => setActiveTab('import')}
                >
                  <i className="fas fa-file-import d-block d-sm-none"></i>
                  <span className="d-none d-sm-inline"><i className="fas fa-file-import me-1"></i>Import</span>
                </button>
              </li>
            </ul>

            {/* Single Entry Tab */}
            {activeTab === 'single' && (
              <div className="row">
                <div className="col-md-8">
                  <div className="card bg-dark border-secondary">
                    <div className="card-header bg-dark border-secondary">
                      <h6 className="mb-0 text-light">Add Single Performance</h6>
                    </div>
                    <div className="card-body bg-dark">
                      <form onSubmit={handleSingleSubmit}>
                        <div className="row">
                          <div className="col-md-6">
                            <div className="mb-3">
                              <label className="form-label text-light">Player *</label>
                              <div className="position-relative">
                                <input
                                  type="text"
                                  className="form-control bg-secondary text-white border-dark"
                                  value={playerSearch}
                                  onChange={(e) => {
                                    setPlayerSearch(e.target.value);
                                    setShowPlayerDropdown(true);
                                    if (e.target.value === '') {
                                      setNewPerformance(prev => ({ ...prev, playerId: '' }));
                                    }
                                  }}
                                  onFocus={() => setShowPlayerDropdown(true)}
                                  placeholder="Search for player..."
                                  required
                                />
                                {showPlayerDropdown && playerSearch && filteredPlayers.length > 0 && (
                                  <div className="dropdown-menu show position-absolute w-100 bg-dark border-secondary" 
                                       style={{ maxHeight: '300px', overflowY: 'auto', zIndex: 1000 }}>
                                    {filteredPlayers.map(player => (
                                      <button
                                        key={player.id}
                                        type="button"
                                        className="dropdown-item d-flex justify-content-between text-light"
                                        onClick={() => {
                                          setNewPerformance(prev => ({ ...prev, playerId: player.id }));
                                          setPlayerSearch(`${player.name} (${player.position})`);
                                          setShowPlayerDropdown(false);
                                        }}
                                      >
                                        <span>
                                          <strong>{player.name}</strong>
                                          <small className="text-muted ms-2">{player.position}</small>
                                        </span>
                                        <small className="text-muted">{player.club}</small>
                                      </button>
                                    ))}
                                  </div>
                                )}
                              </div>
                            </div>
                          </div>
                          <div className="col-md-3">
                            <div className="mb-3">
                              <label className="form-label text-light">Penalties Missed</label>
                              <input
                                type="number"
                                className="form-control bg-secondary text-white border-dark"
                                min="0"
                                value={newPerformance.penaltiesMissed}
                                onChange={(e) => setNewPerformance(prev => ({ 
                                  ...prev, 
                                  penaltiesMissed: parseInt(e.target.value) || 0 
                                }))}
                              />
                            </div>
                          </div>
                          <div className="col-md-3">
                            <div className="mb-3">
                              <label className="form-label text-light">Fantasy Points</label>
                              <div className="form-control d-flex align-items-center justify-content-center bg-secondary border-dark">
                                <span className="badge bg-primary fs-6">{currentFantasyPoints}</span>
                              </div>
                            </div>
                          </div>
                        </div>

                        <div className="row">
                          <div className="col-md-2">
                            <div className="mb-3">
                              <label className="form-label text-light">Goals</label>
                              <input
                                type="number"
                                className="form-control bg-secondary text-white border-dark"
                                min="0"
                                value={newPerformance.goals}
                                onChange={(e) => setNewPerformance(prev => ({ 
                                  ...prev, 
                                  goals: parseInt(e.target.value) || 0 
                                }))}
                              />
                            </div>
                          </div>
                          <div className="col-md-2">
                            <div className="mb-3">
                              <label className="form-label">Penalty Goals</label>
                              <input
                                type="number"
                                className="form-control"
                                min="0"
                                value={newPerformance.penaltyGoals}
                                onChange={(e) => setNewPerformance(prev => ({ 
                                  ...prev, 
                                  penaltyGoals: parseInt(e.target.value) || 0 
                                }))}
                              />
                            </div>
                          </div>
                          <div className="col-md-2">
                            <div className="mb-3">
                              <label className="form-label">Own Goals</label>
                              <input
                                type="number"
                                className="form-control"
                                min="0"
                                value={newPerformance.ownGoals}
                                onChange={(e) => setNewPerformance(prev => ({ 
                                  ...prev, 
                                  ownGoals: parseInt(e.target.value) || 0 
                                }))}
                              />
                            </div>
                          </div>
                          <div className="col-md-2">
                            <div className="mb-3">
                              <label className="form-label">Assists</label>
                              <input
                                type="number"
                                className="form-control"
                                min="0"
                                value={newPerformance.assists}
                                onChange={(e) => setNewPerformance(prev => ({ 
                                  ...prev, 
                                  assists: parseInt(e.target.value) || 0 
                                }))}
                              />
                            </div>
                          </div>
                          <div className="col-md-2">
                            <div className="mb-3">
                              <label className="form-label">Yellow Cards</label>
                              <input
                                type="number"
                                className="form-control"
                                min="0"
                                max="2"
                                value={newPerformance.yellowCards}
                                onChange={(e) => setNewPerformance(prev => ({ 
                                  ...prev, 
                                  yellowCards: parseInt(e.target.value) || 0 
                                }))}
                              />
                            </div>
                          </div>
                          <div className="col-md-2">
                            <div className="mb-3">
                              <label className="form-label">Red Cards</label>
                              <input
                                type="number"
                                className="form-control"
                                min="0"
                                max="1"
                                value={newPerformance.redCards}
                                onChange={(e) => setNewPerformance(prev => ({ 
                                  ...prev, 
                                  redCards: parseInt(e.target.value) || 0 
                                }))}
                              />
                            </div>
                          </div>
                        </div>

                        <div className="row">
                          <div className="col-md-6">
                            <div className="form-check">
                              <input
                                className="form-check-input"
                                type="checkbox"
                                id="singlePlayed"
                                checked={newPerformance.played}
                                onChange={(e) => setNewPerformance(prev => ({ 
                                  ...prev, 
                                  played: e.target.checked 
                                }))}
                              />
                              <label className="form-check-label text-light" htmlFor="singlePlayed">
                                Player participated in match
                              </label>
                            </div>
                          </div>
                          <div className="col-md-6 text-end d-none d-md-block">
                            <button 
                              type="submit" 
                              className="btn btn-success"
                              disabled={loading || !newPerformance.playerId}
                            >
                              {loading ? (
                                <span className="spinner-border spinner-border-sm me-2"></span>
                              ) : (
                                <i className="fas fa-plus me-2"></i>
                              )}
                              Add Performance
                            </button>
                          </div>
                        </div>
                      </form>
                    </div>
                  </div>
                </div>

                {/* Current Performances Sidebar */}
                <div className="col-md-4">
                  <div className="card bg-dark border-secondary">
                    <div className="card-header bg-dark border-secondary">
                      <h6 className="mb-0 text-light">Current Performances ({performances.length})</h6>
                    </div>
                    <div className="card-body bg-dark" style={{ maxHeight: '400px', overflowY: 'auto' }}>
                      {performances.length === 0 ? (
                        <p className="text-muted text-center">No performances recorded yet</p>
                      ) : (
                        performances.map(perf => (
                          <div key={perf.id} className="d-flex justify-content-between align-items-center mb-2 p-2 border border-secondary rounded bg-secondary bg-opacity-25">
                            <div>
                              <strong className="text-white">{perf.playerName}</strong>
                              <small className="text-light text-opacity-75 d-block">{perf.playerPosition} • {perf.playerClub}</small>
                            </div>
                            <div className="text-end">
                              <span className="badge bg-primary">{perf.points} pts</span>
                              <div className="small text-muted">
                                ⚽{perf.goals} 🅰️{perf.assists} ❌{perf.penaltiesMissed}
                              </div>
                            </div>
                          </div>
                        ))
                      )}
                    </div>
                  </div>
                </div>
              </div>
            )}

            {/* Bulk Entry Tab */}
            {activeTab === 'bulk' && (
              <div>
                <div className="row mb-4">
                  <div className="col-md-6">
                    <div className="card">
                      <div className="card-header">
                        <h6 className="mb-0">Add Players</h6>
                      </div>
                      <div className="card-body">
                        <input
                          type="text"
                          className="form-control mb-3"
                          placeholder="Search players..."
                          value={playerSearch}
                          onChange={(e) => setPlayerSearch(e.target.value)}
                        />
                        <div style={{ maxHeight: '300px', overflowY: 'auto' }}>
                          {filteredPlayers.map(player => (
                            <button
                              key={player.id}
                              type="button"
                              className="btn btn-outline-primary btn-sm me-2 mb-2"
                              onClick={() => addPlayerToBulk(player)}
                            >
                              <i className="fas fa-plus me-1"></i>
                              {player.name} ({player.position})
                            </button>
                          ))}
                        </div>
                      </div>
                    </div>
                  </div>
                  <div className="col-md-6">
                    <div className="card">
                      <div className="card-header d-flex justify-content-between align-items-center">
                        <h6 className="mb-0">Quick Actions</h6>
                        <small className="text-muted">{selectedPlayers.length} selected</small>
                      </div>
                      <div className="card-body">
                        <div className="d-flex flex-wrap gap-2 mb-3">
                          <button 
                            className="btn btn-outline-info btn-sm"
                            onClick={() => setSelectedPlayers(bulkEntries.map(e => e.playerId))}
                          >
                            <i className="fas fa-check-square me-1"></i>
                            Select All
                          </button>
                          <button 
                            className="btn btn-outline-secondary btn-sm"
                            onClick={() => setSelectedPlayers([])}
                          >
                            <i className="fas fa-square me-1"></i>
                            Clear Selection
                          </button>
                        </div>
                        {selectedPlayers.length > 0 && (
                          <div className="row g-2">
                            <div className="col-md-6">
                              <input
                                type="number"
                                className="form-control form-control-sm"
                                placeholder="Set Penalties Missed"
                                min="0"
                                onChange={(e) => {
                                  const penaltiesMissed = parseInt(e.target.value) || 0;
                                  setBulkEntries(prev => 
                                    prev.map(entry => 
                                      selectedPlayers.includes(entry.playerId) 
                                        ? { ...entry, penaltiesMissed, calculatedPoints: calculateFantasyPoints({ ...entry, penaltiesMissed }) }
                                        : entry
                                    )
                                  );
                                }}
                              />
                              <small className="text-muted">Penalties Missed for selected</small>
                            </div>
                            <div className="col-md-6">
                              <div className="form-check">
                                <input
                                  className="form-check-input"
                                  type="checkbox"
                                  id="bulkPlayed"
                                  onChange={(e) => {
                                    const played = e.target.checked;
                                    setBulkEntries(prev => 
                                      prev.map(entry => 
                                        selectedPlayers.includes(entry.playerId) 
                                          ? { ...entry, played, calculatedPoints: calculateFantasyPoints({ ...entry, played }) }
                                          : entry
                                      )
                                    );
                                  }}
                                />
                                <label className="form-check-label" htmlFor="bulkPlayed">
                                  <small>Set as played</small>
                                </label>
                              </div>
                            </div>
                          </div>
                        )}
                        {bulkEntries.length > 0 && (
                          <button 
                            className="btn btn-success w-100 mt-3 d-none d-md-block"
                            onClick={handleBulkSubmit}
                            disabled={loading}
                          >
                            {loading ? (
                              <span className="spinner-border spinner-border-sm me-2"></span>
                            ) : (
                              <i className="fas fa-save me-2"></i>
                            )}
                            Save All Performances
                          </button>
                        )}
                      </div>
                    </div>
                  </div>
                </div>

                {/* Bulk Entry Table */}
                {bulkEntries.length > 0 && (
                  <div className="card">
                    <div className="card-header">
                      <h6 className="mb-0">Performance Entry ({bulkEntries.length} players)</h6>
                    </div>
                    <div className="card-body">
                      <div className="table-responsive">
                        <table className="table table-sm">
                          <thead>
                            <tr>
                              <th>
                                <input
                                  type="checkbox"
                                  checked={selectedPlayers.length === bulkEntries.length}
                                  onChange={(e) => {
                                    if (e.target.checked) {
                                      setSelectedPlayers(bulkEntries.map(entry => entry.playerId));
                                    } else {
                                      setSelectedPlayers([]);
                                    }
                                  }}
                                />
                              </th>
                              <th>Player</th>
                              <th>G</th>
                              <th>PG</th>
                              <th>OG</th>
                              <th>A</th>
                              <th>YC</th>
                              <th>RC</th>
                              <th>Penalties Missed</th>
                              <th>Played</th>
                              <th>Points</th>
                              <th>Actions</th>
                            </tr>
                          </thead>
                          <tbody>
                            {bulkEntries.map(entry => (
                              <tr key={entry.playerId} 
                                  className={selectedPlayers.includes(entry.playerId) ? 'table-active' : ''}>
                                <td>
                                  <input
                                    type="checkbox"
                                    checked={selectedPlayers.includes(entry.playerId)}
                                    onChange={(e) => {
                                      if (e.target.checked) {
                                        setSelectedPlayers(prev => [...prev, entry.playerId]);
                                      } else {
                                        setSelectedPlayers(prev => prev.filter(id => id !== entry.playerId));
                                      }
                                    }}
                                  />
                                </td>
                                <td>
                                  <strong>{entry.playerName}</strong>
                                  <small className="text-muted d-block">{entry.position} • {entry.club}</small>
                                </td>
                                <td>
                                  <input
                                    type="number"
                                    className="form-control form-control-sm"
                                    style={{ width: '60px' }}
                                    min="0"
                                    value={entry.goals}
                                    onChange={(e) => updateBulkEntry(entry.playerId, 'goals', parseInt(e.target.value) || 0)}
                                  />
                                </td>
                                <td>
                                  <input
                                    type="number"
                                    className="form-control form-control-sm"
                                    style={{ width: '60px' }}
                                    min="0"
                                    value={entry.penaltyGoals}
                                    onChange={(e) => updateBulkEntry(entry.playerId, 'penaltyGoals', parseInt(e.target.value) || 0)}
                                  />
                                </td>
                                <td>
                                  <input
                                    type="number"
                                    className="form-control form-control-sm"
                                    style={{ width: '60px' }}
                                    min="0"
                                    value={entry.ownGoals}
                                    onChange={(e) => updateBulkEntry(entry.playerId, 'ownGoals', parseInt(e.target.value) || 0)}
                                  />
                                </td>
                                <td>
                                  <input
                                    type="number"
                                    className="form-control form-control-sm"
                                    style={{ width: '60px' }}
                                    min="0"
                                    value={entry.assists}
                                    onChange={(e) => updateBulkEntry(entry.playerId, 'assists', parseInt(e.target.value) || 0)}
                                  />
                                </td>
                                <td>
                                  <input
                                    type="number"
                                    className="form-control form-control-sm"
                                    style={{ width: '60px' }}
                                    min="0"
                                    max="2"
                                    value={entry.yellowCards}
                                    onChange={(e) => updateBulkEntry(entry.playerId, 'yellowCards', parseInt(e.target.value) || 0)}
                                  />
                                </td>
                                <td>
                                  <input
                                    type="number"
                                    className="form-control form-control-sm"
                                    style={{ width: '60px' }}
                                    min="0"
                                    max="1"
                                    value={entry.redCards}
                                    onChange={(e) => updateBulkEntry(entry.playerId, 'redCards', parseInt(e.target.value) || 0)}
                                  />
                                </td>
                                <td>
                                  <input
                                    type="number"
                                    className="form-control form-control-sm"
                                    style={{ width: '80px' }}
                                    min="0"
                                    value={entry.penaltiesMissed}
                                    onChange={(e) => updateBulkEntry(entry.playerId, 'penaltiesMissed', parseInt(e.target.value) || 0)}
                                  />
                                </td>
                                <td>
                                  <input
                                    type="checkbox"
                                    checked={entry.played}
                                    onChange={(e) => updateBulkEntry(entry.playerId, 'played', e.target.checked)}
                                  />
                                </td>
                                <td>
                                  <span className="badge bg-primary">{entry.calculatedPoints || 0}</span>
                                </td>
                                <td>
                                  <button
                                    type="button"
                                    className="btn btn-outline-danger btn-sm"
                                    onClick={() => removeBulkEntry(entry.playerId)}
                                  >
                                    <i className="fas fa-trash"></i>
                                  </button>
                                </td>
                              </tr>
                            ))}
                          </tbody>
                        </table>
                      </div>
                    </div>
                  </div>
                )}
              </div>
            )}

            {/* Import/Export Tab */}
            {activeTab === 'import' && (
              <PerformanceImportExport
                players={players}
                onImport={handleImportPerformances}
                onExport={() => {}}
                existingPerformances={performances}
              />
            )}

            {/* Templates Tab */}
            {activeTab === 'templates' && (
              <div>
                <div className="alert alert-info">
                  <h6><i className="fas fa-info-circle me-2"></i>Performance Templates</h6>
                  <p className="mb-0">
                    Use these pre-configured templates to quickly set common performance patterns. 
                    Switch to Single Entry tab to apply to one player, or Bulk Entry tab to apply to multiple selected players.
                  </p>
                </div>
                
                <div className="row">
                  {PERFORMANCE_TEMPLATES.map((template, index) => (
                    <div key={index} className="col-md-3 mb-3">
                      <div className="card h-100">
                        <div className="card-body text-center">
                          <div className="display-4 mb-2">{template.icon}</div>
                          <h6 className="card-title">{template.name}</h6>
                          <p className="card-text small text-muted">{template.description}</p>
                          <div className="small mb-3">
                            <div><strong>Points:</strong> {calculateFantasyPoints(template.performance)}</div>
                            <div><strong>Penalties Missed:</strong> {template.performance.penaltiesMissed}</div>
                            <div><strong>Played:</strong> {template.performance.played ? 'Yes' : 'No'}</div>
                          </div>
                          <button
                            type="button"
                            className="btn btn-outline-primary btn-sm"
                            onClick={() => applyTemplate(template)}
                          >
                            <i className="fas fa-magic me-1"></i>
                            Apply Template
                          </button>
                        </div>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            )}
          </div>
          <div className="modal-footer sticky-bottom border-secondary" style={{background: '#212529'}}>
            <button 
              type="button" 
              className="btn btn-outline-light" 
              onClick={onClose}
            >
              <i className="fas fa-times me-2"></i>
              Close
            </button>
            {activeTab === 'single' && (
              <button 
                type="button" 
                className="btn btn-success"
                disabled={loading || !newPerformance.playerId}
                onClick={handleSingleSubmit}
              >
                {loading ? (
                  <span className="spinner-border spinner-border-sm me-2"></span>
                ) : (
                  <i className="fas fa-save me-2"></i>
                )}
                Save Performance
              </button>
            )}
            {activeTab === 'bulk' && bulkEntries.length > 0 && (
              <button 
                type="button" 
                className="btn btn-success"
                disabled={loading}
                onClick={handleBulkSubmit}
              >
                {loading ? (
                  <span className="spinner-border spinner-border-sm me-2"></span>
                ) : (
                  <i className="fas fa-save me-2"></i>
                )}
                Save All ({bulkEntries.length})
              </button>
            )}
          </div>
        </div>
      </div>
    </div>
  );
};
