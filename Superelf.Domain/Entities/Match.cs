namespace Superelf.Domain.Entities;

public class Match
{
    public Guid Id { get; set; }
    public required string HomeTeam { get; set; }
    public required string AwayTeam { get; set; }
    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }
    public DateTime MatchDate { get; set; }
    public string? Competition { get; set; }
    public int Round { get; set; }
    public bool IsCompleted { get; set; }
    public Guid? LeagueId { get; set; }
    public League? League { get; set; }
    public string? GoogleCalendarEventId { get; set; } // For tracking calendar events
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public List<PlayerPerformance> PlayerPerformances { get; set; } = new();
}
