using Microsoft.EntityFrameworkCore;
using Superelf.Application.League;
using Superelf.Domain.Entities;
using Superelf.Infrastructure.Data;

namespace Superelf.Infrastructure.Repositories;

public class LeagueRepository : ILeagueRepository
{
    private readonly ApplicationDbContext _context;

    public LeagueRepository(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<Domain.Entities.League>> GetAllAsync()
    {
        return await _context.Leagues
            .Include(l => l.Clubs)
            .Include(l => l.Matches)
            .OrderBy(l => l.Name)
            .ToListAsync();
    }

    public async Task<Domain.Entities.League?> GetByIdAsync(Guid id)
    {
        return await _context.Leagues
            .Include(l => l.Clubs)
            .Include(l => l.Matches)
            .FirstOrDefaultAsync(l => l.Id == id);
    }

    public async Task<Domain.Entities.League?> GetByNameAsync(string name)
    {
        return await _context.Leagues
            .Include(l => l.Clubs)
            .Include(l => l.Matches)
            .FirstOrDefaultAsync(l => l.Name.ToLower() == name.ToLower());
    }

    public async Task<Domain.Entities.League> CreateAsync(Domain.Entities.League league)
    {
        _context.Leagues.Add(league);
        await _context.SaveChangesAsync();
        return league;
    }

    public async Task<Domain.Entities.League> UpdateAsync(Domain.Entities.League league)
    {
        _context.Leagues.Update(league);
        await _context.SaveChangesAsync();
        return league;
    }

    public async Task DeleteAsync(Guid id)
    {
        var league = await _context.Leagues.FindAsync(id);
        if (league != null)
        {
            _context.Leagues.Remove(league);
            await _context.SaveChangesAsync();
        }
    }

    public async Task<bool> ExistsAsync(string name)
    {
        return await _context.Leagues
            .AnyAsync(l => l.Name.ToLower() == name.ToLower());
    }
}
