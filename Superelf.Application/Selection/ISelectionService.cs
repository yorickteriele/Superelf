using Superelf.Domain.Entities;

namespace Superelf.Application.Selection;

public interface ISelectionService
{
    Task<Lineup> GetOrCreateLineupAsync(Guid poolId, string userId);
    Task SubmitSelectionAsync(Guid poolId, string userId, string position, bool isReserve, List<Guid> selectedPlayers, bool isJoker = false, int? slotIndex = null);
    Task SetJokerAsync(Guid poolId, string userId, Guid playerId);
    Task<LineupStatistics> GetLineupStatisticsAsync(Guid lineupId);
    Task<List<FootballPlayer>> GetFootballPlayersByPosition(string position);
}