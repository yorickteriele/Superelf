import React, { useState, useEffect } from 'react';
import {
  adminService,
  FootballPlayer,
  CreatePlayerRequest,
  UpdatePlayerRequest,
  PagedResult,
  PlayerFilter,
  BulkCreatePlayersRequest,
  BulkCreateResult,
  Club
} from '../../services/adminService';

interface PlayerManagementProps {
  onError: (message: string) => void;
  onSuccess: (message: string) => void;
}

export const PlayerManagement: React.FC<PlayerManagementProps> = ({ onError, onSuccess }) => {
  const [playersData, setPlayersData] = useState<PagedResult<FootballPlayer>>({
    items: [],
    totalCount: 0,
    page: 1,
    pageSize: 20,
    totalPages: 0,
    hasNextPage: false,
    hasPreviousPage: false
  });
  
  const [clubs, setClubs] = useState<Club[]>([]);
  const [loading, setLoading] = useState(false);
  const [filter, setFilter] = useState<PlayerFilter>({
    page: 1,
    pageSize: 20,
    sortBy: 'name',
    sortDirection: 'asc'
  });

  // Forms
  const [newPlayer, setNewPlayer] = useState<CreatePlayerRequest>({
    name: '',
    position: 'Midfielder',
    nationality: '',
    club: '',
    clubId: undefined,
    photoUrl: ''
  });

  const [editingPlayer, setEditingPlayer] = useState<FootballPlayer | null>(null);
  const [showBulkModal, setShowBulkModal] = useState(false);
  const [bulkPlayersText, setBulkPlayersText] = useState('');
  const [selectedPlayers, setSelectedPlayers] = useState<string[]>([]);

  // Confirmation dialog state
  const [confirmOpen, setConfirmOpen] = useState(false);
  const [confirmMessage, setConfirmMessage] = useState('');
  const [onConfirm, setOnConfirm] = useState<(() => void) | null>(null);

  useEffect(() => {
    loadData();
  }, [filter]);

  const loadData = async () => {
    setLoading(true);
    try {
      const [playersResponse, clubsResponse] = await Promise.all([
        adminService.getAllPlayers(filter),
        adminService.getAllClubs()
      ]);
      setPlayersData(playersResponse);
      setClubs(clubsResponse);
    } catch (err: any) {
      onError(err.message || 'Failed to load data');
    } finally {
      setLoading(false);
    }
  };

  const handleCreatePlayer = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      await adminService.createPlayer(newPlayer);
      onSuccess('Player created successfully');
      setNewPlayer({
        name: '',
        position: 'Midfielder',
        nationality: '',
        club: '',
        clubId: undefined,
        photoUrl: ''
      });
      await loadData();
    } catch (err: any) {
      onError(err.message || 'Failed to create player');
    } finally {
      setLoading(false);
    }
  };

  const handleUpdatePlayer = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!editingPlayer) return;
    
    setLoading(true);
    try {
      await adminService.updatePlayer(editingPlayer.id, {
        name: editingPlayer.name,
        position: editingPlayer.position,
        nationality: editingPlayer.nationality,
        club: editingPlayer.club,
        clubId: editingPlayer.clubId,
        photoUrl: editingPlayer.photoUrl
      });
      onSuccess('Player updated successfully');
      setEditingPlayer(null);
      await loadData();
    } catch (err: any) {
      onError(err.message || 'Failed to update player');
    } finally {
      setLoading(false);
    }
  };

  const handleDeletePlayer = (playerId: string) => {
    setConfirmMessage('Are you sure you want to delete this player? This action cannot be undone.');
    setOnConfirm(() => async () => {
      setLoading(true);
      try {
        await adminService.deletePlayer(playerId);
        onSuccess('Player deleted successfully');
        await loadData();
      } catch (err: any) {
        onError(err.message || 'Failed to delete player');
      } finally {
        setLoading(false);
      }
      setConfirmOpen(false);
    });
    setConfirmOpen(true);
  };
  // Simple confirmation dialog component
  const ConfirmDialog: React.FC<{ open: boolean; message: string; onConfirm: () => void; onCancel: () => void }> = ({ open, message, onConfirm, onCancel }) => {
    if (!open) return null;
    return (
      <div className="modal show" style={{ display: 'block', background: 'rgba(0,0,0,0.5)' }} tabIndex={-1}>
        <div className="modal-dialog">
          <div className="modal-content">
            <div className="modal-header">
              <h5 className="modal-title">Confirm Action</h5>
              <button type="button" className="btn-close" onClick={onCancel}></button>
            </div>
            <div className="modal-body">
              <p>{message}</p>
            </div>
            <div className="modal-footer">
              <button type="button" className="btn btn-secondary" onClick={onCancel}>Cancel</button>
              <button type="button" className="btn btn-danger" onClick={onConfirm}>Confirm</button>
            </div>
          </div>
        </div>
      </div>
    );
  };

  const handleBulkCreate = async () => {
    if (!bulkPlayersText.trim()) {
      onError('Please enter player data');
      return;
    }

    setLoading(true);
    try {
      const players: CreatePlayerRequest[] = [];
      const lines = bulkPlayersText.trim().split('\n');
      
      for (const line of lines) {
        const parts = line.split(',').map(p => p.trim());
        if (parts.length >= 3) {
          const clubName = parts[3] || '';
          const club = clubs.find(c => c.name.toLowerCase() === clubName.toLowerCase());
          
          players.push({
            name: parts[0],
            position: parts[1],
            nationality: parts[2],
            club: clubName,
            clubId: club?.id,
            photoUrl: parts[4] || ''
          });
        }
      }

      const result = await adminService.bulkCreatePlayers({
        players,
        skipDuplicates: true
      });

      onSuccess(`Bulk create completed: ${result.created} created, ${result.skipped} skipped, ${result.failed} failed`);
      setBulkPlayersText('');
      setShowBulkModal(false);
      await loadData();
    } catch (err: any) {
      onError(err.message || 'Failed to bulk create players');
    } finally {
      setLoading(false);
    }
  };

  const handleFilterChange = (field: keyof PlayerFilter, value: any) => {
    setFilter(prev => ({ ...prev, [field]: value, page: 1 }));
  };

  const handlePageChange = (newPage: number) => {
    setFilter(prev => ({ ...prev, page: newPage }));
  };

  const positions = ['Goalkeeper', 'Defender', 'Midfielder', 'Forward'];
  
  return (
    <div className="container-fluid">
      {/* Confirmation Dialog */}
      <ConfirmDialog
        open={confirmOpen}
        message={confirmMessage}
        onConfirm={() => onConfirm && onConfirm()}
        onCancel={() => setConfirmOpen(false)}
      />
      <div className="row mb-4">
        <div className="col-md-12">
          <div className="d-flex justify-content-between align-items-center">
            <h4>Player Management</h4>
            <div>
              <button 
                className="btn btn-success me-2" 
                onClick={() => setShowBulkModal(true)}
              >
                Bulk Import
              </button>
            </div>
          </div>
        </div>
      </div>

      {/* Filters */}
      <div className="row mb-4">
        <div className="col-md-12">
          <div className="card">
            <div className="card-body">
              <div className="row g-3">
                <div className="col-md-3">
                  <input
                    type="text"
                    className="form-control"
                    placeholder="Search players..."
                    value={filter.search || ''}
                    onChange={(e) => handleFilterChange('search', e.target.value)}
                  />
                </div>
                <div className="col-md-2">
                  <select
                    className="form-select"
                    value={filter.position || ''}
                    onChange={(e) => handleFilterChange('position', e.target.value)}
                  >
                    <option value="">All Positions</option>
                    {positions.map(pos => (
                      <option key={pos} value={pos}>{pos}</option>
                    ))}
                  </select>
                </div>
                <div className="col-md-2">
                  <select
                    className="form-select"
                    value={filter.clubId || ''}
                    onChange={(e) => handleFilterChange('clubId', e.target.value)}
                  >
                    <option value="">All Clubs</option>
                    {clubs.map(club => (
                      <option key={club.id} value={club.id}>{club.name}</option>
                    ))}
                  </select>
                </div>
                <div className="col-md-2">
                  <select
                    className="form-select"
                    value={filter.sortBy || 'name'}
                    onChange={(e) => handleFilterChange('sortBy', e.target.value)}
                  >
                    <option value="name">Sort by Name</option>
                    <option value="position">Sort by Position</option>
                    <option value="nationality">Sort by Nationality</option>
                    <option value="club">Sort by Club</option>
                    <option value="createdAt">Sort by Date</option>
                  </select>
                </div>
                <div className="col-md-1">
                  <select
                    className="form-select"
                    value={filter.sortDirection || 'asc'}
                    onChange={(e) => handleFilterChange('sortDirection', e.target.value)}
                  >
                    <option value="asc">↑</option>
                    <option value="desc">↓</option>
                  </select>
                </div>
                <div className="col-md-2">
                  <select
                    className="form-select"
                    value={filter.pageSize || 20}
                    onChange={(e) => handleFilterChange('pageSize', parseInt(e.target.value))}
                  >
                    <option value={10}>10 per page</option>
                    <option value={20}>20 per page</option>
                    <option value={50}>50 per page</option>
                    <option value={100}>100 per page</option>
                  </select>
                </div>
              </div>
            </div>
          </div>
        </div>
      </div>

      <div className="row">
        {/* Add New Player Form */}
        <div className="col-md-4">
          <div className="card">
            <div className="card-header">
              <h5>Add New Player</h5>
            </div>
            <div className="card-body">
              <form onSubmit={handleCreatePlayer}>
                <div className="mb-3">
                  <input
                    type="text"
                    className="form-control"
                    placeholder="Player Name"
                    value={newPlayer.name}
                    onChange={(e) => setNewPlayer({...newPlayer, name: e.target.value})}
                    required
                  />
                </div>
                <div className="mb-3">
                  <select
                    className="form-select"
                    value={newPlayer.position}
                    onChange={(e) => setNewPlayer({...newPlayer, position: e.target.value})}
                    required
                  >
                    {positions.map(pos => (
                      <option key={pos} value={pos}>{pos}</option>
                    ))}
                  </select>
                </div>
                <div className="mb-3">
                  <input
                    type="text"
                    className="form-control"
                    placeholder="Nationality"
                    value={newPlayer.nationality}
                    onChange={(e) => setNewPlayer({...newPlayer, nationality: e.target.value})}
                    required
                  />
                </div>
                <div className="mb-3">
                  <select
                    className="form-select"
                    value={newPlayer.clubId || ''}
                    onChange={(e) => {
                      const clubId = e.target.value;
                      const club = clubs.find(c => c.id === clubId);
                      setNewPlayer({
                        ...newPlayer, 
                        clubId: clubId || undefined,
                        club: club?.name || ''
                      });
                    }}
                  >
                    <option value="">Select Club</option>
                    {clubs.map(club => (
                      <option key={club.id} value={club.id}>{club.name}</option>
                    ))}
                  </select>
                </div>
                <div className="mb-3">
                  <input
                    type="url"
                    className="form-control"
                    placeholder="Photo URL (optional)"
                    value={newPlayer.photoUrl || ''}
                    onChange={(e) => setNewPlayer({...newPlayer, photoUrl: e.target.value})}
                  />
                </div>
                <button type="submit" className="btn btn-primary w-100" disabled={loading}>
                  {loading ? 'Creating...' : 'Add Player'}
                </button>
              </form>
            </div>
          </div>
        </div>

        {/* Players List */}
        <div className="col-md-8">
          <div className="card">
            <div className="card-header d-flex justify-content-between align-items-center">
              <h5>Players ({playersData.totalCount})</h5>
              <div>
                Page {playersData.page} of {playersData.totalPages}
              </div>
            </div>
            <div className="card-body">
              {loading ? (
                <div className="text-center">
                  <div className="spinner-border" role="status">
                    <span className="visually-hidden">Loading...</span>
                  </div>
                </div>
              ) : (
                <>
                  <div className="table-responsive">
                    <table className="table table-hover">
                      <thead>
                        <tr>
                          <th>Name</th>
                          <th>Position</th>
                          <th>Nationality</th>
                          <th>Club</th>
                          <th>Photo</th>
                          <th>Actions</th>
                        </tr>
                      </thead>
                      <tbody>
                        {playersData.items.map(player => (
                          <tr key={player.id}>
                            <td>{player.name}</td>
                            <td>{player.position}</td>
                            <td>{player.nationality}</td>
                            <td>{player.club || 'N/A'}</td>
                            <td>
                              {player.photoUrl ? (
                                <img 
                                  src={player.photoUrl} 
                                  alt={player.name}
                                  style={{ width: '40px', height: '40px', objectFit: 'cover', borderRadius: '50%' }}
                                  onError={(e) => { e.currentTarget.style.display = 'none'; }}
                                />
                              ) : (
                                <span className="text-muted">No photo</span>
                              )}
                            </td>
                            <td>
                              <div className="btn-group" role="group">
                                <button
                                  className="btn btn-sm btn-outline-primary"
                                  onClick={() => setEditingPlayer(player)}
                                >
                                  Edit
                                </button>
                                <button
                                  className="btn btn-sm btn-outline-danger"
                                  onClick={() => handleDeletePlayer(player.id)}
                                >
                                  Delete
                                </button>
                              </div>
                            </td>
                          </tr>
                        ))}
                      </tbody>
                    </table>
                  </div>

                  {/* Pagination */}
                  <nav>
                    <ul className="pagination justify-content-center">
                      <li className={`page-item ${!playersData.hasPreviousPage ? 'disabled' : ''}`}>
                        <button
                          className="page-link"
                          onClick={() => handlePageChange(playersData.page - 1)}
                          disabled={!playersData.hasPreviousPage}
                        >
                          Previous
                        </button>
                      </li>
                      {[...Array(Math.min(5, playersData.totalPages))].map((_, i) => {
                        const pageNum = playersData.page - 2 + i;
                        if (pageNum > 0 && pageNum <= playersData.totalPages) {
                          return (
                            <li key={pageNum} className={`page-item ${pageNum === playersData.page ? 'active' : ''}`}>
                              <button
                                className="page-link"
                                onClick={() => handlePageChange(pageNum)}
                              >
                                {pageNum}
                              </button>
                            </li>
                          );
                        }
                        return null;
                      })}
                      <li className={`page-item ${!playersData.hasNextPage ? 'disabled' : ''}`}>
                        <button
                          className="page-link"
                          onClick={() => handlePageChange(playersData.page + 1)}
                          disabled={!playersData.hasNextPage}
                        >
                          Next
                        </button>
                      </li>
                    </ul>
                  </nav>
                </>
              )}
            </div>
          </div>
        </div>
      </div>

      {/* Edit Player Modal */}
      {editingPlayer && (
        <div className="modal show d-block" tabIndex={-1}>
          <div className="modal-dialog">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">Edit Player</h5>
                <button
                  type="button"
                  className="btn-close"
                  onClick={() => setEditingPlayer(null)}
                ></button>
              </div>
              <form onSubmit={handleUpdatePlayer}>
                <div className="modal-body">
                  <div className="mb-3">
                    <label className="form-label">Name</label>
                    <input
                      type="text"
                      className="form-control"
                      value={editingPlayer.name}
                      onChange={(e) => setEditingPlayer({...editingPlayer, name: e.target.value})}
                      required
                    />
                  </div>
                  <div className="mb-3">
                    <label className="form-label">Position</label>
                    <select
                      className="form-select"
                      value={editingPlayer.position}
                      onChange={(e) => setEditingPlayer({...editingPlayer, position: e.target.value})}
                      required
                    >
                      {positions.map(pos => (
                        <option key={pos} value={pos}>{pos}</option>
                      ))}
                    </select>
                  </div>
                  <div className="mb-3">
                    <label className="form-label">Nationality</label>
                    <input
                      type="text"
                      className="form-control"
                      value={editingPlayer.nationality}
                      onChange={(e) => setEditingPlayer({...editingPlayer, nationality: e.target.value})}
                      required
                    />
                  </div>
                  <div className="mb-3">
                    <label className="form-label">Club</label>
                    <select
                      className="form-select"
                      value={editingPlayer.clubId || ''}
                      onChange={(e) => {
                        const clubId = e.target.value;
                        const club = clubs.find(c => c.id === clubId);
                        setEditingPlayer({
                          ...editingPlayer,
                          clubId: clubId || undefined,
                          club: club?.name || ''
                        });
                      }}
                    >
                      <option value="">No Club</option>
                      {clubs.map(club => (
                        <option key={club.id} value={club.id}>{club.name}</option>
                      ))}
                    </select>
                  </div>
                  <div className="mb-3">
                    <label className="form-label">Photo URL</label>
                    <input
                      type="url"
                      className="form-control"
                      value={editingPlayer.photoUrl || ''}
                      onChange={(e) => setEditingPlayer({...editingPlayer, photoUrl: e.target.value})}
                      placeholder="Enter photo URL (optional)"
                    />
                  </div>
                </div>
                <div className="modal-footer">
                  <button
                    type="button"
                    className="btn btn-secondary"
                    onClick={() => setEditingPlayer(null)}
                  >
                    Cancel
                  </button>
                  <button type="submit" className="btn btn-primary" disabled={loading}>
                    {loading ? 'Updating...' : 'Update Player'}
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}

      {/* Bulk Import Modal */}
      {showBulkModal && (
        <div className="modal show d-block" tabIndex={-1}>
          <div className="modal-dialog modal-lg">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">Bulk Import Players</h5>
                <button
                  type="button"
                  className="btn-close"
                  onClick={() => setShowBulkModal(false)}
                ></button>
              </div>
              <div className="modal-body">
                <div className="alert alert-info">
                  <strong>Format:</strong> Each line should contain: Name, Position, Nationality, Club, Photo URL (optional)<br />
                  <strong>Example:</strong> Lionel Messi, Forward, Argentina, Barcelona, https://example.com/messi.jpg
                </div>
                <textarea
                  className="form-control"
                  rows={15}
                  placeholder="Enter player data, one per line..."
                  value={bulkPlayersText}
                  onChange={(e) => setBulkPlayersText(e.target.value)}
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
                  className="btn btn-primary"
                  onClick={handleBulkCreate}
                  disabled={loading || !bulkPlayersText.trim()}
                >
                  {loading ? 'Importing...' : 'Import Players'}
                </button>
              </div>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
