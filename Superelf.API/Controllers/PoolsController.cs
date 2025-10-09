using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Superelf.Application.Authentication;
using Superelf.Application.Pool;
using Superelf.Domain.Entities;
using Superelf.API.DTOs;

namespace Superelf.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PoolsController : ControllerBase
{
    private readonly AuthenticationService _authenticationService;
    private readonly ILogger<PoolsController> _logger;
    private readonly PoolService _poolService;
    private readonly IScoringService _scoringService;

    public PoolsController(
        ILogger<PoolsController> logger,
        PoolService poolService,
        AuthenticationService authenticationService,
        IScoringService scoringService)
    {
        _logger = logger;
        _poolService = poolService;
        _authenticationService = authenticationService;
        _scoringService = scoringService;
    }

    [HttpGet]
    public async Task<ActionResult<List<PoolDto>>> GetUserPools()
    {
        var user = await _authenticationService.GetCurrentUserAsync(User);
        if (user == null) return Unauthorized();

        var userPools = await _poolService.GetUserPoolsWithParticipantsAsync(user.Id);
        
        var poolDtos = userPools.Select(poolTuple => new PoolDto
        {
            Id = poolTuple.Pool.Id,
            Name = poolTuple.Pool.Name,
            Code = poolTuple.Pool.Code,
            CreateTime = poolTuple.Pool.CreateTime,
            OwnerName = poolTuple.Pool.Owner.UserName!,
            AllowSelectionEditing = poolTuple.Pool.AllowSelectionEditing,
            Participants = new List<PoolParticipantDto>() // Will be populated separately if needed
        }).ToList();

        return Ok(poolDtos);
    }

    [HttpPost]
    public async Task<ActionResult<PoolDto>> CreatePool(CreatePoolDto createPoolDto)
    {
        var user = await _authenticationService.GetCurrentUserAsync(User);
        if (user == null) return Unauthorized();

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        try
        {
            // Use a dummy code and owner reference that will be replaced by the service
            var dummyCode = "TEMP00"; 
            
            var pool = new Pool { 
                Name = createPoolDto.Name,
                Owner = user,
                Code = dummyCode // Will be replaced by the service with a unique code
            };
            
            var createdPool = await _poolService.CreatePoolAsync(pool, user);
            
            return Ok(new PoolDto
            {
                Id = createdPool.Id,
                Name = createdPool.Name,
                Code = createdPool.Code,
                CreateTime = createdPool.CreateTime,
                OwnerName = user.UserName!,
                Participants = new List<PoolParticipantDto>()
            });
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Pool creation failed");
            return BadRequest("Pool creation failed");
        }
    }

    [HttpPost("join")]
    public async Task<ActionResult> JoinPool(JoinPoolDto joinPoolDto)
    {
        var user = await _authenticationService.GetCurrentUserAsync(User);
        if (user == null) return Unauthorized();

        try
        {
            var result = await _poolService.JoinPoolAsync(joinPoolDto.PoolCode, user);

            if (!result)
            {
                return BadRequest("Invalid code or already a member");
            }

            return Ok("Successfully joined pool");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Join pool failed");
            return BadRequest("Failed to join pool");
        }
    }

    [HttpGet("{poolId}")]
    public async Task<ActionResult<PoolDto>> GetPool(Guid poolId)
    {
        var user = await _authenticationService.GetCurrentUserAsync(User);
        if (user == null) return Unauthorized();

        var poolWithParticipants = await _poolService.GetPoolWithParticipantsAsync(poolId, user);

        if (poolWithParticipants == null)
            return Forbid();

        var (pool, participants) = ((Pool, List<PoolParticipant>))poolWithParticipants;

        // Get participants with their selection status
        var participantsWithStatus = await _poolService.GetPoolParticipantsWithSelectionStatusAsync(poolId);

        var poolDto = new PoolDto
        {
            Id = pool.Id,
            Name = pool.Name,
            Code = pool.Code,
            CreateTime = pool.CreateTime,
            OwnerName = pool.Owner.UserName!,
            AllowSelectionEditing = pool.AllowSelectionEditing,
            Participants = participantsWithStatus.Select(p => new PoolParticipantDto
            {
                UserId = p.Participant.ApplicationUser.Id,
                UserName = p.Participant.ApplicationUser.UserName ?? "Unknown",
                SelectionComplete = p.SelectionComplete,
                SelectedPlayers = p.SelectedPlayers,
                HasJoker = p.HasJoker
            }).ToList()
        };

        return Ok(poolDto);
    }

    [HttpGet("{poolId}/participants")]
    public async Task<ActionResult<List<PoolParticipantDto>>> GetParticipants(Guid poolId)
    {
        var participantsWithStatus = await _poolService.GetPoolParticipantsWithSelectionStatusAsync(poolId);
        
        var participantDtos = participantsWithStatus.Select(p => new PoolParticipantDto
        {
            UserId = p.Participant.ApplicationUser.Id,
            UserName = p.Participant.ApplicationUser.UserName ?? "Unknown",
            SelectionComplete = p.SelectionComplete,
            SelectedPlayers = p.SelectedPlayers,
            HasJoker = p.HasJoker
        }).ToList();

        return Ok(participantDtos);
    }

    [HttpPut("{poolId}/name")]
    public async Task<ActionResult> EditPoolName(Guid poolId, EditPoolNameDto editDto)
    {
        var user = await _authenticationService.GetCurrentUserAsync(User);
        if (user == null) return Unauthorized();

        var result = await _poolService.EditPoolNameAsync(poolId, editDto.NewName, user);
        if (result)
        {
            return Ok("Pool name updated successfully");
        }

        return BadRequest("Failed to update pool name");
    }

    [HttpDelete("{poolId}/participants/{userId}")]
    public async Task<ActionResult> RemoveParticipant(Guid poolId, string userId)
    {
        var user = await _authenticationService.GetCurrentUserAsync(User);
        if (user == null) return Unauthorized();

        var result = await _poolService.RemoveParticipantAsync(poolId, userId, user);
        if (result)
        {
            return Ok("Participant removed successfully");
        }

        return BadRequest("Failed to remove participant");
    }

    [Authorize(Roles = "Admin")]
    [HttpDelete("{poolId}")]
    public async Task<ActionResult> DeletePool(Guid poolId)
    {
        if (await _poolService.DeletePoolAsync(poolId))
        {
            return Ok("Pool deleted successfully");
        }
        
        return BadRequest("Failed to delete pool");
    }

    [Authorize(Roles = "Admin")]
    [HttpGet("admin/all")]
    public async Task<ActionResult<List<PoolDto>>> GetAllPools()
    {
        var pools = await _poolService.GetAllPoolsAsync();
        
        var poolDtos = pools.Select(p => new PoolDto
        {
            Id = p.Id,
            Name = p.Name,
            Code = p.Code,
            CreateTime = p.CreateTime,
            OwnerName = p.Owner?.UserName ?? "Unknown",
            Participants = p.Participants?.Select(pp => new PoolParticipantDto
            {
                UserId = pp.ApplicationUser.Id,
                UserName = pp.ApplicationUser?.UserName ?? "Unknown"
            }).ToList() ?? new List<PoolParticipantDto>()
        }).ToList();

        return Ok(poolDtos);
    }

    [HttpGet("{poolId}/scoreboard")]
    public async Task<ActionResult<List<UserScoreDto>>> GetPoolScoreboard(Guid poolId)
    {
        var user = await _authenticationService.GetCurrentUserAsync(User);
        if (user == null) return Unauthorized();

        // Check if user is participant in the pool
        var poolWithParticipants = await _poolService.GetPoolWithParticipantsAsync(poolId, user);
        if (poolWithParticipants == null)
            return Forbid();

        try
        {
            var scores = await _scoringService.CalculatePoolScoresAsync(poolId);
            return Ok(scores);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error calculating pool scores for pool {PoolId}", poolId);
            return BadRequest("Failed to calculate pool scores");
        }
    }

    [HttpPut("{poolId}/toggle-selection-editing")]
    public async Task<ActionResult> ToggleSelectionEditing(Guid poolId, [FromBody] ToggleSelectionEditingDto toggleDto)
    {
        var user = await _authenticationService.GetCurrentUserAsync(User);
        if (user == null) return Unauthorized();

        try
        {
            var result = await _poolService.ToggleSelectionEditingAsync(poolId, user.Id, toggleDto.AllowSelectionEditing);
            
            if (result)
            {
                return Ok($"Selection editing {(toggleDto.AllowSelectionEditing ? "enabled" : "disabled")} successfully");
            }
            
            return BadRequest("Failed to update selection editing setting. You may not be the pool owner.");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error toggling selection editing for pool {PoolId}", poolId);
            return BadRequest("An error occurred while updating selection editing setting");
        }
    }
}
