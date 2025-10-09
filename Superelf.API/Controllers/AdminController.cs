using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Superelf.Domain.Entities;
using Superelf.Infrastructure.Data;
using Superelf.API.DTOs;
using Superelf.Application;
using System.Text.Json;

namespace Superelf.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly ApplicationDbContext _dbContext;
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly ILogger<AdminController> _logger;
    private readonly IConfiguration _configuration;
    private readonly IImageService _imageService;

    public AdminController(
        ApplicationDbContext dbContext,
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        ILogger<AdminController> logger,
        IConfiguration configuration,
        IImageService imageService)
    {
        _dbContext = dbContext;
        _userManager = userManager;
        _roleManager = roleManager;
        _logger = logger;
        _configuration = configuration;
        _imageService = imageService;
    }

    // USERS MANAGEMENT
    [HttpGet("users")]
    public async Task<ActionResult<List<AdminUserDto>>> GetAllUsers()
    {
        var users = await _userManager.Users.ToListAsync();
        var userDtos = new List<AdminUserDto>();

        foreach (var user in users)
        {
            var roles = await _userManager.GetRolesAsync(user);
            userDtos.Add(new AdminUserDto
            {
                Id = user.Id,
                Username = user.UserName ?? "",
                Email = user.Email ?? "",
                PhoneNumber = user.PhoneNumber,
                EmailConfirmed = user.EmailConfirmed,
                LockoutEnd = user.LockoutEnd,
                AccessFailedCount = user.AccessFailedCount,
                Roles = roles.ToList()
            });
        }

        return Ok(userDtos);
    }

    [HttpPost("users/{userId}/make-admin")]
    public async Task<ActionResult> MakeUserAdmin(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound("User not found");

        if (!await _roleManager.RoleExistsAsync("Admin"))
            return BadRequest("Admin role does not exist");

        var result = await _userManager.AddToRoleAsync(user, "Admin");
        if (result.Succeeded)
        {
            _logger.LogInformation("User {UserId} ({Username}) was made admin", user.Id, user.UserName);
            return Ok("User successfully made admin");
        }

        return BadRequest("Failed to make user admin");
    }

    [HttpPost("users/{userId}/remove-admin")]
    public async Task<ActionResult> RemoveUserAdmin(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound("User not found");

        var result = await _userManager.RemoveFromRoleAsync(user, "Admin");
        if (result.Succeeded)
        {
            _logger.LogInformation("Admin role removed from user {UserId} ({Username})", user.Id, user.UserName);
            return Ok("Admin role removed from user");
        }

        return BadRequest("Failed to remove admin role");
    }

    [HttpDelete("users/{userId}")]
    public async Task<ActionResult> DeleteUser(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);
        if (user == null)
            return NotFound("User not found");

        var result = await _userManager.DeleteAsync(user);
        if (result.Succeeded)
        {
            _logger.LogInformation("User {UserId} ({Username}) deleted", user.Id, user.UserName);
            return Ok("User deleted successfully");
        }

        return BadRequest("Failed to delete user");
    }

    // FOOTBALL PLAYERS MANAGEMENT
    [HttpGet("players")]
    public async Task<ActionResult<PagedResult<FootballPlayerDto>>> GetAllPlayers(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20,
        [FromQuery] string? search = null,
        [FromQuery] string? position = null,
        [FromQuery] string? nationality = null,
        [FromQuery] Guid? clubId = null,
        [FromQuery] string? sortBy = "name",
        [FromQuery] string? sortDirection = "asc")
    {
        var query = _dbContext.FootballPlayers
            .Include(p => p.ClubEntity)
            .AsQueryable();

        // Apply filters
        if (!string.IsNullOrWhiteSpace(search))
        {
            query = query.Where(p => 
                p.Name.ToLower().Contains(search.ToLower()) ||
                p.Nationality.ToLower().Contains(search.ToLower()) ||
                (p.Club != null && p.Club.ToLower().Contains(search.ToLower())) ||
                (p.ClubEntity != null && p.ClubEntity.Name.ToLower().Contains(search.ToLower())));
        }

        if (!string.IsNullOrWhiteSpace(position))
        {
            query = query.Where(p => p.Position.ToLower() == position.ToLower());
        }

        if (!string.IsNullOrWhiteSpace(nationality))
        {
            query = query.Where(p => p.Nationality.ToLower() == nationality.ToLower());
        }

        if (clubId.HasValue)
        {
            query = query.Where(p => p.ClubId == clubId.Value);
        }

        // Apply sorting
        query = sortBy?.ToLower() switch
        {
            "name" => sortDirection?.ToLower() == "desc" ? query.OrderByDescending(p => p.Name) : query.OrderBy(p => p.Name),
            "position" => sortDirection?.ToLower() == "desc" ? query.OrderByDescending(p => p.Position) : query.OrderBy(p => p.Position),
            "nationality" => sortDirection?.ToLower() == "desc" ? query.OrderByDescending(p => p.Nationality) : query.OrderBy(p => p.Nationality),
            "club" => sortDirection?.ToLower() == "desc" ? 
                query.OrderByDescending(p => p.ClubEntity != null ? p.ClubEntity.Name : p.Club) : 
                query.OrderBy(p => p.ClubEntity != null ? p.ClubEntity.Name : p.Club),
            "createdat" => sortDirection?.ToLower() == "desc" ? query.OrderByDescending(p => p.CreatedAt) : query.OrderBy(p => p.CreatedAt),
            _ => query.OrderBy(p => p.Name)
        };

        var totalCount = await query.CountAsync();
        var players = await query
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new FootballPlayerDto
            {
                Id = p.Id,
                Name = p.Name,
                Position = p.Position,
                Nationality = p.Nationality,
                Club = p.ClubEntity != null ? p.ClubEntity.Name : p.Club,
                ClubId = p.ClubId,
                PhotoUrl = p.PhotoUrl,
                CreatedAt = p.CreatedAt
            })
            .ToListAsync();

        var result = new PagedResult<FootballPlayerDto>
        {
            Items = players,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling((double)totalCount / pageSize)
        };

        return Ok(result);
    }

    [HttpPost("players")]
    public async Task<ActionResult<FootballPlayerDto>> CreatePlayer(CreatePlayerDto createDto)
    {
        // Find matching club
        Club? club = null;
        if (createDto.ClubId.HasValue)
        {
            club = await _dbContext.Clubs.FindAsync(createDto.ClubId.Value);
            if (club == null)
                return BadRequest("Club not found");
        }
        else if (!string.IsNullOrWhiteSpace(createDto.Club))
        {
            // Get all clubs for matching
            var allClubs = await _dbContext.Clubs.ToListAsync();
            club = FindBestMatchingClub(createDto.Club, allClubs);
            if (club != null)
            {
                _logger.LogDebug("Found matching club for '{PlayerClub}': {MatchedClub}", createDto.Club, club.Name);
            }
            else
            {
                _logger.LogWarning("No matching club found for '{PlayerClub}'", createDto.Club);
            }
        }

        // Download and save image if PhotoUrl is provided
        string? localPhotoUrl = null;
        if (!string.IsNullOrWhiteSpace(createDto.PhotoUrl))
        {
            localPhotoUrl = await _imageService.DownloadAndSaveImageAsync(createDto.PhotoUrl, createDto.Name);
        }

        var player = new FootballPlayer
        {
            Id = Guid.NewGuid(),
            Name = createDto.Name,
            Position = createDto.Position,
            Nationality = createDto.Nationality,
            Club = createDto.Club, // For backward compatibility
            ClubId = club?.Id,
            ClubEntity = club,
            PhotoUrl = localPhotoUrl ?? createDto.PhotoUrl
        };

        _dbContext.FootballPlayers.Add(player);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("New player created: {PlayerName} ({Position}) - Club: {Club}", 
            player.Name, player.Position, club?.Name ?? createDto.Club ?? "None");

        return CreatedAtAction(nameof(GetAllPlayers), new { id = player.Id }, new FootballPlayerDto
        {
            Id = player.Id,
            Name = player.Name,
            Position = player.Position,
            Nationality = player.Nationality,
            Club = club?.Name ?? player.Club,
            ClubId = player.ClubId,
            PhotoUrl = player.PhotoUrl,
            CreatedAt = player.CreatedAt
        });
    }

    [HttpPost("players/bulk")]
    public async Task<ActionResult<BulkCreateResult>> BulkCreatePlayers(BulkCreatePlayersDto bulkDto)
    {
        var result = new BulkCreateResult();
        var playersToAdd = new List<FootballPlayer>();

        // Get all clubs for matching
        var allClubs = await _dbContext.Clubs.ToListAsync();

        foreach (var playerDto in bulkDto.Players)
        {
            try
            {
                // Check for duplicates if skipDuplicates is true
                if (bulkDto.SkipDuplicates)
                {
                    var existingPlayer = await _dbContext.FootballPlayers
                        .FirstOrDefaultAsync(p => p.Name.ToLower() == playerDto.Name.ToLower() && 
                                                  p.Position.ToLower() == playerDto.Position.ToLower());
                    if (existingPlayer != null)
                    {
                        result.Skipped++;
                        result.SkippedPlayers.Add($"{playerDto.Name} ({playerDto.Position}) - already exists");
                        continue;
                    }
                }

                // Find matching club
                Club? club = null;
                if (playerDto.ClubId.HasValue)
                {
                    // If ClubId is provided, use it directly
                    club = await _dbContext.Clubs.FindAsync(playerDto.ClubId.Value);
                    if (club == null)
                    {
                        result.Failed++;
                        result.FailedPlayers.Add($"{playerDto.Name} - Club not found");
                        continue;
                    }
                }
                else if (!string.IsNullOrWhiteSpace(playerDto.Club))
                {
                    // Try to find club by name matching
                    club = FindBestMatchingClub(playerDto.Club, allClubs);
                    if (club != null)
                    {
                        _logger.LogDebug("Found matching club for '{PlayerClub}': {MatchedClub}", playerDto.Club, club.Name);
                    }
                    else
                    {
                        _logger.LogWarning("No matching club found for '{PlayerClub}'", playerDto.Club);
                    }
                }

                // Download and save image if PhotoUrl is provided
                string? localPhotoUrl = null;
                if (!string.IsNullOrWhiteSpace(playerDto.PhotoUrl))
                {
                    localPhotoUrl = await _imageService.DownloadAndSaveImageAsync(playerDto.PhotoUrl, playerDto.Name);
                }

                var player = new FootballPlayer
                {
                    Id = Guid.NewGuid(),
                    Name = playerDto.Name,
                    Position = playerDto.Position,
                    Nationality = playerDto.Nationality,
                    Club = playerDto.Club,
                    ClubId = club?.Id,
                    ClubEntity = club,
                    PhotoUrl = localPhotoUrl ?? playerDto.PhotoUrl
                };

                playersToAdd.Add(player);
                result.Created++;
            }
            catch (Exception ex)
            {
                result.Failed++;
                result.FailedPlayers.Add($"{playerDto.Name} - {ex.Message}");
            }
        }

        if (playersToAdd.Any())
        {
            _dbContext.FootballPlayers.AddRange(playersToAdd);
            await _dbContext.SaveChangesAsync();
        }

        _logger.LogInformation("Bulk player creation completed: {Created} created, {Skipped} skipped, {Failed} failed", 
            result.Created, result.Skipped, result.Failed);

        return Ok(result);
    }

    [HttpPut("players/{playerId}")]
    public async Task<ActionResult> UpdatePlayer(Guid playerId, UpdatePlayerDto updateDto)
    {
        var player = await _dbContext.FootballPlayers.FindAsync(playerId);
        if (player == null)
            return NotFound("Player not found");

        // Find matching club
        Club? club = null;
        if (updateDto.ClubId.HasValue)
        {
            club = await _dbContext.Clubs.FindAsync(updateDto.ClubId.Value);
            if (club == null)
                return BadRequest("Club not found");
        }
        else if (!string.IsNullOrWhiteSpace(updateDto.Club))
        {
            // Get all clubs for matching
            var allClubs = await _dbContext.Clubs.ToListAsync();
            club = FindBestMatchingClub(updateDto.Club, allClubs);
            if (club != null)
            {
                _logger.LogDebug("Found matching club for '{PlayerClub}': {MatchedClub}", updateDto.Club, club.Name);
            }
            else
            {
                _logger.LogWarning("No matching club found for '{PlayerClub}'", updateDto.Club);
            }
        }

        // Handle image update
        if (updateDto.PhotoUrl != player.PhotoUrl)
        {
            // Delete old image if it's a local file
            if (!string.IsNullOrWhiteSpace(player.PhotoUrl) && (player.PhotoUrl.StartsWith("/") || player.PhotoUrl.StartsWith("images/")))
            {
                await _imageService.DeleteImageAsync(player.PhotoUrl);
            }

            // Download and save new image if PhotoUrl is provided
            string? localPhotoUrl = null;
            if (!string.IsNullOrWhiteSpace(updateDto.PhotoUrl))
            {
                localPhotoUrl = await _imageService.DownloadAndSaveImageAsync(updateDto.PhotoUrl, updateDto.Name);
            }

            player.PhotoUrl = localPhotoUrl ?? updateDto.PhotoUrl;
        }

        player.Name = updateDto.Name;
        player.Position = updateDto.Position;
        player.Nationality = updateDto.Nationality;
        player.Club = updateDto.Club; // For backward compatibility
        player.ClubId = club?.Id;
        player.ClubEntity = club;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Player updated: {PlayerName} ({Position}) - Club: {Club}", 
            player.Name, player.Position, club?.Name ?? updateDto.Club ?? "None");
        return Ok("Player updated successfully");
    }

    [HttpDelete("players/{playerId}")]
    public async Task<ActionResult> DeletePlayer(Guid playerId)
    {
        var player = await _dbContext.FootballPlayers.FindAsync(playerId);
        if (player == null)
            return NotFound("Player not found");

        // Check if player is used in any lineups
        var isPlayerInUse = await _dbContext.LineupLines.AnyAsync(ll => ll.FootballPlayer.Id == playerId);
        if (isPlayerInUse)
        {
            return BadRequest("Cannot delete player: player is currently used in lineups");
        }

        // Delete associated image file if it's a local file
        if (!string.IsNullOrWhiteSpace(player.PhotoUrl) && (player.PhotoUrl.StartsWith("/") || player.PhotoUrl.StartsWith("images/")))
        {
            await _imageService.DeleteImageAsync(player.PhotoUrl);
        }

        _dbContext.FootballPlayers.Remove(player);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Player deleted: {PlayerName}", player.Name);
        return Ok("Player deleted successfully");
    }

    // CLUBS MANAGEMENT
    [HttpGet("clubs")]
    public async Task<ActionResult<List<ClubDto>>> GetAllClubs()
    {
        var clubs = await _dbContext.Clubs
            .Select(c => new ClubDto
            {
                Id = c.Id,
                Name = c.Name,
                ShortName = c.ShortName,
                LogoUrl = c.LogoUrl,
                Country = c.Country,
                PlayerCount = c.Players.Count,
                CreatedAt = c.CreatedAt
            })
            .OrderBy(c => c.Name)
            .ToListAsync();

        return Ok(clubs);
    }

    [HttpPost("clubs")]
    public async Task<ActionResult<ClubDto>> CreateClub(CreateClubDto createDto)
    {
        // Check if club already exists
        var existingClub = await _dbContext.Clubs
            .FirstOrDefaultAsync(c => c.Name.ToLower() == createDto.Name.ToLower());
        
        if (existingClub != null)
            return BadRequest("Club with this name already exists");

        var club = new Club
        {
            Id = Guid.NewGuid(),
            Name = createDto.Name,
            ShortName = createDto.ShortName,
            LogoUrl = createDto.LogoUrl,
            Country = createDto.Country
        };

        _dbContext.Clubs.Add(club);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("New club created: {ClubName}", club.Name);

        return CreatedAtAction(nameof(GetAllClubs), new { id = club.Id }, new ClubDto
        {
            Id = club.Id,
            Name = club.Name,
            ShortName = club.ShortName,
            LogoUrl = club.LogoUrl,
            Country = club.Country,
            PlayerCount = 0,
            CreatedAt = club.CreatedAt
        });
    }

    [HttpPut("clubs/{clubId}")]
    public async Task<ActionResult> UpdateClub(Guid clubId, UpdateClubDto updateDto)
    {
        var club = await _dbContext.Clubs.FindAsync(clubId);
        if (club == null)
            return NotFound("Club not found");

        club.Name = updateDto.Name;
        club.ShortName = updateDto.ShortName;
        club.LogoUrl = updateDto.LogoUrl;
        club.Country = updateDto.Country;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Club updated: {ClubName}", club.Name);
        return Ok("Club updated successfully");
    }

    [HttpDelete("clubs/{clubId}")]
    public async Task<ActionResult> DeleteClub(Guid clubId)
    {
        var club = await _dbContext.Clubs
            .Include(c => c.Players)
            .FirstOrDefaultAsync(c => c.Id == clubId);
        
        if (club == null)
            return NotFound("Club not found");

        if (club.Players.Any())
        {
            return BadRequest($"Cannot delete club: {club.Players.Count} players are assigned to this club. Please reassign or remove players first.");
        }

        _dbContext.Clubs.Remove(club);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Club deleted: {ClubName}", club.Name);
        return Ok("Club deleted successfully");
    }

    [HttpPost("clubs/bulk-update")]
    public async Task<ActionResult> BulkUpdatePlayerClubs(BulkUpdateClubsDto updateDto)
    {
        var players = await _dbContext.FootballPlayers
            .Where(p => updateDto.PlayerIds.Contains(p.Id))
            .ToListAsync();

        if (!players.Any())
            return NotFound("No players found");

        // Find matching club
        Club? club = null;
        if (updateDto.NewClubId.HasValue)
        {
            club = await _dbContext.Clubs.FindAsync(updateDto.NewClubId.Value);
            if (club == null)
                return BadRequest("Club not found");
        }
        else if (!string.IsNullOrWhiteSpace(updateDto.NewClub))
        {
            // Get all clubs for matching
            var allClubs = await _dbContext.Clubs.ToListAsync();
            club = FindBestMatchingClub(updateDto.NewClub, allClubs);
            if (club != null)
            {
                _logger.LogDebug("Found matching club for '{NewClub}': {MatchedClub}", updateDto.NewClub, club.Name);
            }
            else
            {
                _logger.LogWarning("No matching club found for '{NewClub}'", updateDto.NewClub);
            }
        }

        foreach (var player in players)
        {
            player.Club = updateDto.NewClub; // For backward compatibility
            player.ClubId = club?.Id;
        }

        await _dbContext.SaveChangesAsync();

        var clubName = club?.Name ?? updateDto.NewClub ?? "None";

        _logger.LogInformation("Bulk updated {Count} players to club: {Club}", players.Count, clubName);
        return Ok($"Updated {players.Count} players to club: {clubName}");
    }

    // MATCH MANAGEMENT
    [HttpGet("matches")]
    public async Task<ActionResult<List<MatchDto>>> GetAllMatches([FromQuery] bool onlyCompleted = false)
    {
        var query = _dbContext.Matches
            .Include(m => m.PlayerPerformances)
            .ThenInclude(pp => pp.Player)
            .AsQueryable();
        foreach (var match in query)
        {
                match.IsCompleted = match.MatchDate < DateTime.UtcNow; // Ensure future matches are marked as not completed
        }
        
        
        
        if (onlyCompleted)
            query = query.Where(m => m.IsCompleted);

        var matches = await query
            .OrderBy(m => m.MatchDate)
            .Select(m => new MatchDto
            {
                Id = m.Id,
                HomeTeam = m.HomeTeam,
                AwayTeam = m.AwayTeam,
                HomeScore = m.HomeScore,
                AwayScore = m.AwayScore,
                MatchDate = m.MatchDate,
                Competition = m.Competition,
                Round = m.Round,
                IsCompleted = m.IsCompleted,
                CreatedAt = m.CreatedAt,
                PlayerPerformances = m.PlayerPerformances.Select(pp => new PlayerPerformanceDto
                {
                    Id = pp.Id,
                    MatchId = pp.Match.Id,
                    PlayerId = pp.Player.Id,
                    PlayerName = pp.Player.Name,
                    PlayerPosition = pp.Player.Position,
                    PlayerClub = pp.Player.Club ?? "",
                    Goals = pp.Goals,
                    PenaltyGoals = pp.PenaltyGoals,
                    PenaltiesMissed = pp.PenaltiesMissed,
                    OwnGoals = pp.OwnGoals,
                    Assists = pp.Assists,
                    YellowCards = pp.YellowCards,
                    RedCards = pp.RedCards,
                    Played = pp.Played,
                    Points = pp.Points
                }).ToList()
            })
            .ToListAsync();
        await _dbContext.SaveChangesAsync();
        return Ok(matches);
    }

    [HttpGet("matches/{matchId}")]
    public async Task<ActionResult<MatchDto>> GetMatch(Guid matchId)
    {
        var match = await _dbContext.Matches
            .Include(m => m.PlayerPerformances)
            .ThenInclude(pp => pp.Player)
            .FirstOrDefaultAsync(m => m.Id == matchId);

        if (match == null)
            return NotFound("Match not found");

        var matchDto = new MatchDto
        {
            Id = match.Id,
            HomeTeam = match.HomeTeam,
            AwayTeam = match.AwayTeam,
            HomeScore = match.HomeScore,
            AwayScore = match.AwayScore,
            MatchDate = match.MatchDate,
            Competition = match.Competition,
            Round = match.Round,
            IsCompleted = match.IsCompleted,
            CreatedAt = match.CreatedAt,
            PlayerPerformances = match.PlayerPerformances.Select(pp => new PlayerPerformanceDto
            {
                Id = pp.Id,
                MatchId = pp.Match.Id,
                PlayerId = pp.Player.Id,
                PlayerName = pp.Player.Name,
                PlayerPosition = pp.Player.Position,
                PlayerClub = pp.Player.Club ?? "",
                Goals = pp.Goals,
                PenaltyGoals = pp.PenaltyGoals,
                PenaltiesMissed = pp.PenaltiesMissed,
                OwnGoals = pp.OwnGoals,
                Assists = pp.Assists,
                YellowCards = pp.YellowCards,
                RedCards = pp.RedCards,
                Played = pp.Played,
                Points = pp.Points
            }).ToList()
        };

        return Ok(matchDto);
    }

    [HttpPost("matches")]
    public async Task<ActionResult<MatchDto>> CreateMatch(CreateMatchDto createDto)
    {
        var match = new Match
        {
            Id = Guid.NewGuid(),
            HomeTeam = createDto.HomeTeam,
            AwayTeam = createDto.AwayTeam,
            MatchDate = createDto.MatchDate,
            Competition = createDto.Competition,
            Round = createDto.Round,
            IsCompleted = false
        };

        _dbContext.Matches.Add(match);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("New match created: {HomeTeam} vs {AwayTeam} on {MatchDate}", 
            match.HomeTeam, match.AwayTeam, match.MatchDate.ToString("yyyy-MM-dd"));

        return CreatedAtAction(nameof(GetMatch), new { matchId = match.Id }, new MatchDto
        {
            Id = match.Id,
            HomeTeam = match.HomeTeam,
            AwayTeam = match.AwayTeam,
            HomeScore = match.HomeScore,
            AwayScore = match.AwayScore,
            MatchDate = match.MatchDate,
            Competition = match.Competition,
            Round = match.Round,
            IsCompleted = match.IsCompleted,
            CreatedAt = match.CreatedAt,
            PlayerPerformances = new List<PlayerPerformanceDto>()
        });
    }

    [HttpPut("matches/{matchId}")]
    public async Task<ActionResult> UpdateMatch(Guid matchId, UpdateMatchDto updateDto)
    {
        var match = await _dbContext.Matches.FindAsync(matchId);
        if (match == null)
            return NotFound("Match not found");

        match.HomeTeam = updateDto.HomeTeam;
        match.AwayTeam = updateDto.AwayTeam;
        match.HomeScore = updateDto.HomeScore;
        match.AwayScore = updateDto.AwayScore;
        match.MatchDate = updateDto.MatchDate;
        match.Competition = updateDto.Competition;
        match.Round = updateDto.Round;
        match.IsCompleted = updateDto.IsCompleted;

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Match updated: {HomeTeam} vs {AwayTeam}", match.HomeTeam, match.AwayTeam);
        return Ok("Match updated successfully");
    }

    [HttpDelete("matches/{matchId}")]
    public async Task<ActionResult> DeleteMatch(Guid matchId)
    {
        var match = await _dbContext.Matches
            .Include(m => m.PlayerPerformances)
            .FirstOrDefaultAsync(m => m.Id == matchId);
        
        if (match == null)
            return NotFound("Match not found");

        // Remove all player performances first
        if (match.PlayerPerformances.Any())
        {
            _dbContext.PlayerPerformances.RemoveRange(match.PlayerPerformances);
        }

        _dbContext.Matches.Remove(match);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Match deleted: {HomeTeam} vs {AwayTeam}", match.HomeTeam, match.AwayTeam);
        return Ok("Match deleted successfully");
    }

    // PLAYER PERFORMANCE MANAGEMENT
    [HttpGet("matches/{matchId}/performances")]
    public async Task<ActionResult<List<PlayerPerformanceDto>>> GetMatchPerformances(Guid matchId)
    {
        var match = await _dbContext.Matches.FindAsync(matchId);
        if (match == null)
            return NotFound("Match not found");

        var performances = await _dbContext.PlayerPerformances
            .Include(pp => pp.Player)
            .Where(pp => pp.Match.Id == matchId)
            .Select(pp => new PlayerPerformanceDto
            {
                Id = pp.Id,
                MatchId = pp.Match.Id,
                PlayerId = pp.Player.Id,
                PlayerName = pp.Player.Name,
                PlayerPosition = pp.Player.Position,
                PlayerClub = pp.Player.Club ?? "",
                Goals = pp.Goals,
                PenaltyGoals = pp.PenaltyGoals,
                PenaltiesMissed = pp.PenaltiesMissed,
                OwnGoals = pp.OwnGoals,
                Assists = pp.Assists,
                YellowCards = pp.YellowCards,
                RedCards = pp.RedCards,
                Played = pp.Played,
                Points = pp.Points
            })
            .ToListAsync();

        return Ok(performances);
    }

    [HttpPost("matches/{matchId}/performances")]
    public async Task<ActionResult<PlayerPerformanceDto>> AddPlayerPerformance(Guid matchId, CreatePlayerPerformanceDto createDto)
    {
        var match = await _dbContext.Matches.FindAsync(matchId);
        if (match == null)
            return NotFound("Match not found");

        var player = await _dbContext.FootballPlayers.FindAsync(createDto.PlayerId);
        if (player == null)
            return NotFound("Player not found");

        // Check if performance already exists for this player in this match
        var existingPerformance = await _dbContext.PlayerPerformances
            .FirstOrDefaultAsync(pp => pp.Match.Id == matchId && pp.Player.Id == createDto.PlayerId);
        
        if (existingPerformance != null)
            return BadRequest("Performance for this player in this match already exists");

        var performance = new PlayerPerformance
        {
            Id = Guid.NewGuid(),
            Match = match,
            Player = player,
            Goals = createDto.Goals,
            PenaltyGoals = createDto.PenaltyGoals,
            PenaltiesMissed = createDto.PenaltiesMissed,
            OwnGoals = createDto.OwnGoals,
            Assists = createDto.Assists,
            YellowCards = createDto.YellowCards,
            RedCards = createDto.RedCards,
            Played = createDto.Played,
            Points = CalculateFantasyPoints(createDto, match, player.Club ?? "") // Calculate fantasy points with match context
        };

        _dbContext.PlayerPerformances.Add(performance);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Player performance added: {PlayerName} in {HomeTeam} vs {AwayTeam}", 
            player.Name, match.HomeTeam, match.AwayTeam);

        return CreatedAtAction(nameof(GetMatchPerformances), new { matchId = matchId }, new PlayerPerformanceDto
        {
            Id = performance.Id,
            MatchId = performance.Match.Id,
            PlayerId = performance.Player.Id,
            PlayerName = performance.Player.Name,
            PlayerPosition = performance.Player.Position,
            PlayerClub = performance.Player.Club ?? "",
            Goals = performance.Goals,
            PenaltyGoals = performance.PenaltyGoals,
            PenaltiesMissed = performance.PenaltiesMissed,
            OwnGoals = performance.OwnGoals,
            Assists = performance.Assists,
            YellowCards = performance.YellowCards,
            RedCards = performance.RedCards,
            Played = performance.Played,
            Points = performance.Points
        });
    }

    [HttpPut("performances/{performanceId}")]
    public async Task<ActionResult> UpdatePlayerPerformance(Guid performanceId, UpdatePlayerPerformanceDto updateDto)
    {
        var performance = await _dbContext.PlayerPerformances
            .Include(pp => pp.Player)
            .Include(pp => pp.Match)
            .FirstOrDefaultAsync(pp => pp.Id == performanceId);
        
        if (performance == null)
            return NotFound("Performance not found");

        performance.Goals = updateDto.Goals;
        performance.PenaltyGoals = updateDto.PenaltyGoals;
        performance.PenaltiesMissed = updateDto.PenaltiesMissed;
        performance.OwnGoals = updateDto.OwnGoals;
        performance.Assists = updateDto.Assists;
        performance.YellowCards = updateDto.YellowCards;
        performance.RedCards = updateDto.RedCards;
        performance.Played = updateDto.Played;
        performance.Points = CalculateFantasyPoints(updateDto, performance.Match, performance.Player.Club ?? ""); // Recalculate fantasy points with match context

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Player performance updated: {PlayerName}", performance.Player.Name);
        return Ok("Performance updated successfully");
    }

    [HttpDelete("performances/{performanceId}")]
    public async Task<ActionResult> DeletePlayerPerformance(Guid performanceId)
    {
        var performance = await _dbContext.PlayerPerformances
            .Include(pp => pp.Player)
            .FirstOrDefaultAsync(pp => pp.Id == performanceId);
        
        if (performance == null)
            return NotFound("Performance not found");

        _dbContext.PlayerPerformances.Remove(performance);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Player performance deleted: {PlayerName}", performance.Player.Name);
        return Ok("Performance deleted successfully");
    }

    [HttpPost("matches/{matchId}/performances/bulk")]
    public async Task<ActionResult> BulkAddPlayerPerformances(Guid matchId, BulkPlayerPerformanceDto bulkDto)
    {
        var match = await _dbContext.Matches.FindAsync(matchId);
        if (match == null)
            return NotFound("Match not found");

        var playerIds = bulkDto.Performances.Select(p => p.PlayerId).ToList();
        var players = await _dbContext.FootballPlayers
            .Where(p => playerIds.Contains(p.Id))
            .ToDictionaryAsync(p => p.Id, p => p);

        // Check for existing performances
        var existingPerformances = await _dbContext.PlayerPerformances
            .Where(pp => pp.Match.Id == matchId && playerIds.Contains(pp.Player.Id))
            .Select(pp => pp.Player.Id)
            .ToListAsync();

        var newPerformances = new List<PlayerPerformance>();
        var skippedCount = 0;

        foreach (var performanceDto in bulkDto.Performances)
        {
            if (existingPerformances.Contains(performanceDto.PlayerId))
            {
                skippedCount++;
                continue; // Skip if performance already exists
            }

            if (!players.ContainsKey(performanceDto.PlayerId))
            {
                skippedCount++;
                continue; // Skip if player not found
            }

            var performance = new PlayerPerformance
            {
                Id = Guid.NewGuid(),
                Match = match,
                Player = players[performanceDto.PlayerId],
                Goals = performanceDto.Goals,
                PenaltyGoals = performanceDto.PenaltyGoals,
                PenaltiesMissed = performanceDto.PenaltiesMissed,
                OwnGoals = performanceDto.OwnGoals,
                Assists = performanceDto.Assists,
                YellowCards = performanceDto.YellowCards,
                RedCards = performanceDto.RedCards,
                Played = performanceDto.Played,
                Points = CalculateFantasyPoints(performanceDto, match, players[performanceDto.PlayerId].Club ?? "") // Calculate with match context
            };

            newPerformances.Add(performance);
        }

        if (newPerformances.Any())
        {
            _dbContext.PlayerPerformances.AddRange(newPerformances);
            await _dbContext.SaveChangesAsync();
        }

        _logger.LogInformation("Bulk added {Count} player performances for match {HomeTeam} vs {AwayTeam}. Skipped: {SkippedCount}", 
            newPerformances.Count, match.HomeTeam, match.AwayTeam, skippedCount);

        return Ok($"Added {newPerformances.Count} performances. Skipped {skippedCount} (already exist or player not found).");
    }

    private int CalculateFantasyPoints(CreatePlayerPerformanceDto dto)
    {
        // For create operations, we need match context for win/draw/lose points
        // This will be handled in the calling method where we have access to the match
        return CalculateFantasyPoints(dto.Goals, dto.PenaltyGoals, dto.PenaltiesMissed, dto.OwnGoals, dto.Assists, dto.YellowCards, dto.RedCards, dto.Played);
    }

    private int CalculateFantasyPoints(UpdatePlayerPerformanceDto dto)
    {
        return CalculateFantasyPoints(dto.Goals, dto.PenaltyGoals, dto.PenaltiesMissed, dto.OwnGoals, dto.Assists, dto.YellowCards, dto.RedCards, dto.Played);
    }

    private int CalculateFantasyPoints(CreatePlayerPerformanceDto dto, Match match, string playerClub)
    {
        return CalculateFantasyPoints(dto.Goals, dto.PenaltyGoals, dto.PenaltiesMissed, dto.OwnGoals, dto.Assists, dto.YellowCards, dto.RedCards, dto.Played, match, playerClub);
    }

    private int CalculateFantasyPoints(UpdatePlayerPerformanceDto dto, Match match, string playerClub)
    {
        return CalculateFantasyPoints(dto.Goals, dto.PenaltyGoals, dto.PenaltiesMissed, dto.OwnGoals, dto.Assists, dto.YellowCards, dto.RedCards, dto.Played, match, playerClub);
    }

    private int CalculateFantasyPoints(int goals, int penaltyGoals, int penaltiesMissed, int ownGoals, int assists, int yellowCards, int redCards, bool played)
    {
        // Legacy method without match context - used for backward compatibility
        return CalculateFantasyPoints(goals, penaltyGoals, penaltiesMissed, ownGoals, assists, yellowCards, redCards, played, null, null);
    }

    private int CalculateFantasyPoints(int goals, int penaltyGoals, int penaltiesMissed, int ownGoals, int assists, int yellowCards, int redCards, bool played, Match? match, string? playerClub)
    {
        // If player didn't play, they get 0 points regardless of team result
        if (!played) return 0;
        
        int points = 0; // No base points
        
        // Regular goals (excluding penalty goals)
        var regularGoals = Math.Max(0, goals - penaltyGoals);
        points += regularGoals * 6; // 6 points per regular goal
        
        // Penalty goals
        points += penaltyGoals * 4; // 4 points per penalty goal
        
        // Other stats
        points -= penaltiesMissed * 2; // -2 points for missed penalties
        points -= ownGoals * 2; // -2 points for own goals
        points += assists * 4; // 4 points per assist
        points -= yellowCards * 1; // -1 point per yellow card
        points -= redCards * 3; // -3 points per red card
        
        // Win/Draw/Lose points (only if match is completed and player actually played)
        if (match != null && match.IsCompleted && !string.IsNullOrEmpty(playerClub) && match.HomeScore.HasValue && match.AwayScore.HasValue)
        {
            var homeScore = match.HomeScore.Value;
            var awayScore = match.AwayScore.Value;
            
            if (homeScore == awayScore)
            {
                // Draw
                points += 1;
            }
            else if ((playerClub == match.HomeTeam && homeScore > awayScore) ||
                     (playerClub == match.AwayTeam && awayScore > homeScore))
            {
                // Win
                points += 3;
            }
            // Lose = 0 points (no addition needed)
        }
        
        return Math.Max(0, points);
    }

    // Helper method to ensure DateTime is UTC for PostgreSQL compatibility
    private static DateTime EnsureUtc(DateTime dateTime)
    {
        return dateTime.Kind switch
        {
            DateTimeKind.Utc => dateTime,
            DateTimeKind.Local => dateTime.ToUniversalTime(),
            DateTimeKind.Unspecified => DateTime.SpecifyKind(dateTime, DateTimeKind.Utc),
            _ => dateTime
        };
    }

    // Helper method to find the best matching club by name
    private static Club? FindBestMatchingClub(string playerClubName, List<Club> allClubs)
    {
        if (string.IsNullOrWhiteSpace(playerClubName) || !allClubs.Any())
            return null;

        var normalizedPlayerClub = playerClubName.Trim().ToLowerInvariant();

        // First, try exact match
        var exactMatch = allClubs.FirstOrDefault(c => 
            c.Name.Equals(playerClubName, StringComparison.OrdinalIgnoreCase));
        if (exactMatch != null)
            return exactMatch;

        // Second, try contains match (player club name contains database club name or vice versa)
        var containsMatch = allClubs.FirstOrDefault(c => 
            normalizedPlayerClub.Contains(c.Name.ToLowerInvariant()) || 
            c.Name.ToLowerInvariant().Contains(normalizedPlayerClub));
        if (containsMatch != null)
            return containsMatch;

        // Third, try similarity matching using Levenshtein distance
        var bestMatch = allClubs
            .Select(c => new { Club = c, Distance = CalculateLevenshteinDistance(normalizedPlayerClub, c.Name.ToLowerInvariant()) })
            .Where(x => x.Distance <= Math.Max(normalizedPlayerClub.Length, x.Club.Name.Length) * 0.3) // Allow 30% difference
            .OrderBy(x => x.Distance)
            .FirstOrDefault();

        return bestMatch?.Club;
    }

    // Helper method to calculate Levenshtein distance for string similarity
    private static int CalculateLevenshteinDistance(string s1, string s2)
    {
        int[,] d = new int[s1.Length + 1, s2.Length + 1];

        for (int i = 0; i <= s1.Length; i++)
            d[i, 0] = i;

        for (int j = 0; j <= s2.Length; j++)
            d[0, j] = j;

        for (int i = 1; i <= s1.Length; i++)
        {
            for (int j = 1; j <= s2.Length; j++)
            {
                int cost = s1[i - 1] == s2[j - 1] ? 0 : 1;
                d[i, j] = Math.Min(Math.Min(d[i - 1, j] + 1, d[i, j - 1] + 1), d[i - 1, j - 1] + cost);
            }
        }

        return d[s1.Length, s2.Length];
    }

    // DATABASE STATISTICS
    [HttpGet("stats")]
    public async Task<ActionResult<AdminStatsDto>> GetDatabaseStats()
    {
        var stats = new AdminStatsDto
        {
            TotalUsers = await _userManager.Users.CountAsync(),
            TotalAdmins = (await _userManager.GetUsersInRoleAsync("Admin")).Count,
            TotalPlayers = await _dbContext.FootballPlayers.CountAsync(),
            TotalClubs = await _dbContext.Clubs.CountAsync(),
            TotalMatches = await _dbContext.Matches.CountAsync(),
            CompletedMatches = await _dbContext.Matches.CountAsync(m => m.IsCompleted),
            TotalPools = await _dbContext.Pools.CountAsync(),
            TotalLineups = await _dbContext.Lineups.CountAsync(),
            PlayersByPosition = await _dbContext.FootballPlayers
                .GroupBy(p => p.Position)
                .Select(g => new PositionStatsDto { Position = g.Key, Count = g.Count() })
                .ToListAsync(),
            PlayersByNationality = await _dbContext.FootballPlayers
                .GroupBy(p => p.Nationality)
                .Select(g => new NationalityStatsDto { Nationality = g.Key, Count = g.Count() })
                .OrderByDescending(n => n.Count)
                .Take(10)
                .ToListAsync(),
            PlayersByClub = await _dbContext.FootballPlayers
                .Where(p => !string.IsNullOrEmpty(p.Club))
                .GroupBy(p => p.Club!)
                .Select(g => new ClubStatsDto { Club = g.Key, Count = g.Count() })
                .OrderByDescending(c => c.Count)
                .Take(10)
                .ToListAsync()
        };

        return Ok(stats);
    }

    // MATCH PROCESSING (placeholder for future implementation)
    [HttpPost("matches/process")]
    public ActionResult ProcessMatches(ProcessMatchesDto processDto)
    {
        // Placeholder for match processing logic
        // This would typically involve:
        // 1. Processing match results
        // 2. Calculating player scores
        // 3. Updating user scores in pools
        
        _logger.LogInformation("Match processing requested (not yet implemented)");
        return Ok("Match processing feature coming soon");
    }

    // SYSTEM MAINTENANCE
    [HttpPost("maintenance/cleanup")]
    public async Task<ActionResult> CleanupDatabase()
    {
        var deleted = 0;

        // Clean up incomplete lineups (simplified - remove all incomplete lineups)
        var oldIncompleteLineups = await _dbContext.Lineups
            .Where(l => !l.Complete)
            .ToListAsync();

        if (oldIncompleteLineups.Any())
        {
            _dbContext.Lineups.RemoveRange(oldIncompleteLineups);
            deleted = oldIncompleteLineups.Count;
        }

        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Database cleanup completed. Removed {Count} incomplete lineups", deleted);
        return Ok($"Cleanup completed. Removed {deleted} incomplete lineups");
    }

    // LEAGUE MANAGEMENT
    [HttpGet("leagues")]
    public async Task<ActionResult<List<AdminLeagueDto>>> GetAllLeagues()
    {
        try
        {
            var leagues = await _dbContext.Leagues
                .Include(l => l.Clubs)
                .Include(l => l.Matches)
                .OrderBy(l => l.Name)
                .ToListAsync();

            var leagueDtos = leagues.Select(l => new AdminLeagueDto
            {
                Id = l.Id.ToString(),
                Name = l.Name,
                ShortName = l.ShortName,
                Country = l.Country,
                LogoUrl = l.LogoUrl,
                GoogleCalendarId = l.GoogleCalendarId,
                IsActive = l.IsActive,
                CreatedAt = l.CreatedAt,
                LastSyncAt = l.LastSyncAt,
                ClubCount = l.Clubs?.Count ?? 0,
                MatchCount = l.Matches?.Count ?? 0
            }).ToList();

            return Ok(leagueDtos);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting leagues");
            return StatusCode(500, $"Error getting leagues: {ex.Message}");
        }
    }

    [HttpGet("leagues/{id}")]
    public async Task<ActionResult<AdminLeagueDto>> GetLeague(string id)
    {
        if (!Guid.TryParse(id, out var leagueId))
        {
            return BadRequest("Invalid league ID format");
        }

        var league = await _dbContext.Leagues
            .Include(l => l.Clubs)
            .Include(l => l.Matches)
            .FirstOrDefaultAsync(l => l.Id == leagueId);

        if (league == null)
        {
            return NotFound($"League with ID {id} not found");
        }

        var leagueDto = new AdminLeagueDto
        {
            Id = league.Id.ToString(),
            Name = league.Name,
            ShortName = league.ShortName,
            Country = league.Country,
            LogoUrl = league.LogoUrl,
            GoogleCalendarId = league.GoogleCalendarId,
            IsActive = league.IsActive,
            CreatedAt = league.CreatedAt,
            LastSyncAt = league.LastSyncAt,
            ClubCount = league.Clubs?.Count ?? 0,
            MatchCount = league.Matches?.Count ?? 0
        };

        return Ok(leagueDto);
    }

    [HttpPost("leagues")]
    public async Task<ActionResult<AdminLeagueDto>> CreateLeague([FromBody] CreateLeagueRequestDto request)
    {
        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        // Check if league with same name already exists
        var existingLeague = await _dbContext.Leagues
            .FirstOrDefaultAsync(l => l.Name.ToLower() == request.Name.ToLower());

        if (existingLeague != null)
        {
            return BadRequest($"League with name '{request.Name}' already exists");
        }

        var league = new League
        {
            Id = Guid.NewGuid(),
            Name = request.Name,
            ShortName = request.ShortName,
            Country = request.Country,
            LogoUrl = request.LogoUrl,
            GoogleCalendarId = request.GoogleCalendarId,
            IsActive = request.IsActive,
            CreatedAt = DateTime.UtcNow
        };

        _dbContext.Leagues.Add(league);
        await _dbContext.SaveChangesAsync();

        var leagueDto = new AdminLeagueDto
        {
            Id = league.Id.ToString(),
            Name = league.Name,
            ShortName = league.ShortName,
            Country = league.Country,
            LogoUrl = league.LogoUrl,
            GoogleCalendarId = league.GoogleCalendarId,
            IsActive = league.IsActive,
            CreatedAt = league.CreatedAt,
            LastSyncAt = league.LastSyncAt,
            ClubCount = 0,
            MatchCount = 0
        };

        _logger.LogInformation("Created new league: {LeagueName} (ID: {LeagueId})", league.Name, league.Id);
        return CreatedAtAction(nameof(GetLeague), new { id = league.Id.ToString() }, leagueDto);
    }

    [HttpPut("leagues/{id}")]
    public async Task<ActionResult<AdminLeagueDto>> UpdateLeague(string id, [FromBody] UpdateLeagueRequestDto request)
    {
        if (!Guid.TryParse(id, out var leagueId))
        {
            return BadRequest("Invalid league ID format");
        }

        if (!ModelState.IsValid)
        {
            return BadRequest(ModelState);
        }

        var league = await _dbContext.Leagues
            .Include(l => l.Clubs)
            .Include(l => l.Matches)
            .FirstOrDefaultAsync(l => l.Id == leagueId);

        if (league == null)
        {
            return NotFound($"League with ID {id} not found");
        }

        // Check if another league with the same name exists
        var existingLeague = await _dbContext.Leagues
            .FirstOrDefaultAsync(l => l.Name.ToLower() == request.Name.ToLower() && l.Id != leagueId);

        if (existingLeague != null)
        {
            return BadRequest($"Another league with name '{request.Name}' already exists");
        }

        league.Name = request.Name;
        league.ShortName = request.ShortName;
        league.Country = request.Country;
        league.LogoUrl = request.LogoUrl;
        league.GoogleCalendarId = request.GoogleCalendarId;
        league.IsActive = request.IsActive;

        await _dbContext.SaveChangesAsync();

        var leagueDto = new AdminLeagueDto
        {
            Id = league.Id.ToString(),
            Name = league.Name,
            ShortName = league.ShortName,
            Country = league.Country,
            LogoUrl = league.LogoUrl,
            GoogleCalendarId = league.GoogleCalendarId,
            IsActive = league.IsActive,
            CreatedAt = league.CreatedAt,
            LastSyncAt = league.LastSyncAt,
            ClubCount = league.Clubs?.Count ?? 0,
            MatchCount = league.Matches?.Count ?? 0
        };

        _logger.LogInformation("Updated league: {LeagueName} (ID: {LeagueId})", league.Name, league.Id);
        return Ok(leagueDto);
    }

    [HttpDelete("leagues/{id}")]
    public async Task<ActionResult> DeleteLeague(string id)
    {
        if (!Guid.TryParse(id, out var leagueId))
        {
            return BadRequest("Invalid league ID format");
        }

        var league = await _dbContext.Leagues
            .Include(l => l.Clubs)
            .Include(l => l.Matches)
            .FirstOrDefaultAsync(l => l.Id == leagueId);

        if (league == null)
        {
            return NotFound($"League with ID {id} not found");
        }

        // Check if league has associated clubs or matches
        var clubCount = league.Clubs?.Count ?? 0;
        var matchCount = league.Matches?.Count ?? 0;

        if (clubCount > 0 || matchCount > 0)
        {
            return BadRequest($"Cannot delete league '{league.Name}' because it has {clubCount} clubs and {matchCount} matches. Please remove or reassign them first.");
        }

        _dbContext.Leagues.Remove(league);
        await _dbContext.SaveChangesAsync();

        _logger.LogInformation("Deleted league: {LeagueName} (ID: {LeagueId})", league.Name, league.Id);
        return NoContent();
    }

    // GOOGLE CALENDAR INTEGRATION
    [HttpPost("leagues/{id}/sync-calendar")]
    public async Task<ActionResult> SyncGoogleCalendar(string id, [FromBody] GoogleCalendarSyncRequestDto request)
    {
        if (!Guid.TryParse(id, out var leagueId))
        {
            return BadRequest("Invalid league ID format");
        }

        var league = await _dbContext.Leagues.FirstOrDefaultAsync(l => l.Id == leagueId);
        if (league == null)
        {
            return NotFound($"League with ID {id} not found");
        }

        try
        {
            // Update league's calendar ID if provided
            if (!string.IsNullOrEmpty(request.GoogleCalendarId))
            {
                league.GoogleCalendarId = request.GoogleCalendarId;
            }

            if (string.IsNullOrEmpty(league.GoogleCalendarId))
            {
                return BadRequest("No Google Calendar ID configured for this league");
            }

            // Set default date range if not provided
            var startDate = request.StartDate ?? DateTime.UtcNow.AddMonths(-1); // Default: 1 month ago
            var endDate = request.EndDate ?? DateTime.UtcNow.AddMonths(6);      // Default: 6 months ahead

            // Get calendar events (placeholder data for now)
            var calendarMatches = await GetCalendarMatchesAsync(league.GoogleCalendarId!, startDate, endDate);
            
            // Log detailed information for debugging
            _logger.LogWarning("🔍 SYNC DEBUG INFO:");
            _logger.LogWarning("📅 Calendar ID: {CalendarId}", league.GoogleCalendarId);
            _logger.LogWarning("📊 Found {EventCount} calendar events (MOCK DATA)", calendarMatches.Count);
            foreach (var match in calendarMatches)
            {
                _logger.LogWarning("⚽ Event: {HomeTeam} vs {AwayTeam} on {MatchDate}", 
                    match.HomeTeam, match.AwayTeam, match.MatchDate.ToString("yyyy-MM-dd"));
            }
            
            // Sync clubs and matches
            var syncResult = await SyncClubsAndMatchesAsync(league, calendarMatches, request.ForceSync);

            // Update last sync time
            league.LastSyncAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();

            var resultMessage = $"✅ Calendar sync completed for league '{league.Name}' from {startDate:yyyy-MM-dd} to {endDate:yyyy-MM-dd}. " +
                     $"Found {calendarMatches.Count} events. " +
                     $"Added: {syncResult.ClubsAdded} clubs, {syncResult.MatchesAdded} matches. " +
                     $"Unlinked from calendar: {syncResult.ClubsRemoved} clubs, {syncResult.MatchesRemoved} matches (matches preserved with IDs intact).";
            
            _logger.LogInformation("📝 {ResultMessage}", resultMessage);
            
            return Ok(resultMessage);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing Google Calendar for league {LeagueId}", leagueId);
            return StatusCode(500, "An error occurred while syncing the calendar");
        }
    }

    [HttpPost("test-calendar-access")]
    public async Task<ActionResult<object>> TestCalendarAccess([FromBody] TestCalendarAccessRequestDto request)
    {
        if (string.IsNullOrEmpty(request.CalendarId))
        {
            return BadRequest("Calendar ID is required");
        }

        try
        {
            var apiKey = _configuration["GoogleCalendar:ApiKey"];
            
            if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_GOOGLE_API_KEY_HERE")
            {
                return Ok(new
                {
                    Success = false,
                    Error = "Google Calendar API key is required",
                    Message = "Google now requires an API key for all calendar access, including public calendars",
                    Instructions = new[]
                    {
                        "1. Go to https://console.cloud.google.com/",
                        "2. Create a project or select existing one",
                        "3. Enable Google Calendar API", 
                        "4. Create credentials (API Key)",
                        "5. Set GoogleCalendar:ApiKey in appsettings.Development.json",
                        "6. Make sure your calendar is public"
                    }
                });
            }

            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(10);

            // Test with a simple calendar metadata call
            var testUrl = $"https://www.googleapis.com/calendar/v3/calendars/{Uri.EscapeDataString(request.CalendarId)}" +
                         $"?key={apiKey}";

            _logger.LogInformation("🧪 Testing calendar access for: {CalendarId}", request.CalendarId);

            var response = await httpClient.GetAsync(testUrl);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var calendarInfo = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(content);
                
                var calendarSummary = calendarInfo.TryGetProperty("summary", out var summaryProp) 
                    ? summaryProp.GetString() 
                    : "Unknown";

                return Ok(new
                {
                    Success = true,
                    Message = "✅ Calendar API access successful",
                    Method = "Google Calendar API (with API key)",
                    CalendarId = request.CalendarId,
                    CalendarName = calendarSummary,
                    ResponseSize = content.Length
                });
            }
            else
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogError("❌ Calendar access test failed: {StatusCode} - {Content}", response.StatusCode, errorContent);

                string errorMessage = response.StatusCode switch
                {
                    System.Net.HttpStatusCode.Forbidden => "Access forbidden - check if calendar is public and API key has Calendar API enabled",
                    System.Net.HttpStatusCode.NotFound => "Calendar not found - check if calendar ID is correct",
                    System.Net.HttpStatusCode.BadRequest => "Bad request - check calendar ID format",
                    _ => $"HTTP {(int)response.StatusCode}: {errorContent}"
                };

                return Ok(new
                {
                    Success = false,
                    Error = errorMessage,
                    StatusCode = (int)response.StatusCode,
                    CalendarId = request.CalendarId,
                    Suggestion = "Google now requires API keys for all calendar access. Make sure your API key is valid and calendar is public."
                });
            }
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Exception during calendar access test for {CalendarId}", request.CalendarId);
            return Ok(new
            {
                Success = false,
                Error = "Exception occurred during test",
                Message = ex.Message,
                CalendarId = request.CalendarId
            });
        }
    }

    private async Task<(bool Success, object Result)> TestWithGoogleApi(HttpClient httpClient, string calendarId, string apiKey)
    {
        try
        {
            var testUrl = $"https://www.googleapis.com/calendar/v3/calendars/{Uri.EscapeDataString(calendarId)}" +
                         $"?key={apiKey}";

            var response = await httpClient.GetAsync(testUrl);
            
            if (response.IsSuccessStatusCode)
            {
                var content = await response.Content.ReadAsStringAsync();
                var calendarInfo = System.Text.Json.JsonSerializer.Deserialize<JsonElement>(content);
                
                var calendarSummary = calendarInfo.TryGetProperty("summary", out var summaryProp) 
                    ? summaryProp.GetString() 
                    : "Unknown";

                return (true, new
                {
                    Success = true,
                    Message = "✅ Calendar API access successful",
                    Method = "Google Calendar API (with API key)",
                    CalendarId = calendarId,
                    CalendarName = calendarSummary,
                    ResponseSize = content.Length
                });
            }
            else
            {
                return (false, new { Success = false });
            }
        }
        catch
        {
            return (false, new { Success = false });
        }
    }

    [HttpPost("debug-calendar/{leagueId}")]
    public async Task<ActionResult<object>> DebugCalendarData(string leagueId, [FromBody] GoogleCalendarSyncRequestDto request)
    {
        try
        {
            if (!Guid.TryParse(leagueId, out var id))
            {
                return BadRequest("Invalid league ID format");
            }

            var league = await _dbContext.Leagues.FirstOrDefaultAsync(l => l.Id == id);
            if (league == null)
            {
                return NotFound($"League with ID {leagueId} not found");
            }

            var startDate = request.StartDate ?? DateTime.UtcNow.AddMonths(-1);
            var endDate = request.EndDate ?? DateTime.UtcNow.AddMonths(6);

            // Get what the sync would process
            var calendarMatches = await GetCalendarMatchesAsync(request.GoogleCalendarId, startDate, endDate);
            
            // Get existing data
            var existingClubs = await _dbContext.Clubs.Where(c => c.LeagueId == league.Id).ToListAsync();
            var existingMatches = await _dbContext.Matches.Where(m => m.LeagueId == league.Id).ToListAsync();

            var debugInfo = new
            {
                League = new { league.Id, league.Name, league.GoogleCalendarId },
                RequestInfo = new
                {
                    CalendarId = request.GoogleCalendarId,
                    StartDate = startDate.ToString("yyyy-MM-dd"),
                    EndDate = endDate.ToString("yyyy-MM-dd"),
                    ForceSync = request.ForceSync
                },
                CalendarData = new
                {
                    EventCount = calendarMatches.Count,
                    Events = calendarMatches.Select(m => new
                    {
                        m.EventId,
                        m.HomeTeam,
                        m.AwayTeam,
                        MatchDate = m.MatchDate.ToString("yyyy-MM-dd HH:mm"),
                        m.Competition,
                        m.Round
                    }).ToList()
                },
                ExistingData = new
                {
                    ClubCount = existingClubs.Count,
                    Clubs = existingClubs.Select(c => new { c.Id, c.Name }).ToList(),
                    MatchCount = existingMatches.Count,
                    Matches = existingMatches.Select(m => new 
                    { 
                        m.Id, 
                        m.HomeTeam, 
                        m.AwayTeam, 
                        MatchDate = m.MatchDate.ToString("yyyy-MM-dd HH:mm"),
                        m.Competition,
                        m.GoogleCalendarEventId
                    }).ToList()
                },
                WhatWouldHappen = new
                {
                    TeamsFromCalendar = calendarMatches.SelectMany(m => new[] { m.HomeTeam, m.AwayTeam }).Distinct().ToList(),
                    NewClubsToAdd = calendarMatches.SelectMany(m => new[] { m.HomeTeam, m.AwayTeam })
                        .Distinct()
                        .Where(team => !existingClubs.Any(c => c.Name.Equals(team, StringComparison.OrdinalIgnoreCase)))
                        .ToList(),
                    ClubsToRemove = request.ForceSync 
                        ? existingClubs.Where(c => !calendarMatches.SelectMany(m => new[] { m.HomeTeam, m.AwayTeam })
                            .Any(team => team.Equals(c.Name, StringComparison.OrdinalIgnoreCase))).Select(c => c.Name).ToList()
                        : new List<string>(),
                    NewMatchesToAdd = calendarMatches.Where(cm => !existingMatches.Any(em => em.GoogleCalendarEventId == cm.EventId)).Count(),
                    MatchesToRemove = existingMatches.Where(em => !string.IsNullOrEmpty(em.GoogleCalendarEventId) && 
                        !calendarMatches.Any(cm => cm.EventId == em.GoogleCalendarEventId)).Count()
                }
            };

            return Ok(debugInfo);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error debugging calendar data for league {LeagueId}", leagueId);
            return StatusCode(500, ex.Message);
        }
    }

    // Helper methods for Google Calendar sync
    private async Task<List<CalendarMatchData>> GetCalendarMatchesAsync(string calendarId, DateTime startDate, DateTime endDate)
    {
        try
        {
            _logger.LogInformation("Fetching calendar events from {CalendarId} between {StartDate} and {EndDate}", 
                calendarId, startDate.ToString("yyyy-MM-dd"), endDate.ToString("yyyy-MM-dd"));

            using var httpClient = new HttpClient();
            httpClient.Timeout = TimeSpan.FromSeconds(30);
            
            // Get API key from configuration - NOW REQUIRED by Google
            var apiKey = _configuration["GoogleCalendar:ApiKey"];
            
            if (string.IsNullOrEmpty(apiKey) || apiKey == "YOUR_GOOGLE_API_KEY_HERE")
            {
                _logger.LogError("❌ GOOGLE CALENDAR API KEY IS REQUIRED!");
                _logger.LogError("📝 Google now requires an API key for all calendar access.");
                _logger.LogError("🔧 To fix this:");
                _logger.LogError("1. Go to https://console.cloud.google.com/");
                _logger.LogError("2. Create a project or select existing one");
                _logger.LogError("3. Enable Google Calendar API");
                _logger.LogError("4. Create credentials (API Key)");
                _logger.LogError("5. Set GoogleCalendar:ApiKey in appsettings.Development.json");
                _logger.LogError("6. Make sure your calendar is public");
                throw new InvalidOperationException("Google Calendar API key is required. Google no longer allows unauthenticated access to calendar data.");
            }
            
            return await TryGoogleCalendarApi(httpClient, calendarId, startDate, endDate, apiKey);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error fetching calendar matches from {CalendarId}", calendarId);
            throw;
        }
    }

    private async Task<List<CalendarMatchData>?> TryGoogleCalendarApi(HttpClient httpClient, string calendarId, DateTime startDate, DateTime endDate, string apiKey)
    {
        try
        {
            // Format dates for Google Calendar API (RFC3339)
            var timeMin = startDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            var timeMax = endDate.ToUniversalTime().ToString("yyyy-MM-ddTHH:mm:ss.fffZ");
            
            var url = $"https://www.googleapis.com/calendar/v3/calendars/{Uri.EscapeDataString(calendarId)}/events" +
                     $"?key={apiKey}" +
                     $"&timeMin={timeMin}" +
                     $"&timeMax={timeMax}" +
                     $"&singleEvents=true" +
                     $"&orderBy=startTime" +
                     $"&maxResults=2500";

            _logger.LogDebug("🔗 Calling Google Calendar API for calendar: {CalendarId}", calendarId);

            var response = await httpClient.GetAsync(url);
            
            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("⚠️ Google Calendar API failed: {StatusCode} - {Content}", response.StatusCode, errorContent);
                return null; // Will fallback to public feed
            }

            var jsonContent = await response.Content.ReadAsStringAsync();
            _logger.LogInformation("� Received Google Calendar API response: {Length} characters", jsonContent.Length);

            return await ParseGoogleCalendarResponse(jsonContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Google Calendar API failed for {CalendarId}", calendarId);
            throw;
        }
    }

    private async Task<List<CalendarMatchData>> ParseGoogleCalendarResponse(string jsonContent)
    {
        try
        {
            _logger.LogDebug("📄 Response preview: {Preview}", jsonContent.Length > 200 ? jsonContent.Substring(0, 200) + "..." : jsonContent);

            // Parse Google Calendar JSON response with proper options
            var jsonOptions = new JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            
            var calendarResponse = System.Text.Json.JsonSerializer.Deserialize<GoogleCalendarResponse>(jsonContent, jsonOptions);
            var matches = new List<CalendarMatchData>();

            if (calendarResponse?.Items != null && calendarResponse.Items.Count > 0)
            {
                _logger.LogInformation("📋 Processing {EventCount} calendar events", calendarResponse.Items.Count);
                
                foreach (var item in calendarResponse.Items)
                {
                    var matchData = ParseGoogleCalendarEvent(item);
                    if (matchData != null)
                    {
                        matches.Add(matchData);
                        _logger.LogDebug("✅ Parsed match: {HomeTeam} vs {AwayTeam} on {MatchDate}", 
                            matchData.HomeTeam, matchData.AwayTeam, matchData.MatchDate.ToString("yyyy-MM-dd HH:mm"));
                    }
                }
            }
            else
            {
                _logger.LogWarning("⚠️ No events found in calendar for the specified date range");
            }

            _logger.LogInformation("✅ Successfully parsed {MatchCount} matches from Google Calendar", matches.Count);
            return matches;
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "📄 JSON parsing error for calendar response");
            throw;
        }
    }

    private List<CalendarMatchData> GetMockCalendarData()
    {
        return new List<CalendarMatchData>
        {
            new CalendarMatchData
            {
                EventId = "MOCK_EVENT_1",
                HomeTeam = "🚫 FAKE_TEAM_Arsenal",
                AwayTeam = "🚫 FAKE_TEAM_Chelsea", 
                MatchDate = DateTime.UtcNow.AddDays(7),
                Competition = "🚫 MOCK Premier League",
                Round = 1
            },
            new CalendarMatchData
            {
                EventId = "MOCK_EVENT_2", 
                HomeTeam = "🚫 FAKE_TEAM_Liverpool",
                AwayTeam = "🚫 FAKE_TEAM_Manchester_United",
                MatchDate = DateTime.UtcNow.AddDays(14),
                Competition = "🚫 MOCK Premier League",
                Round = 1
            }
        };
    }

    private CalendarMatchData? ParseGoogleCalendarEvent(GoogleCalendarEvent eventItem)
    {
        try
        {
            if (string.IsNullOrEmpty(eventItem.Summary))
            {
                _logger.LogDebug("⏭️ Skipping event with empty summary");
                return null;
            }

            var summary = eventItem.Summary.Trim();
            _logger.LogDebug("🔍 Parsing event: {Summary}", summary);
            
            // Parse different match formats:
            // "Team A vs Team B"
            // "Team A - Team B" 
            // "Team A v Team B"
            // "Team A : Team B"
            var vsPatterns = new[]
            {
                @"^(.+?)\s+vs\.?\s+(.+?)$",
                @"^(.+?)\s+-\s+(.+?)$", 
                @"^(.+?)\s+v\.?\s+(.+?)$",
                @"^(.+?)\s+:\s+(.+?)$",
                @"^(.+?)\s+–\s+(.+?)$",  // em dash
                @"^(.+?)\s+—\s+(.+?)$"   // em dash variant
            };

            string homeTeam = "";
            string awayTeam = "";

            foreach (var pattern in vsPatterns)
            {
                var match = System.Text.RegularExpressions.Regex.Match(summary, pattern, System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                if (match.Success)
                {
                    homeTeam = match.Groups[1].Value.Trim();
                    awayTeam = match.Groups[2].Value.Trim();
                    
                    // Remove scores in parentheses from team names (e.g., "Go Ahead Eagles (2-2)" -> "Go Ahead Eagles")
                    homeTeam = RemoveScoreFromTeamName(homeTeam);
                    awayTeam = RemoveScoreFromTeamName(awayTeam);
                    
                    _logger.LogDebug("✅ Successfully parsed teams using pattern: {HomeTeam} vs {AwayTeam}", homeTeam, awayTeam);
                    break;
                }
            }

            if (string.IsNullOrEmpty(homeTeam) || string.IsNullOrEmpty(awayTeam))
            {
                _logger.LogDebug("⏭️ Could not parse match from event: {Summary}", summary);
                return null;
            }

            // Get match date
            DateTime matchDate;
            if (!string.IsNullOrEmpty(eventItem.Start?.DateTime))
            {
                if (DateTime.TryParse(eventItem.Start.DateTime, out matchDate))
                {
                    // Ensure the DateTime is in UTC for PostgreSQL compatibility
                    matchDate = EnsureUtc(matchDate);
                    _logger.LogDebug("📅 Parsed DateTime: {MatchDate} UTC", matchDate.ToString("yyyy-MM-dd HH:mm"));
                }
                else
                {
                    _logger.LogWarning("⚠️ Could not parse DateTime: {DateTime}, using current UTC time", eventItem.Start.DateTime);
                    matchDate = DateTime.UtcNow;
                }
            }
            else if (!string.IsNullOrEmpty(eventItem.Start?.Date))
            {
                if (DateTime.TryParse(eventItem.Start.Date, out matchDate))
                {
                    // If it's an all-day event, set a default time and ensure UTC
                    matchDate = matchDate.AddHours(15); // 3 PM default
                    matchDate = EnsureUtc(matchDate);
                    _logger.LogDebug("📅 Parsed Date (set to 15:00 UTC): {MatchDate}", matchDate.ToString("yyyy-MM-dd HH:mm"));
                }
                else
                {
                    _logger.LogWarning("⚠️ Could not parse Date: {Date}, using current UTC time", eventItem.Start.Date);
                    matchDate = DateTime.UtcNow;
                }
            }
            else
            {
                _logger.LogWarning("⚠️ No start date/time found in event, using current UTC time");
                matchDate = DateTime.UtcNow;
            }

            var competition = ExtractCompetitionFromEvent(eventItem);
            var round = ExtractRoundFromEvent(eventItem);

            var calendarMatch = new CalendarMatchData
            {
                EventId = eventItem.Id ?? Guid.NewGuid().ToString(),
                HomeTeam = homeTeam,
                AwayTeam = awayTeam,
                MatchDate = matchDate,
                Competition = competition,
                Round = round
            };

            _logger.LogDebug("✅ Successfully parsed match: {HomeTeam} vs {AwayTeam} on {MatchDate} - {Competition} (Round {Round})", 
                homeTeam, awayTeam, matchDate.ToString("yyyy-MM-dd HH:mm"), competition, round);

            return calendarMatch;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "❌ Error parsing calendar event: {EventSummary}", eventItem.Summary);
            return null;
        }
    }

    private string RemoveScoreFromTeamName(string teamName)
    {
        if (string.IsNullOrEmpty(teamName))
            return teamName;
            
        // Remove scores in parentheses at the end of team names
        // Patterns: "(2-1)", "(0-0)", "(3-2)", etc.
        var scorePattern = @"\s*\(\d+-\d+\)\s*$";
        var cleanedName = System.Text.RegularExpressions.Regex.Replace(teamName, scorePattern, "", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
        
        return cleanedName.Trim();
    }

    private string ExtractCompetitionFromEvent(GoogleCalendarEvent eventItem)
    {
        // Try to extract competition from description or location
        if (!string.IsNullOrEmpty(eventItem.Description))
        {
            var desc = eventItem.Description.ToLower();
            if (desc.Contains("premier league")) return "Premier League";
            if (desc.Contains("champions league")) return "Champions League";
            if (desc.Contains("europa league")) return "Europa League";
            if (desc.Contains("fa cup")) return "FA Cup";
            if (desc.Contains("league cup")) return "League Cup";
            if (desc.Contains("eredivisie")) return "Eredivisie";
        }
        
        if (!string.IsNullOrEmpty(eventItem.Location))
        {
            return eventItem.Location;
        }
        
        return "League";
    }

    private int ExtractRoundFromEvent(GoogleCalendarEvent eventItem)
    {
        if (!string.IsNullOrEmpty(eventItem.Description))
        {
            var roundMatch = System.Text.RegularExpressions.Regex.Match(eventItem.Description, @"round\s+(\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (roundMatch.Success && int.TryParse(roundMatch.Groups[1].Value, out int round))
            {
                return round;
            }

            var matchdayMatch = System.Text.RegularExpressions.Regex.Match(eventItem.Description, @"matchday\s+(\d+)", System.Text.RegularExpressions.RegexOptions.IgnoreCase);
            if (matchdayMatch.Success && int.TryParse(matchdayMatch.Groups[1].Value, out int matchday))
            {
                return matchday;
            }
        }
        
        return 1; // Default round
    }

    private async Task<SyncResult> SyncClubsAndMatchesAsync(League league, List<CalendarMatchData> calendarMatches, bool forceSync)
    {
        var result = new SyncResult();

        try
        {
            // Get existing clubs and matches for this league
            var existingClubs = await _dbContext.Clubs
                .Where(c => c.LeagueId == league.Id)
                .ToListAsync();
            
            var existingMatches = await _dbContext.Matches
                .Where(m => m.LeagueId == league.Id)
                .ToListAsync();

            // Extract unique team names from calendar matches
            var calendarTeams = calendarMatches
                .SelectMany(m => new[] { m.HomeTeam, m.AwayTeam })
                .Distinct()
                .ToList();

            // Add new clubs that don't exist
            foreach (var teamName in calendarTeams)
            {
                if (!existingClubs.Any(c => c.Name.Equals(teamName, StringComparison.OrdinalIgnoreCase)))
                {
                    var newClub = new Club
                    {
                        Id = Guid.NewGuid(),
                        Name = teamName,
                        LeagueId = league.Id,
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    _dbContext.Clubs.Add(newClub);
                    existingClubs.Add(newClub);
                    result.ClubsAdded++;
                    
                    _logger.LogInformation("Added new club: {ClubName} to league {LeagueName}", teamName, league.Name);
                }
            }

            // Remove clubs that are no longer in calendar (if not force sync, only remove unused clubs)
            if (forceSync)
            {
                var clubsToRemove = existingClubs
                    .Where(c => !calendarTeams.Any(t => t.Equals(c.Name, StringComparison.OrdinalIgnoreCase)))
                    .ToList();

                foreach (var club in clubsToRemove)
                {
                    // Check if club has players or other dependencies
                    var hasPlayers = await _dbContext.FootballPlayers.AnyAsync(p => p.ClubId == club.Id);
                    if (!hasPlayers)
                    {
                        _dbContext.Clubs.Remove(club);
                        result.ClubsRemoved++;
                        
                        _logger.LogInformation("Removed club: {ClubName} from league {LeagueName}", club.Name, league.Name);
                    }
                }
            }

            // Add/update matches
            foreach (var calendarMatch in calendarMatches)
            {
                // First, try to find by exact calendar event ID
                var existingMatch = existingMatches
                    .FirstOrDefault(m => m.GoogleCalendarEventId == calendarMatch.EventId);

                // If not found by event ID, try to find by teams (for cases where event ID changed)
                if (existingMatch == null)
                {
                    existingMatch = existingMatches
                        .FirstOrDefault(m => 
                            string.IsNullOrEmpty(m.GoogleCalendarEventId) && // Only match unlinked matches
                            m.HomeTeam.Equals(calendarMatch.HomeTeam, StringComparison.OrdinalIgnoreCase) &&
                            m.AwayTeam.Equals(calendarMatch.AwayTeam, StringComparison.OrdinalIgnoreCase) &&
                            m.Competition.Equals(calendarMatch.Competition, StringComparison.OrdinalIgnoreCase));
                }

                if (existingMatch == null)
                {
                    // Add new match
                    var newMatch = new Match
                    {
                        Id = Guid.NewGuid(),
                        HomeTeam = calendarMatch.HomeTeam,
                        AwayTeam = calendarMatch.AwayTeam,
                        MatchDate = EnsureUtc(calendarMatch.MatchDate),
                        Competition = calendarMatch.Competition,
                        Round = calendarMatch.Round,
                        LeagueId = league.Id,
                        GoogleCalendarEventId = calendarMatch.EventId,
                        IsCompleted = false,
                        CreatedAt = DateTime.UtcNow
                    };
                    
                    _dbContext.Matches.Add(newMatch);
                    result.MatchesAdded++;
                    
                    _logger.LogInformation("➕ Added new match: {HomeTeam} vs {AwayTeam} on {MatchDate}", 
                        calendarMatch.HomeTeam, calendarMatch.AwayTeam, calendarMatch.MatchDate.ToString("yyyy-MM-dd HH:mm"));
                }
                else
                {
                    // Check if match date has changed (match moved)
                    var oldDate = existingMatch.MatchDate;
                    var newDate = EnsureUtc(calendarMatch.MatchDate);
                    var dateChanged = Math.Abs((oldDate - newDate).TotalMinutes) > 1; // Allow 1 minute tolerance

                    // Update existing match
                    existingMatch.HomeTeam = calendarMatch.HomeTeam;
                    existingMatch.AwayTeam = calendarMatch.AwayTeam;
                    existingMatch.MatchDate = newDate;
                    existingMatch.Competition = calendarMatch.Competition;
                    existingMatch.Round = calendarMatch.Round;
                    existingMatch.GoogleCalendarEventId = calendarMatch.EventId; // Link to calendar event

                    if (dateChanged)
                    {
                        _logger.LogWarning("📅 Match moved: {HomeTeam} vs {AwayTeam} from {OldDate} to {NewDate}", 
                            calendarMatch.HomeTeam, calendarMatch.AwayTeam, 
                            oldDate.ToString("yyyy-MM-dd HH:mm"), newDate.ToString("yyyy-MM-dd HH:mm"));
                    }
                    else
                    {
                        _logger.LogDebug("✏️ Updated match details: {HomeTeam} vs {AwayTeam}", 
                            calendarMatch.HomeTeam, calendarMatch.AwayTeam);
                    }
                }
            }

            // CHANGED: Don't remove matches that are no longer in calendar
            // Matches are preserved because teams only play each other a limited number of times
            // and matches may have important data like player performances
            var orphanedMatches = existingMatches
                .Where(m => !string.IsNullOrEmpty(m.GoogleCalendarEventId) && 
                           !calendarMatches.Any(cm => cm.EventId == m.GoogleCalendarEventId))
                .ToList();

            foreach (var match in orphanedMatches)
            {
                // Instead of deleting, just unlink from calendar and log
                match.GoogleCalendarEventId = null; // Unlink from calendar
                result.MatchesRemoved++; // Count as "removed" from calendar sync
                
                _logger.LogWarning("🔗 Unlinked match from calendar (match preserved): {HomeTeam} vs {AwayTeam} on {MatchDate}", 
                    match.HomeTeam, match.AwayTeam, match.MatchDate.ToString("yyyy-MM-dd HH:mm"));
            }

            await _dbContext.SaveChangesAsync();
            
            _logger.LogInformation("📊 Sync completed for league {LeagueName}. Added: {ClubsAdded} clubs, {MatchesAdded} matches. Unlinked: {ClubsRemoved} clubs, {MatchesUnlinked} matches from calendar",
                league.Name, result.ClubsAdded, result.MatchesAdded, result.ClubsRemoved, result.MatchesRemoved);

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error syncing clubs and matches for league {LeagueName}", league.Name);
            throw;
        }
    }
}

// Helper classes
public class CalendarMatchData
{
    public string EventId { get; set; } = string.Empty;
    public string HomeTeam { get; set; } = string.Empty;
    public string AwayTeam { get; set; } = string.Empty;
    public DateTime MatchDate { get; set; }
    public string Competition { get; set; } = string.Empty;
    public int Round { get; set; } = 1;
}

public class SyncResult
{
    public int ClubsAdded { get; set; }
    public int ClubsRemoved { get; set; }
    public int MatchesAdded { get; set; }
    public int MatchesRemoved { get; set; }
}

// Google Calendar API response classes
public class GoogleCalendarResponse
{
    public List<GoogleCalendarEvent>? Items { get; set; }
}

public class GoogleCalendarEvent
{
    public string? Id { get; set; }
    public string? Summary { get; set; }
    public string? Description { get; set; }
    public string? Location { get; set; }
    public GoogleCalendarDateTime? Start { get; set; }
    public GoogleCalendarDateTime? End { get; set; }
}

public class GoogleCalendarDateTime
{
    public string? DateTime { get; set; }
    public string? Date { get; set; }
    public string? TimeZone { get; set; }
}
