import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Pool, poolService } from '../services/poolService';
import { useAuth } from '../context/AuthContext';

const PoolDetail: React.FC = () => {
  const { poolId } = useParams<{ poolId: string }>();
  const [pool, setPool] = useState<Pool | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [newName, setNewName] = useState<string>('');
  const [isEditing, setIsEditing] = useState<boolean>(false);
  const [success, setSuccess] = useState<string | null>(null);
  const { user } = useAuth();
  const navigate = useNavigate();

  useEffect(() => {
    const getPoolDetails = async () => {
      if (!poolId) return;
      
      setLoading(true);
      try {
        const poolData = await poolService.getPool(poolId);
        if (poolData) {
          console.log('Pool data received:', poolData);
          setPool(poolData);
        }
      } catch (err) {
        console.error('Error fetching pool details:', err);
        setError('Failed to load pool details');
      } finally {
        setLoading(false);
      }
    };
    
    if (poolId) {
      getPoolDetails();
    }
    // eslint-disable-next-line react-hooks/exhaustive-deps
  }, [poolId]);

  // eslint-disable-next-line @typescript-eslint/no-unused-vars
  const fetchPoolDetails = async () => {
    if (!poolId) return;
    
    setLoading(true);
    try {
      const poolData = await poolService.getPool(poolId);
      if (poolData) {
        console.log('Pool data refreshed:', poolData);
        console.log('Participants:', poolData.participants);
        setPool(poolData);
        setNewName(poolData.name);
      } else {
        setError('Failed to fetch pool details');
      }
    } catch (error) {
      console.error('Error fetching pool details:', error);
      setError('An error occurred while fetching pool details');
    } finally {
      setLoading(false);
    }
  };

  const handleEditName = async (e: React.FormEvent) => {
    e.preventDefault();
    
    if (!poolId || !newName.trim()) {
      return;
    }
    
    try {
      const result = await poolService.editPoolName(poolId, { newName: newName.trim() });
      if (result.success) {
        setPool(prev => prev ? { ...prev, name: newName.trim() } : null);
        setIsEditing(false);
        setSuccess('Pool name updated successfully');
        setTimeout(() => setSuccess(null), 3000);
      } else {
        setError(result.message || 'Failed to update pool name');
      }
    } catch (error) {
      setError('An error occurred while updating pool name');
    }
  };

  const handleRemoveParticipant = async (userId: string) => {
    if (!poolId || !pool) return;
    
    // Don't allow removing yourself
    if (userId === user?.id) {
      setError("You cannot remove yourself from the pool");
      return;
    }
    
    try {
      const result = await poolService.removeParticipant(poolId, userId);
      if (result.success) {
        // Update participants list
        setPool(prev => {
          if (!prev) return null;
          return {
            ...prev,
            participants: prev.participants.filter(p => p.userId !== userId)
          };
        });
        setSuccess('Participant removed successfully');
        setTimeout(() => setSuccess(null), 3000);
      } else {
        setError(result.message || 'Failed to remove participant');
      }
    } catch (error) {
      setError('An error occurred while removing participant');
    }
  };

  const isOwner = pool?.ownerName === user?.username;

  console.log('PoolDetail Debug:', {
    poolOwnerName: pool?.ownerName,
    currentUsername: user?.username,
    isOwner: isOwner
  });

  if (loading) {
    return (
      <div className="container py-5 text-center">
        <div className="spinner-border text-primary" role="status">
          <span className="visually-hidden">Loading...</span>
        </div>
      </div>
    );
  }

  if (!pool) {
    return (
      <div className="container py-5">
        <div className="alert alert-danger">
          {error || "Pool not found"}
        </div>
        <button 
          className="btn btn-primary"
          onClick={() => navigate('/pools')}
        >
          Back to Pools
        </button>
      </div>
    );
  }

  return (
    <div className="container py-5">
      <div className="d-flex justify-content-between align-items-center mb-4">
        <button 
          className="btn btn-outline-secondary"
          onClick={() => navigate('/pools')}
        >
          <i className="bi bi-arrow-left me-2"></i>
          Back to Pools
        </button>
        
        {isOwner && !isEditing && (
          <button 
            className="btn btn-outline-primary"
            onClick={() => setIsEditing(true)}
          >
            <i className="bi bi-pencil me-2"></i>
            Edit Name
          </button>
        )}
      </div>
      
      {/* Alert Messages */}
      {error && (
        <div className="alert alert-danger alert-dismissible fade show" role="alert">
          {error}
          <button 
            type="button" 
            className="btn-close" 
            onClick={() => setError(null)}
            aria-label="Close"
          ></button>
        </div>
      )}
      
      {success && (
        <div className="alert alert-success alert-dismissible fade show" role="alert">
          {success}
          <button 
            type="button" 
            className="btn-close" 
            onClick={() => setSuccess(null)}
            aria-label="Close"
          ></button>
        </div>
      )}
      
      <div className="card shadow mb-4">
        <div className="card-body">
          {isEditing ? (
            <form onSubmit={handleEditName} className="mb-3">
              <div className="input-group">
                <input 
                  type="text" 
                  className="form-control"
                  value={newName}
                  onChange={(e) => setNewName(e.target.value)}
                />
                <button 
                  type="submit" 
                  className="btn btn-primary"
                  disabled={!newName.trim() || newName === pool.name}
                >
                  Save
                </button>
                <button 
                  type="button" 
                  className="btn btn-outline-secondary"
                  onClick={() => {
                    setNewName(pool.name);
                    setIsEditing(false);
                  }}
                >
                  Cancel
                </button>
              </div>
            </form>
          ) : (
            <h2 className="card-title">{pool.name}</h2>
          )}
          
          <div className="d-flex gap-3 mb-2">
            <div>
              <span className="text-muted">Pool Code:</span>
              <div className="input-group">
                <input 
                  type="text" 
                  className="form-control"
                  value={pool.code}
                  readOnly
                />
                <button 
                  className="btn btn-outline-secondary" 
                  type="button"
                  onClick={() => {
                    navigator.clipboard.writeText(pool.code);
                    setSuccess('Pool code copied to clipboard!');
                    setTimeout(() => setSuccess(null), 3000);
                  }}
                >
                  Copy
                </button>
              </div>
            </div>
          </div>
          
          <div className="mb-3">
            <small className="text-muted">
              Created by: {pool.ownerName}<br />
              Created on: {new Date(pool.createTime).toLocaleDateString()}
            </small>
          </div>
          
          {/* Selection Actions */}
          <div className="d-flex gap-2">
            <button 
              className="btn btn-primary"
              onClick={() => navigate(`/pools/${poolId}/selection`)}
            >
              <i className="bi bi-person-plus me-2"></i>
              My Team Selection
            </button>
          </div>
        </div>
      </div>
      
      <div className="card shadow">
        <div className="card-header bg-transparent">
          <h5 className="mb-0">Participants ({pool.participants.length})</h5>
        </div>
        <div className="card-body">
          {pool.participants.length === 0 ? (
            <p className="text-muted">No participants yet</p>
          ) : (
            <div className="table-responsive">
              <table className="table table-hover">
                <thead>
                  <tr>
                    <th>Username</th>
                    <th>Selection Status</th>
                    {isOwner && <th className="text-end">Actions</th>}
                  </tr>
                </thead>
                <tbody>
                  {pool.participants.map(participant => (
                    <tr key={participant.userId}>
                      <td>
                        {participant.userName}
                        {participant.userName === user?.username && ' (You)'}
                        {participant.userName === pool.ownerName && ' (Owner)'}
                      </td>
                      <td>
                        <div className="d-flex flex-column gap-1">
                          <span className={`badge ${participant.selectionComplete ? 'bg-success' : participant.selectedPlayers > 0 ? 'bg-warning' : 'bg-secondary'}`}>
                            {participant.selectionComplete ? 'Complete' : participant.selectedPlayers > 0 ? 'In Progress' : 'Not Started'}
                          </span>
                          <small className="text-muted">
                            {participant.selectedPlayers}/15 players
                            {participant.hasJoker && (
                              <span className="ms-1">
                                <i className="bi bi-star-fill text-warning"></i>
                              </span>
                            )}
                          </small>
                        </div>
                      </td>
                      {isOwner && (
                        <td className="text-end">
                          <div className="btn-group">
                            <button
                              className="btn btn-sm btn-outline-primary"
                              onClick={() => navigate(`/pools/${poolId}/selection/${participant.userId}`)}
                              title="View Selection"
                            >
                              <i className="bi bi-eye"></i>
                            </button>
                            {participant.userName !== pool.ownerName && (
                              <button
                                className="btn btn-sm btn-outline-danger"
                                onClick={() => handleRemoveParticipant(participant.userId)}
                                disabled={participant.userId === user?.id}
                                title="Remove Participant"
                              >
                                <i className="bi bi-trash"></i>
                              </button>
                            )}
                          </div>
                        </td>
                      )}
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>
    </div>
  );
};

export default PoolDetail;
