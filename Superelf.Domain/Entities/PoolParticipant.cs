using Microsoft.EntityFrameworkCore;

namespace Superelf.Domain.Entities;

public class PoolParticipant {
    public Guid Id { get; set; }

    [DeleteBehavior(DeleteBehavior.ClientCascade)]
    public required ApplicationUser ApplicationUser { get; set; }

    [DeleteBehavior(DeleteBehavior.ClientCascade)]
    public required Pool Pool { get; set; }
    
}