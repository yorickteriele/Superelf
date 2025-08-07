namespace Superelf.Domain.Entities;

public class League
{
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public string? ShortName { get; set; }
    public string? Country { get; set; }
    public string? LogoUrl { get; set; }
    public string? GoogleCalendarId { get; set; }
    public DateTime? LastSyncAt { get; set; }
    public bool IsActive { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public List<Match> Matches { get; set; } = new();
    public List<Club> Clubs { get; set; } = new();
}
