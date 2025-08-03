using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.SignalR;
using Superelf.Application.Authentication;
using Superelf.Application.Pool;
using Superelf.Domain.Entities;
using Superelf.API.DTOs;
using Superelf.API.Hubs;

namespace Superelf.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
public class PoolsController : ControllerBase
{
    private readonly AuthenticationService _authenticationService;
    private readonly ILogger<PoolsController> _logger;
    private readonly IHubContext<PoolHub> _poolHub;
    private readonly PoolService _poolService;

    public PoolsController(
        ILogger<PoolsController> logger,
        PoolService poolService,
        AuthenticationService authenticationService,
        IHubContext<PoolHub> poolHub)
    {
        _logger = logger;
        _poolService = poolService;
        _authenticationService = authenticationService;
        _poolHub = poolHub;
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

            var pool = await _poolService.GetPoolByCodeAsync(joinPoolDto.PoolCode);
            
            if (pool != null)
            {
                await _poolHub.Clients.Group(pool.Id.ToString()).SendAsync("LeaderboardUpdated");
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
            await _poolHub.Clients.Group(poolId.ToString()).SendAsync("PoolUpdated");
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
            await _poolHub.Clients.Group(poolId.ToString()).SendAsync("PoolUpdated");
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
}
