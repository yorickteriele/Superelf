namespace Superelf.Domain.Entities;

public class UserScore {
    public Guid Id { get; set; }
    public required ApplicationUser User { get; set; }
    public required Pool Pool { get; set; }
    public int Score { get; set; }
    public bool HasUsedJoker { get; set; }
}