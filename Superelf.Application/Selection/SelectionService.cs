// Superelf.Application/Selection/SelectionService.cs

using Superelf.Domain.Entities;

namespace Superelf.Application.Selection;

public class SelectionService {
    private readonly ISelectionRepository _selectionRepository;

    public SelectionService(ISelectionRepository selectionRepository) {
        _selectionRepository = selectionRepository;
    }

    public async Task<Lineup> GetOrCreateLineupAsync(Guid poolId, string userId) {
        var lineup = await _selectionRepository.GetLineupWithPlayersAsync(poolId, userId);
        if (lineup == null) {
            var poolUser = await _selectionRepository.GetPoolParticipantAsync(poolId, userId);
            if (poolUser != null) {
                lineup = new Lineup { PoolUser = poolUser };
                await _selectionRepository.AddLineupAsync(lineup);
            } else {
                throw new InvalidOperationException($"User {userId} is not a participant in pool {poolId}");
            }
        }

        return lineup!;
    }

    public async Task SubmitSelectionAsync(
        Guid poolId,
        string userId,
        string position,
        bool isReserve,
        List<Guid> selectedPlayers,
        bool isJoker = false,
        int? slotIndex = null
    ) {
        var maxSelection = isReserve ? 1 : GetMaxSelectionForPosition(position);

        if (selectedPlayers.Count > maxSelection)
            throw new InvalidOperationException($"Maximaal {maxSelection} spelers toegestaan voor deze positie.");

        var lineup = await _selectionRepository.GetLineupWithPlayersAsync(poolId, userId);
        if (lineup == null)
            throw new InvalidOperationException("Lineup niet gevonden.");

        // Check for duplicate players, but exclude the player being replaced if slotIndex is provided
        var existingPlayerIds = lineup.LineupLines.Select(ll => ll.FootballPlayer.Id).ToList();
        
        if (slotIndex.HasValue)
        {
            // If replacing a specific slot, exclude the player currently in that slot
            var specificPosition = GetSpecificPositionForPlayer(position, slotIndex.Value, isReserve);
            var playerBeingReplaced = lineup.LineupLines
                .Where(ll => ll.FootballPlayer.Position == position && 
                            ll.IsReserve == isReserve && 
                            ll.SpecificPosition == specificPosition)
                .Select(ll => ll.FootballPlayer.Id)
                .FirstOrDefault();
            
            var otherPlayerIds = existingPlayerIds.Where(id => id != playerBeingReplaced).ToList();
            if (selectedPlayers.Any(id => otherPlayerIds.Contains(id)))
                throw new InvalidOperationException("Speler staat al in de selectie.");
        }
        else
        {
            // Check all existing players
            if (selectedPlayers.Any(id => existingPlayerIds.Contains(id)))
                throw new InvalidOperationException("Speler staat al in de selectie.");
        }

        // Check if there's already a joker selected in the lineup
        if (isJoker && lineup.LineupLines.Any(ll => ll.IsJoker))
            throw new InvalidOperationException("Je kunt maar één speler als joker aanwijzen.");

        var newPlayers = _selectionRepository.GetPlayersByIds(selectedPlayers);
        
        // Get existing nationalities, excluding the player being replaced if slotIndex is provided
        var existingNationalities = new List<string>();
        if (slotIndex.HasValue)
        {
            // If replacing a specific slot, exclude the nationality of the player being replaced
            var specificPosition = GetSpecificPositionForPlayer(position, slotIndex.Value, isReserve);
            var playerBeingReplaced = lineup.LineupLines
                .Where(ll => ll.FootballPlayer.Position == position && 
                            ll.IsReserve == isReserve && 
                            ll.SpecificPosition == specificPosition)
                .FirstOrDefault();
            
            existingNationalities = lineup.LineupLines
                .Where(ll => ll.FootballPlayer.Id != playerBeingReplaced?.FootballPlayer.Id)
                .Select(ll => ll.FootballPlayer.Nationality)
                .Distinct()
                .ToList();
        }
        else
        {
            existingNationalities = lineup.LineupLines
                .Select(ll => ll.FootballPlayer.Nationality)
                .Distinct()
                .ToList();
        }
            
        // Get nationalities that would be added (not already in the lineup)
        var newUniqueNationalities = newPlayers
            .Select(p => p.Nationality)
            .Distinct()
            .Where(n => !existingNationalities.Contains(n))
            .Count();

        var currentNationalityCount = existingNationalities.Count;
        if (currentNationalityCount + newUniqueNationalities > 15)
            throw new InvalidOperationException("Maximaal 15 verschillende landen toegestaan. Kies spelers uit andere landen.");

        // If slotIndex is provided, only remove the specific slot, otherwise remove all for this position
        List<LineupLine> existingLinesToRemove;
        if (slotIndex.HasValue)
        {
            // Only remove the specific slot being replaced
            var specificPosition = GetSpecificPositionForPlayer(position, slotIndex.Value, isReserve);
            existingLinesToRemove = lineup.LineupLines
                .Where(ll => ll.FootballPlayer.Position == position && 
                            ll.IsReserve == isReserve && 
                            ll.SpecificPosition == specificPosition)
                .ToList();
        }
        else
        {
            // Fallback: remove all existing lines for this position (for backward compatibility)
            existingLinesToRemove = lineup.LineupLines
                .Where(ll => ll.FootballPlayer.Position == position && ll.IsReserve == isReserve)
                .ToList();
        }

        await _selectionRepository.RemoveLineupLinesAsync(existingLinesToRemove);
        
        // Create new lineup lines with specific positions
        var newLineupLines = new List<LineupLine>();
        for (int i = 0; i < newPlayers.Count; i++)
        {
            // Use slotIndex if provided, otherwise fall back to loop index
            var positionIndex = slotIndex.HasValue ? slotIndex.Value : i;
            var specificPosition = GetSpecificPositionForPlayer(position, positionIndex, isReserve);
            newLineupLines.Add(new LineupLine {
                Lineup = lineup,
                FootballPlayer = newPlayers[i],
                IsReserve = isReserve,
                SpecificPosition = specificPosition,
                IsJoker = isJoker && newPlayers.Count == 1 // Only set joker if one player is selected
            });
        }
        
        await _selectionRepository.AddLineupLinesAsync(newLineupLines);
        
        // Update complete status based on whether all required positions are filled
        await UpdateLineupCompletionStatus(lineup.Id);
    }
    
    public async Task SetJokerAsync(Guid poolId, string userId, Guid playerId) {
        var lineup = await _selectionRepository.GetLineupWithPlayersAsync(poolId, userId);
        if (lineup == null)
            throw new InvalidOperationException("Lineup niet gevonden.");
            
        // Reset existing joker if any
        foreach (var line in lineup.LineupLines) {
            line.IsJoker = line.FootballPlayer.Id == playerId;
        }
        
        await _selectionRepository.UpdateLineupAsync(lineup);
    }
    
    private async Task UpdateLineupCompletionStatus(Guid lineupId) {
        var lineup = await _selectionRepository.GetLineupByIdAsync(lineupId);
        if (lineup == null) return;
        
        // Count players by position and reserve status
        var goalkeepers = lineup.LineupLines.Count(ll => ll.FootballPlayer.Position == "Goalkeeper" && !ll.IsReserve);
        var defenders = lineup.LineupLines.Count(ll => ll.FootballPlayer.Position == "Defender" && !ll.IsReserve);
        var midfielders = lineup.LineupLines.Count(ll => ll.FootballPlayer.Position == "Midfielder" && !ll.IsReserve);
        var forwards = lineup.LineupLines.Count(ll => ll.FootballPlayer.Position == "Forward" && !ll.IsReserve);
        
        var reserveGoalkeepers = lineup.LineupLines.Count(ll => ll.FootballPlayer.Position == "Goalkeeper" && ll.IsReserve);
        var reserveDefenders = lineup.LineupLines.Count(ll => ll.FootballPlayer.Position == "Defender" && ll.IsReserve);
        var reserveMidfielders = lineup.LineupLines.Count(ll => ll.FootballPlayer.Position == "Midfielder" && ll.IsReserve);
        var reserveForwards = lineup.LineupLines.Count(ll => ll.FootballPlayer.Position == "Forward" && ll.IsReserve);
        
        // Check if formation is correct (1-4-3-3 + reserves)
        var formationCorrect = goalkeepers == 1 && defenders == 4 && midfielders == 3 && forwards == 3
                            && reserveGoalkeepers == 1 && reserveDefenders == 1 && reserveMidfielders == 1 && reserveForwards == 1;
        
        // Check if we have 15 unique nationalities
        var uniqueNationalities = lineup.LineupLines
            .Select(ll => ll.FootballPlayer.Nationality)
            .Distinct()
            .Count();
            
        lineup.Complete = formationCorrect && uniqueNationalities == 15;
        await _selectionRepository.UpdateLineupAsync(lineup);
    }

    public async Task<LineupStatistics> GetLineupStatisticsAsync(Guid lineupId) {
        var lineup = await _selectionRepository.GetLineupByIdAsync(lineupId);
        if (lineup == null)
            throw new InvalidOperationException("Lineup niet gevonden.");
            
        var stats = new LineupStatistics {
            TotalPlayers = lineup.LineupLines.Count,
            UniqueNationalities = lineup.LineupLines
                .Select(ll => ll.FootballPlayer.Nationality)
                .Distinct()
                .Count(),
            HasJoker = lineup.LineupLines.Any(ll => ll.IsJoker),
            JokerPlayerId = lineup.LineupLines.FirstOrDefault(ll => ll.IsJoker)?.FootballPlayer.Id ?? Guid.Empty,
            FormationComplete = lineup.Complete
        };
        
        return stats;
    }

    private int GetMaxSelectionForPosition(string position) {
        return position switch {
            "Goalkeeper" => 1,
            "Defender" => 4,
            "Midfielder" => 3,
            "Forward" => 3,
            _ => throw new ArgumentException("Ongeldige positie")
        };
    }

    private int GetSpecificPositionForPlayer(string position, int index, bool isReserve)
    {
        if (isReserve)
        {
            // Reserve positions: 100-199 range
            return position switch
            {
                "Goalkeeper" => 100,
                "Defender" => 101,
                "Midfielder" => 102,
                "Forward" => 103,
                _ => 100 + index
            };
        }
        
        // Starting positions based on 4-3-3 formation
        return position switch
        {
            "Goalkeeper" => 1, // Goalkeeper
            "Defender" => index switch
            {
                0 => 2, // Left Back
                1 => 3, // Left Center Back
                2 => 4, // Right Center Back
                3 => 5, // Right Back
                _ => 2 + index
            },
            "Midfielder" => index switch
            {
                0 => 6, // Left Midfielder
                1 => 7, // Center Midfielder
                2 => 8, // Right Midfielder
                _ => 6 + index
            },
            "Forward" => index switch
            {
                0 => 9,  // Left Wing
                1 => 10, // Center Forward
                2 => 11, // Right Wing
                _ => 9 + index
            },
            _ => index + 1
        };
    }

    public async Task<List<FootballPlayer>> GetFootballPlayersByPosition(string position) {
        return await _selectionRepository.GetPlayersByPositionAsync(position);
    }
}

public class LineupStatistics {
    public int TotalPlayers { get; set; }
    public int UniqueNationalities { get; set; }
    public bool HasJoker { get; set; }
    public Guid JokerPlayerId { get; set; }
    public bool FormationComplete { get; set; }
}