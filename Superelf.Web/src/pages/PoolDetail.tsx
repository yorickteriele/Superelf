import React, { useEffect, useState } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import { Pool, poolService, UserScore } from '../services/poolService';
import { useAuth } from '../context/AuthContext';
import PlayerScoresModal from '../components/common/PlayerScoresModal';
import selectionService, { SelectionDto } from '../services/selectionService';

/** Pool details with participant management and leaderboard */
const PoolDetail: React.FC = () => {
  const { poolId } = useParams<{ poolId: string }>();
  const [pool, setPool] = useState<Pool | null>(null);
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [newName, setNewName] = useState<string>('');
  const [isEditing, setIsEditing] = useState<boolean>(false);
  const [success, setSuccess] = useState<string | null>(null);
  const [scoreboard, setScoreboard] = useState<UserScore[]>([]);
  const [scoreboardLoading, setScoreboardLoading] = useState<boolean>(false);
  const [combinedViewLoading, setCombinedViewLoading] = useState<boolean>(false);
  const [selectedUserScore, setSelectedUserScore] = useState<UserScore | null>(null);
  const [showPlayerScoresModal, setShowPlayerScoresModal] = useState<boolean>(false);
  const [participantSelections, setParticipantSelections] = useState<Map<string, SelectionDto>>(new Map());
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

  const handleToggleSelectionEditing = async () => {
    if (!poolId || !pool) return;
    
    try {
      const result = await poolService.toggleSelectionEditing(poolId, {
        allowSelectionEditing: !pool.allowSelectionEditing
      });
      
      if (result.success) {
        // Update pool state
        setPool(prev => {
          if (!prev) return null;
          return {
            ...prev,
            allowSelectionEditing: !prev.allowSelectionEditing
          };
        });
        setSuccess(`Selection editing ${!pool.allowSelectionEditing ? 'enabled' : 'disabled'} successfully`);
        setTimeout(() => setSuccess(null), 3000);
      } else {
        setError(result.message || 'Failed to toggle selection editing');
      }
    } catch (error) {
      setError('An error occurred while toggling selection editing');
    }
  };

  const loadScoreboard = async () => {
    if (!poolId) return;
    
    setCombinedViewLoading(true);
    try {
      const scores = await poolService.getScoreboard(poolId);
      setScoreboard(scores);
    } catch (error) {
      console.error('Error loading scoreboard:', error);
      setError('Failed to load scoreboard');
    } finally {
      setCombinedViewLoading(false);
    }
  };

  const loadParticipantSelections = async () => {
    if (!poolId || !pool?.participants || !Array.isArray(pool.participants)) {
      console.log('Skipping participant selections load - missing poolId or participants');
      return;
    }
    
    console.log(`Loading selections for ${pool.participants.length} participants...`);
    
    try {
      // Process participants in batches to avoid too many concurrent requests
      const batchSize = 5;
      const batches: Array<{userId: string, selection: SelectionDto} | null> = [];
      
      for (let i = 0; i < pool.participants.length; i += batchSize) {
        const batch = pool.participants.slice(i, i + batchSize);
        const batchPromises = batch.map(async (participant) => {
          try {
            const selection = await selectionService.getUserSelection(poolId, participant.userId);
            return { userId: participant.userId, selection };
          } catch (error) {
            console.error(`Error loading selection for user ${participant.userId}:`, error);
            return null;
          }
        });
        
        const batchResults = await Promise.all(batchPromises);
        batches.push(...batchResults.filter(Boolean));
      }
      
      // Update selections in a single state update
      setParticipantSelections(prevSelections => {
        const updatedSelections = new Map(prevSelections);
        
        batches.forEach(result => {
          if (result) {
            updatedSelections.set(result.userId, result.selection);
          }
        });
        
        console.log(`Updated selections for ${updatedSelections.size} participants`);
        return updatedSelections;
      });
      
    } catch (error) {
      console.error('Error in loadParticipantSelections:', error);
    }
  };

  // Load scoreboard automatically when component mounts
  useEffect(() => {
    if (poolId) {
      loadScoreboard();
    }
  }, [poolId]);

  // Load participant selections when pool data is available
  useEffect(() => {
    if (pool && Array.isArray(pool.participants) && pool.participants.length > 0) {
      console.log('Loading participant selections...');
      loadParticipantSelections();
    }
  }, [pool, poolId]);
  
  // Add debug effect to log when participantSelections changes
  useEffect(() => {
    console.log('Participant selections updated:', Array.from(participantSelections.entries()));
  }, [participantSelections]);

  // Create combined participant data with scores
  const getCombinedParticipantData = () => {
    return pool?.participants.map(participant => {
      const userScore = scoreboard.find(score => score.userId === participant.userId);
      return {
        ...participant,
        score: userScore?.score || 0,
        hasScore: !!userScore,
        playerScores: userScore?.playerScores || [],
        hasUsedJoker: userScore?.hasUsedJoker || false
      };
    }).sort((a, b) => {
      // Sort by score (descending), then by name if no scores
      if (a.hasScore && b.hasScore) {
        return b.score - a.score;
      }
      if (a.hasScore && !b.hasScore) return -1;
      if (!a.hasScore && b.hasScore) return 1;
      return a.userName.localeCompare(b.userName);
    }) || [];
  };

  const getSelectionStatus = (participant: any) => {
    // Debug log to see participant data
    console.log(`Status for ${participant.userName}:`, {
      selectionComplete: participant.selectionComplete,
      selectedPlayers: participant.selectedPlayers,
      hasSelectionData: participantSelections.has(participant.userId)
    });

    // First check the participant's selectionComplete flag from pool data
    if (participant.selectionComplete) {
      return { status: 'Complete', badge: 'bg-success' };
    }

    // Then check if there are any players selected
    if (participant.selectedPlayers > 0) {
      return { status: 'In Progress', badge: 'bg-warning' };
    }

    // Default to Not Started
    return { status: 'Not Started', badge: 'bg-secondary' };
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
          
          {/* Pool Owner Controls */}
          {isOwner && (
            <div className="mb-3 p-3 bg-light rounded">
              <h6 className="mb-2">Pool Settings</h6>
              <div className="form-check form-switch">
                <input 
                  className="form-check-input" 
                  type="checkbox" 
                  id="allowSelectionEditing"
                  checked={pool.allowSelectionEditing}
                  onChange={handleToggleSelectionEditing}
                />
                <label className="form-check-label" htmlFor="allowSelectionEditing">
                  Allow participants to make/edit selections
                </label>
              </div>
              <small className="text-muted">
                {pool.allowSelectionEditing 
                  ? "Participants can currently create and modify their team selections." 
                  : "Selection editing is currently disabled for all participants."
                }
              </small>
            </div>
          )}
          
          {/* Selection Actions */}
          <div className="d-flex gap-2">
            <button 
              className="btn btn-primary"
              onClick={() => navigate(`/pools/${poolId}/selection`)}
              disabled={!pool.allowSelectionEditing && !isOwner}
            >
              <i className="bi bi-person-plus me-2"></i>
              My Team Selection
            </button>
            {!pool.allowSelectionEditing && !isOwner && (
              <small className="text-muted align-self-center ms-2">
                Selection editing is currently disabled by the pool owner.
              </small>
            )}
          </div>
        </div>
      </div>
      
      <div className="card shadow">
        <div className="card-header bg-transparent d-flex justify-content-between align-items-center">
          <h5 className="mb-0">Leaderboard & Participants ({pool.participants.length})</h5>
          <button
            className="btn btn-sm btn-outline-secondary"
            onClick={loadScoreboard}
            disabled={combinedViewLoading}
          >
            <i className={`bi ${combinedViewLoading ? 'bi-arrow-clockwise spin' : 'bi-arrow-clockwise'}`}></i>
            Refresh
          </button>
        </div>
        <div className="card-body">
          {combinedViewLoading ? (
            <div className="text-center py-4">
              <div className="spinner-border text-primary" role="status">
                <span className="visually-hidden">Loading...</span>
              </div>
            </div>
          ) : pool.participants.length === 0 ? (
            <p className="text-muted">No participants yet</p>
          ) : (
            <div className="table-responsive">
              <table className="table table-hover">
                <thead>
                  <tr>
                    <th style={{ width: '50px' }}>#</th>
                    <th>Player</th>
                    <th>Selection Status</th>
                    <th className="text-center">Score</th>
                    <th className="text-center">Joker</th>
                    <th className="text-end">Actions</th>
                  </tr>
                </thead>
                <tbody>
                  {getCombinedParticipantData().map((participant, index) => {
                    const selectionStatus = getSelectionStatus(participant);
                    const isCurrentUser = participant.userName === user?.username;
                    const isOwnerUser = participant.userName === pool.ownerName;
                    
                    return (
                      <tr key={participant.userId} className={isCurrentUser ? 'table-primary' : ''}>
                        <td className="fw-bold">
                          {participant.hasScore ? (
                            <>
                              {index + 1}
                              {index === 0 && <i className="bi bi-trophy-fill text-warning ms-1"></i>}
                            </>
                          ) : (
                            <span className="text-muted">-</span>
                          )}
                        </td>
                        <td>
                          <div className="d-flex flex-column">
                            <span>
                              {participant.userName}
                              {isCurrentUser && ' (You)'}
                              {isOwnerUser && ' (Owner)'}
                            </span>
                          </div>
                        </td>
                        <td>
                          <div className="d-flex flex-column gap-1">
                            <span className={`badge ${selectionStatus.badge}`}>
                              {selectionStatus.status}
                            </span>
                            <small className="text-muted">
                              {participant.selectedPlayers}/15 players
                              {participant.hasJoker && (
                                <span className="ms-1">
                                  <i className="bi bi-star-fill text-warning" title="Has Joker"></i>
                                </span>
                              )}
                            </small>
                          </div>
                        </td>
                        <td className="text-center">
                          {participant.hasScore ? (
                            <span className={`fw-bold ${participant.score >= 0 ? 'text-success' : 'text-danger'}`}>
                              {participant.score >= 0 ? '+' : ''}{participant.score}
                            </span>
                          ) : (
                            <span className="text-muted">-</span>
                          )}
                        </td>
                        <td className="text-center">
                          {participant.hasUsedJoker && (
                            <i className="bi bi-star-fill text-warning" title="Joker Used"></i>
                          )}
                        </td>
                        <td className="text-end">
                          <div className="btn-group">
                            {participant.hasScore && (
                              <button
                                className="btn btn-sm btn-outline-info"
                                onClick={() => {
                                  const userScore = scoreboard.find(score => score.userId === participant.userId);
                                  if (userScore) {
                                    setSelectedUserScore(userScore);
                                    setShowPlayerScoresModal(true);
                                  }
                                }}
                                title="View Player Scores"
                              >
                                <i className="bi bi-list-ul"></i>
                              </button>
                            )}
                            <button
                              className="btn btn-sm btn-outline-primary"
                              onClick={() => navigate(`/pools/${poolId}/selection/${participant.userId}`)}
                              title="View Team Selection"
                            >
                              <i className="bi bi-eye"></i>
                            </button>
                            {isOwner && participant.userName !== pool.ownerName && (
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
                      </tr>
                    );
                  })}
                </tbody>
              </table>
            </div>
          )}
        </div>
      </div>



      {/* Player Scores Modal */}
      {selectedUserScore && (
        <PlayerScoresModal
          isOpen={showPlayerScoresModal}
          onClose={() => {
            setShowPlayerScoresModal(false);
            setSelectedUserScore(null);
          }}
          userName={selectedUserScore.userName}
          playerScores={selectedUserScore.playerScores}
        />
      )}
    </div>
  );
};

export default PoolDetail;
