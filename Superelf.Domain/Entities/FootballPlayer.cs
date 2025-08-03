namespace Superelf.Domain.Entities;

public class FootballPlayer {
    public Guid Id { get; set; }
    public required string Name { get; set; }
    public required string Position { get; set; }
    public required string Nationality { get; set; }
    public string? Club { get; set; }
}