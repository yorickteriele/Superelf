using System.Security.Cryptography;
using Superelf.Domain.Entities;
using Superelf.Application.Selection;

namespace Superelf.Application.Pool;

public class PoolService {
    private const string AllowedChars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    private const int CodeLength = 6;
    private const int MaxAttempts = 10;
    private readonly IPoolRepository _poolRepository;
    private readonly SelectionService _selectionService;

    public PoolService(IPoolRepository poolRepository, SelectionService selectionService) {
        _poolRepository = poolRepository;
        _selectionService = selectionService;
    }

    public async Task<Domain.Entities.Pool> CreatePoolAsync(Domain.Entities.Pool pool, ApplicationUser owner) {
        pool.Code = await GenerateUniqueCodeAsync();
        pool.Owner = owner;

        if (await _poolRepository.CreatePoolAsync(pool)) {
            await _poolRepository.AddParticipantToPoolAsync(pool, owner);
            return pool;
        }

        throw new Exception("Failed to create pool.");
    }

    public async Task<bool> JoinPoolAsync(string poolCode, ApplicationUser user) {
        var pool = await _poolRepository.GetPoolByCodeAsync(poolCode);
        if (pool == null) return false;

        var isAlreadyMember = await _poolRepository.IsUserInPoolAsync(pool, user);
        if (isAlreadyMember) return false;

        return await _poolRepository.AddParticipantToPoolAsync(pool, user);
    }

    private async Task<string> GenerateUniqueCodeAsync() {
        for (var i = 0; i < MaxAttempts; i++) {
            var code = GenerateCode();
            var exists = await _poolRepository.GetPoolByCodeAsync(code) != null;
            if (!exists) return code;
        }

        throw new Exception("Could not generate a unique code.");
    }

    private string GenerateCode() {
        var charArray = new char[CodeLength];
        var randomBytes = new byte[CodeLength];
        using (var rng = RandomNumberGenerator.Create()) {
            rng.GetBytes(randomBytes);
        }

        for (var i = 0; i < CodeLength; i++) charArray[i] = AllowedChars[randomBytes[i] % AllowedChars.Length];
        return new string(charArray);
    }

    public async Task<IEnumerable<(Domain.Entities.Pool Pool, int PlayerCount)>>
        GetUserPoolsWithParticipantsAsync(string userId) {
        var pools = await _poolRepository.GetUserPools(userId);
        return pools.Select(p => (
            Pool: p,
            PlayerCount: _poolRepository.GetParticipantCount(p.Id).GetAwaiter().GetResult()
        ));
    }

    public async Task<(Domain.Entities.Pool Pool, List<PoolParticipant> Participants)?> GetPoolWithParticipantsAsync(
        Guid poolId, ApplicationUser user) {
        var pool = await _poolRepository.GetPoolByIdAsync(poolId);

        if (pool == null || !await _poolRepository.IsUserInPoolAsync(pool, user))
            return null;

        var participants = await _poolRepository.GetPoolParticipantsAsync(poolId);
        return (pool, participants);
    }

    public async Task<List<PoolParticipant>> GetPoolParticipantsAsync(Guid poolId) {
        return await _poolRepository.GetPoolParticipantsAsync(poolId);
    }

    public async Task<List<(PoolParticipant Participant, bool SelectionComplete, int SelectedPlayers, bool HasJoker)>> GetPoolParticipantsWithSelectionStatusAsync(Guid poolId) {
        var participants = await _poolRepository.GetPoolParticipantsAsync(poolId);
        var result = new List<(PoolParticipant, bool, int, bool)>();

        foreach (var participant in participants) {
            var lineup = await _selectionService.GetOrCreateLineupAsync(poolId, participant.ApplicationUser.Id);
            var stats = await _selectionService.GetLineupStatisticsAsync(lineup.Id);
            
            Console.WriteLine($"User: {participant.ApplicationUser.UserName}, Players: {stats.TotalPlayers}, Complete: {lineup.Complete}, HasJoker: {stats.HasJoker}");
            
            result.Add((participant, lineup.Complete, stats.TotalPlayers, stats.HasJoker));
        }

        return result;
    }

    public async Task<Domain.Entities.Pool?> GetPoolByCodeAsync(string code) {
        return await _poolRepository.GetPoolByCodeAsync(code);
    }

    public async Task<bool> EditPoolNameAsync(Guid poolId, string newName, ApplicationUser requester) {
        var pool = await _poolRepository.GetPoolByIdAsync(poolId);
        if (pool == null || pool.Owner.Id != requester.Id) return false;

        pool.Name = newName;
        return await _poolRepository.EditPoolNameAsync(pool);
    }

    public async Task<bool> RemoveParticipantAsync(Guid poolId, string userId, ApplicationUser requester) {
        var pool = await _poolRepository.GetPoolByIdAsync(poolId);
        if (pool == null || pool.Owner.Id != requester.Id) return false;
        if (pool.Owner.Id == userId) return false; 

        return await _poolRepository.RemoveParticipantFromPoolAsync(poolId, userId);
    }

    public async Task<bool> DeletePoolAsync(Guid poolId) {
        return await _poolRepository.DeletePoolAsync(poolId);
    }

    public async Task<List<Domain.Entities.Pool>> GetAllPoolsAsync() {
        return await _poolRepository.GetAllPoolsAsync();
    }
}