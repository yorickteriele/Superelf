import React, { useEffect, useState, useCallback } from 'react';
import { useParams, useNavigate } from 'react-router-dom';
import selectionService, { 
  SelectionDto, 
  FootballPlayerDto, 
  SubmitSelectionDto, 
  SelectionTableDto,
  SelectedPlayerDto
} from '../services/selectionService';
import { poolService, Pool } from '../services/poolService';
import { useAuth } from '../context/AuthContext';
import '../styles/Selection.css';

/** Interactive team selection interface with field visualization */
const Selection: React.FC = () => {
  const { poolId, userId } = useParams<{ poolId: string; userId?: string }>();
  const navigate = useNavigate();
  const { user } = useAuth();
  const [loading, setLoading] = useState<boolean>(true);
  const [error, setError] = useState<string | null>(null);
  const [success, setSuccess] = useState<string | null>(null);
  const [selection, setSelection] = useState<SelectionDto | null>(null);
  const [pool, setPool] = useState<Pool | null>(null);
  const [isViewingOtherUser, setIsViewingOtherUser] = useState<boolean>(false);
  
  // Modal states
  const [showPlayerModal, setShowPlayerModal] = useState<boolean>(false);
  const [currentPosition, setCurrentPosition] = useState<string>('');
  const [isReserve, setIsReserve] = useState<boolean>(false);
  const [maxSelection, setMaxSelection] = useState<number>(1);
  const [currentSlotIndex, setCurrentSlotIndex] = useState<number>(0);
  const [playerTableData, setPlayerTableData] = useState<SelectionTableDto | null>(null);
  const [selectedPlayers, setSelectedPlayers] = useState<string[]>([]);
  const [jokerPlayer, setJokerPlayer] = useState<string | null>(null);
  const [isJokerSelection, setIsJokerSelection] = useState<boolean>(false);
  
  // New states for club filtering, search, and player management
  const [clubFilter, setClubFilter] = useState<string>('');
  const [searchFilter, setSearchFilter] = useState<string>('');
  const [uniqueClubsOnly, setUniqueClubsOnly] = useState<boolean>(true);
  const [availableClubs, setAvailableClubs] = useState<string[]>([]);
  const [filteredPlayers, setFilteredPlayers] = useState<FootballPlayerDto[]>([]);
  
  // Define fetchSelectionData using useCallback
  const fetchSelectionData = useCallback(async () => {
    setLoading(true);
    try {
      const [selectionData, poolData] = await Promise.all([
        userId 
          ? selectionService.getUserSelection(poolId as string, userId)
          : selectionService.getSelection(poolId as string),
        poolService.getPool(poolId as string)
      ]);
      setSelection(selectionData);
      setPool(poolData);
    } catch (err: any) {
      setError(err.message || 'Failed to load selection data');
    } finally {
      setLoading(false);
    }
  }, [poolId, userId]);

  // Helper function to check if editing is allowed
  const isEditingAllowed = useCallback(() => {
    if (!pool || !user) return false;
    if (isViewingOtherUser) return false;
    // Pool owner can always edit, otherwise check allowSelectionEditing
    return pool.ownerName === user.username || pool.allowSelectionEditing;
  }, [pool, user, isViewingOtherUser]);

  // Load initial selection data
  useEffect(() => {
    if (!poolId) return;
    
    // Check if we're viewing another user's selection
    setIsViewingOtherUser(!!userId);
    
    fetchSelectionData();
  }, [poolId, userId, fetchSelectionData]);

  // Filter players based on club selection, search, and unique clubs
  useEffect(() => {
    if (playerTableData) {
      let filtered = playerTableData.players;
      
      // Apply unique clubs filter
      if (uniqueClubsOnly && selection) {
        // Get clubs of already selected players
        const selectedClubs = new Set<string>();
        
        // Add clubs from all selected players
        [
          selection.selectedBasisGoalkeeper,
          ...selection.selectedBasisDefenders,
          ...selection.selectedBasisMidfielders,
          ...selection.selectedBasisForwards,
          selection.selectedReserveGoalkeeper,
          selection.selectedReserveDefender,
          selection.selectedReserveMidfielder,
          selection.selectedReserveForward
        ].forEach(selectedPlayer => {
          if (selectedPlayer?.player?.club) {
            selectedClubs.add(selectedPlayer.player.club);
          }
        });
        
        // Filter out players from clubs that are already selected
        // Exception: Allow players that are already selected (for editing existing selections)
        filtered = filtered.filter(player => 
          !selectedClubs.has(player.club || '') || playerTableData.alreadySelectedPlayers.includes(player.id)
        );
      }
      
      // Apply club filter
      if (clubFilter) {
        filtered = filtered.filter(player => 
          player.club && player.club.toLowerCase().includes(clubFilter.toLowerCase())
        );
      }
      
      // Apply search filter
      if (searchFilter) {
        filtered = filtered.filter(player => 
          player.name.toLowerCase().includes(searchFilter.toLowerCase())
        );
      }
      
      setFilteredPlayers(filtered);
      
      // Extract unique clubs for filter dropdown
      const clubs = Array.from(new Set(playerTableData.players
        .map(player => player.club)
        .filter(Boolean) as string[]
      )).sort();
      setAvailableClubs(clubs);
    }
  }, [playerTableData, clubFilter, searchFilter, uniqueClubsOnly, selection]);

  const openPlayerSelection = async (position: string, isRes: boolean = false, maxSel: number = 1, slotIndex?: number, jokerMode: boolean = false) => {
    if (!poolId) return;

    setCurrentPosition(position);
    setIsReserve(isRes);
    setMaxSelection(maxSel);
    setCurrentSlotIndex(slotIndex || 0);
    setSelectedPlayers([]);
    setIsJokerSelection(jokerMode);
    setClubFilter(''); // Reset club filter
    setSearchFilter(''); // Reset search filter
    
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
      // If player is already selected, deselect it
      setSelectedPlayers(selectedPlayers.filter(id => id !== playerId));
    } else {
      // Always replace the current selection with the new player (max 1 player per slot)
      setSelectedPlayers([playerId]);
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
    setSelectedPlayers([]);
  };

  const handleSubmitSelection = async () => {
    if (!poolId) return;
    
    try {
      const selectionData: SubmitSelectionDto = {
        position: currentPosition,
        isReserve: isReserve,
        selectedPlayers: selectedPlayers,
        isJoker: isJokerSelection && jokerPlayer !== null,
        slotIndex: currentSlotIndex
      };
      
      await selectionService.submitSelection(poolId, selectionData);
      
      // If the selection was for a joker, also set the joker
      if (isJokerSelection && jokerPlayer) {
        await selectionService.setJoker(poolId, jokerPlayer);
      }
      
      setSuccess(selectedPlayers.length > 0 ? 'Players selected successfully!' : 'Player removed successfully!');
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

  const handleSetJoker = async (playerId: string) => {
    if (!poolId) return;
    
    try {
      await selectionService.setJoker(poolId, playerId);
      setSuccess('Joker set successfully!');
      await fetchSelectionData(); // Refresh the data
      setTimeout(() => setSuccess(null), 3000);
    } catch (err: any) {
      setError(err.message || 'Failed to set joker');
    }
  };

  const handleRemoveJoker = async () => {
    if (!poolId) return;
    
    try {
      // Set joker to empty to remove it
      await selectionService.setJoker(poolId, '');
      setSuccess('Joker removed successfully!');
      await fetchSelectionData(); // Refresh the data
      setTimeout(() => setSuccess(null), 3000); 
    } catch (err: any) {
      setError(err.message || 'Failed to remove joker');
    }
  };

  // New function to remove a player from a specific slot
  const handleRemovePlayer = async (position: string, isReserve: boolean = false, slotIndex: number = 0) => {
    if (!poolId || isViewingOtherUser) return;
    
    try {
      // Submit an empty selection to remove the player
      const selectionData: SubmitSelectionDto = {
        position: position,
        isReserve: isReserve,
        selectedPlayers: [],
        isJoker: false,
        slotIndex: slotIndex
      };
      
      await selectionService.submitSelection(poolId, selectionData);
      setSuccess('Player removed successfully!');
      await fetchSelectionData(); // Refresh the data
      setTimeout(() => setSuccess(null), 3000);
    } catch (err: any) {
      setError(err.message || 'Failed to remove player');
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

  const findSelectedPlayer = React.useMemo(() => {
    return (selectedPlayer?: SelectedPlayerDto | null): FootballPlayerDto | undefined => {
      if (!selectedPlayer) return undefined;
      return selectedPlayer.player;
    };
  }, []);

  const getPlayerStyle = (player?: FootballPlayerDto) => {
    if (!player) return {};

    return {
      backgroundImage: player.photoUrl ? `url(${player.photoUrl})` : 'none',
      backgroundSize: 'cover',
      backgroundPosition: 'center',
      backgroundColor: player.photoUrl ? 'transparent' : (player.club ? `var(--club-${player.club.replace(/\s+/g, '-').toLowerCase()})` : '#333'),
    };
  };

  const handleImageError = (e: React.SyntheticEvent<HTMLImageElement>) => {
    const target = e.target as HTMLImageElement;
    target.style.display = 'none';
    // Show placeholder or fallback
    const container = target.closest('.player-image');
    if (container) {
      (container as HTMLElement).style.backgroundImage = 'none';
      (container as HTMLElement).style.backgroundColor = '#333';
    }
  };

  const isPlayerJoker = (playerId?: string) => {
    return selection?.jokerPlayerId === playerId;
  };

  const isSelectedPlayerJoker = (selectedPlayer?: SelectedPlayerDto | null) => {
    return selectedPlayer?.isJoker || false;
  };

  // Calculate completion percentage
  const calculateCompletionPercentage = () => {
    if (!selection) return 0;
    
    // Get all selected players (11 basis + 4 reserves = 15 total)
    const allSelectedPlayers = [
      selection.selectedBasisGoalkeeper,
      ...selection.selectedBasisDefenders,
      ...selection.selectedBasisMidfielders,
      ...selection.selectedBasisForwards,
      selection.selectedReserveGoalkeeper,
      selection.selectedReserveDefender,
      selection.selectedReserveMidfielder,
      selection.selectedReserveForward
    ].filter(player => player !== null && player !== undefined);
    
    // Count unique clubs from selected players
    const uniqueClubs = new Set(
      allSelectedPlayers
        .map(player => player?.player?.club)
        .filter(club => club !== undefined && club !== null && club !== '')
    );
    
    // Check completion criteria
    const totalPlayers = allSelectedPlayers.length;
    const totalUniqueClubs = uniqueClubs.size;
    const hasJoker = selection.hasJoker;
    
    // Debug logging (can be removed in production)
    console.log('Selection Status Debug:', {
      totalPlayers,
      totalUniqueClubs,
      hasJoker,
      clubs: Array.from(uniqueClubs)
    });
    
    // 100% complete when: 15 players + 15 unique clubs + 1 joker
    if (totalPlayers === 15 && totalUniqueClubs === 15 && hasJoker) {
      return 100;
    }
    
    // Calculate partial completion based on progress toward 15 players
    const playerProgress = Math.min(totalPlayers / 15, 1) * 70; // 70% for players
    const jokerProgress = hasJoker ? 15 : 0; // 15% for joker
    const clubProgress = Math.min(totalUniqueClubs / 15, 1) * 15; // 15% for unique clubs
    
    return Math.round(playerProgress + jokerProgress + clubProgress);
  };

  // Helper function to get position name based on index
  const getPositionName = (position: string, index: number): string => {
    switch (position) {
      case 'Goalkeeper':
        return 'Goalkeeper';
      case 'Defender':
        const defenderNames = ['Left Back', 'Left Center Back', 'Right Center Back', 'Right Back'];
        return defenderNames[index] || 'Defender';
      case 'Midfielder':
        const midfielderNames = ['Left Midfielder', 'Center Midfielder', 'Right Midfielder'];
        return midfielderNames[index] || 'Midfielder';
      case 'Forward':
        const forwardNames = ['Left Wing', 'Center Forward', 'Right Wing'];
        return forwardNames[index] || 'Forward';
      default:
        return position;
    }
  };
  
  // Render player slot
  const renderPlayerSlot = (position: string, isReserve: boolean = false, maxPlayers: number = 1, selectedPlayer?: SelectedPlayerDto | null, index: number = 0) => {
    const player = selectedPlayer ? findSelectedPlayer(selectedPlayer) : undefined;
    const isJoker = selectedPlayer ? isSelectedPlayerJoker(selectedPlayer) : false;
    const positionName = selectedPlayer?.positionName || getPositionName(position, index);
    
    const handleStarClick = (e: React.MouseEvent) => {
      e.stopPropagation();
      if (isViewingOtherUser || !isEditingAllowed()) return;
      
      if (player && !isJoker && !selection?.hasJoker) {
        // Set as joker when no joker exists
        handleSetJoker(player.id);
      } else if (player && !isJoker && selection?.hasJoker) {
        // Switch joker to this player (remove current joker and set this one)
        handleSetJoker(player.id);
      } else if (player && isJoker) {
        // Remove joker
        handleRemoveJoker();
      } else if (!player && !selection?.hasJoker) {
        // If no player and no joker exists, open joker selection mode
        setIsJokerSelection(true);
        setCurrentPosition(position);
        setIsReserve(isReserve);
        setMaxSelection(1);
        setCurrentSlotIndex(index);
        setSelectedPlayers([]);
        setJokerPlayer(null);
        openPlayerSelection(position, isReserve, 1, index, true);
      } else if (!player && selection?.hasJoker) {
        // If no player but joker exists, allow changing the joker to this position
        setIsJokerSelection(true);
        setCurrentPosition(position);
        setIsReserve(isReserve);
        setMaxSelection(1);
        setCurrentSlotIndex(index);
        setSelectedPlayers([]);
        setJokerPlayer(null);
        openPlayerSelection(position, isReserve, 1, index, true);
      }
    };
    
    const handleSlotClick = () => {
      if (!isViewingOtherUser && isEditingAllowed()) {
        openPlayerSelection(position, isReserve, 1, index, false);
      }
    };

    const handleRemoveClick = (e: React.MouseEvent) => {
      e.stopPropagation();
      if (!isViewingOtherUser && isEditingAllowed() && player) {
        handleRemovePlayer(position, isReserve, index);
      }
    };
    
    return (
      <div 
        className={`player-slot ${isJoker ? 'joker' : ''} ${isViewingOtherUser ? 'readonly' : ''} ${!isEditingAllowed() ? 'locked' : ''}`}
        onClick={handleSlotClick}
        data-club={player?.club}
        style={{ cursor: (isViewingOtherUser || !isEditingAllowed()) ? 'default' : 'pointer' }}
        title={!isEditingAllowed() && !isViewingOtherUser ? 'Selection editing is disabled by the pool owner' : ''}
      >
        {/* Joker Star - only show when there's a player */}
        {player && (
          <div
            className="joker-star"
            onClick={handleStarClick}
            style={{
              position: 'absolute',
              top: '-8px',
              right: '-8px',
              fontSize: isJoker ? '20px' : '18px',
              background: 'rgba(255, 255, 255, 0.1)',
              width: isJoker ? '30px' : '28px',
              height: isJoker ? '30px' : '28px',
              borderRadius: '50%',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              boxShadow: isJoker ? '0 4px 8px rgba(255, 215, 0, 0.4)' : '0 2px 4px rgba(0, 0, 0, 0.3)',
              border: isJoker ? '2px solid #ffd700' : '1px solid rgba(255, 255, 255, 0.3)',
              zIndex: 1000,
              transition: 'all 0.3s ease',
              opacity: isJoker ? 1 : 0.7,
              color: isJoker ? '#ffd700' : 'rgba(255, 255, 255, 0.8)',
              cursor: 'pointer',
              animation: isJoker ? 'jokerStar 1.5s ease-in-out infinite' : 'none'
            }}
            title={isJoker ? 'Click to remove joker' : 'Click to set as joker'}
          >
            {isJoker ? '★' : '☆'}
          </div>
        )}

        {/* Remove button for selected players */}
        {player && !isViewingOtherUser && (
          <div
            className="remove-player"
            onClick={handleRemoveClick}
            style={{
              position: 'absolute',
              top: '-8px',
              left: '-8px',
              fontSize: '16px',
              background: 'rgba(255, 0, 0, 0.8)',
              width: '24px',
              height: '24px',
              borderRadius: '50%',
              display: 'flex',
              alignItems: 'center',
              justifyContent: 'center',
              boxShadow: '0 2px 4px rgba(0, 0, 0, 0.3)',
              border: '1px solid rgba(255, 255, 255, 0.3)',
              zIndex: 1000,
              transition: 'all 0.3s ease',
              color: 'white',
              cursor: 'pointer',
              fontWeight: 'bold'
            }}
            title="Remove player"
          >
            ×
          </div>
        )}
        
        {player ? (
          <>
            <div className="player-image" style={getPlayerStyle(player)}>
              {player.photoUrl && (
                <img 
                  src={`${window.location.origin}${player.photoUrl}`} 
                  alt={player.name}
                  style={{ width: '100%', height: '100%', objectFit: 'cover', borderRadius: '50%' }}
                  onError={handleImageError}
                />
              )}
            </div>
            <div className="player-name">{player.name}</div>
            <div className="player-position">{positionName}</div>
          </>
        ) : (
          <>
            <div className="player-name">{isViewingOtherUser ? 'Empty' : `Select ${positionName}`}</div>
            <div className="player-position">{positionName}</div>
          </>
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
              {loading ? '🔄 Creating...' : '⚽ Create Selection'}
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
              {loading ? '🔄 Creating...' : '⚽ Create Selection'}
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
          onClick={() => navigate(`/pools/${poolId}`)}
        >
          <i className="bi bi-arrow-left me-2"></i>
          Back to Pool
        </button>

        <div className="header-buttons">
          {!isViewingOtherUser && !selection?.complete && isEditingAllowed() && (
            <button 
              className="btn btn-primary"
              onClick={handleCreateSelection}
              disabled={loading}
            >
              {loading ? '🔄 Creating...' : '⚽ Create Selection'}
            </button>
          )}

          {!isViewingOtherUser && selection?.complete && completionPercentage === 100 && !selection?.hasJoker && isEditingAllowed() && (
            <button 
              className="btn btn-warning"
              onClick={handleFinalizeSelection}
              disabled={loading}
            >
              {loading ? '🔄 Finalizing...' : '🚀 Finalize Selection'}
            </button>
          )}

          {selection?.complete && (
            <button className="btn btn-success" disabled>
              🏆 Team Complete! ({completionPercentage}%) 🏆
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

      {/* Selection Editing Status */}
      {!isViewingOtherUser && !isEditingAllowed() && (
        <div className="alert alert-warning">
          <i className="bi bi-lock-fill me-2"></i>
          Selection editing is currently disabled by the pool owner. You can view your selection but cannot make changes.
        </div>
      )}

      {/* Selection Status */}
      <div className="selection-status">
        <div className="status-info">
          <div className="status-item">
            <span className="status-label">Players</span>
            <span className="status-value">{(() => {
              const allSelectedPlayers = [
                selection!.selectedBasisGoalkeeper,
                ...selection!.selectedBasisDefenders,
                ...selection!.selectedBasisMidfielders,
                ...selection!.selectedBasisForwards,
                selection!.selectedReserveGoalkeeper,
                selection!.selectedReserveDefender,
                selection!.selectedReserveMidfielder,
                selection!.selectedReserveForward
              ].filter(player => player !== null && player !== undefined);
              return `${allSelectedPlayers.length}/15`;
            })()}</span>
          </div>
          <div className="status-item">
            <span className="status-label">Unique Clubs</span>
            <span className="status-value">{(() => {
              const allSelectedPlayers = [
                selection!.selectedBasisGoalkeeper,
                ...selection!.selectedBasisDefenders,
                ...selection!.selectedBasisMidfielders,
                ...selection!.selectedBasisForwards,
                selection!.selectedReserveGoalkeeper,
                selection!.selectedReserveDefender,
                selection!.selectedReserveMidfielder,
                selection!.selectedReserveForward
              ].filter(player => player !== null && player !== undefined);
              
              const uniqueClubs = new Set(
                allSelectedPlayers
                  .map(player => player?.player?.club)
                  .filter(club => club !== undefined && club !== null && club !== '')
              );
              return `${uniqueClubs.size}/15`;
            })()}</span>
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
          
          {/* Forwards */}
          <div className="position-row">
            {Array.from({ length: 3 }).map((_, idx) => (
              <React.Fragment key={`fwd-${idx}`}>
                {renderPlayerSlot('Forward', false, 1, selection!.selectedBasisForwards[idx], idx)}
              </React.Fragment>
            ))}
          </div>
          
          {/* Midfielders */}
          <div className="position-row">
            {Array.from({ length: 3 }).map((_, idx) => (
              <React.Fragment key={`mid-${idx}`}>
                {renderPlayerSlot('Midfielder', false, 1, selection!.selectedBasisMidfielders[idx], idx)}
              </React.Fragment>
            ))}
          </div>
          
          {/* Defenders */}
          <div className="position-row">
            {Array.from({ length: 4 }).map((_, idx) => (
              <React.Fragment key={`def-${idx}`}>
                {renderPlayerSlot('Defender', false, 1, selection!.selectedBasisDefenders[idx], idx)}
              </React.Fragment>
            ))}
          </div>
          
          {/* Goalkeeper */}
          <div className="position-row">
            {renderPlayerSlot('Goalkeeper', false, 1, selection!.selectedBasisGoalkeeper, 0)}
          </div>
          
          <div className="goal-area bottom"></div>
        </div>
      </div>
      
      {/* Reserves */}
      <h3 className="reserves-title">Reserves</h3>
      <div className="reserves-container">
        <div className="reserve-position">
          <span className="reserve-title">Goalkeeper</span>
          {renderPlayerSlot('Goalkeeper', true, 1, selection!.selectedReserveGoalkeeper, 0)}
        </div>
        
        <div className="reserve-position">
          <span className="reserve-title">Defender</span>
          {renderPlayerSlot('Defender', true, 1, selection!.selectedReserveDefender, 0)}
        </div>
        
        <div className="reserve-position">
          <span className="reserve-title">Midfielder</span>
          {renderPlayerSlot('Midfielder', true, 1, selection!.selectedReserveMidfielder, 0)}
        </div>
        
        <div className="reserve-position">
          <span className="reserve-title">Forward</span>
          {renderPlayerSlot('Forward', true, 1, selection!.selectedReserveForward, 0)}
        </div>
      </div>
      
      {/* Player Selection Modal */}
      {showPlayerModal && playerTableData && (
        <div className="player-selection-modal">
          <div className="modal-content" style={{ display: 'flex', flexDirection: 'column', height: '90vh' }}>
            <div className="modal-header">
              <h3 className="modal-title">
                {isReserve ? 'Reserve ' : ''}
                {currentPosition} Selection
              </h3>
              <button className="close-button" onClick={() => setShowPlayerModal(false)}>×</button>
            </div>
            
            <div className="selection-info">
              <p>
                Select a {currentPosition}.
                {!isReserve && ' Each player must be from a unique club.'}
              </p>
              
              {/* Search and Filter Controls */}
              <div className="filter-controls" style={{ display: 'flex', gap: '20px', alignItems: 'center', marginBottom: '15px', flexWrap: 'wrap' }}>
                {/* Search Filter */}
                <div className="search-filter">
                  <label htmlFor="searchFilter">Search Players:</label>
                  <input
                    id="searchFilter"
                    type="text"
                    placeholder="Enter player name..."
                    value={searchFilter}
                    onChange={(e) => setSearchFilter(e.target.value)}
                    style={{
                      padding: '8px 12px',
                      borderRadius: '6px',
                      border: '1px solid rgba(255, 255, 255, 0.3)',
                      background: 'rgba(255, 255, 255, 0.1)',
                      color: 'white',
                      marginLeft: '10px',
                      minWidth: '200px',
                      fontSize: '14px'
                    }}
                  />
                </div>
                
                {/* Club Filter */}
                <div className="club-filter">
                  <label htmlFor="clubFilter">Filter by Club:</label>
                  <select
                    id="clubFilter"
                    value={clubFilter}
                    onChange={(e) => setClubFilter(e.target.value)}
                    style={{
                      padding: '8px 12px',
                      borderRadius: '6px',
                      border: '1px solid rgba(255, 255, 255, 0.3)',
                      background: 'rgba(255, 255, 255, 0.1)',
                      color: 'white',
                      marginLeft: '10px',
                      minWidth: '200px'
                    }}
                  >
                    <option value="">All Clubs</option>
                    {availableClubs.map(club => (
                      <option key={club} value={club}>{club}</option>
                    ))}
                  </select>
                </div>
                
                {/* Unique Clubs Checkbox */}
                <div className="unique-clubs-filter" style={{ display: 'flex', alignItems: 'center' }}>
                  <input
                    id="uniqueClubsOnly"
                    type="checkbox"
                    checked={uniqueClubsOnly}
                    onChange={(e) => setUniqueClubsOnly(e.target.checked)}
                    style={{
                      marginRight: '8px',
                      transform: 'scale(1.2)'
                    }}
                  />
                  <label htmlFor="uniqueClubsOnly" style={{ color: 'white', fontSize: '14px' }}>
                    Unique clubs only
                  </label>
                </div>
              </div>
              
              <div className="selection-actions">
                <button 
                  className={`joker-toggle ${isJokerSelection ? 'active' : ''}`}
                  onClick={handleJokerMode}
                  disabled={selection!.hasJoker && !isPlayerJoker(selectedPlayers[0])}
                >
                  {isJokerSelection ? '★ Joker Mode Active ★' : '🎯 Select as Joker'}
                </button>
              </div>
            </div>
            
            {/* Scrollable Player Table */}
            <div style={{ flex: 1, overflow: 'auto', marginBottom: '10px' }}>
            
            <table className="player-table">
              <thead>
                <tr>
                  <th>Photo</th>
                  <th>Name</th>
                  <th>Club</th>
                  <th>Nationality</th>
                  <th>Actions</th>
                </tr>
              </thead>
              <tbody>
                {filteredPlayers.map((player: FootballPlayerDto, index: number) => {
                  const isSelected = selectedPlayers.includes(player.id);
                  const isJoker = jokerPlayer === player.id;
                  const alreadySelected = playerTableData.alreadySelectedPlayers.includes(player.id);
                  
                  return (
                    <tr 
                      key={player.id}
                      className={`player-row ${isSelected ? 'selected' : ''} ${isJoker ? 'joker' : ''}`}
                    >
                      <td>
                        <div className="player-photo-container">
                          {player.photoUrl ? (
                            <img
                              src={(() => {
                                if (player.photoUrl.startsWith('http') || player.photoUrl.startsWith('//')) {
                                  return player.photoUrl;
                                } else if (player.photoUrl.startsWith('/')) {
                                  return `${window.location.origin}${player.photoUrl}`;
                                } else {
                                  return `${window.location.origin}/${player.photoUrl}`;
                                }
                              })()}
                              alt={player.name}
                              className="player-photo"
                              onError={(e) => {
                                const target = e.target as HTMLImageElement;
                                target.style.display = 'none';
                                target.nextElementSibling?.classList.remove('d-none');
                              }}
                            />
                          ) : null}
                          <div className={`player-photo-placeholder ${player.photoUrl ? 'd-none' : ''}`}>
                            {player.name.charAt(0).toUpperCase()}
                          </div>
                        </div>
                      </td>
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
                                {isJoker ? '❌ Remove Joker' : '★ Set as Joker ★'}
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
            
            {/* Sticky Footer with Confirmation Buttons */}
            <div style={{
              position: 'sticky',
              bottom: 0,
              background: 'rgba(0, 0, 0, 0.9)',
              padding: '15px',
              borderTop: '1px solid rgba(255, 255, 255, 0.2)',
              display: 'flex',
              justifyContent: 'center',
              gap: '15px'
            }}>
              <button 
                className="submit-button"
                onClick={handleSubmitSelection}
                disabled={isJokerSelection && !jokerPlayer}
                style={{
                  padding: '12px 24px',
                  fontSize: '16px',
                  fontWeight: 'bold',
                  borderRadius: '8px',
                  border: 'none',
                  background: selectedPlayers.length > 0 ? '#28a745' : '#dc3545',
                  color: 'white',
                  cursor: selectedPlayers.length > 0 ? 'pointer' : 'not-allowed',
                  transition: 'all 0.3s ease'
                }}
              >
                {isJokerSelection && jokerPlayer ? '🎯 Confirm Joker Selection' : selectedPlayers.length > 0 ? '✅ Confirm Selection' : '🗑️ Remove Player'}
              </button>
            </div>
          </div>
        </div>
      )}
    </div>
  );
};

export default Selection;
