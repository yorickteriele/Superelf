using Superelf.Domain.Entities;
using Superelf.Application.Selection;
using Superelf.Application.Performance;

namespace Superelf.Application.Pool;

public interface IScoringService
{
    Task<List<UserScoreDto>> CalculatePoolScoresAsync(Guid poolId);
    Task<UserScoreDto?> CalculateUserScoreAsync(Guid poolId, string userId);
    Task<int> CalculatePlayerScoreAsync(FootballPlayer player, List<PlayerPerformance> performances);
}

public class ScoringService : IScoringService
{
    private readonly ISelectionRepository _selectionRepository;
    private readonly IPoolRepository _poolRepository;
    private readonly IPerformanceRepository _performanceRepository;

    public ScoringService(
        ISelectionRepository selectionRepository,
        IPoolRepository poolRepository,
        IPerformanceRepository performanceRepository)
    {
        _selectionRepository = selectionRepository;
        _poolRepository = poolRepository;
        _performanceRepository = performanceRepository;
    }

    public async Task<List<UserScoreDto>> CalculatePoolScoresAsync(Guid poolId)
    {
        var participants = await _poolRepository.GetPoolParticipantsAsync(poolId);
        var scores = new List<UserScoreDto>();

        foreach (var participant in participants)
        {
            var score = await CalculateUserScoreAsync(poolId, participant.ApplicationUser.Id);
            if (score != null)
            {
                scores.Add(score);
            }
        }

        return scores.OrderByDescending(s => s.Score).ToList();
    }

    public async Task<UserScoreDto?> CalculateUserScoreAsync(Guid poolId, string userId)
    {
        var lineup = await _selectionRepository.GetLineupWithPlayersAsync(poolId, userId);
        if (lineup == null || !lineup.Complete)
        {
            return null;
        }

        var totalScore = 0;
        var playerScores = new List<PlayerScoreDto>();

        foreach (var lineupLine in lineup.LineupLines)
        {
            var player = lineupLine.FootballPlayer;
            var performances = await _performanceRepository.GetCompletedPerformancesForPlayerAsync(player.Id);
            var playerScore = await CalculatePlayerScoreAsync(player, performances);
            
            playerScores.Add(new PlayerScoreDto
            {
                PlayerId = player.Id,
                PlayerName = player.Name,
                Position = player.Position,
                Club = player.Club ?? "",
                Score = playerScore,
                IsJoker = lineupLine.IsJoker
            });
            
            totalScore += playerScore;
        }

        return new UserScoreDto
        {
            UserId = userId,
            UserName = lineup.PoolUser.ApplicationUser.UserName ?? "Unknown",
            Score = totalScore,
            PlayerScores = playerScores,
            HasUsedJoker = playerScores.Any(p => p.IsJoker)
        };
    }

    public async Task<int> CalculatePlayerScoreAsync(FootballPlayer player, List<PlayerPerformance> performances)
    {
        if (!performances.Any())
            return 0;

        var totalScore = 0;

        foreach (var performance in performances)
        {
            var score = CalculatePerformanceScore(player, performance);
            totalScore += score;
        }

        return totalScore;
    }

    private int CalculatePerformanceScore(FootballPlayer player, PlayerPerformance performance)
    {
        if (!performance.Played)
            return 0;

        var points = 0;

        // Dutch scoring system
        // Doelpunt middenvelder (Midfielder goal) - 4 points
        // Doelpunt aanvaller (Forward goal) - 3 points
        // Keeper houdt de 'nul' (Goalkeeper clean sheet) - 4 points
        // Verdediger houdt de 'nul' (Defender clean sheet) - 2 points
        // Keeper stopt strafschop (Goalkeeper saves penalty) - 3 points
        // Gewonnen wedstrijd (Won match) - 3 points
        // Gelijkspel (Draw) - 1 point
        // Assist geven waar uit wordt gescoord (Assist) - 2 points
        // Speler scoort uit strafschop (Player scores from penalty) - 1 point
        // Speler mist strafschop (Player misses penalty) - -1 point
        // Gele kaart (Yellow card) - -1 point
        // 2x geel = rood (2 yellow = red) - -3 points
        // Eigendoelpunt (Own goal) - -4 points
        // Rode kaart (Red card) - -4 points

        // Goal points based on position
        var regularGoals = performance.Goals - performance.PenaltyGoals;
        if (player.Position.ToLower() == "midfielder")
        {
            points += regularGoals * 4; // Midfielder goals
        }
        else
        {
            points += regularGoals * 3; // Forward goals
        }

        // Penalty goals
        points += performance.PenaltyGoals * 1;

        // Missed penalties
        points -= performance.PenaltiesMissed * 1;

        // Own goals
        points -= performance.OwnGoals * 4;

        // Assists
        points += performance.Assists * 2;

        // Cards
        points -= performance.YellowCards * 1;
        points -= performance.RedCards * 4;

        // Clean sheet bonuses (simplified - would need match result data)
        // For now, we'll skip clean sheet and match result bonuses
        // as they require additional match data

        return points;
    }

    // Performance data retrieval is handled via repository to keep Application layer clean
}

public class UserScoreDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public int Score { get; set; }
    public List<PlayerScoreDto> PlayerScores { get; set; } = new();
    public bool HasUsedJoker { get; set; }
}

public class PlayerScoreDto
{
    public Guid PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string Club { get; set; } = string.Empty;
    public int Score { get; set; }
    public bool IsJoker { get; set; }
}
