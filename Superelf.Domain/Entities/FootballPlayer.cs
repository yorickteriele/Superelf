namespace Superelf.Domain.Entities;

public class FootballPlayer {
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Position { get; set; }
    public required string Nationality { get; set; }
    public string? Club { get; set; } // Keep for backward compatibility
    public Guid? ClubId { get; set; }
    public Club? ClubEntity { get; set; }
    public string? PhotoUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    
    public List<PlayerPerformance> Performances { get; set; } = new();
}