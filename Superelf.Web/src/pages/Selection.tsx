import React, { useEffect, useState, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import selectionService, { 
  SelectionDto, 
  FootballPlayerDto, 
  SubmitSelectionDto, 
  SelectionTableDto 
} from '../services/selectionService';
import '../styles/Selection.css';

const Selection: React.FC = () => {
  const { poolId, userId } = useParams<{ poolId: string; userId?: string }>();
  const navigate = useNavigate();
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [selection, setSelection] = useState<SelectionDto | null>(null);
  const [isViewingOtherUser, setIsViewingOtherUser] = useState<boolean>(false);
  
  // Modal states
  const [showPlayerModal, setShowPlayerModal] = useState<boolean>(false);
  const [currentPosition, setCurrentPosition] = useState<string>('');
  const [isReserve, setIsReserve] = useState<boolean>(false);
  const [maxSelection, setMaxSelection] = useState<number>(1);
  const [playerTableData, setPlayerTableData] = useState<SelectionTableDto | null>(null);
  const [selectedPlayers, setSelectedPlayers] = useState<string[]>([]);
  const [jokerPlayer, setJokerPlayer] = useState<string | null>(null);
  const [isJokerSelection, setIsJokerSelection] = useState<boolean>(false);
  
  // Define fetchSelectionData using useCallback
  const fetchSelectionData = useCallback(async () => {
    setLoading(true);
    try {
      const data = await selectionService.getSelection(poolId as string);
      setSelection(data);
    } catch (err: any) {
      setError(err.message || 'Failed to load selection data');
    } finally {
      setLoading(false);
    }
  }, [poolId]);

  // Load initial selection data
  useEffect(() => {
    if (!poolId) return;
    
    // Check if we're viewing another user's selection
    setIsViewingOtherUser(!!userId);
    
    fetchSelectionData();
  }, [poolId, userId, fetchSelectionData]);

  const openPlayerSelection = async (position: string, isRes: boolean = false, maxSel: number = 1) => {
    if (!poolId) return;

    setCurrentPosition(position);
    setIsReserve(isRes);
    setMaxSelection(maxSel);
    setSelectedPlayers([]);
    setIsJokerSelection(false);
    
    try {
      const data = await selectionService.getPlayersForSelection(poolId, position, isRes, maxSel);
      setPlayerTableData(data);
      setShowPlayerModal(true);
    } catch (err: any) {
      setError(err.message || `Failed to load ${position} data`);
    }
  };

  const togglePlayerSelection = (playerId: string) => {
    if (selectedPlayers.includes(playerId)) {
      setSelectedPlayers(selectedPlayers.filter(id => id !== playerId));
    } else {
      if (maxSelection === 1) {
        setSelectedPlayers([playerId]);
      } else if (selectedPlayers.length < maxSelection) {
        setSelectedPlayers([...selectedPlayers, playerId]);
      }
    }
  };

  const toggleJokerSelection = (playerId: string) => {
    if (jokerPlayer === playerId) {
      setJokerPlayer(null);
    } else {
      setJokerPlayer(playerId);
    }
  };

  const handleJokerMode = () => {
    setIsJokerSelection(!isJokerSelection);
    // Reset selections when toggling joker mode
    setJokerPlayer(null);
  };

  const handleSubmitSelection = async () => {
    if (!poolId || selectedPlayers.length === 0) return;
    
    try {
      const selectionData: SubmitSelectionDto = {
        position: currentPosition,
        isReserve: isReserve,
        selectedPlayers: selectedPlayers,
        isJoker: isJokerSelection && jokerPlayer !== null
      };
      
      await selectionService.submitSelection(poolId, selectionData);
      
      // If the selection was for a joker, also set the joker
      if (isJokerSelection && jokerPlayer) {
        await selectionService.setJoker(poolId, jokerPlayer);
      }
      
      setSuccess('Players selected successfully!');
      setShowPlayerModal(false);
      fetchSelectionData(); // Refresh the data
      
      setTimeout(() => setSuccess(null), 3000);
    } catch (err: any) {
      setError(err.message || 'Failed to submit selection');
    }
  };

  const handleCreateSelection = async () => {
    if (!poolId) return;
    
    try {
      setLoading(true);
      // Create or refresh the selection for this pool
      await fetchSelectionData();
      setSuccess('Selection created successfully!');
      setTimeout(() => setSuccess(null), 3000);
    } catch (err: any) {
      setError(err.message || 'Failed to create selection');
    } finally {
      setLoading(false);
    }
  };

  const handleFinalizeSelection = async () => {
    if (!poolId || !selection?.complete) return;
    
    try {
      setLoading(true);
      await selectionService.finalizeSelection(poolId);
      setSuccess('Selection finalized successfully!');
      await fetchSelectionData(); // Refresh the data
      setTimeout(() => setSuccess(null), 3000);
    } catch (err: any) {
      setError(err.message || 'Failed to finalize selection');
    } finally {
      setLoading(false);
    }
  };

  const findPlayer = React.useMemo(() => {
    return (playerId?: string): FootballPlayerDto | undefined => {
      if (!playerId || !selection) return undefined;
  
      return [...selection.goalkeepers, ...selection.defenders, 
              ...selection.midfielders, ...selection.forwards]
              .find(player => player.id === playerId);
    };
  }, [selection]);

  const getPlayerStyle = (player?: FootballPlayerDto) => {
    if (!player) return {};

    return {
      backgroundColor: player.club ? `var(--club-${player.club.replace(/\s+/g, '-').toLowerCase()})` : '#333',
    };
  };

  const isPlayerJoker = (playerId?: string) => {
    return selection?.jokerPlayerId === playerId;
  };

  // Calculate completion percentage
  const calculateCompletionPercentage = () => {
    if (!selection) return 0;
    
    const totalRequired = 1 + 4 + 3 + 3 + 4; // GK + DEF + MID + FWD + Reserves
    let totalSelected = 0;
    
    // Count selected players
    if (selection.selectedBasisGoalkeeper) totalSelected++;
    totalSelected += selection.selectedBasisDefenders.length;
    totalSelected += selection.selectedBasisMidfielders.length;
    totalSelected += selection.selectedBasisForwards.length;
    
    // Count reserves
    if (selection.selectedReserveGoalkeeper) totalSelected++;
    if (selection.selectedReserveDefender) totalSelected++;
    if (selection.selectedReserveMidfielder) totalSelected++;
    if (selection.selectedReserveForward) totalSelected++;
    
    return Math.round((totalSelected / totalRequired) * 100);
  };
  
  // Render player slot
  const renderPlayerSlot = (position: string, isReserve: boolean = false, maxPlayers: number = 1, playerId?: string) => {
    const player = findPlayer(playerId);
    const isJoker = isPlayerJoker(playerId);
    
    return (
      <div 
        className={`player-slot ${isJoker ? 'joker' : ''} ${isViewingOtherUser ? 'readonly' : ''}`}
        onClick={() => !isViewingOtherUser && openPlayerSelection(position, isReserve, maxPlayers)}
        data-club={player?.club}
        style={{ cursor: isViewingOtherUser ? 'default' : 'pointer' }}
      >
        {player ? (
          <>
            <div className="player-image" style={getPlayerStyle(player)}></div>
            <div className="player-name">{player.name}</div>
          </>
        ) : (
          <div className="player-name">{isViewingOtherUser ? 'Empty' : `Select ${position}`}</div>
        )}
      </div>
    );
  };

  if (loading) {
    return (
      <div className="selection-container">
        <div className="text-center" style={{ padding: '50px' }}>
          <div className="spinner-border text-primary" role="status">
            <span className="visually-hidden">Loading...</span>
          </div>
          <p style={{ marginTop: '20px', color: 'white' }}>Loading selection...</p>
        </div>
      </div>
    );
  }

  if (error && !selection) {
    return (
      <div className="selection-container">
        {/* Header */}
        <div className="header-actions">
          <button 
            className="btn btn-outline-light"
            onClick={() => navigate(`/pools/${poolId}`)}
          >
            Back to Pool
          </button>

          <div className="header-buttons">
            <button 
              className="btn btn-primary"
              onClick={handleCreateSelection}
              disabled={loading}
            >
              {loading ? 'Creating...' : 'Create Selection'}
            </button>
          </div>
        </div>

        <div className="alert alert-error">
          {error}
          <button 
            type="button" 
            className="close-button" 
            onClick={() => setError(null)}
          >
            &times;
          </button>
        </div>
      </div>
    );
  }

  if (!selection) {
    return (
      <div className="selection-container">
        {/* Header */}
        <div className="header-actions">
          <button 
            className="btn btn-outline-light"
            onClick={() => navigate(`/pools/${poolId}`)}
          >
            Back to Pool
          </button>

          <div className="header-buttons">
            <button 
              className="btn btn-primary"
              onClick={handleCreateSelection}
              disabled={loading}
            >
              {loading ? 'Creating...' : 'Create Selection'}
            </button>
          </div>
        </div>

        <div className="text-center" style={{ padding: '50px' }}>
          <h3 style={{ color: 'white', marginBottom: '20px' }}>No Selection Found</h3>
          <p style={{ color: 'rgba(255, 255, 255, 0.7)' }}>
            Click "Create Selection" to start building your team.
          </p>
        </div>
      </div>
    );
  }

  const completionPercentage = calculateCompletionPercentage();
  
  return (
    <div className="selection-container">
      {/* Header */}
      <div className="header-actions">
        <button 
          className="btn btn-outline-light"
          onClick={() => navigate(isViewingOtherUser ? `/pools/${poolId}/overview` : `/pools/${poolId}`)}
        >
          <i className="bi bi-arrow-left me-2"></i>
          {isViewingOtherUser ? 'Back to Overview' : 'Back to Pool'}
        </button>

        <div className="header-buttons">
          {!isViewingOtherUser && !selection?.complete && (
            <button 
              className="btn btn-primary"
              onClick={handleCreateSelection}
              disabled={loading}
            >
              {loading ? 'Creating...' : 'Create Selection'}
            </button>
          )}

          {!isViewingOtherUser && selection?.complete && completionPercentage === 100 && !selection?.hasJoker && (
            <button 
              className="btn btn-warning"
              onClick={handleFinalizeSelection}
              disabled={loading}
            >
              {loading ? 'Finalizing...' : 'Finalize Selection'}
            </button>
          )}

          {selection?.complete && (
            <button className="btn btn-success" disabled>
              Team Complete! ({completionPercentage}%)
            </button>
          )}
          
          {isViewingOtherUser && (
            <span className="badge bg-info fs-6">Viewing Another User's Selection</span>
          )}
        </div>
      </div>

      {/* Alert Messages */}
      {error && (
        <div className="alert alert-error">
          {error}
          <button 
            type="button" 
            className="close-button" 
            onClick={() => setError(null)}
          >
            &times;
          </button>
        </div>
      )}
      
      {success && (
        <div className="alert alert-success">
          {success}
          <button 
            type="button" 
            className="close-button" 
            onClick={() => setSuccess(null)}
          >
            &times;
          </button>
        </div>
      )}

      {/* Selection Status */}
      <div className="selection-status">
        <div className="status-info">
          <div className="status-item">
            <span className="status-label">Formation</span>
            <span className="status-value">{selection!.formation}</span>
          </div>
          <div className="status-item">
            <span className="status-label">Players</span>
            <span className="status-value">{selection!.totalPlayers}/11</span>
          </div>
          <div className="status-item">
            <span className="status-label">Unique Clubs</span>
            <span className="status-value">{selection!.uniqueNationalities}</span>
          </div>
          <div className="status-item">
            <span className="status-label">Joker</span>
            <span className="status-value">{selection!.hasJoker ? 'Selected' : 'Not Selected'}</span>
          </div>
        </div>
        
        <div>
          <div className="progress-container">
            <div 
              className="progress-bar"
              style={{ width: `${completionPercentage}%` }}
            ></div>
          </div>
          <div className="text-center">{completionPercentage}% Complete</div>
        </div>
      </div>

      {/* Football Field */}
      <div className="field-container">
        <div className="football-field">
          <div className="goal-area top"></div>
          
          {/* Goalkeeper */}
          <div className="position-row">
            {renderPlayerSlot('Goalkeeper', false, 1, selection!.selectedBasisGoalkeeper)}
          </div>
          
          {/* Defenders */}
          <div className="position-row">
            {Array.from({ length: 4 }).map((_, idx) => (
              <React.Fragment key={`def-${idx}`}>
                {renderPlayerSlot('Defender', false, 4, selection!.selectedBasisDefenders[idx])}
              </React.Fragment>
            ))}
          </div>
          
          {/* Midfielders */}
          <div className="position-row">
            {Array.from({ length: 3 }).map((_, idx) => (
              <React.Fragment key={`mid-${idx}`}>
                {renderPlayerSlot('Midfielder', false, 3, selection!.selectedBasisMidfielders[idx])}
              </React.Fragment>
            ))}
          </div>
          
          {/* Forwards */}
          <div className="position-row">
            {Array.from({ length: 3 }).map((_, idx) => (
              <React.Fragment key={`fwd-${idx}`}>
                {renderPlayerSlot('Forward', false, 3, selection!.selectedBasisForwards[idx])}
              </React.Fragment>
            ))}
          </div>
          
          <div className="goal-area bottom"></div>
        </div>
      </div>
      
      {/* Reserves */}
      <h3 className="reserves-title">Reserves</h3>
      <div className="reserves-container">
        <div className="reserve-position">
          <span className="reserve-title">Goalkeeper</span>
          {renderPlayerSlot('Goalkeeper', true, 1, selection!.selectedReserveGoalkeeper)}
        </div>
        
        <div className="reserve-position">
          <span className="reserve-title">Defender</span>
          {renderPlayerSlot('Defender', true, 1, selection!.selectedReserveDefender)}
        </div>
        
        <div className="reserve-position">
          <span className="reserve-title">Midfielder</span>
          {renderPlayerSlot('Midfielder', true, 1, selection!.selectedReserveMidfielder)}
        </div>
        
        <div className="reserve-position">
          <span className="reserve-title">Forward</span>
          {renderPlayerSlot('Forward', true, 1, selection!.selectedReserveForward)}
        </div>
      </div>
      
      {/* Player Selection Modal */}
      {showPlayerModal && playerTableData && (
        <div className="player-selection-modal">
          <div className="modal-content">
            <div className="modal-header">
              <h3 className="modal-title">
                {isReserve ? 'Reserve ' : ''}
                {currentPosition} Selection
              </h3>
              <button className="close-button" onClick={() => setShowPlayerModal(false)}>×</button>
            </div>
            
            <div className="selection-info">
              <p>
                Select {maxSelection > 1 ? `${maxSelection} ${currentPosition}s` : `a ${currentPosition}`}.
                {!isReserve && ' Each player must be from a unique club.'}
              </p>
              
              <div className="selection-actions">
                <button 
                  className={`joker-toggle ${isJokerSelection ? 'active' : ''}`}
                  onClick={handleJokerMode}
                  disabled={selection!.hasJoker && !isPlayerJoker(selectedPlayers[0])}
                >
                  {isJokerSelection ? 'Joker Mode Active' : 'Select as Joker'}
                </button>
                
                <button 
                  className="submit-button"
                  onClick={handleSubmitSelection}
                  disabled={!selectedPlayers.length || (isJokerSelection && !jokerPlayer)}
                >
                  Confirm Selection
                </button>
              </div>
            </div>
            
            <table className="player-table">
              <thead>
                <tr>
                  <th>Name</th>
                  <th>Club</th>
                  <th>Nationality</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {playerTableData.players.map((player: FootballPlayerDto) => {
                  const isSelected = selectedPlayers.includes(player.id);
                  const isJoker = jokerPlayer === player.id;
                  const alreadySelected = playerTableData.alreadySelectedPlayers.includes(player.id);
                  
                  return (
                    <tr 
                      key={player.id}
                      className={`player-row ${isSelected ? 'selected' : ''} ${isJoker ? 'joker' : ''}`}
                    >
                      <td>{player.name}</td>
                      <td>{player.club}</td>
                      <td>{player.nationality}</td>
                      <td>
                        {!alreadySelected && (
                          <div className="action-buttons">
                            <button 
                              className={`select-button ${isSelected ? 'selected' : ''}`}
                              onClick={() => togglePlayerSelection(player.id)}
                            >
                              {isSelected ? 'Deselect' : 'Select'}
                            </button>
                            
                            {isJokerSelection && isSelected && (
                              <button 
                                className={`joker-button ${isJoker ? 'selected' : ''}`}
                                onClick={() => toggleJokerSelection(player.id)}
                              >
                                {isJoker ? 'Remove Joker' : 'Set as Joker'}
                              </button>
                            )}
                          </div>
                        )}
                        {alreadySelected && (
                          <span className="already-selected">Already Selected</span>
                        )}
                      </td>
                    </tr>
                  );
                })}
              </tbody>
            </table>
          </div>
        </div>
      )}
    </div>
  );
};

export default Selection;
