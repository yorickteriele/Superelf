using Superelf.Domain.Entities;

namespace Superelf.Application.Selection;

public interface ISelectionRepository {
    public Task<bool> IsParticipantInPoolAsync(Guid poolId, string userId);
    public Task<Lineup?> GetLineupWithPlayersAsync(Guid poolId, string userId);
    public Task<Lineup?> GetLineupByIdAsync(Guid lineupId);
    public Task<List<FootballPlayer>> GetPlayersByPositionAsync(string position);
    public Task AddLineupAsync(Lineup lineup);
    public Task UpdateLineupAsync(Lineup lineup);
    public Task RemoveLineupLinesAsync(IEnumerable<LineupLine> lines);
    public Task AddLineupLinesAsync(IEnumerable<LineupLine> lines);
    public Task<int> CountUniqueNationalitiesAsync(Guid lineupId);
    public Task<PoolParticipant?> GetPoolParticipantAsync(Guid poolId, string userId);
    public List<FootballPlayer> GetPlayersByIds(List<Guid> selectedPlayers);
}