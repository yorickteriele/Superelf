import React, { useState, useEffect } from 'react';
import {
  adminService,
  League,
  CreateLeagueRequest,
  UpdateLeagueRequest,
  GoogleCalendarSyncRequest
} from '../../services/adminService';

interface LeagueManagementProps {
  onError: (message: string) => void;
  onSuccess: (message: string) => void;
}

export const LeagueManagement: React.FC<LeagueManagementProps> = ({ onError, onSuccess }) => {
  const [leagues, setLeagues] = useState<League[]>([]);
  const [loading, setLoading] = useState(false);
  const [editingLeague, setEditingLeague] = useState<League | null>(null);
  const [showSyncModal, setShowSyncModal] = useState<League | null>(null);

  const [newLeague, setNewLeague] = useState<CreateLeagueRequest>({
    name: '',
    shortName: '',
    country: '',
    logoUrl: '',
    googleCalendarId: '',
    isActive: true
  });

  const [syncRequest, setSyncRequest] = useState<GoogleCalendarSyncRequest>({
    leagueId: '',
    googleCalendarId: '',
    forceSync: false,
    startDate: '2025-08-01', // Start of 25/26 season
    endDate: '2026-05-31'    // End of 25/26 season
  });

  useEffect(() => {
    loadLeagues();
  }, []);

  const loadLeagues = async () => {
    setLoading(true);
    try {
      const data = await adminService.getAllLeagues();
      setLeagues(data);
    } catch (err: any) {
      onError(err.message || 'Failed to load leagues');
    } finally {
      setLoading(false);
    }
  };

  const handleCreateLeague = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      await adminService.createLeague(newLeague);
      onSuccess('League created successfully');
      setNewLeague({
        name: '',
        shortName: '',
        country: '',
        logoUrl: '',
        googleCalendarId: '',
        isActive: true
      });
      await loadLeagues();
    } catch (err: any) {
      onError(err.message || 'Failed to create league');
    } finally {
      setLoading(false);
    }
  };

  const handleUpdateLeague = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!editingLeague) return;
    
    setLoading(true);
    try {
      await adminService.updateLeague(editingLeague.id, {
        name: editingLeague.name,
        shortName: editingLeague.shortName,
        country: editingLeague.country,
        logoUrl: editingLeague.logoUrl,
        googleCalendarId: editingLeague.googleCalendarId,
        isActive: editingLeague.isActive
      });
      onSuccess('League updated successfully');
      setEditingLeague(null);
      await loadLeagues();
    } catch (err: any) {
      onError(err.message || 'Failed to update league');
    } finally {
      setLoading(false);
    }
  };

  const handleDeleteLeague = async (leagueId: string) => {
    if (!window.confirm('Are you sure you want to delete this league? This will affect all associated clubs and matches.')) return;
    
    setLoading(true);
    try {
      await adminService.deleteLeague(leagueId);
      onSuccess('League deleted successfully');
      await loadLeagues();
    } catch (err: any) {
      onError(err.message || 'Failed to delete league');
    } finally {
      setLoading(false);
    }
  };

  const handleSyncCalendar = async () => {
    if (!showSyncModal) return;
    
    setLoading(true);
    try {
      await adminService.syncGoogleCalendar(syncRequest);
      onSuccess('Calendar sync completed successfully');
      setShowSyncModal(null);
      await loadLeagues();
    } catch (err: any) {
      onError(err.message || 'Failed to sync calendar');
    } finally {
      setLoading(false);
    }
  };

  const handleDebugCalendar = async () => {
    if (!showSyncModal) return;
    
    setLoading(true);
    try {
      const debugData = await adminService.debugCalendarData(syncRequest.leagueId, syncRequest);
      console.log('=== CALENDAR DEBUG DATA ===');
      console.log(JSON.stringify(debugData, null, 2));
      
      // Safe access to debug data properties
      const calendarSource = debugData?.CalendarData?.Source || 'Unknown';
      const eventCount = debugData?.CalendarData?.EventCount || 0;
      const existingClubs = debugData?.ExistingData?.ClubCount || 0;
      const existingMatches = debugData?.ExistingData?.MatchCount || 0;
      
      // Show debug data in an alert for easy viewing
      alert(`DEBUG DATA LOGGED TO CONSOLE\n\nCalendar Source: ${calendarSource}\nEvents Found: ${eventCount}\nExisting Clubs: ${existingClubs}\nExisting Matches: ${existingMatches}\n\nCheck browser console (F12) for full details!\n\nRaw response logged to console.`);
      
    } catch (err: any) {
      console.error('Debug error:', err);
      console.error('Full error object:', JSON.stringify(err, null, 2));
      onError(`Debug failed: ${err.message || 'Unknown error'}. Check console for details.`);
    } finally {
      setLoading(false);
    }
  };

  const handleTestCalendar = async (calendarId: string) => {
    if (!calendarId) {
      onError('Please enter a calendar ID first');
      return;
    }
    
    setLoading(true);
    try {
      const isAccessible = await adminService.testCalendarAccess(calendarId);
      if (isAccessible) {
        onSuccess('Calendar access test successful');
      } else {
        onError('Calendar access test failed - check the calendar ID and permissions');
      }
    } catch (err: any) {
      onError(err.message || 'Failed to test calendar access');
    } finally {
      setLoading(false);
    }
  };

  const openSyncModal = (league: League) => {
    setShowSyncModal(league);
    setSyncRequest({
      leagueId: league.id,
      googleCalendarId: league.googleCalendarId || '',
      forceSync: false
    });
  };

  return (
    <div className="container-fluid">
      <div className="row mb-4">
        <div className="col-md-12">
          <h4>League Management</h4>
          <p className="text-muted">Manage leagues and Google Calendar integration</p>
        </div>
      </div>

      <div className="row">
        {/* Add New League Form */}
        <div className="col-md-4">
          <div className="card">
            <div className="card-header">
              <h5>Add New League</h5>
            </div>
            <div className="card-body">
              <form onSubmit={handleCreateLeague}>
                <div className="mb-3">
                  <input
                    type="text"
                    className="form-control"
                    placeholder="League Name *"
                    value={newLeague.name}
                    onChange={(e) => setNewLeague({...newLeague, name: e.target.value})}
                    required
                  />
                </div>
                <div className="mb-3">
                  <input
                    type="text"
                    className="form-control"
                    placeholder="Short Name"
                    value={newLeague.shortName || ''}
                    onChange={(e) => setNewLeague({...newLeague, shortName: e.target.value})}
                  />
                </div>
                <div className="mb-3">
                  <input
                    type="text"
                    className="form-control"
                    placeholder="Country"
                    value={newLeague.country || ''}
                    onChange={(e) => setNewLeague({...newLeague, country: e.target.value})}
                  />
                </div>
                <div className="mb-3">
                  <input
                    type="url"
                    className="form-control"
                    placeholder="Logo URL"
                    value={newLeague.logoUrl || ''}
                    onChange={(e) => setNewLeague({...newLeague, logoUrl: e.target.value})}
                  />
                </div>
                <div className="mb-3">
                  <input
                    type="text"
                    className="form-control"
                    placeholder="Google Calendar ID"
                    value={newLeague.googleCalendarId || ''}
                    onChange={(e) => setNewLeague({...newLeague, googleCalendarId: e.target.value})}
                  />
                  <div className="form-text">
                    <small>Enter the Google Calendar ID to enable automatic match synchronization</small>
                  </div>
                </div>
                <div className="mb-3 form-check">
                  <input
                    type="checkbox"
                    className="form-check-input"
                    id="isActive"
                    checked={newLeague.isActive}
                    onChange={(e) => setNewLeague({...newLeague, isActive: e.target.checked})}
                  />
                  <label className="form-check-label" htmlFor="isActive">
                    Active League
                  </label>
                </div>
                <button type="submit" className="btn btn-primary w-100" disabled={loading}>
                  {loading ? 'Creating...' : 'Add League'}
                </button>
              </form>
            </div>
          </div>
        </div>

        {/* Leagues List */}
        <div className="col-md-8">
          <div className="card">
            <div className="card-header">
              <h5>Leagues ({leagues.length})</h5>
            </div>
            <div className="card-body">
              {loading && leagues.length === 0 ? (
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
                        <th>Name</th>
                        <th>Country</th>
                        <th>Clubs</th>
                        <th>Matches</th>
                        <th>Calendar</th>
                        <th>Status</th>
                        <th>Actions</th>
                      </tr>
                    </thead>
                    <tbody>
                      {leagues.map(league => (
                        <tr key={league.id}>
                          <td>
                            <div>
                              <strong>{league.name}</strong>
                              {league.shortName && <small className="text-muted d-block">{league.shortName}</small>}
                            </div>
                          </td>
                          <td>{league.country || 'N/A'}</td>
                          <td>
                            <span className="badge bg-info">{league.clubCount}</span>
                          </td>
                          <td>
                            <span className="badge bg-secondary">{league.matchCount}</span>
                          </td>
                          <td>
                            {league.googleCalendarId ? (
                              <div>
                                <span className="badge bg-success mb-1">Connected</span>
                                {league.lastSyncAt && (
                                  <small className="text-muted d-block">
                                    Last sync: {new Date(league.lastSyncAt).toLocaleDateString()}
                                  </small>
                                )}
                              </div>
                            ) : (
                              <span className="badge bg-warning">Not configured</span>
                            )}
                          </td>
                          <td>
                            <span className={`badge ${league.isActive ? 'bg-success' : 'bg-secondary'}`}>
                              {league.isActive ? 'Active' : 'Inactive'}
                            </span>
                          </td>
                          <td>
                            <div className="btn-group" role="group">
                              <button
                                className="btn btn-sm btn-outline-primary"
                                onClick={() => setEditingLeague(league)}
                              >
                                Edit
                              </button>
                              {league.googleCalendarId && (
                                <button
                                  className="btn btn-sm btn-outline-success"
                                  onClick={() => openSyncModal(league)}
                                >
                                  Sync
                                </button>
                              )}
                              <button
                                className="btn btn-sm btn-outline-danger"
                                onClick={() => handleDeleteLeague(league.id)}
                              >
                                Delete
                              </button>
                            </div>
                          </td>
                        </tr>
                      ))}
                      {leagues.length === 0 && !loading && (
                        <tr>
                          <td colSpan={7} className="text-center text-muted">
                            No leagues found. Create your first league above.
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

      {/* Edit League Modal */}
      {editingLeague && (
        <div className="modal show d-block" tabIndex={-1}>
          <div className="modal-dialog">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">Edit League</h5>
                <button
                  type="button"
                  className="btn-close"
                  onClick={() => setEditingLeague(null)}
                ></button>
              </div>
              <form onSubmit={handleUpdateLeague}>
                <div className="modal-body">
                  <div className="mb-3">
                    <label className="form-label">League Name</label>
                    <input
                      type="text"
                      className="form-control"
                      value={editingLeague.name}
                      onChange={(e) => setEditingLeague({...editingLeague, name: e.target.value})}
                      required
                    />
                  </div>
                  <div className="mb-3">
                    <label className="form-label">Short Name</label>
                    <input
                      type="text"
                      className="form-control"
                      value={editingLeague.shortName || ''}
                      onChange={(e) => setEditingLeague({...editingLeague, shortName: e.target.value})}
                    />
                  </div>
                  <div className="mb-3">
                    <label className="form-label">Country</label>
                    <input
                      type="text"
                      className="form-control"
                      value={editingLeague.country || ''}
                      onChange={(e) => setEditingLeague({...editingLeague, country: e.target.value})}
                    />
                  </div>
                  <div className="mb-3">
                    <label className="form-label">Logo URL</label>
                    <input
                      type="url"
                      className="form-control"
                      value={editingLeague.logoUrl || ''}
                      onChange={(e) => setEditingLeague({...editingLeague, logoUrl: e.target.value})}
                    />
                  </div>
                  <div className="mb-3">
                    <label className="form-label">Google Calendar ID</label>
                    <div className="input-group">
                      <input
                        type="text"
                        className="form-control"
                        value={editingLeague.googleCalendarId || ''}
                        onChange={(e) => setEditingLeague({...editingLeague, googleCalendarId: e.target.value})}
                      />
                      <button
                        type="button"
                        className="btn btn-outline-secondary"
                        onClick={() => handleTestCalendar(editingLeague.googleCalendarId || '')}
                        disabled={!editingLeague.googleCalendarId}
                      >
                        Test
                      </button>
                    </div>
                    <div className="form-text">
                      <small>Enter the Google Calendar ID to enable automatic match synchronization</small>
                    </div>
                  </div>
                  <div className="mb-3 form-check">
                    <input
                      type="checkbox"
                      className="form-check-input"
                      id="editIsActive"
                      checked={editingLeague.isActive}
                      onChange={(e) => setEditingLeague({...editingLeague, isActive: e.target.checked})}
                    />
                    <label className="form-check-label" htmlFor="editIsActive">
                      Active League
                    </label>
                  </div>
                </div>
                <div className="modal-footer">
                  <button
                    type="button"
                    className="btn btn-secondary"
                    onClick={() => setEditingLeague(null)}
                  >
                    Cancel
                  </button>
                  <button
                    type="submit"
                    className="btn btn-primary"
                    disabled={loading}
                  >
                    {loading ? 'Updating...' : 'Update League'}
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}

      {/* Google Calendar Sync Modal */}
      {showSyncModal && (
        <div className="modal show d-block" tabIndex={-1}>
          <div className="modal-dialog">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">Sync Google Calendar</h5>
                <button
                  type="button"
                  className="btn-close"
                  onClick={() => setShowSyncModal(null)}
                ></button>
              </div>
              <div className="modal-body">
                <div className="alert alert-info">
                  <strong>League:</strong> {showSyncModal.name}<br />
                  <strong>Calendar ID:</strong> {showSyncModal.googleCalendarId}
                </div>
                <div className="mb-3">
                  <label className="form-label">Google Calendar ID</label>
                  <input
                    type="text"
                    className="form-control"
                    value={syncRequest.googleCalendarId}
                    onChange={(e) => setSyncRequest({...syncRequest, googleCalendarId: e.target.value})}
                  />
                </div>
                <div className="row mb-3">
                  <div className="col-md-6">
                    <label className="form-label">Start Date</label>
                    <input
                      type="date"
                      className="form-control"
                      value={syncRequest.startDate}
                      onChange={(e) => setSyncRequest({...syncRequest, startDate: e.target.value})}
                    />
                    <small className="text-muted">Sync events from this date onwards</small>
                  </div>
                  <div className="col-md-6">
                    <label className="form-label">End Date</label>
                    <input
                      type="date"
                      className="form-control"
                      value={syncRequest.endDate}
                      onChange={(e) => setSyncRequest({...syncRequest, endDate: e.target.value})}
                    />
                    <small className="text-muted">Sync events up to this date</small>
                  </div>
                </div>
                <div className="mb-3 form-check">
                  <input
                    type="checkbox"
                    className="form-check-input"
                    id="forceSync"
                    checked={syncRequest.forceSync}
                    onChange={(e) => setSyncRequest({...syncRequest, forceSync: e.target.checked})}
                  />
                  <label className="form-check-label" htmlFor="forceSync">
                    Force full sync (re-import all events)
                  </label>
                </div>
                <div className="alert alert-warning">
                  <small>
                    <strong>Note:</strong> This will synchronize matches from your Google Calendar within the specified date range. 
                    Make sure the calendar contains football match events in the format "Team A vs Team B" or "Team A - Team B".
                    Historical data (like from August 1, 2025) can be synced by adjusting the start date.
                  </small>
                </div>
              </div>
              <div className="modal-footer">
                <button
                  type="button"
                  className="btn btn-secondary"
                  onClick={() => setShowSyncModal(null)}
                >
                  Cancel
                </button>
                <button
                  type="button"
                  className="btn btn-info me-2"
                  onClick={handleDebugCalendar}
                  disabled={loading || !syncRequest.googleCalendarId}
                >
                  {loading ? 'Debugging...' : 'Debug Calendar Data'}
                </button>
                <button
                  type="button"
                  className="btn btn-primary"
                  onClick={handleSyncCalendar}
                  disabled={loading || !syncRequest.googleCalendarId}
                >
                  {loading ? 'Syncing...' : 'Start Sync'}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
