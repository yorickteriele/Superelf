import React, { useState, useEffect, useCallback } from 'react';
import { 
  adminService, 
  Match, 
  CreateMatchRequest, 
  PlayerPerformance,
  CreatePlayerPerformanceRequest,
  FootballPlayer,
  Club
} from '../../services/adminService';
import { SimplePerformanceEntry } from './SimplePerformanceEntry';

interface MatchManagementProps {
  onError: (message: string) => void;
  onSuccess: (message: string) => void;
}

export const MatchManagement: React.FC<MatchManagementProps> = ({ onError, onSuccess }) => {
  const [matches, setMatches] = useState<Match[]>([]);
  const [loading, setLoading] = useState(false);
  const [editingMatch, setEditingMatch] = useState<Match | null>(null);
  const [showPerformances, setShowPerformances] = useState<string | null>(null);
  const [performances, setPerformances] = useState<PlayerPerformance[]>([]);
  const [players, setPlayers] = useState<FootballPlayer[]>([]);
  const [clubs, setClubs] = useState<Club[]>([]);
  const [homeTeamSearch, setHomeTeamSearch] = useState('');
  const [awayTeamSearch, setAwayTeamSearch] = useState('');
  const [showHomeDropdown, setShowHomeDropdown] = useState(false);
  const [showAwayDropdown, setShowAwayDropdown] = useState(false);
  const [showBulkModal, setShowBulkModal] = useState(false);
  const [bulkMatchesText, setBulkMatchesText] = useState('');

  const [newMatch, setNewMatch] = useState<CreateMatchRequest>({
    homeTeam: '',
    awayTeam: '',
    matchDate: new Date().toISOString().split('T')[0],
    competition: 'Eredivisie',
    round: 1
  });

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

  const loadData = useCallback(async () => {
    setLoading(true);
    try {
      const [matchesData, playersData, clubsData] = await Promise.all([
        adminService.getAllMatches(),
        adminService.getAllPlayers({ pageSize: 1000 }), // Get all players for performance selection
        adminService.getAllClubs()
      ]);
      setMatches(matchesData);
      setPlayers(playersData.items);
      setClubs(clubsData);
    } catch (err: any) {
      onError(err.message || 'Failed to load data');
    } finally {
      setLoading(false);
    }
  }, [onError]);

  useEffect(() => {
    loadData();
  }, [loadData]);

  // Close dropdowns when clicking outside
  useEffect(() => {
    const handleClickOutside = (event: MouseEvent) => {
      const target = event.target as Element;
      if (!target.closest('.position-relative')) {
        setShowHomeDropdown(false);
        setShowAwayDropdown(false);
      }
    };

    document.addEventListener('mousedown', handleClickOutside);
    return () => {
      document.removeEventListener('mousedown', handleClickOutside);
    };
  }, []);

  // Initialize search fields when match data changes
  useEffect(() => {
    setHomeTeamSearch(newMatch.homeTeam);
    setAwayTeamSearch(newMatch.awayTeam);
  }, [newMatch.homeTeam, newMatch.awayTeam]);

  const loadPerformances = async (matchId: string) => {
    try {
      const data = await adminService.getMatchPerformances(matchId);
      setPerformances(data);
    } catch (err: any) {
      onError(err.message || 'Failed to load performances');
    }
  };

  const handleCreateMatch = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      await adminService.createMatch(newMatch);
      onSuccess('Match created successfully');
      setNewMatch({
        homeTeam: '',
        awayTeam: '',
        matchDate: new Date().toISOString().split('T')[0],
        competition: 'Eredivisie',
        round: 1
      });
      await loadData();
    } catch (err: any) {
      onError(err.message || 'Failed to create match');
    } finally {
      setLoading(false);
    }
  };

  const handleUpdateMatch = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!editingMatch) return;

    setLoading(true);
    try {
      await adminService.updateMatch(editingMatch.id, {
        homeTeam: editingMatch.homeTeam,
        awayTeam: editingMatch.awayTeam,
        homeScore: editingMatch.homeScore,
        awayScore: editingMatch.awayScore,
        matchDate: editingMatch.matchDate,
        competition: editingMatch.competition,
        round: editingMatch.round,
        isCompleted: editingMatch.isCompleted
      });
      onSuccess('Match updated successfully');
      setEditingMatch(null);
      await loadData();
    } catch (err: any) {
      onError(err.message || 'Failed to update match');
    } finally {
      setLoading(false);
    }
  };

  const handleDeleteMatch = async (matchId: string) => {
    if (!window.confirm('Are you sure you want to delete this match? This will also delete all player performances.')) return;

    setLoading(true);
    try {
      await adminService.deleteMatch(matchId);
      onSuccess('Match deleted successfully');
      await loadData();
    } catch (err: any) {
      onError(err.message || 'Failed to delete match');
    } finally {
      setLoading(false);
    }
  };

  const handleBulkImport = async () => {
    setLoading(true);
    try {
      const matches: CreateMatchRequest[] = [];
      const lines = bulkMatchesText.trim().split('\n');
      
      for (const line of lines) {
        const parts = line.split(',').map(p => p.trim());
        if (parts.length >= 3) {
          matches.push({
            homeTeam: parts[0],
            awayTeam: parts[1],
            matchDate: parts[2],
            competition: parts[3] || 'Eredivisie',
            round: parseInt(parts[4]) || 1
          });
        }
      }

      // Create matches sequentially to avoid overwhelming the server
      let created = 0;
      let failed = 0;
      
      for (const match of matches) {
        try {
          await adminService.createMatch(match);
          created++;
        } catch (err) {
          failed++;
          console.error('Failed to create match:', match, err);
        }
      }

      onSuccess(`Bulk import completed: ${created} matches created, ${failed} failed`);
      setBulkMatchesText('');
      setShowBulkModal(false);
      await loadData();
    } catch (err: any) {
      onError(err.message || 'Failed to bulk import matches');
    } finally {
      setLoading(false);
    }
  };

  const handleAddPerformance = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!showPerformances) return;

    try {
      await adminService.addPlayerPerformance(showPerformances, newPerformance);
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
      await loadPerformances(showPerformances);
    } catch (err: any) {
      onError(err.message || 'Failed to add performance');
    }
  };

  const handleDeletePerformance = async (performanceId: string) => {
    if (!window.confirm('Are you sure you want to delete this performance?')) return;

    try {
      await adminService.deletePlayerPerformance(performanceId);
      onSuccess('Performance deleted successfully');
      if (showPerformances) {
        await loadPerformances(showPerformances);
      }
    } catch (err: any) {
      onError(err.message || 'Failed to delete performance');
    }
  };

  const competitions = ['Eredivisie', 'Champions League', 'Europa League', 'KNVB Beker', 'Conference League', 'International'];

  // Filter clubs based on search input
  const filteredHomeClubs = clubs.filter(club =>
    club.name.toLowerCase().includes(homeTeamSearch.toLowerCase())
  ).slice(0, 10);

  const filteredAwayClubs = clubs.filter(club =>
    club.name.toLowerCase().includes(awayTeamSearch.toLowerCase())
  ).slice(0, 10);

  return (
    <div className="container-fluid">
      <div className="row mb-4">
        <div className="col-md-12">
          <h4>Match Management</h4>
        </div>
      </div>

      <div className="row">
        {/* Add New Match Form */}
        <div className="col-md-4">
          <div className="card">
            <div className="card-header">
              <h5><i className="fas fa-plus-circle me-2"></i>Add New Match</h5>
            </div>
            <div className="card-body">
              <form onSubmit={handleCreateMatch}>
                <div className="mb-3">
                  <label className="form-label">Home Team *</label>
                  <div className="position-relative">
                    <input
                      type="text"
                      className="form-control"
                      value={homeTeamSearch}
                      onChange={(e) => {
                        setHomeTeamSearch(e.target.value);
                        setShowHomeDropdown(true);
                        setNewMatch({...newMatch, homeTeam: e.target.value});
                      }}
                      onFocus={() => setShowHomeDropdown(true)}
                      placeholder="Search for home team..."
                      required
                    />
                    {showHomeDropdown && homeTeamSearch && filteredHomeClubs.length > 0 && (
                      <div className="dropdown-menu show position-absolute w-100" style={{ maxHeight: '200px', overflowY: 'auto', zIndex: 1000 }}>
                        {filteredHomeClubs.map(club => (
                          <button
                            key={club.id}
                            type="button"
                            className="dropdown-item d-flex justify-content-between align-items-center"
                            onClick={() => {
                              setNewMatch({...newMatch, homeTeam: club.name});
                              setHomeTeamSearch(club.name);
                              setShowHomeDropdown(false);
                            }}
                          >
                            <span>{club.name}</span>
                            <small className="text-muted">🏠</small>
                          </button>
                        ))}
                      </div>
                    )}
                  </div>
                </div>
                <div className="mb-3">
                  <label className="form-label">Away Team *</label>
                  <div className="position-relative">
                    <input
                      type="text"
                      className="form-control"
                      value={awayTeamSearch}
                      onChange={(e) => {
                        setAwayTeamSearch(e.target.value);
                        setShowAwayDropdown(true);
                        setNewMatch({...newMatch, awayTeam: e.target.value});
                      }}
                      onFocus={() => setShowAwayDropdown(true)}
                      placeholder="Search for away team..."
                      required
                    />
                    {showAwayDropdown && awayTeamSearch && filteredAwayClubs.length > 0 && (
                      <div className="dropdown-menu show position-absolute w-100" style={{ maxHeight: '200px', overflowY: 'auto', zIndex: 1000 }}>
                        {filteredAwayClubs.map(club => (
                          <button
                            key={club.id}
                            type="button"
                            className="dropdown-item d-flex justify-content-between align-items-center"
                            onClick={() => {
                              setNewMatch({...newMatch, awayTeam: club.name});
                              setAwayTeamSearch(club.name);
                              setShowAwayDropdown(false);
                            }}
                          >
                            <span>{club.name}</span>
                            <small className="text-muted">✈️</small>
                          </button>
                        ))}
                      </div>
                    )}
                  </div>
                </div>
                <div className="mb-3">
                  <label className="form-label">Match Date *</label>
                  <input
                    type="date"
                    className="form-control"
                    value={newMatch.matchDate}
                    onChange={(e) => setNewMatch({...newMatch, matchDate: e.target.value})}
                    min={new Date().toISOString().split('T')[0]}
                    required
                  />
                  <small className="text-muted">
                    <i className="fas fa-info-circle me-1"></i>
                    Defaults to future dates
                  </small>
                </div>
                <div className="mb-3">
                  <label className="form-label">Competition</label>
                  <select
                    className="form-select"
                    value={newMatch.competition || ''}
                    onChange={(e) => setNewMatch({...newMatch, competition: e.target.value})}
                  >
                    {competitions.map(comp => (
                      <option key={comp} value={comp}>{comp}</option>
                    ))}
                  </select>
                </div>
                <div className="mb-3">
                  <label className="form-label">Round</label>
                  <input
                    type="number"
                    className="form-control"
                    value={newMatch.round}
                    onChange={(e) => setNewMatch({...newMatch, round: parseInt(e.target.value) || 1})}
                    min="1"
                    max="38"
                  />
                  <small className="text-muted">
                    <i className="fas fa-info-circle me-1"></i>
                    Matchday or round number
                  </small>
                </div>
                <button 
                  type="submit" 
                  className="btn btn-primary w-100" 
                  disabled={loading || !newMatch.homeTeam || !newMatch.awayTeam || newMatch.homeTeam === newMatch.awayTeam}
                >
                  {loading ? (
                    <>
                      <span className="spinner-border spinner-border-sm me-2"></span>
                      Creating...
                    </>
                  ) : (
                    <>
                      <i className="fas fa-plus me-2"></i>
                      Add Match
                    </>
                  )}
                </button>
                {newMatch.homeTeam === newMatch.awayTeam && newMatch.homeTeam && (
                  <small className="text-danger d-block mt-2">
                    <i className="fas fa-exclamation-triangle me-1"></i>
                    Home and away teams cannot be the same
                  </small>
                )}
              </form>
            </div>
          </div>
        </div>

        {/* Matches List */}
        <div className="col-md-8">
          <div className="card">
            <div className="card-header d-flex justify-content-between align-items-center">
              <h5 className="mb-0">Matches ({matches.length})</h5>
              <div className="btn-group">
                <button
                  className="btn btn-outline-success btn-sm"
                  onClick={() => setShowBulkModal(true)}
                  disabled={loading}
                  title="Import multiple matches at once"
                >
                  <i className="fas fa-upload me-1"></i>
                  Bulk Import
                </button>
                <div className="btn-group" role="group">
                  <button
                    type="button"
                    className="btn btn-outline-secondary dropdown-toggle btn-sm"
                    data-bs-toggle="dropdown"
                    aria-expanded="false"
                    title="Quick actions for matches"
                  >
                    <i className="fas fa-tools me-1"></i>
                    Actions
                  </button>
                  <ul className="dropdown-menu">
                    <li>
                      <button 
                        className="dropdown-item"
                        onClick={() => {
                          const completedMatches = matches.filter(m => m.isCompleted);
                          if (completedMatches.length === 0) {
                            onError('No completed matches found');
                            return;
                          }
                          onSuccess(`Found ${completedMatches.length} completed matches`);
                        }}
                      >
                        <i className="fas fa-check-circle me-2"></i>
                        View Completed ({matches.filter(m => m.isCompleted).length})
                      </button>
                    </li>
                    <li>
                      <button 
                        className="dropdown-item"
                        onClick={() => {
                          const upcomingMatches = matches.filter(m => !m.isCompleted);
                          if (upcomingMatches.length === 0) {
                            onError('No upcoming matches found');
                            return;
                          }
                          onSuccess(`Found ${upcomingMatches.length} upcoming matches`);
                        }}
                      >
                        <i className="fas fa-clock me-2"></i>
                        View Upcoming ({matches.filter(m => !m.isCompleted).length})
                      </button>
                    </li>
                    <li><hr className="dropdown-divider" /></li>
                    <li>
                      <button 
                        className="dropdown-item"
                        onClick={() => {
                          const matchesWithPerformances = matches.filter(m => m.playerPerformances.length > 0);
                          if (matchesWithPerformances.length === 0) {
                            onError('No matches with performance data found');
                            return;
                          }
                          onSuccess(`Found ${matchesWithPerformances.length} matches with performance data`);
                        }}
                      >
                        <i className="fas fa-chart-line me-2"></i>
                        With Performances ({matches.filter(m => m.playerPerformances.length > 0).length})
                      </button>
                    </li>
                    <li>
                      <button 
                        className="dropdown-item text-warning"
                        onClick={() => {
                          const matchesNeedingPerformances = matches.filter(m => m.isCompleted && m.playerPerformances.length === 0);
                          if (matchesNeedingPerformances.length === 0) {
                            onSuccess('All completed matches have performance data!');
                            return;
                          }
                          onError(`${matchesNeedingPerformances.length} completed matches need performance data`);
                        }}
                      >
                        <i className="fas fa-exclamation-triangle me-2"></i>
                        Need Performances ({matches.filter(m => m.isCompleted && m.playerPerformances.length === 0).length})
                      </button>
                    </li>
                  </ul>
                </div>
              </div>
            </div>
            <div className="card-body">
              {loading && matches.length === 0 ? (
                <div className="text-center">
                  <div className="spinner-border" role="status">
                    <span className="visually-hidden">Loading...</span>
                  </div>
                </div>
              ) : (
                <div className="table-responsive">
                  <table className="table table-hover">
                    <thead>
                      <tr>
                        <th>Teams</th>
                        <th>Score</th>
                        <th>Date</th>
                        <th>Competition</th>
                        <th>Status</th>
                        <th>Performances</th>
                        <th>Actions</th>
                      </tr>
                    </thead>
                    <tbody>
                      {matches.map(match => (
                        <tr key={match.id}>
                          <td>
                            <strong>{match.homeTeam}</strong> vs <strong>{match.awayTeam}</strong>
                          </td>
                          <td>
                            {match.isCompleted ? (
                              <span className="badge bg-success">
                                {match.homeScore} - {match.awayScore}
                              </span>
                            ) : (
                              <span className="text-muted">Not played</span>
                            )}
                          </td>
                          <td>{new Date(match.matchDate).toLocaleDateString()}</td>
                          <td>
                            <small className="text-muted">
                              {match.competition} - Round {match.round}
                            </small>
                          </td>
                          <td>
                            <span className={`badge ${match.isCompleted ? 'bg-success' : 'bg-warning'}`}>
                              {match.isCompleted ? 'Completed' : 'Scheduled'}
                            </span>
                          </td>
                          <td>
                            <button
                              className={`btn btn-sm ${match.playerPerformances.length > 0 ? 'btn-info' : 'btn-outline-info'}`}
                              onClick={() => {
                                setShowPerformances(match.id);
                              }}
                              title={match.playerPerformances.length > 0 ? 'View/Edit Performances' : 'Add Player Performances'}
                            >
                              <i className={`fas ${match.playerPerformances.length > 0 ? 'fa-chart-line' : 'fa-plus'} me-1`}></i>
                              {match.playerPerformances.length > 0 ? 
                                `${match.playerPerformances.length} Recorded` : 
                                'Add Performances'
                              }
                            </button>
                          </td>
                          <td>
                            <div className="btn-group" role="group">
                              <button
                                className="btn btn-sm btn-outline-primary"
                                onClick={() => setEditingMatch(match)}
                              >
                                Edit
                              </button>
                              <button
                                className="btn btn-sm btn-outline-danger"
                                onClick={() => handleDeleteMatch(match.id)}
                              >
                                Delete
                              </button>
                            </div>
                          </td>
                        </tr>
                      ))}
                      {matches.length === 0 && !loading && (
                        <tr>
                          <td colSpan={7} className="text-center text-muted">
                            No matches found. Add your first match using the form on the left.
                          </td>
                        </tr>
                      )}
                    </tbody>
                  </table>
                </div>
              )}
            </div>
          </div>
        </div>
      </div>

      {/* Edit Match Modal */}
      {editingMatch && (
        <div className="modal show d-block" tabIndex={-1}>
          <div className="modal-dialog">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">Edit Match</h5>
                <button
                  type="button"
                  className="btn-close"
                  onClick={() => setEditingMatch(null)}
                ></button>
              </div>
              <form onSubmit={handleUpdateMatch}>
                <div className="modal-body">
                  <div className="row">
                    <div className="col-md-6">
                      <div className="mb-3">
                        <label className="form-label">Home Team</label>
                        <input
                          type="text"
                          className="form-control"
                          value={editingMatch.homeTeam}
                          onChange={(e) => setEditingMatch({...editingMatch, homeTeam: e.target.value})}
                          required
                        />
                      </div>
                    </div>
                    <div className="col-md-6">
                      <div className="mb-3">
                        <label className="form-label">Away Team</label>
                        <input
                          type="text"
                          className="form-control"
                          value={editingMatch.awayTeam}
                          onChange={(e) => setEditingMatch({...editingMatch, awayTeam: e.target.value})}
                          required
                        />
                      </div>
                    </div>
                  </div>
                  <div className="row">
                    <div className="col-md-6">
                      <div className="mb-3">
                        <label className="form-label">Home Score</label>
                        <input
                          type="number"
                          className="form-control"
                          value={editingMatch.homeScore || 0}
                          onChange={(e) => setEditingMatch({...editingMatch, homeScore: parseInt(e.target.value) || 0})}
                          min="0"
                        />
                      </div>
                    </div>
                    <div className="col-md-6">
                      <div className="mb-3">
                        <label className="form-label">Away Score</label>
                        <input
                          type="number"
                          className="form-control"
                          value={editingMatch.awayScore || 0}
                          onChange={(e) => setEditingMatch({...editingMatch, awayScore: parseInt(e.target.value) || 0})}
                          min="0"
                        />
                      </div>
                    </div>
                  </div>
                  <div className="mb-3">
                    <label className="form-label">Match Date</label>
                    <input
                      type="date"
                      className="form-control"
                      value={editingMatch.matchDate.split('T')[0]}
                      onChange={(e) => setEditingMatch({...editingMatch, matchDate: e.target.value})}
                      required
                    />
                  </div>
                  <div className="mb-3">
                    <label className="form-label">Competition</label>
                    <select
                      className="form-select"
                      value={editingMatch.competition || ''}
                      onChange={(e) => setEditingMatch({...editingMatch, competition: e.target.value})}
                    >
                      {competitions.map(comp => (
                        <option key={comp} value={comp}>{comp}</option>
                      ))}
                    </select>
                  </div>
                  <div className="mb-3">
                    <label className="form-label">Round</label>
                    <input
                      type="number"
                      className="form-control"
                      value={editingMatch.round}
                      onChange={(e) => setEditingMatch({...editingMatch, round: parseInt(e.target.value) || 1})}
                      min="1"
                    />
                  </div>
                  <div className="mb-3 form-check">
                    <input
                      type="checkbox"
                      className="form-check-input"
                      id="isCompleted"
                      checked={editingMatch.isCompleted}
                      onChange={(e) => setEditingMatch({...editingMatch, isCompleted: e.target.checked})}
                    />
                    <label className="form-check-label" htmlFor="isCompleted">
                      Match Completed
                    </label>
                  </div>
                </div>
                <div className="modal-footer">
                  <button
                    type="button"
                    className="btn btn-secondary"
                    onClick={() => setEditingMatch(null)}
                  >
                    Cancel
                  </button>
                  <button type="submit" className="btn btn-primary" disabled={loading}>
                    {loading ? 'Updating...' : 'Update Match'}
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}

      {/* Enhanced Player Performances Modal */}
      {showPerformances && (
        <SimplePerformanceEntry
          match={matches.find(m => m.id === showPerformances)!}
          onError={onError}
          onSuccess={onSuccess}
          onClose={() => setShowPerformances(null)}
        />
      )}

      {/* Bulk Import Modal */}
      {showBulkModal && (
        <div className="modal show d-block" tabIndex={-1}>
          <div className="modal-dialog modal-lg">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">Bulk Import Matches</h5>
                <button
                  type="button"
                  className="btn-close"
                  onClick={() => setShowBulkModal(false)}
                ></button>
              </div>
              <div className="modal-body">
                <div className="alert alert-info">
                  <strong>Format:</strong> Each line should contain: Home Team, Away Team, Date (YYYY-MM-DD), Competition (optional), Round (optional)<br />
                  <strong>Example:</strong> Ajax, PSV, 2024-08-15, Eredivisie, 1<br />
                  <br />
                  <strong>Future Feature:</strong> Google Calendar integration will be available soon! You'll be able to import matches directly from a calendar link.
                </div>
                <textarea
                  className="form-control"
                  rows={15}
                  value={bulkMatchesText}
                  onChange={(e) => setBulkMatchesText(e.target.value)}
                  placeholder="Ajax, PSV, 2024-08-15, Eredivisie, 1&#10;Feyenoord, AZ, 2024-08-16, Eredivisie, 1&#10;..."
                />
              </div>
              <div className="modal-footer">
                <button
                  type="button"
                  className="btn btn-secondary"
                  onClick={() => setShowBulkModal(false)}
                >
                  Cancel
                </button>
                <button
                  type="button"
                  className="btn btn-success"
                  onClick={handleBulkImport}
                  disabled={loading || !bulkMatchesText.trim()}
                >
                  {loading ? 'Importing...' : 'Import Matches'}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
