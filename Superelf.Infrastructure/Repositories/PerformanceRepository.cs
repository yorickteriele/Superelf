using Microsoft.EntityFrameworkCore;
using Superelf.Application.Performance;
using Superelf.Domain.Entities;
using Superelf.Infrastructure.Data;

namespace Superelf.Infrastructure.Repositories;

public class PerformanceRepository : IPerformanceRepository
{
    private readonly ApplicationDbContext _context;

    public PerformanceRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<PlayerPerformance>> GetCompletedPerformancesForPlayerAsync(Guid playerId)
    {
        return await _context.PlayerPerformances
            .Include(pp => pp.Match)
            .Include(pp => pp.Player)
            .Where(pp => pp.Player.Id == playerId && pp.Match.IsCompleted)
            .ToListAsync();
    }
}




