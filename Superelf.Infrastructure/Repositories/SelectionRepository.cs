using Microsoft.EntityFrameworkCore;
using Superelf.Application.Selection;
using Superelf.Domain.Entities;
using Superelf.Infrastructure.Data;

namespace Superelf.Infrastructure.Repositories;

public class SelectionRepository : ISelectionRepository {
    private readonly ApplicationDbContext _context;

    public SelectionRepository(ApplicationDbContext context) {
        _context = context;
    }

    public async Task<bool> IsParticipantInPoolAsync(Guid poolId, string userId) {
        return await _context.PoolParticipants
            .AnyAsync(pp => pp.Pool.Id == poolId && pp.ApplicationUser.Id == userId);
    }

    public async Task<Lineup?> GetLineupWithPlayersAsync(Guid poolId, string userId) {
        return await _context.Lineups
            .Include(l => l.PoolUser)
            .ThenInclude(pu => pu.ApplicationUser)
            .Include(l => l.PoolUser)
            .ThenInclude(pu => pu.Pool)
            .Include(l => l.LineupLines)
            .ThenInclude(ll => ll.FootballPlayer)
            .FirstOrDefaultAsync(l =>
                l.PoolUser.Pool.Id == poolId &&
                l.PoolUser.ApplicationUser.Id == userId);
    }
    
    public async Task<Lineup?> GetLineupByIdAsync(Guid lineupId) {
        return await _context.Lineups
            .Include(l => l.PoolUser)
            .ThenInclude(pu => pu.ApplicationUser)
            .Include(l => l.PoolUser)
            .ThenInclude(pu => pu.Pool)
            .Include(l => l.LineupLines)
            .ThenInclude(ll => ll.FootballPlayer)
            .FirstOrDefaultAsync(l => l.Id == lineupId);
    }

    public async Task<List<FootballPlayer>> GetPlayersByPositionAsync(string position) {
        return await _context.FootballPlayers
            .Where(p => p.Position == position)
            .ToListAsync();
    }

    public async Task AddLineupAsync(Lineup lineup) {
        if (lineup != null) {
            _context.Lineups.Add(lineup);
            await _context.SaveChangesAsync();
        }
    }

    public async Task UpdateLineupAsync(Lineup lineup) {
        if (lineup != null) {
            _context.Lineups.Update(lineup);
            await _context.SaveChangesAsync();
        }
    }

    public async Task RemoveLineupLinesAsync(IEnumerable<LineupLine> lines) {
        _context.LineupLines.RemoveRange(lines);
        await _context.SaveChangesAsync();
    }

    public async Task AddLineupLinesAsync(IEnumerable<LineupLine> lines) {
        _context.LineupLines.AddRange(lines);
        await _context.SaveChangesAsync();
    }

    public async Task<int> CountUniqueNationalitiesAsync(Guid lineupId) {
        return await _context.LineupLines
            .Where(ll => ll.Lineup != null && ll.Lineup.Id == lineupId)
            .Select(ll => ll.FootballPlayer.Nationality)
            .Distinct()
            .CountAsync();
    }

    public Task<PoolParticipant?> GetPoolParticipantAsync(Guid poolId, string userId) {
        return _context.PoolParticipants
            .Include(p => p.Pool)
            .Include(p => p.ApplicationUser)
            .Where(p => p.Pool.Id == poolId && p.ApplicationUser.Id == userId)
            .FirstOrDefaultAsync();
    }

    public List<FootballPlayer> GetPlayersByIds(List<Guid> selectedPlayers) {
        return _context.FootballPlayers.Where(p => selectedPlayers.Contains(p.Id)).ToList();
    }
}