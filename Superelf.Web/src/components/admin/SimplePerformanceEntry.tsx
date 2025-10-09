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
  const [isMobile, setIsMobile] = useState(window.innerWidth < 768);

  // Show/hide floating button based on scroll position
  useEffect(() => {
    const handleScroll = () => {
      const scrolled = window.scrollY > 100;
      setShowFloatingButton(scrolled);
    };
    
    const handleResize = () => {
      setIsMobile(window.innerWidth < 768);
    };
    
    window.addEventListener('scroll', handleScroll);
    window.addEventListener('resize', handleResize);
    return () => {
      window.removeEventListener('scroll', handleScroll);
      window.removeEventListener('resize', handleResize);
    };
  }, []);
  
  // Modern dark mode styling for the performance dialog
  const styles = {
    modalOverlay: {
      background: 'rgba(0, 0, 0, 0.8)',
      backdropFilter: 'blur(8px)',
    } as React.CSSProperties,
    modalContent: {
      background: '#0f172a',
      borderRadius: '20px',
      boxShadow: '0 25px 50px -12px rgba(0, 0, 0, 0.5)',
      border: '1px solid #1e293b',
      overflow: 'hidden',
      maxHeight: '90vh',
      display: 'flex',
      flexDirection: 'column',
    } as React.CSSProperties,
    modalHeader: {
      background: 'linear-gradient(135deg, #1e293b 0%, #334155 100%)',
      border: 'none',
      borderBottom: '1px solid #334155',
      padding: '1.5rem 2rem',
    } as React.CSSProperties,
    modalBody: {
      background: '#0f172a',
      padding: '0',
      overflowY: 'auto',
      flex: '1',
      minHeight: '0',
    } as React.CSSProperties,
    numberInput: {
      textAlign: 'center' as const,
      minWidth: '55px',
      maxWidth: '70px',
      background: '#1e293b',
      border: '1px solid #334155',
      borderRadius: '10px',
      color: '#f1f5f9',
      fontSize: '0.875rem',
      fontWeight: '500',
      transition: 'all 0.2s ease',
    } as React.CSSProperties,
    numberInputFocus: {
      borderColor: '#3b82f6',
      boxShadow: '0 0 0 3px rgba(59, 130, 246, 0.2)',
      background: '#1e293b',
    } as React.CSSProperties,
    teamCard: {
      background: '#1e293b',
      borderRadius: '16px',
      border: '1px solid #334155',
      boxShadow: '0 8px 25px -5px rgba(0, 0, 0, 0.3)',
      overflow: 'hidden',
    } as React.CSSProperties,
    teamHeader: {
      background: 'linear-gradient(135deg, #3b82f6 0%, #1d4ed8 100%)',
      border: 'none',
      padding: '1.25rem 1.5rem',
    } as React.CSSProperties,
    tableContainer: {
      background: '#1e293b',
      borderRadius: '16px',
      overflow: 'hidden',
      boxShadow: '0 8px 25px -5px rgba(0, 0, 0, 0.3)',
      border: '1px solid #334155',
    } as React.CSSProperties,
    tableResponsive: {
      overflowX: 'auto',
      WebkitOverflowScrolling: 'touch',
      scrollbarWidth: 'thin',
      scrollbarColor: '#4b5563 #1e293b',
    } as React.CSSProperties,
    mobileCard: {
      background: '#1e293b',
      borderRadius: '16px',
      border: '1px solid #334155',
      marginBottom: '1rem',
      padding: '1.5rem',
      boxShadow: '0 4px 15px rgba(0, 0, 0, 0.2)',
    } as React.CSSProperties,
    mobilePlayerHeader: {
      borderBottom: '1px solid #334155',
      paddingBottom: '1rem',
      marginBottom: '1.5rem',
    } as React.CSSProperties,
    mobileStatsGrid: {
      display: 'grid',
      gridTemplateColumns: 'repeat(2, 1fr)',
      gap: '1rem',
      marginBottom: '1.5rem',
    } as React.CSSProperties,
    mobileStatItem: {
      background: '#0f172a',
      borderRadius: '12px',
      padding: '1rem',
      textAlign: 'center',
      border: '1px solid #334155',
    } as React.CSSProperties,
    mobileStatLabel: {
      fontSize: '0.75rem',
      color: '#94a3b8',
      marginBottom: '0.5rem',
      fontWeight: '500',
    } as React.CSSProperties,
    mobileInput: {
      background: '#1e293b',
      border: '2px solid #334155',
      borderRadius: '8px',
      color: '#f1f5f9',
      fontSize: '1.125rem',
      fontWeight: '600',
      textAlign: 'center',
      width: '100%',
      padding: '0.75rem',
      minHeight: '48px',
    } as React.CSSProperties,
    mobileToggle: {
      display: 'flex',
      alignItems: 'center',
      justifyContent: 'space-between',
      background: '#0f172a',
      borderRadius: '12px',
      padding: '1rem 1.5rem',
      border: '1px solid #334155',
      marginBottom: '1rem',
    } as React.CSSProperties,
    mobileSwitch: {
      width: '60px',
      height: '34px',
      appearance: 'none',
      background: '#374151',
      borderRadius: '17px',
      position: 'relative',
      cursor: 'pointer',
      transition: 'all 0.3s ease',
    } as React.CSSProperties,
    desktopOnly: {
      display: 'block',
    } as React.CSSProperties,
    mobileOnly: {
      display: 'none',
    } as React.CSSProperties,
    desktopTableView: {
      display: 'block',
      '@media (max-width: 767px)': {
        display: 'none'
      }
    } as React.CSSProperties,
    mobileCardView: {
      display: 'none',
      '@media (max-width: 767px)': {
        display: 'block'
      }
    } as React.CSSProperties,
    tableHeader: {
      background: 'linear-gradient(135deg, #374151 0%, #4b5563 100%)',
      borderBottom: '1px solid #4b5563',
    } as React.CSSProperties,
    playerRow: {
      background: '#1e293b',
      transition: 'all 0.2s ease',
      borderBottom: '1px solid #334155',
    } as React.CSSProperties,
    playerRowHover: {
      background: '#334155',
      transform: 'scale(1.01)',
    } as React.CSSProperties,
    scoreInputGroup: {
      borderRadius: '12px',
      overflow: 'hidden',
      boxShadow: '0 4px 15px rgba(0, 0, 0, 0.2)',
    } as React.CSSProperties,
    scoreInput: {
      background: '#1e293b',
      border: '1px solid #334155',
      color: '#f1f5f9',
    } as React.CSSProperties,
    pointsBadge: {
      borderRadius: '10px',
      fontSize: '0.875rem',
      fontWeight: '600',
      padding: '0.5rem 0.75rem',
      minWidth: '50px',
      background: 'linear-gradient(135deg, #3b82f6 0%, #1d4ed8 100%)',
      color: 'white',
      border: 'none',
    } as React.CSSProperties,
    pointsBadgeInactive: {
      background: '#374151',
      color: '#9ca3af',
    } as React.CSSProperties,
    stickyNavbar: {
      position: 'sticky' as 'sticky',
      top: '0',
      zIndex: 100,
      marginBottom: '1.5rem',
      background: '#1e293b',
      borderRadius: '16px',
      boxShadow: '0 8px 25px -5px rgba(0, 0, 0, 0.3)',
      border: '1px solid #334155',
    } as React.CSSProperties,
    navLink: {
      color: '#94a3b8',
      borderRadius: '12px',
      transition: 'all 0.2s ease',
      border: 'none',
    } as React.CSSProperties,
    navLinkActive: {
      background: 'linear-gradient(135deg, #3b82f6 0%, #1d4ed8 100%)',
      color: 'white',
    } as React.CSSProperties,
    infoAlert: {
      background: 'linear-gradient(135deg, #1e3a8a 0%, #1e40af 100%)',
      border: '1px solid #3b82f6',
      borderRadius: '16px',
      color: '#bfdbfe',
    } as React.CSSProperties,
    mobileScroll: {
      WebkitOverflowScrolling: 'touch' as any,
      overscrollBehavior: 'contain',
      padding: '1rem',
      height: '100%',
    } as React.CSSProperties,
    button: {
      borderRadius: '12px',
      padding: '0.75rem 1.5rem',
      fontWeight: '600',
      fontSize: '0.875rem',
      transition: 'all 0.2s ease',
    } as React.CSSProperties,
    primaryButton: {
      background: 'linear-gradient(135deg, #3b82f6 0%, #1d4ed8 100%)',
      border: 'none',
      color: 'white',
    } as React.CSSProperties,
    secondaryButton: {
      background: 'transparent',
      border: '1px solid #4b5563',
      color: '#9ca3af',
    } as React.CSSProperties
  };
  const [loading, setLoading] = useState(false);
  const [matchScore, setMatchScore] = useState({
    homeScore: match.homeScore || 0,
    awayScore: match.awayScore || 0,
    isCompleted: match.isCompleted || false
  });
  const [updatedPerformances, setUpdatedPerformances] = useState<Set<string>>(new Set());
  const [players, setPlayers] = useState<FootballPlayer[]>([]);
  const [performances, setPerformances] = useState<PlayerPerformance[]>([]);
  const [playerEntries, setPlayerEntries] = useState<PlayerPerformanceEntry[]>([]);

  // Calculate fantasy points
  const calculateFantasyPoints = useCallback((perf: CreatePlayerPerformanceRequest, playerClub?: string): number => {
    // If player didn't play, they get 0 points regardless of team result
    if (!perf.played) return 0;
    
    let points = 0; // No base points
    
    // Regular goals (excluding penalty goals)
    const regularGoals = Math.max(0, (perf.goals || 0) - (perf.penaltyGoals || 0));
    points += regularGoals * 6; // 6 points per regular goal
    
    // Penalty goals
    points += (perf.penaltyGoals || 0) * 4; // 4 points per penalty goal
    
    // Other stats
    points -= (perf.penaltiesMissed || 0) * 2; // -2 points for missed penalties
    points -= (perf.ownGoals || 0) * 2; // -2 points for own goals
    points += (perf.assists || 0) * 4; // 4 points per assist
    points -= (perf.yellowCards || 0) * 1; // -1 point per yellow card
    points -= (perf.redCards || 0) * 3; // -3 points per red card
    
    // Win/Draw/Lose points (only if match is completed and player actually played)
    if (matchScore.isCompleted && playerClub) {
      const homeScore = matchScore.homeScore;
      const awayScore = matchScore.awayScore;
      
      if (homeScore === awayScore) {
        // Draw
        points += 1;
      } else if (
        (playerClub === match.homeTeam && homeScore > awayScore) ||
        (playerClub === match.awayTeam && awayScore > homeScore)
      ) {
        // Win
        points += 3;
      }
      // Lose = 0 points (no addition needed)
    }
    
    return Math.max(0, points);
  }, [matchScore, match]);

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
          calculatedPoints: calculateFantasyPoints(performance, player.club),
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
          
          // Mark this performance as updated if it's an existing one
          if (entry.existingPerformance) {
            setUpdatedPerformances(prev => new Set(prev).add(playerId));
          }
          
          return {
            ...entry,
            performance: updatedPerformance,
            calculatedPoints: calculateFantasyPoints(updatedPerformance, entry.player.club)
          };
        }
        return entry;
      })
    );
  };

  const updateMatchScore = async () => {
    try {
      await adminService.updateMatch(match.id, {
        homeScore: matchScore.homeScore,
        awayScore: matchScore.awayScore,
        isCompleted: matchScore.isCompleted,
        homeTeam: match.homeTeam,
        awayTeam: match.awayTeam,
        matchDate: match.matchDate,
        competition: match.competition,
        round: match.round
      });
      onSuccess('Match score updated successfully');
    } catch (err: any) {
      onError(err.message || 'Failed to update match score');
    }
  };

  const handleSaveAll = async () => {
    setLoading(true);
    try {
      // First save the match score if it's changed
      if (matchScore.homeScore !== match.homeScore || 
          matchScore.awayScore !== match.awayScore || 
          matchScore.isCompleted !== match.isCompleted) {
        await updateMatchScore();
      }

      // Save new performances
      const entriesToCreate = playerEntries.filter(entry => 
        entry.performance.played && !entry.existingPerformance
      );
      
      // Update existing performances that have been modified
      const entriesToUpdate = playerEntries.filter(entry => 
        entry.existingPerformance && updatedPerformances.has(entry.player.id)
      );
      
      let createCount = 0;
      let updateCount = 0;
      
      // Create new performances
      if (entriesToCreate.length > 0) {
        try {
          const performances = entriesToCreate.map(entry => entry.performance);
          await adminService.addBulkPlayerPerformances(match.id, performances);
          createCount = entriesToCreate.length;
        } catch (err) {
          // Fallback to individual creates
          for (const entry of entriesToCreate) {
            try {
              await adminService.addPlayerPerformance(match.id, entry.performance);
              createCount++;
            } catch (error) {
              console.error('Failed to create performance:', error);
            }
          }
        }
      }
      
      // Update existing performances
      for (const entry of entriesToUpdate) {
        if (entry.existingPerformance) {
          try {
            await adminService.updatePlayerPerformance(entry.existingPerformance.id, {
              goals: entry.performance.goals,
              penaltyGoals: entry.performance.penaltyGoals,
              penaltiesMissed: entry.performance.penaltiesMissed,
              ownGoals: entry.performance.ownGoals,
              assists: entry.performance.assists,
              yellowCards: entry.performance.yellowCards,
              redCards: entry.performance.redCards,
              played: entry.performance.played
            });
            updateCount++;
          } catch (error) {
            console.error('Failed to update performance:', error);
          }
        }
      }
      
      if (createCount === 0 && updateCount === 0) {
        onError('No changes to save');
        return;
      }
      
      const messages = [];
      if (createCount > 0) messages.push(`${createCount} new performances`);
      if (updateCount > 0) messages.push(`${updateCount} updated performances`);
      
      onSuccess(`Saved ${messages.join(' and ')}`);
      setUpdatedPerformances(new Set()); // Clear updated performances
      await loadData(); // Reload data
    } catch (err: any) {
      onError(err.message || 'Failed to save performances');
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
      <div className="modal show position-fixed top-0 start-0 w-100 h-100" tabIndex={-1} style={{zIndex: 1050, ...styles.modalOverlay, display: 'flex', alignItems: 'center', justifyContent: 'center', padding: '1rem'}}>
        <div className="modal-dialog" style={{maxWidth: '500px', width: '100%', margin: '0'}}>
          <div className="modal-content" style={styles.modalContent}>
            <div className="modal-header" style={styles.modalHeader}>
              <h5 className="modal-title text-white">Loading...</h5>
            </div>
            <div className="modal-body" style={styles.modalBody}>
              <div className="d-flex justify-content-center">
                <div className="spinner-border text-primary" role="status">
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
    <div className="modal show position-fixed top-0 start-0 w-100 h-100" tabIndex={-1} style={{zIndex: 1050, ...styles.modalOverlay, display: 'flex', alignItems: 'center', justifyContent: 'center', padding: '1rem'}}>
      <div className="modal-dialog" style={{maxWidth: '1200px', maxHeight: '90vh', width: '100%', margin: '0'}}>
        <div className="modal-content" style={styles.modalContent}>
          <div className="modal-header text-white" style={styles.modalHeader}>
            <div className="d-flex flex-column w-100">
              <div className="d-flex justify-content-between align-items-start w-100">
                <h5 className="modal-title mb-0">
                  <i className="fas fa-futbol me-2"></i>
                  Performance Entry - {match.homeTeam} vs {match.awayTeam}
                </h5>
                <button type="button" className="btn-close btn-close-white" onClick={onClose}></button>
              </div>
              <div className="d-flex align-items-center gap-2 mt-2">
                <small className="text-light me-2">Match Score:</small>
                <div className="input-group input-group-sm" style={{ maxWidth: '320px', ...styles.scoreInputGroup }}>
                  <input
                    type="number"
                    className="form-control form-control-sm text-center"
                    style={styles.scoreInput}
                    value={matchScore.homeScore}
                    min="0"
                    onChange={(e) => setMatchScore({...matchScore, homeScore: parseInt(e.target.value) || 0})}
                    placeholder="Home"
                  />
                  <span className="input-group-text" style={{background: '#334155', border: '1px solid #334155', color: '#f1f5f9'}}>-</span>
                  <input
                    type="number"
                    className="form-control form-control-sm text-center"
                    style={styles.scoreInput}
                    value={matchScore.awayScore}
                    min="0"
                    onChange={(e) => setMatchScore({...matchScore, awayScore: parseInt(e.target.value) || 0})}
                    placeholder="Away"
                  />
                  <div className="input-group-text" style={{background: '#334155', border: '1px solid #334155'}}>
                    <div className="form-check form-switch mb-0">
                      <input
                        className="form-check-input"
                        type="checkbox"
                        checked={matchScore.isCompleted}
                        onChange={(e) => setMatchScore({...matchScore, isCompleted: e.target.checked})}
                        id="matchCompletedSwitch"
                      />
                      <label className="form-check-label small ms-1 text-nowrap" htmlFor="matchCompletedSwitch">
                        {matchScore.isCompleted ? 'Final' : 'Live'}
                      </label>
                    </div>
                  </div>
                </div>
              </div>
            </div>
          </div>
          <div className="modal-body" style={{...styles.modalBody, ...styles.mobileScroll}}>
            <nav className="nav nav-pills nav-justified p-3 sticky-top" style={styles.stickyNavbar}>
              {allGroupedPlayers.map((group, index) => (
                <a 
                  key={index} 
                  className="nav-link" 
                  style={styles.navLink}
                  href={`#team-${index}`}
                  onClick={(e) => {
                    e.preventDefault();
                    document.getElementById(`team-${index}`)?.scrollIntoView({ behavior: 'smooth' });
                  }}
                  onMouseEnter={(e) => {
                    e.currentTarget.style.background = 'linear-gradient(135deg, #3b82f6 0%, #1d4ed8 100%)';
                    e.currentTarget.style.color = 'white';
                  }}
                  onMouseLeave={(e) => {
                    e.currentTarget.style.background = 'transparent';
                    e.currentTarget.style.color = '#94a3b8';
                  }}
                >
                  {group.title} ({group.players.length})
                </a>
              ))}
            </nav>
            <div className="alert py-3 mb-4 d-flex align-items-center" style={styles.infoAlert}>
              <i className="fas fa-info-circle me-2 fa-lg"></i>
              <div className="small">
                Enter performance data for players. New performances will be created, existing ones will be updated.
                <div className="mt-1 d-flex">
                  <span className="badge bg-success me-2">■</span> Existing data (editable)
                  <span className="badge bg-info mx-2">■</span> New data
                  <span className="badge bg-warning mx-2">■</span> Modified data
                </div>
              </div>
            </div>

            {allGroupedPlayers.map((group, groupIndex) => (
              <div id={`team-${groupIndex}`} key={groupIndex} className="mb-4">
                <div className="card mb-4" style={styles.teamCard}>
                  <div className="card-header text-white" style={styles.teamHeader}>
                    <h5 className="mb-0">
                      <i className="fas fa-users me-2"></i>
                      {group.title} ({group.players.length} players)
                    </h5>
                  </div>
                </div>
                
                {/* Desktop Table View */}
                <div style={{...styles.tableContainer, display: !isMobile ? 'block' : 'none'}}>
                  <div className="table-responsive" style={styles.tableResponsive}>
                    <table className="table table-sm table-hover" style={{minWidth: '800px', marginBottom: '0'}}>
                    <thead className="sticky-top" style={styles.tableHeader}>
                      <tr>
                        <th style={{ minWidth: '160px', color: 'white', fontWeight: '600', padding: '0.75rem 0.5rem', fontSize: '0.875rem' }}>Player</th>
                        <th style={{ minWidth: '60px', color: 'white', fontWeight: '600', padding: '0.75rem 0.5rem', textAlign: 'center', fontSize: '0.875rem' }}>Goals</th>
                        <th style={{ minWidth: '65px', color: 'white', fontWeight: '600', padding: '0.75rem 0.5rem', textAlign: 'center', fontSize: '0.875rem' }}>Pen G</th>
                        <th style={{ minWidth: '65px', color: 'white', fontWeight: '600', padding: '0.75rem 0.5rem', textAlign: 'center', fontSize: '0.875rem' }}>Miss P</th>
                        <th style={{ minWidth: '60px', color: 'white', fontWeight: '600', padding: '0.75rem 0.5rem', textAlign: 'center', fontSize: '0.875rem' }}>Own G</th>
                        <th style={{ minWidth: '60px', color: 'white', fontWeight: '600', padding: '0.75rem 0.5rem', textAlign: 'center', fontSize: '0.875rem' }}>Assists</th>
                        <th style={{ minWidth: '60px', color: 'white', fontWeight: '600', padding: '0.75rem 0.5rem', textAlign: 'center', fontSize: '0.875rem' }}>Yellow</th>
                        <th style={{ minWidth: '55px', color: 'white', fontWeight: '600', padding: '0.75rem 0.5rem', textAlign: 'center', fontSize: '0.875rem' }}>Red</th>
                        <th style={{ minWidth: '65px', color: 'white', fontWeight: '600', padding: '0.75rem 0.5rem', textAlign: 'center', fontSize: '0.875rem' }}>Played</th>
                        <th style={{ minWidth: '60px', color: 'white', fontWeight: '600', padding: '0.75rem 0.5rem', textAlign: 'center', fontSize: '0.875rem' }}>Points</th>
                      </tr>
                    </thead>
                    <tbody>
                      {group.players.map(entry => (
                        <tr 
                          key={entry.player.id}
                          style={{
                            ...styles.playerRow,
                            background: entry.existingPerformance && updatedPerformances.has(entry.player.id) ? '#fbbf24' :
                                       entry.existingPerformance ? '#10b981' : 
                                       entry.performance.played ? '#3b82f6' : '#1e293b',
                            color: '#f1f5f9'
                          }}
                          onMouseEnter={(e) => {
                            e.currentTarget.style.background = '#334155';
                            e.currentTarget.style.transform = 'scale(1.01)';
                          }}
                          onMouseLeave={(e) => {
                            const bgColor = entry.existingPerformance && updatedPerformances.has(entry.player.id) ? '#fbbf24' :
                                           entry.existingPerformance ? '#10b981' : 
                                           entry.performance.played ? '#3b82f6' : '#1e293b';
                            e.currentTarget.style.background = bgColor;
                            e.currentTarget.style.transform = 'scale(1)';
                          }}
                        >
                          <td className="ps-2" style={{padding: '0.75rem 0.5rem', color: '#f1f5f9', minWidth: '160px'}}>
                            <div className="d-flex flex-column">
                              <strong className="text-nowrap text-truncate" style={{ maxWidth: '140px', fontSize: '0.875rem' }}>
                                {entry.player.name}
                              </strong>
                              <small className="text-muted text-nowrap" style={{color: '#94a3b8', fontSize: '0.75rem'}}>
                                {entry.player.position} • {entry.player.club || 'No Club'}
                              </small>
                            </div>
                          </td>
                          <td className="text-center" style={{padding: '1rem 0.5rem'}}>
                            <input
                              type="number"
                              className="form-control form-control-sm"
                              min="0"
                              style={{...styles.numberInput, minWidth: '45px', fontSize: '0.8rem'}}
                              value={entry.performance.goals}
                              onChange={(e) => updatePlayerEntry(entry.player.id, 'goals', parseInt(e.target.value) || 0)}
                              onFocus={(e) => {
                                e.target.style.borderColor = '#3b82f6';
                                e.target.style.boxShadow = '0 0 0 3px rgba(59, 130, 246, 0.2)';
                              }}
                              onBlur={(e) => {
                                e.target.style.borderColor = '#334155';
                                e.target.style.boxShadow = 'none';
                              }}
                            />
                          </td>
                          <td className="text-center" style={{padding: '1rem 0.5rem'}}>
                            <input
                              type="number"
                              className="form-control form-control-sm"
                              min="0"
                              style={styles.numberInput}
                              value={entry.performance.penaltyGoals}
                              onChange={(e) => updatePlayerEntry(entry.player.id, 'penaltyGoals', parseInt(e.target.value) || 0)}
                              onFocus={(e) => {
                                e.target.style.borderColor = '#3b82f6';
                                e.target.style.boxShadow = '0 0 0 3px rgba(59, 130, 246, 0.2)';
                              }}
                              onBlur={(e) => {
                                e.target.style.borderColor = '#334155';
                                e.target.style.boxShadow = 'none';
                              }}
                            />
                          </td>
                          <td className="text-center" style={{padding: '1rem 0.5rem'}}>
                            <input
                              type="number"
                              className="form-control form-control-sm"
                              min="0"
                              style={styles.numberInput}
                              value={entry.performance.penaltiesMissed}
                              onChange={(e) => updatePlayerEntry(entry.player.id, 'penaltiesMissed', parseInt(e.target.value) || 0)}
                              onFocus={(e) => {
                                e.target.style.borderColor = '#3b82f6';
                                e.target.style.boxShadow = '0 0 0 3px rgba(59, 130, 246, 0.2)';
                              }}
                              onBlur={(e) => {
                                e.target.style.borderColor = '#334155';
                                e.target.style.boxShadow = 'none';
                              }}
                            />
                          </td>
                          <td className="text-center" style={{padding: '1rem 0.5rem'}}>
                            <input
                              type="number"
                              className="form-control form-control-sm"
                              min="0"
                              style={styles.numberInput}
                              value={entry.performance.ownGoals}
                              onChange={(e) => updatePlayerEntry(entry.player.id, 'ownGoals', parseInt(e.target.value) || 0)}
                              onFocus={(e) => {
                                e.target.style.borderColor = '#3b82f6';
                                e.target.style.boxShadow = '0 0 0 3px rgba(59, 130, 246, 0.2)';
                              }}
                              onBlur={(e) => {
                                e.target.style.borderColor = '#334155';
                                e.target.style.boxShadow = 'none';
                              }}
                            />
                          </td>
                          <td className="text-center" style={{padding: '1rem 0.5rem'}}>
                            <input
                              type="number"
                              className="form-control form-control-sm"
                              min="0"
                              style={styles.numberInput}
                              value={entry.performance.assists}
                              onChange={(e) => updatePlayerEntry(entry.player.id, 'assists', parseInt(e.target.value) || 0)}
                              onFocus={(e) => {
                                e.target.style.borderColor = '#3b82f6';
                                e.target.style.boxShadow = '0 0 0 3px rgba(59, 130, 246, 0.2)';
                              }}
                              onBlur={(e) => {
                                e.target.style.borderColor = '#334155';
                                e.target.style.boxShadow = 'none';
                              }}
                            />
                          </td>
                          <td className="text-center" style={{padding: '1rem 0.5rem'}}>
                            <input
                              type="number"
                              className="form-control form-control-sm"
                              min="0"
                              max="2"
                              style={styles.numberInput}
                              value={entry.performance.yellowCards}
                              onChange={(e) => updatePlayerEntry(entry.player.id, 'yellowCards', parseInt(e.target.value) || 0)}
                              onFocus={(e) => {
                                e.target.style.borderColor = '#3b82f6';
                                e.target.style.boxShadow = '0 0 0 3px rgba(59, 130, 246, 0.2)';
                              }}
                              onBlur={(e) => {
                                e.target.style.borderColor = '#334155';
                                e.target.style.boxShadow = 'none';
                              }}
                            />
                          </td>
                          <td className="text-center" style={{padding: '1rem 0.5rem'}}>
                            <input
                              type="number"
                              className="form-control form-control-sm"
                              min="0"
                              max="1"
                              style={styles.numberInput}
                              value={entry.performance.redCards}
                              onChange={(e) => updatePlayerEntry(entry.player.id, 'redCards', parseInt(e.target.value) || 0)}
                              onFocus={(e) => {
                                e.target.style.borderColor = '#3b82f6';
                                e.target.style.boxShadow = '0 0 0 3px rgba(59, 130, 246, 0.2)';
                              }}
                              onBlur={(e) => {
                                e.target.style.borderColor = '#334155';
                                e.target.style.boxShadow = 'none';
                              }}
                            />
                          </td>
                          <td className="text-center" style={{padding: '0.75rem 0.25rem'}}>
                            <div className="form-check d-flex justify-content-center">
                              <input
                                className="form-check-input"
                                type="checkbox"
                                checked={entry.performance.played}
                                onChange={(e) => updatePlayerEntry(entry.player.id, 'played', e.target.checked)}
                                style={{ width: '18px', height: '18px', accentColor: '#3b82f6' }}
                              />
                            </div>
                          </td>
                          <td className="text-center" style={{padding: '0.75rem 0.25rem'}}>
                            <span 
                              style={{
                                ...styles.pointsBadge,
                                ...(entry.performance.played ? {} : styles.pointsBadgeInactive)
                              }}
                            >
                              {entry.calculatedPoints}
                            </span>
                          </td>
                        </tr>
                      ))}
                    </tbody>
                    </table>
                  </div>
                </div>
                
                {/* Mobile Card View */}
                <div style={{display: isMobile ? 'block' : 'none'}}>
                  {group.players.map(entry => (
                    <div key={entry.player.id} style={{
                      ...styles.mobileCard,
                      border: entry.existingPerformance && updatedPerformances.has(entry.player.id) ? '2px solid #fbbf24' :
                              entry.existingPerformance ? '2px solid #10b981' : 
                              entry.performance.played ? '2px solid #3b82f6' : '1px solid #334155'
                    }}>
                      {/* Player Header */}
                      <div style={styles.mobilePlayerHeader}>
                        <h6 className="mb-1" style={{color: '#f1f5f9', fontSize: '1.125rem', fontWeight: '600'}}>
                          {entry.player.name}
                        </h6>
                        <div style={{color: '#94a3b8', fontSize: '0.875rem'}}>
                          {entry.player.position} • {entry.player.club || 'No Club'}
                        </div>
                        <div className="mt-2">
                          <span style={{
                            ...styles.pointsBadge,
                            ...(entry.performance.played ? {} : styles.pointsBadgeInactive),
                            fontSize: '1rem',
                            padding: '0.5rem 1rem'
                          }}>
                            {entry.calculatedPoints} points
                          </span>
                        </div>
                      </div>
                      
                      {/* Player Status Toggle */}
                      <div style={styles.mobileToggle}>
                        <span style={{color: '#f1f5f9', fontSize: '1rem', fontWeight: '500'}}>
                          Player participated
                        </span>
                        <label style={{position: 'relative', display: 'inline-block'}}>
                          <input
                            type="checkbox"
                            checked={entry.performance.played}
                            onChange={(e) => updatePlayerEntry(entry.player.id, 'played', e.target.checked)}
                            style={{
                              ...styles.mobileSwitch,
                              background: entry.performance.played ? '#3b82f6' : '#374151'
                            }}
                          />
                          <span style={{
                            position: 'absolute',
                            top: '3px',
                            left: entry.performance.played ? '29px' : '3px',
                            width: '28px',
                            height: '28px',
                            background: 'white',
                            borderRadius: '50%',
                            transition: 'all 0.3s ease',
                            pointerEvents: 'none'
                          }} />
                        </label>
                      </div>
                      
                      {/* Stats Grid */}
                      <div style={styles.mobileStatsGrid}>
                        <div style={styles.mobileStatItem}>
                          <div style={styles.mobileStatLabel}>Goals</div>
                          <input
                            type="number"
                            min="0"
                            value={entry.performance.goals}
                            onChange={(e) => updatePlayerEntry(entry.player.id, 'goals', parseInt(e.target.value) || 0)}
                            style={styles.mobileInput}
                          />
                        </div>
                        <div style={styles.mobileStatItem}>
                          <div style={styles.mobileStatLabel}>Penalty Goals</div>
                          <input
                            type="number"
                            min="0"
                            value={entry.performance.penaltyGoals}
                            onChange={(e) => updatePlayerEntry(entry.player.id, 'penaltyGoals', parseInt(e.target.value) || 0)}
                            style={styles.mobileInput}
                          />
                        </div>
                        <div style={styles.mobileStatItem}>
                          <div style={styles.mobileStatLabel}>Penalties Missed</div>
                          <input
                            type="number"
                            min="0"
                            value={entry.performance.penaltiesMissed}
                            onChange={(e) => updatePlayerEntry(entry.player.id, 'penaltiesMissed', parseInt(e.target.value) || 0)}
                            style={styles.mobileInput}
                          />
                        </div>
                        <div style={styles.mobileStatItem}>
                          <div style={styles.mobileStatLabel}>Own Goals</div>
                          <input
                            type="number"
                            min="0"
                            value={entry.performance.ownGoals}
                            onChange={(e) => updatePlayerEntry(entry.player.id, 'ownGoals', parseInt(e.target.value) || 0)}
                            style={styles.mobileInput}
                          />
                        </div>
                        <div style={styles.mobileStatItem}>
                          <div style={styles.mobileStatLabel}>Assists</div>
                          <input
                            type="number"
                            min="0"
                            value={entry.performance.assists}
                            onChange={(e) => updatePlayerEntry(entry.player.id, 'assists', parseInt(e.target.value) || 0)}
                            style={styles.mobileInput}
                          />
                        </div>
                        <div style={styles.mobileStatItem}>
                          <div style={styles.mobileStatLabel}>Yellow Cards</div>
                          <input
                            type="number"
                            min="0"
                            max="2"
                            value={entry.performance.yellowCards}
                            onChange={(e) => updatePlayerEntry(entry.player.id, 'yellowCards', parseInt(e.target.value) || 0)}
                            style={styles.mobileInput}
                          />
                        </div>
                      </div>
                      
                      {/* Red Card - Full Width */}
                      <div style={{...styles.mobileStatItem, marginBottom: '0'}}>
                        <div style={styles.mobileStatLabel}>Red Cards</div>
                        <input
                          type="number"
                          min="0"
                          max="1"
                          value={entry.performance.redCards}
                          onChange={(e) => updatePlayerEntry(entry.player.id, 'redCards', parseInt(e.target.value) || 0)}
                          style={styles.mobileInput}
                        />
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            ))}
            
            {/* Enhanced floating save button for mobile */}
            {showFloatingButton && isMobile && (
              <div 
                className="position-fixed" 
                style={{
                  bottom: '20px', 
                  left: '50%',
                  transform: 'translateX(-50%)',
                  zIndex: 1060
                }}
              >
                <button 
                  className="btn btn-lg shadow"
                  style={{ 
                    background: 'linear-gradient(135deg, #3b82f6 0%, #1d4ed8 100%)',
                    border: 'none',
                    color: 'white',
                    borderRadius: '25px',
                    padding: '1rem 2rem',
                    fontSize: '1.125rem',
                    fontWeight: '600',
                    minWidth: '200px'
                  }} 
                  onClick={handleSaveAll}
                  disabled={loading}
                >
                  {loading ? (
                    <>
                      <span className="spinner-border spinner-border-sm me-2"></span>
                      Saving...
                    </>
                  ) : (
                    <>
                      <i className="fas fa-save me-2"></i>
                      Save All Changes
                    </>
                  )}
                </button>
              </div>
            )}
          </div>
          <div className="modal-footer d-flex justify-content-between" style={{padding: '1.5rem 2rem', borderTop: '1px solid #334155', background: '#1e293b', borderRadius: '0 0 20px 20px'}}>
            <button 
              className="btn"
              onClick={onClose}
              style={{...styles.button, ...styles.secondaryButton}}
              onMouseEnter={(e) => {
                e.currentTarget.style.background = '#374151';
                e.currentTarget.style.color = '#f1f5f9';
              }}
              onMouseLeave={(e) => {
                e.currentTarget.style.background = 'transparent';
                e.currentTarget.style.color = '#9ca3af';
              }}
            >
              <i className="fas fa-times me-1"></i> Close
            </button>
            <button 
              className="btn"
              onClick={handleSaveAll}
              disabled={loading}
              style={{
                ...styles.button, 
                ...styles.primaryButton,
                opacity: loading ? 0.7 : 1,
                cursor: loading ? 'not-allowed' : 'pointer'
              }}
              onMouseEnter={(e) => {
                if (!loading) {
                  e.currentTarget.style.transform = 'translateY(-2px)';
                  e.currentTarget.style.boxShadow = '0 10px 25px -5px rgba(59, 130, 246, 0.4)';
                }
              }}
              onMouseLeave={(e) => {
                if (!loading) {
                  e.currentTarget.style.transform = 'translateY(0)';
                  e.currentTarget.style.boxShadow = 'none';
                }
              }}
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
