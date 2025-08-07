namespace Superelf.Domain.Entities;

public class LineupLine {
    public Guid Id { get; set; }
    public Lineup? Lineup { get; set; }
    public required FootballPlayer FootballPlayer { get; set; }
    public bool IsReserve { get; set; }
    public int SpecificPosition { get; set; } // left to right
    
    public bool IsJoker { get; set; }
}