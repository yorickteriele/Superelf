using Superelf.Domain.Entities;

namespace Superelf.Application.Pool;

public interface IPoolRepository {
    public Task<bool> CreatePoolAsync(Domain.Entities.Pool pool);
    public Task<bool> EditPoolNameAsync(Domain.Entities.Pool pool);
    public Task<Domain.Entities.Pool?> GetPoolByCodeAsync(string code);
    public Task<bool> AddParticipantToPoolAsync(Domain.Entities.Pool poolId, ApplicationUser userId);
    public Task<bool> IsUserInPoolAsync(Domain.Entities.Pool poolId, ApplicationUser userId);
    public Task<List<Domain.Entities.Pool>> GetUserPools(string userId);
    public Task<int> GetParticipantCount(Guid poolId);
    public Task<Domain.Entities.Pool?> GetPoolByIdAsync(Guid poolId);
    public Task<List<PoolParticipant>> GetPoolParticipantsAsync(Guid poolId);
    public Task<bool> RemoveParticipantFromPoolAsync(Guid poolId, string userId);
    public Task<bool> DeletePoolAsync(Guid poolId);
    public Task<List<Domain.Entities.Pool>> GetAllPoolsAsync();
}