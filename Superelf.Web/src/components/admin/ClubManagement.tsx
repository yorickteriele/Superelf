import React, { useState, useEffect } from 'react';
import { adminService, Club, CreateClubRequest, UpdateClubRequest } from '../../services/adminService';

interface ClubManagementProps {
  onError: (message: string) => void;
  onSuccess: (message: string) => void;
}

export const ClubManagement: React.FC<ClubManagementProps> = ({ onError, onSuccess }) => {
  const [clubs, setClubs] = useState<Club[]>([]);
  const [loading, setLoading] = useState(false);
  const [editingClub, setEditingClub] = useState<Club | null>(null);

  const [newClub, setNewClub] = useState<CreateClubRequest>({
    name: '',
    shortName: '',
    logoUrl: '',
    country: ''
  });

  useEffect(() => {
    loadClubs();
  }, []);

  const loadClubs = async () => {
    setLoading(true);
    try {
      const data = await adminService.getAllClubs();
      setClubs(data);
    } catch (err: any) {
      onError(err.message || 'Failed to load clubs');
    } finally {
      setLoading(false);
    }
  };

  const handleCreateClub = async (e: React.FormEvent) => {
    e.preventDefault();
    setLoading(true);
    try {
      await adminService.createClub(newClub);
      onSuccess('Club created successfully');
      setNewClub({ name: '', shortName: '', logoUrl: '', country: '' });
      await loadClubs();
    } catch (err: any) {
      onError(err.message || 'Failed to create club');
    } finally {
      setLoading(false);
    }
  };

  const handleUpdateClub = async (e: React.FormEvent) => {
    e.preventDefault();
    if (!editingClub) return;

    setLoading(true);
    try {
      await adminService.updateClub(editingClub.id, {
        name: editingClub.name,
        shortName: editingClub.shortName,
        logoUrl: editingClub.logoUrl,
        country: editingClub.country
      });
      onSuccess('Club updated successfully');
      setEditingClub(null);
      await loadClubs();
    } catch (err: any) {
      onError(err.message || 'Failed to update club');
    } finally {
      setLoading(false);
    }
  };

  const handleDeleteClub = async (clubId: string) => {
    if (!window.confirm('Are you sure you want to delete this club?')) return;

    setLoading(true);
    try {
      await adminService.deleteClub(clubId);
      onSuccess('Club deleted successfully');
      await loadClubs();
    } catch (err: any) {
      onError(err.message || 'Failed to delete club');
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="container-fluid">
      <div className="row mb-4">
        <div className="col-md-12">
          <h4>Club Management</h4>
        </div>
      </div>

      <div className="row">
        {/* Add New Club Form */}
        <div className="col-md-4">
          <div className="card">
            <div className="card-header">
              <h5>Add New Club</h5>
            </div>
            <div className="card-body">
              <form onSubmit={handleCreateClub}>
                <div className="mb-3">
                  <label className="form-label">Club Name *</label>
                  <input
                    type="text"
                    className="form-control"
                    value={newClub.name}
                    onChange={(e) => setNewClub({...newClub, name: e.target.value})}
                    required
                  />
                </div>
                <div className="mb-3">
                  <label className="form-label">Short Name</label>
                  <input
                    type="text"
                    className="form-control"
                    placeholder="e.g., BAR, RM, LIV"
                    value={newClub.shortName || ''}
                    onChange={(e) => setNewClub({...newClub, shortName: e.target.value})}
                    maxLength={5}
                  />
                </div>
                <div className="mb-3">
                  <label className="form-label">Logo URL</label>
                  <input
                    type="url"
                    className="form-control"
                    placeholder="https://example.com/logo.png"
                    value={newClub.logoUrl || ''}
                    onChange={(e) => setNewClub({...newClub, logoUrl: e.target.value})}
                  />
                </div>
                <div className="mb-3">
                  <label className="form-label">Country</label>
                  <input
                    type="text"
                    className="form-control"
                    placeholder="e.g., Spain, England, Germany"
                    value={newClub.country || ''}
                    onChange={(e) => setNewClub({...newClub, country: e.target.value})}
                  />
                </div>
                <button type="submit" className="btn btn-primary w-100" disabled={loading}>
                  {loading ? 'Creating...' : 'Add Club'}
                </button>
              </form>
            </div>
          </div>
        </div>

        {/* Clubs List */}
        <div className="col-md-8">
          <div className="card">
            <div className="card-header">
              <h5>Clubs ({clubs.length})</h5>
            </div>
            <div className="card-body">
              {loading && clubs.length === 0 ? (
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
                        <th>Logo</th>
                        <th>Name</th>
                        <th>Short Name</th>
                        <th>Country</th>
                        <th>Players</th>
                        <th>Created</th>
                        <th>Actions</th>
                      </tr>
                    </thead>
                    <tbody>
                      {clubs.map(club => (
                        <tr key={club.id}>
                          <td>
                            {club.logoUrl ? (
                              <img 
                                src={club.logoUrl} 
                                alt={club.name}
                                style={{ width: '30px', height: '30px', objectFit: 'contain' }}
                                onError={(e) => {
                                  (e.target as HTMLImageElement).style.display = 'none';
                                }}
                              />
                            ) : (
                              <div className="bg-secondary rounded d-flex align-items-center justify-content-center" 
                                   style={{ width: '30px', height: '30px', fontSize: '12px' }}>
                                {club.shortName || club.name.charAt(0)}
                              </div>
                            )}
                          </td>
                          <td>{club.name}</td>
                          <td>{club.shortName || 'N/A'}</td>
                          <td>{club.country || 'N/A'}</td>
                          <td>
                            <span className="badge bg-info">{club.playerCount}</span>
                          </td>
                          <td>{new Date(club.createdAt).toLocaleDateString()}</td>
                          <td>
                            <div className="btn-group" role="group">
                              <button
                                className="btn btn-sm btn-outline-primary"
                                onClick={() => setEditingClub(club)}
                              >
                                Edit
                              </button>
                              <button
                                className="btn btn-sm btn-outline-danger"
                                onClick={() => handleDeleteClub(club.id)}
                                disabled={club.playerCount > 0}
                                title={club.playerCount > 0 ? 'Cannot delete club with players' : 'Delete club'}
                              >
                                Delete
                              </button>
                            </div>
                          </td>
                        </tr>
                      ))}
                      {clubs.length === 0 && !loading && (
                        <tr>
                          <td colSpan={7} className="text-center text-muted">
                            No clubs found. Add your first club using the form on the left.
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

      {/* Edit Club Modal */}
      {editingClub && (
        <div className="modal show d-block" tabIndex={-1}>
          <div className="modal-dialog">
            <div className="modal-content">
              <div className="modal-header">
                <h5 className="modal-title">Edit Club</h5>
                <button
                  type="button"
                  className="btn-close"
                  onClick={() => setEditingClub(null)}
                ></button>
              </div>
              <form onSubmit={handleUpdateClub}>
                <div className="modal-body">
                  <div className="mb-3">
                    <label className="form-label">Club Name *</label>
                    <input
                      type="text"
                      className="form-control"
                      value={editingClub.name}
                      onChange={(e) => setEditingClub({...editingClub, name: e.target.value})}
                      required
                    />
                  </div>
                  <div className="mb-3">
                    <label className="form-label">Short Name</label>
                    <input
                      type="text"
                      className="form-control"
                      value={editingClub.shortName || ''}
                      onChange={(e) => setEditingClub({...editingClub, shortName: e.target.value})}
                      maxLength={5}
                    />
                  </div>
                  <div className="mb-3">
                    <label className="form-label">Logo URL</label>
                    <input
                      type="url"
                      className="form-control"
                      value={editingClub.logoUrl || ''}
                      onChange={(e) => setEditingClub({...editingClub, logoUrl: e.target.value})}
                    />
                  </div>
                  <div className="mb-3">
                    <label className="form-label">Country</label>
                    <input
                      type="text"
                      className="form-control"
                      value={editingClub.country || ''}
                      onChange={(e) => setEditingClub({...editingClub, country: e.target.value})}
                    />
                  </div>
                  {editingClub.logoUrl && (
                    <div className="mb-3">
                      <label className="form-label">Logo Preview</label>
                      <div>
                        <img 
                          src={editingClub.logoUrl} 
                          alt="Logo preview"
                          style={{ maxWidth: '100px', maxHeight: '100px', objectFit: 'contain' }}
                          onError={(e) => {
                            (e.target as HTMLImageElement).style.display = 'none';
                          }}
                        />
                      </div>
                    </div>
                  )}
                </div>
                <div className="modal-footer">
                  <button
                    type="button"
                    className="btn btn-secondary"
                    onClick={() => setEditingClub(null)}
                  >
                    Cancel
                  </button>
                  <button type="submit" className="btn btn-primary" disabled={loading}>
                    {loading ? 'Updating...' : 'Update Club'}
                  </button>
                </div>
              </form>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};
