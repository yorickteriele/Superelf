using Microsoft.EntityFrameworkCore;
using Superelf.Application.Pool;
using Superelf.Domain.Entities;
using Superelf.Infrastructure.Data;

namespace Superelf.Infrastructure.Repositories;

public class PoolRepository : IPoolRepository {
    private readonly ApplicationDbContext _context;

    public PoolRepository(ApplicationDbContext context) {
        _context = context;
    }

    public async Task<bool> CreatePoolAsync(Pool pool) {
        await _context.Pools.AddAsync(pool);
        return await SaveAsync();
    }

    public async Task<bool> EditPoolNameAsync(Pool pool) {
        _context.Pools.Update(pool);
        return await SaveAsync();
    }

    public async Task<Pool?> GetPoolByCodeAsync(string code) {
        return await _context.Pools.FirstOrDefaultAsync(p => p.Code == code);
    }

    public async Task<bool> AddParticipantToPoolAsync(Pool poolId, ApplicationUser userId) {
        var participant = new PoolParticipant {
            Pool = poolId,
            ApplicationUser = userId
        };
        await _context.PoolParticipants.AddAsync(participant);
        return await SaveAsync();
    }

    public async Task<bool> IsUserInPoolAsync(Pool pool, ApplicationUser user) {
        return await _context.PoolParticipants
            .AnyAsync(pp => pp.Pool.Id == pool.Id && pp.ApplicationUser.Id == user.Id);
    }

    public async Task<bool> SaveAsync() {
        return await _context.SaveChangesAsync() > 0;
    }

    public async Task<List<Pool>> GetUserPools(string userId) {
        return await _context.PoolParticipants
            .Include(p => p.ApplicationUser)
            .Where(pp => pp.ApplicationUser.Id == userId)
            .Include(p => p.Pool.Owner)
            .Select(pp => pp.Pool)
            .Distinct()
            .ToListAsync();
    }

    public async Task<int> GetParticipantCount(Guid poolId) {
        return await _context.PoolParticipants
            .CountAsync(pp => pp.Pool.Id == poolId);
    }

    public async Task<Pool?> GetPoolByIdAsync(Guid poolId) {
        return await _context.Pools
            .Include(p => p.Owner)
            .FirstOrDefaultAsync(p => p.Id == poolId);
    }

    public async Task<List<PoolParticipant>> GetPoolParticipantsAsync(Guid poolId) {
        return await _context.PoolParticipants
            .Include(pp => pp.ApplicationUser)
            .Where(pp => pp.Pool.Id == poolId)
            .ToListAsync();
    }

    public async Task<bool> RemoveParticipantFromPoolAsync(Guid poolId, string userId) {
        var participant = await _context.PoolParticipants
            .FirstOrDefaultAsync(pp => pp.Pool.Id == poolId && pp.ApplicationUser.Id == userId);

        if (participant == null) return false;

        _context.PoolParticipants.Remove(participant);
        return await SaveAsync();
    }

    public async Task<bool> DeletePoolAsync(Guid poolId) {
        var pool = await _context.Pools
            .Include(p => p.Participants)
            .FirstOrDefaultAsync(p => p.Id == poolId);
        if (pool == null) return false;
        var participantIds = pool.Participants.Select(pp => pp.Id).ToList();
        var lineups = await _context.Lineups
            .Where(l => participantIds.Contains(l.PoolUser.Id))
            .Include(l => l.LineupLines)
            .ToListAsync();
        foreach (var lineup in lineups) {
            _context.LineupLines.RemoveRange(lineup.LineupLines);
            _context.Lineups.Remove(lineup);
        }
        _context.PoolParticipants.RemoveRange(pool.Participants);
        _context.Pools.Remove(pool);
        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<List<Pool>> GetAllPoolsAsync() {
        return await _context.Pools.ToListAsync();
    }
}