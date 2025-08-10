using Superelf.Domain.Entities;

namespace Superelf.Application.Performance;

public interface IPerformanceRepository
{
    Task<List<PlayerPerformance>> GetCompletedPerformancesForPlayerAsync(Guid playerId);
}




