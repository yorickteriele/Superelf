namespace Superelf.Domain.Entities;

public class Club
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? ShortName { get; set; }
    public string? LogoUrl { get; set; }
    public string? Country { get; set; }
    public Guid? LeagueId { get; set; }
    public League? League { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public List<FootballPlayer> Players { get; set; } = new();

    // TEMP: For migration test
    public string? MigrationTestColumn { get; set; }
}
