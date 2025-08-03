namespace Superelf.Domain.Entities;

public class Lineup {
    public Guid Id { get; set; }
    public required PoolParticipant PoolUser { get; set; }
    public bool Complete { get; set; }

    public List<LineupLine> LineupLines { get; set; } = new();
}