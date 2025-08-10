import React from 'react';
import { PlayerScore } from '../../services/poolService';

interface PlayerScoresModalProps {
  isOpen: boolean;
  onClose: () => void;
  userName: string;
  playerScores: PlayerScore[];
}

const PlayerScoresModal: React.FC<PlayerScoresModalProps> = ({
  isOpen,
  onClose,
  userName,
  playerScores
}) => {
  if (!isOpen) return null;

  const totalScore = playerScores.reduce((sum, player) => sum + player.score, 0);
  const jokerPlayers = playerScores.filter(p => p.isJoker);
  const regularPlayers = playerScores.filter(p => !p.isJoker);

  return (
    <div className="modal fade show d-block" style={{ backgroundColor: 'rgba(0,0,0,0.5)' }}>
      <div className="modal-dialog modal-lg">
        <div className="modal-content">
          <div className="modal-header">
            <h5 className="modal-title">
              {userName}'s Team Details
            </h5>
            <button
              type="button"
              className="btn-close"
              onClick={onClose}
              aria-label="Close"
            ></button>
          </div>
          <div className="modal-body">
            <div className="row mb-3">
              <div className="col-md-6">
                <h6>Total Score: <span className={`fw-bold ${totalScore >= 0 ? 'text-success' : 'text-danger'}`}>
                  {totalScore >= 0 ? '+' : ''}{totalScore}
                </span></h6>
              </div>
              <div className="col-md-6 text-end">
                <span className="badge bg-primary">{playerScores.length} Players</span>
                {jokerPlayers.length > 0 && (
                  <span className="badge bg-warning ms-2">
                    <i className="bi bi-star-fill"></i> Joker Used
                  </span>
                )}
              </div>
            </div>

            <div className="table-responsive">
              <table className="table table-sm">
                <thead>
                  <tr>
                    <th>Position</th>
                    <th>Player</th>
                    <th>Club</th>
                    <th className="text-center">Score</th>
                    <th className="text-center">Joker</th>
                  </tr>
                </thead>
                <tbody>
                  {playerScores.map((player, index) => (
                    <tr key={player.playerId} className={player.score < 0 ? 'table-danger' : player.score > 0 ? 'table-success' : ''}>
                      <td>
                        <span className="badge bg-secondary">{player.position}</span>
                      </td>
                      <td>{player.playerName}</td>
                      <td>{player.club}</td>
                      <td className="text-center">
                        <span className={`fw-bold ${player.score >= 0 ? 'text-success' : 'text-danger'}`}>
                          {player.score >= 0 ? '+' : ''}{player.score}
                        </span>
                      </td>
                      <td className="text-center">
                        {player.isJoker && (
                          <i className="bi bi-star-fill text-warning"></i>
                        )}
                      </td>
                    </tr>
                  ))}
                </tbody>
              </table>
            </div>

            {/* Scoring System Reference */}
            <div className="mt-4">
              <h6>Scoring System:</h6>
              <div className="row">
                <div className="col-md-6">
                  <ul className="list-unstyled small">
                    <li><strong>Doelpunt middenvelder:</strong> 4 points</li>
                    <li><strong>Doelpunt aanvaller:</strong> 3 points</li>
                    <li><strong>Keeper houdt de 'nul':</strong> 4 points</li>
                    <li><strong>Verdediger houdt de 'nul':</strong> 2 points</li>
                    <li><strong>Assist:</strong> 2 points</li>
                    <li><strong>Gelijkspel:</strong> 1 point</li>
                  </ul>
                </div>
                <div className="col-md-6">
                  <ul className="list-unstyled small">
                    <li><strong>Speler scoort uit strafschop:</strong> 1 point</li>
                    <li><strong>Speler mist strafschop:</strong> -1 point</li>
                    <li><strong>Gele kaart:</strong> -1 point</li>
                    <li><strong>2x geel = rood:</strong> -3 points</li>
                    <li><strong>Eigendoelpunt:</strong> -4 points</li>
                    <li><strong>Rode kaart:</strong> -4 points</li>
                  </ul>
                </div>
              </div>
            </div>
          </div>
          <div className="modal-footer">
            <button
              type="button"
              className="btn btn-secondary"
              onClick={onClose}
            >
              Close
            </button>
          </div>
        </div>
      </div>
    </div>
  );
};

export default PlayerScoresModal;


