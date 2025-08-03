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

            if (line.IsReserve)
                switch (player.Position.ToLower())
                {
                    case "goalkeeper":
                        selectionDto.SelectedReserveGoalkeeper = player.Id;
                        break;
                    case "defender":
                        selectionDto.SelectedReserveDefender = player.Id;
                        break;
                    case "midfielder":
                        selectionDto.SelectedReserveMidfielder = player.Id;
                        break;
                    case "forward":
                        selectionDto.SelectedReserveForward = player.Id;
                        break;
                }
            else
                switch (player.Position.ToLower())
                {
                    case "goalkeeper":
                        selectionDto.SelectedBasisGoalkeeper = player.Id;
                        break;
                    case "defender":
                        selectionDto.SelectedBasisDefenders.Add(player.Id);
                        break;
                    case "midfielder":
                        selectionDto.SelectedBasisMidfielders.Add(player.Id);
                        break;
                    case "forward":
                        selectionDto.SelectedBasisForwards.Add(player.Id);
                        break;
                }
        }

        // Get all players by position
        var goalkeepers = await _selectionService.GetFootballPlayersByPosition("GoalKeeper");
        var defenders = await _selectionService.GetFootballPlayersByPosition("Defender");
        var midfielders = await _selectionService.GetFootballPlayersByPosition("Midfielder");
        var forwards = await _selectionService.GetFootballPlayersByPosition("Forward");

        selectionDto.Goalkeepers = goalkeepers.Select(p => new FootballPlayerDto
        {
            Id = p.Id,
            Name = p.Name,
            Position = p.Position,
            Nationality = p.Nationality,
            Club = p.Club
        }).ToList();

        selectionDto.Defenders = defenders.Select(p => new FootballPlayerDto
        {
            Id = p.Id,
            Name = p.Name,
            Position = p.Position,
            Nationality = p.Nationality,
            Club = p.Club
        }).ToList();

        selectionDto.Midfielders = midfielders.Select(p => new FootballPlayerDto
        {
            Id = p.Id,
            Name = p.Name,
            Position = p.Position,
            Nationality = p.Nationality,
            Club = p.Club
        }).ToList();

        selectionDto.Forwards = forwards.Select(p => new FootballPlayerDto
        {
            Id = p.Id,
            Name = p.Name,
            Position = p.Position,
            Nationality = p.Nationality,
            Club = p.Club
        }).ToList();

        return Ok(selectionDto);
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
                submitDto.IsJoker
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
                Club = p.Club
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
