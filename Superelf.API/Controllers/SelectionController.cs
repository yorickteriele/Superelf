using System.Security.Claims;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Superelf.Application.Selection;
using Superelf.API.DTOs;

namespace Superelf.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class SelectionController : ControllerBase
{
    private readonly SelectionService _selectionService;

    public SelectionController(SelectionService selectionService)
    {
        _selectionService = selectionService;
    }

    [HttpGet("{poolId}")]
    public async Task<ActionResult<SelectionDto>> GetSelection(Guid poolId)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var lineup = await _selectionService.GetOrCreateLineupAsync(poolId, userId);

        var selectionDto = new SelectionDto
        {
            PoolId = poolId,
            Complete = lineup.Complete,
            Formation = "4-3-3"
        };

        // Get lineup statistics for progress bar
        var stats = await _selectionService.GetLineupStatisticsAsync(lineup.Id);
        selectionDto.TotalPlayers = stats.TotalPlayers;
        selectionDto.UniqueNationalities = stats.UniqueNationalities;
        selectionDto.HasJoker = stats.HasJoker;
        selectionDto.JokerPlayerId = stats.JokerPlayerId;

        foreach (var line in lineup.LineupLines)
        {
            var player = line.FootballPlayer;

            // If this player is marked as joker, set it in the DTO
            if (line.IsJoker)
            {
                selectionDto.JokerPlayerId = player.Id;
            }

            // Create SelectedPlayerDto with full player information
            var selectedPlayerDto = new SelectedPlayerDto
            {
                PlayerId = player.Id,
                Player = new FootballPlayerDto
                {
                    Id = player.Id,
                    Name = player.Name,
                    Position = player.Position,
                    Nationality = player.Nationality,
                    Club = player.Club,
                    ClubId = player.ClubId,
                    PhotoUrl = player.PhotoUrl,
                    CreatedAt = player.CreatedAt,
                    SpecificPosition = line.SpecificPosition,
                    IsJoker = line.IsJoker
                },
                SpecificPosition = line.SpecificPosition,
                IsJoker = line.IsJoker,
                PositionName = GetPositionName(line.SpecificPosition, line.IsReserve)
            };

            // Map players to their specific positions based on formation
            if (!line.IsReserve)
            {
                switch (player.Position)
                {
                    case "Goalkeeper":
                        selectionDto.SelectedBasisGoalkeeper = selectedPlayerDto;
                        break;
                    case "Defender":
                        // Ensure we have enough space in the list
                        while (selectionDto.SelectedBasisDefenders.Count <= 3)
                            selectionDto.SelectedBasisDefenders.Add(null);
                        // Map specific positions to array indices: 2->0, 3->1, 4->2, 5->3
                        var defenderIndex = line.SpecificPosition - 2;
                        if (defenderIndex >= 0 && defenderIndex < 4)
                            selectionDto.SelectedBasisDefenders[defenderIndex] = selectedPlayerDto;
                        break;
                    case "Midfielder":
                        while (selectionDto.SelectedBasisMidfielders.Count <= 2)
                            selectionDto.SelectedBasisMidfielders.Add(null);
                        // Map specific positions to array indices: 6->0, 7->1, 8->2
                        var midfielderIndex = line.SpecificPosition - 6;
                        if (midfielderIndex >= 0 && midfielderIndex < 3)
                            selectionDto.SelectedBasisMidfielders[midfielderIndex] = selectedPlayerDto;
                        break;
                    case "Forward":
                        while (selectionDto.SelectedBasisForwards.Count <= 2)
                            selectionDto.SelectedBasisForwards.Add(null);
                        // Map specific positions to array indices: 9->0, 10->1, 11->2
                        var forwardIndex = line.SpecificPosition - 9;
                        if (forwardIndex >= 0 && forwardIndex < 3)
                            selectionDto.SelectedBasisForwards[forwardIndex] = selectedPlayerDto;
                        break;
                }
            }
            else
            {
                // Handle reserve players
                switch (player.Position)
                {
                    case "Goalkeeper":
                        selectionDto.SelectedReserveGoalkeeper = selectedPlayerDto;
                        break;
                    case "Defender":
                        selectionDto.SelectedReserveDefender = selectedPlayerDto;
                        break;
                    case "Midfielder":
                        selectionDto.SelectedReserveMidfielder = selectedPlayerDto;
                        break;
                    case "Forward":
                        selectionDto.SelectedReserveForward = selectedPlayerDto;
                        break;
                }
            }
        }

        // Get available players for each position
        var goalkeepers = await _selectionService.GetFootballPlayersByPosition("Goalkeeper");
        var defenders = await _selectionService.GetFootballPlayersByPosition("Defender");
        var midfielders = await _selectionService.GetFootballPlayersByPosition("Midfielder");
        var forwards = await _selectionService.GetFootballPlayersByPosition("Forward");

        selectionDto.Goalkeepers = goalkeepers.Select(p => new FootballPlayerDto
        {
            Id = p.Id,
            Name = p.Name,
            Position = p.Position,
            Nationality = p.Nationality,
            Club = p.Club,
            ClubId = p.ClubId,
            PhotoUrl = p.PhotoUrl,
            CreatedAt = p.CreatedAt,
            SpecificPosition = null,
            IsJoker = false
        }).ToList();

        selectionDto.Defenders = defenders.Select(p => new FootballPlayerDto
        {
            Id = p.Id,
            Name = p.Name,
            Position = p.Position,
            Nationality = p.Nationality,
            Club = p.Club,
            ClubId = p.ClubId,
            PhotoUrl = p.PhotoUrl,
            CreatedAt = p.CreatedAt,
            SpecificPosition = null,
            IsJoker = false
        }).ToList();

        selectionDto.Midfielders = midfielders.Select(p => new FootballPlayerDto
        {
            Id = p.Id,
            Name = p.Name,
            Position = p.Position,
            Nationality = p.Nationality,
            Club = p.Club,
            ClubId = p.ClubId,
            PhotoUrl = p.PhotoUrl,
            CreatedAt = p.CreatedAt,
            SpecificPosition = null,
            IsJoker = false
        }).ToList();

        selectionDto.Forwards = forwards.Select(p => new FootballPlayerDto
        {
            Id = p.Id,
            Name = p.Name,
            Position = p.Position,
            Nationality = p.Nationality,
            Club = p.Club,
            ClubId = p.ClubId,
            PhotoUrl = p.PhotoUrl,
            CreatedAt = p.CreatedAt,
            SpecificPosition = null,
            IsJoker = false
        }).ToList();

        return Ok(selectionDto);
    }

    private string GetPositionName(int specificPosition, bool isReserve)
    {
        if (isReserve)
        {
            return specificPosition switch
            {
                100 => "Reserve Goalkeeper",
                101 => "Reserve Defender",
                102 => "Reserve Midfielder",
                103 => "Reserve Forward",
                _ => "Reserve Player"
            };
        }

        return specificPosition switch
        {
            1 => "Goalkeeper",
            2 => "Left Back",
            3 => "Left Center Back",
            4 => "Right Center Back",
            5 => "Right Back",
            6 => "Left Midfielder",
            7 => "Center Midfielder",
            8 => "Right Midfielder",
            9 => "Left Wing",
            10 => "Center Forward",
            11 => "Right Wing",
            _ => "Player"
        };
    }

    [HttpPost("{poolId}/submit")]
    public async Task<ActionResult> SubmitSelection(Guid poolId, SubmitSelectionDto submitDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        try
        {
            await _selectionService.SubmitSelectionAsync(
                poolId,
                userId,
                submitDto.Position,
                submitDto.IsReserve,
                submitDto.SelectedPlayers,
                submitDto.IsJoker,
                submitDto.SlotIndex
            );

            return Ok($"{submitDto.SelectedPlayers.Count} player(s) added to your selection.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpPost("{poolId}/joker")]
    public async Task<ActionResult> SetJoker(Guid poolId, SetJokerDto jokerDto)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        try
        {
            await _selectionService.SetJokerAsync(poolId, userId, jokerDto.PlayerId);
            return Ok("Joker successfully set.");
        }
        catch (Exception ex)
        {
            return BadRequest(ex.Message);
        }
    }

    [HttpGet("{poolId}/players/{position}")]
    public async Task<ActionResult<SelectionTableDto>> GetPlayersForSelection(
        Guid poolId,
        string position,
        [FromQuery] bool isReserve = false,
        [FromQuery] int maxSelection = 1)
    {
        var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
        if (userId == null) return Unauthorized();

        var lineup = await _selectionService.GetOrCreateLineupAsync(poolId, userId);
        var stats = await _selectionService.GetLineupStatisticsAsync(lineup.Id);

        var players = await _selectionService.GetFootballPlayersByPosition(position);

        // Get list of already selected players' IDs
        var alreadySelectedPlayers = lineup.LineupLines
            .Select(ll => ll.FootballPlayer.Id)
            .ToList();

        // Get list of already selected nationalities
        var selectedNationalities = lineup.LineupLines
            .Select(ll => ll.FootballPlayer.Nationality)
            .Distinct()
            .ToList();

        var selectionTableDto = new SelectionTableDto
        {
            PoolId = poolId,
            Position = position,
            IsReserve = isReserve,
            MaxSelection = maxSelection,
            Players = players.Select(p => new FootballPlayerDto
            {
                Id = p.Id,
                Name = p.Name,
                Position = p.Position,
                Nationality = p.Nationality,
                Club = p.Club,
                ClubId = p.ClubId,
                PhotoUrl = p.PhotoUrl,
                CreatedAt = p.CreatedAt
            }).ToList(),
            UniqueNationalities = stats.UniqueNationalities,
            TotalSelectedPlayers = stats.TotalPlayers,
            RemainingPlayers = 15 - stats.TotalPlayers,
            AlreadySelectedPlayers = alreadySelectedPlayers,
            SelectedNationalities = selectedNationalities
        };

        return Ok(selectionTableDto);
    }
}
