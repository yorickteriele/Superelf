namespace Superelf.Domain.Entities;

public class PlayerPerformance
{
    public Guid Id { get; set; }
    public required Match Match { get; set; }
    public required FootballPlayer Player { get; set; }
    public int Goals { get; set; }
    public int PenaltyGoals { get; set; }
    public int PenaltiesMissed { get; set; }
    public int OwnGoals { get; set; }
    public int Assists { get; set; }
    public int YellowCards { get; set; }
    public int RedCards { get; set; }
    public bool Played { get; set; }
    public int Points { get; set; } 
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
