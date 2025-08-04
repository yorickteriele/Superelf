using Superelf.Domain.Entities;

namespace Superelf.Application.League;

public interface ILeagueRepository
{
    Task<List<Domain.Entities.League>> GetAllAsync();
    Task<Domain.Entities.League?> GetByIdAsync(Guid id);
    Task<Domain.Entities.League?> GetByNameAsync(string name);
    Task<Domain.Entities.League> CreateAsync(Domain.Entities.League league);
    Task<Domain.Entities.League> UpdateAsync(Domain.Entities.League league);
    Task DeleteAsync(Guid id);
    Task<bool> ExistsAsync(string name);
}

public class LeagueService
{
    private readonly ILeagueRepository _leagueRepository;

    public LeagueService(ILeagueRepository leagueRepository)
    {
        _leagueRepository = leagueRepository;
    }

    public async Task<List<Domain.Entities.League>> GetAllLeaguesAsync()
    {
        return await _leagueRepository.GetAllAsync();
    }

    public async Task<Domain.Entities.League?> GetLeagueByIdAsync(Guid id)
    {
        return await _leagueRepository.GetByIdAsync(id);
    }

    public async Task<Domain.Entities.League> CreateLeagueAsync(string name, string? shortName = null, string? country = null, string? logoUrl = null, string? googleCalendarId = null)
    {
        if (await _leagueRepository.ExistsAsync(name))
        {
            throw new InvalidOperationException($"League with name '{name}' already exists");
        }

        var league = new Domain.Entities.League
        {
            Id = Guid.NewGuid(),
            Name = name,
            ShortName = shortName,
            Country = country,
            LogoUrl = logoUrl,
            GoogleCalendarId = googleCalendarId,
            IsActive = true,
            CreatedAt = DateTime.UtcNow
        };

        return await _leagueRepository.CreateAsync(league);
    }

    public async Task<Domain.Entities.League> UpdateLeagueAsync(Guid id, string name, string? shortName = null, string? country = null, string? logoUrl = null, string? googleCalendarId = null, bool isActive = true)
    {
        var league = await _leagueRepository.GetByIdAsync(id);
        if (league == null)
        {
            throw new ArgumentException($"League with ID {id} not found");
        }

        // Check if name change conflicts with existing league
        if (league.Name != name && await _leagueRepository.ExistsAsync(name))
        {
            throw new InvalidOperationException($"League with name '{name}' already exists");
        }

        league.Name = name;
        league.ShortName = shortName;
        league.Country = country;
        league.LogoUrl = logoUrl;
        league.GoogleCalendarId = googleCalendarId;
        league.IsActive = isActive;

        return await _leagueRepository.UpdateAsync(league);
    }

    public async Task DeleteLeagueAsync(Guid id)
    {
        var league = await _leagueRepository.GetByIdAsync(id);
        if (league == null)
        {
            throw new ArgumentException($"League with ID {id} not found");
        }

        await _leagueRepository.DeleteAsync(id);
    }
}
