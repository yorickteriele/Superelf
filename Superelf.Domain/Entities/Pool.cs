namespace Superelf.Domain.Entities;

public class Pool {
    public Guid Id { get; set; }
    public required ApplicationUser Owner { get; set; }
    public required string Name { get; set; }
    public required string Code { get; set; }
    public DateTime CreateTime { get; set; } = DateTime.UtcNow;
    public bool AllowSelectionEditing { get; set; } = true;
    
    public List<PoolParticipant> Participants { get; set; } = new();
}