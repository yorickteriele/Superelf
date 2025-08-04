namespace Superelf.API.DTOs;

// Paging DTOs
public class PagedResult<T>
{
    public List<T> Items { get; set; } = new();
    public int TotalCount { get; set; }
    public int Page { get; set; }
    public int PageSize { get; set; }
    public int TotalPages { get; set; }
    public bool HasNextPage => Page < TotalPages;
    public bool HasPreviousPage => Page > 1;
}

// Admin User DTOs
public class AdminUserDto
{
    public string Id { get; set; } = string.Empty;
    public string Username { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string? PhoneNumber { get; set; }
    public bool EmailConfirmed { get; set; }
    public DateTimeOffset? LockoutEnd { get; set; }
    public int AccessFailedCount { get; set; }
    public List<string> Roles { get; set; } = new();
}

// Bulk Player Management DTOs  
public class BulkCreatePlayersDto
{
    public List<CreatePlayerDto> Players { get; set; } = new();
    public bool SkipDuplicates { get; set; } = true;
}

public class BulkCreateResult
{
    public int Created { get; set; }
    public int Skipped { get; set; }
    public int Failed { get; set; }
    public List<string> SkippedPlayers { get; set; } = new();
    public List<string> FailedPlayers { get; set; } = new();
}

public class PlayerFilterDto
{
    public string? Search { get; set; }
    public string? Position { get; set; }
    public string? Nationality { get; set; }
    public Guid? ClubId { get; set; }
    public string? SortBy { get; set; } = "name";
    public string? SortDirection { get; set; } = "asc";
    public int Page { get; set; } = 1;
    public int PageSize { get; set; } = 20;
}

// Club Management DTOs
public class ClubDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public string? LogoUrl { get; set; }
    public string? Country { get; set; }
    public int PlayerCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateClubDto
{
    public string Name { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public string? LogoUrl { get; set; }
    public string? Country { get; set; }
}

public class UpdateClubDto
{
    public string Name { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public string? LogoUrl { get; set; }
    public string? Country { get; set; }
}

// Enhanced Player Management DTOs
public class CreatePlayerDto
{
    public string Name { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string Nationality { get; set; } = string.Empty;
    public string? Club { get; set; } // For backward compatibility
    public Guid? ClubId { get; set; }
    public string? PhotoUrl { get; set; }
}

public class UpdatePlayerDto
{
    public string Name { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string Nationality { get; set; } = string.Empty;
    public string? Club { get; set; } // For backward compatibility
    public Guid? ClubId { get; set; }
    public string? PhotoUrl { get; set; }
}

public class BulkUpdateClubsDto
{
    public List<Guid> PlayerIds { get; set; } = new();
    public string NewClub { get; set; } = string.Empty;
    public Guid? NewClubId { get; set; }
}

// Match Management DTOs
public class MatchDto
{
    public Guid Id { get; set; }
    public string HomeTeam { get; set; } = string.Empty;
    public string AwayTeam { get; set; } = string.Empty;
    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }
    public DateTime MatchDate { get; set; }
    public string? Competition { get; set; }
    public int Round { get; set; }
    public bool IsCompleted { get; set; }
    public List<PlayerPerformanceDto> PlayerPerformances { get; set; } = new();
    public DateTime CreatedAt { get; set; }
}

public class CreateMatchDto
{
    public string HomeTeam { get; set; } = string.Empty;
    public string AwayTeam { get; set; } = string.Empty;
    public DateTime MatchDate { get; set; }
    public string? Competition { get; set; }
    public int Round { get; set; }
}

public class UpdateMatchDto
{
    public string HomeTeam { get; set; } = string.Empty;
    public string AwayTeam { get; set; } = string.Empty;
    public int? HomeScore { get; set; }
    public int? AwayScore { get; set; }
    public DateTime MatchDate { get; set; }
    public string? Competition { get; set; }
    public int Round { get; set; }
    public bool IsCompleted { get; set; }
}

// Player Performance DTOs
public class PlayerPerformanceDto
{
    public Guid Id { get; set; }
    public Guid MatchId { get; set; }
    public Guid PlayerId { get; set; }
    public string PlayerName { get; set; } = string.Empty;
    public string PlayerPosition { get; set; } = string.Empty;
    public string PlayerClub { get; set; } = string.Empty;
    public int Goals { get; set; }
    public int PenaltyGoals { get; set; }
    public int PenaltiesMissed { get; set; }
    public int OwnGoals { get; set; }
    public int Assists { get; set; }
    public int YellowCards { get; set; }
    public int RedCards { get; set; }
    public bool Played { get; set; }
    public int Points { get; set; }
}

public class CreatePlayerPerformanceDto
{
    public Guid PlayerId { get; set; }
    public int Goals { get; set; }
    public int PenaltyGoals { get; set; }
    public int PenaltiesMissed { get; set; }
    public int OwnGoals { get; set; }
    public int Assists { get; set; }
    public int YellowCards { get; set; }
    public int RedCards { get; set; }
    public bool Played { get; set; }
}

public class UpdatePlayerPerformanceDto
{
    public int Goals { get; set; }
    public int PenaltyGoals { get; set; }
    public int PenaltiesMissed { get; set; }
    public int OwnGoals { get; set; }
    public int Assists { get; set; }
    public int YellowCards { get; set; }
    public int RedCards { get; set; }
    public bool Played { get; set; }
}

public class BulkPlayerPerformanceDto
{
    public Guid MatchId { get; set; }
    public List<CreatePlayerPerformanceDto> Performances { get; set; } = new();
}

// Enhanced Statistics DTOs
public class AdminStatsDto
{
    public int TotalUsers { get; set; }
    public int TotalAdmins { get; set; }
    public int TotalPlayers { get; set; }
    public int TotalClubs { get; set; }
    public int TotalMatches { get; set; }
    public int CompletedMatches { get; set; }
    public int TotalPools { get; set; }
    public int TotalLineups { get; set; }
    public List<PositionStatsDto> PlayersByPosition { get; set; } = new();
    public List<NationalityStatsDto> PlayersByNationality { get; set; } = new();
    public List<ClubStatsDto> PlayersByClub { get; set; } = new();
}

public class PositionStatsDto
{
    public string Position { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class NationalityStatsDto
{
    public string Nationality { get; set; } = string.Empty;
    public int Count { get; set; }
}

public class ClubStatsDto
{
    public string Club { get; set; } = string.Empty;
    public int Count { get; set; }
}

// Enhanced Match Processing DTOs
public class ProcessMatchesDto
{
    public List<MatchResultDto> MatchResults { get; set; } = new();
}

public class MatchResultDto
{
    public Guid MatchId { get; set; }
    public string HomeTeam { get; set; } = string.Empty;
    public string AwayTeam { get; set; } = string.Empty;
    public int HomeScore { get; set; }
    public int AwayScore { get; set; }
    public List<PlayerPerformanceDto> PlayerPerformances { get; set; } = new();
}

// Player Import/Export DTOs
public class ImportPlayersDto
{
    public List<CreatePlayerDto> Players { get; set; } = new();
    public bool OverwriteExisting { get; set; }
}

public class ExportPlayersDto
{
    public List<Guid>? PlayerIds { get; set; }
    public string? Position { get; set; }
    public string? Nationality { get; set; }
    public Guid? ClubId { get; set; }
}

// League Management DTOs
public class LeagueDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public string? Country { get; set; }
    public string? LogoUrl { get; set; }
    public string? GoogleCalendarId { get; set; }
    public DateTime? LastSyncAt { get; set; }
    public bool IsActive { get; set; }
    public int ClubCount { get; set; }
    public int MatchCount { get; set; }
    public DateTime CreatedAt { get; set; }
}

public class CreateLeagueDto
{
    public string Name { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public string? Country { get; set; }
    public string? LogoUrl { get; set; }
    public string? GoogleCalendarId { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateLeagueDto
{
    public string Name { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public string? Country { get; set; }
    public string? LogoUrl { get; set; }
    public string? GoogleCalendarId { get; set; }
    public bool IsActive { get; set; } = true;
}

public class GoogleCalendarSyncDto
{
    public Guid LeagueId { get; set; }
    public string GoogleCalendarId { get; set; } = string.Empty;
    public bool ForceSync { get; set; } = false;
}

// Request DTOs for API endpoints
public class CreateLeagueRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public string? Country { get; set; }
    public string? LogoUrl { get; set; }
    public string? GoogleCalendarId { get; set; }
    public bool IsActive { get; set; } = true;
}

public class UpdateLeagueRequestDto
{
    public string Name { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public string? Country { get; set; }
    public string? LogoUrl { get; set; }
    public string? GoogleCalendarId { get; set; }
    public bool IsActive { get; set; } = true;
}

public class GoogleCalendarSyncRequestDto
{
    public string GoogleCalendarId { get; set; } = string.Empty;
    public bool ForceSync { get; set; } = false;
    public DateTime? StartDate { get; set; }
    public DateTime? EndDate { get; set; }
}

public class TestCalendarAccessRequestDto
{
    public string CalendarId { get; set; } = string.Empty;
}

// Response DTOs
public class AdminLeagueDto
{
    public string Id { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? ShortName { get; set; }
    public string? Country { get; set; }
    public string? LogoUrl { get; set; }
    public string? GoogleCalendarId { get; set; }
    public DateTime? LastSyncAt { get; set; }
    public bool IsActive { get; set; }
    public int ClubCount { get; set; }
    public int MatchCount { get; set; }
    public DateTime CreatedAt { get; set; }
}
