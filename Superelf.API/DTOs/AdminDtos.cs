namespace Superelf.API.DTOs;

/// <summary>
/// Generic data transfer object for paginated results.
/// Provides metadata about the current page and total results.
/// </summary>
/// <typeparam name="T">The type of items being paginated</typeparam>
public class PagedResult<T>
{
    /// <summary>
    /// The items on the current page
    /// </summary>
    public List<T> Items { get; set; } = new();
    
    /// <summary>
    /// Total number of items across all pages
    /// </summary>
    public int TotalCount { get; set; }
    
    /// <summary>
    /// Current page number (1-based)
    /// </summary>
    public int Page { get; set; }
    
    /// <summary>
    /// Number of items per page
    /// </summary>
    public int PageSize { get; set; }
    
    /// <summary>
    /// Total number of pages
    /// </summary>
    public int TotalPages { get; set; }
    
    /// <summary>
    /// Whether there is a page after the current one
    /// </summary>
    public bool HasNextPage => Page < TotalPages;
    
    /// <summary>
    /// Whether there is a page before the current one
    /// </summary>
    public bool HasPreviousPage => Page > 1;
}

/// <summary>
/// Data transfer object for administrator user management.
/// Contains detailed user information including authentication status.
/// </summary>
public class AdminUserDto
{
    /// <summary>
    /// Unique identifier for the user
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// User's display name
    /// </summary>
    public string Username { get; set; } = string.Empty;
    
    /// <summary>
    /// User's email address
    /// </summary>
    public string Email { get; set; } = string.Empty;
    
    /// <summary>
    /// Optional contact phone number
    /// </summary>
    public string? PhoneNumber { get; set; }
    
    /// <summary>
    /// Whether the email address has been verified
    /// </summary>
    public bool EmailConfirmed { get; set; }
    
    /// <summary>
    /// When any account lockout expires
    /// </summary>
    public DateTimeOffset? LockoutEnd { get; set; }
    
    /// <summary>
    /// Number of failed login attempts
    /// </summary>
    public int AccessFailedCount { get; set; }
    
    /// <summary>
    /// List of security roles assigned to the user
    /// </summary>
    public List<string> Roles { get; set; } = new();
}

/// <summary>
/// Data transfer object for creating multiple players in one request.
/// </summary>
public class BulkCreatePlayersDto
{
    /// <summary>
    /// List of players to create
    /// </summary>
    public List<CreatePlayerDto> Players { get; set; } = new();
    
    /// <summary>
    /// Whether to skip players that already exist instead of erroring
    /// </summary>
    public bool SkipDuplicates { get; set; } = true;
}

/// <summary>
/// Data transfer object for the result of a bulk player creation operation.
/// Provides counts and details of successes and failures.
/// </summary>
public class BulkCreateResult
{
    /// <summary>
    /// Number of players successfully created
    /// </summary>
    public int Created { get; set; }
    
    /// <summary>
    /// Number of players skipped (already existed)
    /// </summary>
    public int Skipped { get; set; }
    
    /// <summary>
    /// Number of players that failed to create
    /// </summary>
    public int Failed { get; set; }
    
    /// <summary>
    /// Names of players that were skipped
    /// </summary>
    public List<string> SkippedPlayers { get; set; } = new();
    
    /// <summary>
    /// Names of players that failed to create
    /// </summary>
    public List<string> FailedPlayers { get; set; } = new();
}

/// <summary>
/// Data transfer object for filtering and sorting player listings.
/// </summary>
public class PlayerFilterDto
{
    /// <summary>
    /// Text to search in player names
    /// </summary>
    public string? Search { get; set; }
    
    /// <summary>
    /// Filter by player position (GK, DEF, MID, FWD)
    /// </summary>
    public string? Position { get; set; }
    
    /// <summary>
    /// Filter by player nationality
    /// </summary>
    public string? Nationality { get; set; }
    
    /// <summary>
    /// Filter by player's club
    /// </summary>
    public Guid? ClubId { get; set; }
    
    /// <summary>
    /// Field to sort by (default: "name")
    /// </summary>
    public string? SortBy { get; set; } = "name";
    
    /// <summary>
    /// Sort direction ("asc" or "desc")
    /// </summary>
    public string? SortDirection { get; set; } = "asc";
    
    /// <summary>
    /// Page number for pagination
    /// </summary>
    public int Page { get; set; } = 1;
    
    /// <summary>
    /// Items per page
    /// </summary>
    public int PageSize { get; set; } = 20;
}

/// <summary>
/// Data transfer object representing a football club.
/// Contains club information and statistics.
/// </summary>
public class ClubDto
{
    /// <summary>
    /// Unique identifier for the club
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Full name of the club
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Abbreviated or common name of the club
    /// </summary>
    public string? ShortName { get; set; }
    
    /// <summary>
    /// URL to the club's logo image
    /// </summary>
    public string? LogoUrl { get; set; }
    
    /// <summary>
    /// Country where the club is based
    /// </summary>
    public string? Country { get; set; }
    
    /// <summary>
    /// Number of players in the club
    /// </summary>
    public int PlayerCount { get; set; }
    
    /// <summary>
    /// When the club was added to the system
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Data transfer object for creating a new club.
/// </summary>
public class CreateClubDto
{
    /// <summary>
    /// Full name of the club
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Abbreviated or common name of the club
    /// </summary>
    public string? ShortName { get; set; }
    
    /// <summary>
    /// URL to the club's logo image
    /// </summary>
    public string? LogoUrl { get; set; }
    
    /// <summary>
    /// Country where the club is based
    /// </summary>
    public string? Country { get; set; }
}

/// <summary>
/// Data transfer object for updating an existing club.
/// </summary>
public class UpdateClubDto
{
    /// <summary>
    /// Full name of the club
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Abbreviated or common name of the club
    /// </summary>
    public string? ShortName { get; set; }
    
    /// <summary>
    /// URL to the club's logo image
    /// </summary>
    public string? LogoUrl { get; set; }
    
    /// <summary>
    /// Country where the club is based
    /// </summary>
    public string? Country { get; set; }
}

/// <summary>
/// Data transfer object for creating a new football player.
/// </summary>
public class CreatePlayerDto
{
    /// <summary>
    /// Full name of the player
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Player's position (GK, DEF, MID, FWD)
    /// </summary>
    public string Position { get; set; } = string.Empty;
    
    /// <summary>
    /// Player's nationality
    /// </summary>
    public string Nationality { get; set; } = string.Empty;
    
    /// <summary>
    /// Name of the player's current club (legacy field)
    /// </summary>
    public string? Club { get; set; } // For backward compatibility
    
    /// <summary>
    /// ID of the player's current club
    /// </summary>
    public Guid? ClubId { get; set; }
    
    /// <summary>
    /// URL to the player's photo
    /// </summary>
    public string? PhotoUrl { get; set; }
}

/// <summary>
/// Data transfer object for updating an existing player.
/// </summary>
public class UpdatePlayerDto
{
    /// <summary>
    /// Full name of the player
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Player's position (GK, DEF, MID, FWD)
    /// </summary>
    public string Position { get; set; } = string.Empty;
    
    /// <summary>
    /// Player's nationality
    /// </summary>
    public string Nationality { get; set; } = string.Empty;
    
    /// <summary>
    /// Name of the player's current club (legacy field)
    /// </summary>
    public string? Club { get; set; } // For backward compatibility
    
    /// <summary>
    /// ID of the player's current club
    /// </summary>
    public Guid? ClubId { get; set; }
    
    /// <summary>
    /// URL to the player's photo
    /// </summary>
    public string? PhotoUrl { get; set; }
}

/// <summary>
/// Data transfer object for updating multiple players' club assignments.
/// </summary>
public class BulkUpdateClubsDto
{
    /// <summary>
    /// IDs of the players to update
    /// </summary>
    public List<Guid> PlayerIds { get; set; } = new();
    
    /// <summary>
    /// Name of the club to assign (legacy field)
    /// </summary>
    public string NewClub { get; set; } = string.Empty;
    
    /// <summary>
    /// ID of the club to assign
    /// </summary>
    public Guid? NewClubId { get; set; }
}

/// <summary>
/// Data transfer object representing a football match.
/// Contains match details and player performances.
/// </summary>
public class MatchDto
{
    /// <summary>
    /// Unique identifier for the match
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Name of the home team
    /// </summary>
    public string HomeTeam { get; set; } = string.Empty;
    
    /// <summary>
    /// Name of the away team
    /// </summary>
    public string AwayTeam { get; set; } = string.Empty;
    
    /// <summary>
    /// Goals scored by home team
    /// </summary>
    public int? HomeScore { get; set; }
    
    /// <summary>
    /// Goals scored by away team
    /// </summary>
    public int? AwayScore { get; set; }
    
    /// <summary>
    /// Date and time when the match is played
    /// </summary>
    public DateTime MatchDate { get; set; }
    
    /// <summary>
    /// Name of the competition/league
    /// </summary>
    public string? Competition { get; set; }
    
    /// <summary>
    /// Competition round or matchday number
    /// </summary>
    public int Round { get; set; }
    
    /// <summary>
    /// Whether the match has been played and results recorded
    /// </summary>
    public bool IsCompleted { get; set; }
    
    /// <summary>
    /// List of individual player performances
    /// </summary>
    public List<PlayerPerformanceDto> PlayerPerformances { get; set; } = new();
    
    /// <summary>
    /// When the match was added to the system
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Data transfer object for creating a new match.
/// </summary>
public class CreateMatchDto
{
    /// <summary>
    /// Name of the home team
    /// </summary>
    public string HomeTeam { get; set; } = string.Empty;
    
    /// <summary>
    /// Name of the away team
    /// </summary>
    public string AwayTeam { get; set; } = string.Empty;
    
    /// <summary>
    /// Date and time when the match will be played
    /// </summary>
    public DateTime MatchDate { get; set; }
    
    /// <summary>
    /// Name of the competition/league
    /// </summary>
    public string? Competition { get; set; }
    
    /// <summary>
    /// Competition round or matchday number
    /// </summary>
    public int Round { get; set; }
}

/// <summary>
/// Data transfer object for updating an existing match.
/// </summary>
public class UpdateMatchDto
{
    /// <summary>
    /// Name of the home team
    /// </summary>
    public string HomeTeam { get; set; } = string.Empty;
    
    /// <summary>
    /// Name of the away team
    /// </summary>
    public string AwayTeam { get; set; } = string.Empty;
    
    /// <summary>
    /// Goals scored by home team
    /// </summary>
    public int? HomeScore { get; set; }
    
    /// <summary>
    /// Goals scored by away team
    /// </summary>
    public int? AwayScore { get; set; }
    
    /// <summary>
    /// Date and time when the match is played
    /// </summary>
    public DateTime MatchDate { get; set; }
    
    /// <summary>
    /// Name of the competition/league
    /// </summary>
    public string? Competition { get; set; }
    
    /// <summary>
    /// Competition round or matchday number
    /// </summary>
    public int Round { get; set; }
    
    /// <summary>
    /// Whether the match has been played and results recorded
    /// </summary>
    public bool IsCompleted { get; set; }
}

/// <summary>
/// Data transfer object representing a player's performance in a match.
/// Contains detailed statistics and point calculation.
/// </summary>
public class PlayerPerformanceDto
{
    /// <summary>
    /// Unique identifier for the performance record
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// ID of the match this performance is for
    /// </summary>
    public Guid MatchId { get; set; }
    
    /// <summary>
    /// ID of the player
    /// </summary>
    public Guid PlayerId { get; set; }
    
    /// <summary>
    /// Name of the player
    /// </summary>
    public string PlayerName { get; set; } = string.Empty;
    
    /// <summary>
    /// Player's position in this match
    /// </summary>
    public string PlayerPosition { get; set; } = string.Empty;
    
    /// <summary>
    /// Player's club at time of match
    /// </summary>
    public string PlayerClub { get; set; } = string.Empty;
    
    /// <summary>
    /// Goals scored (excluding penalties)
    /// </summary>
    public int Goals { get; set; }
    
    /// <summary>
    /// Penalty goals scored
    /// </summary>
    public int PenaltyGoals { get; set; }
    
    /// <summary>
    /// Number of penalties missed
    /// </summary>
    public int PenaltiesMissed { get; set; }
    
    /// <summary>
    /// Own goals scored
    /// </summary>
    public int OwnGoals { get; set; }
    
    /// <summary>
    /// Assists provided
    /// </summary>
    public int Assists { get; set; }
    
    /// <summary>
    /// Yellow cards received
    /// </summary>
    public int YellowCards { get; set; }
    
    /// <summary>
    /// Red cards received
    /// </summary>
    public int RedCards { get; set; }
    
    /// <summary>
    /// Whether the player was in the squad
    /// </summary>
    public bool Played { get; set; }
    
    /// <summary>
    /// Fantasy points earned from this performance
    /// </summary>
    public int Points { get; set; }
}

/// <summary>
/// Data transfer object for creating a new player performance record.
/// </summary>
public class CreatePlayerPerformanceDto
{
    /// <summary>
    /// ID of the player
    /// </summary>
    public Guid PlayerId { get; set; }
    
    /// <summary>
    /// Goals scored (excluding penalties)
    /// </summary>
    public int Goals { get; set; }
    
    /// <summary>
    /// Penalty goals scored
    /// </summary>
    public int PenaltyGoals { get; set; }
    
    /// <summary>
    /// Number of penalties missed
    /// </summary>
    public int PenaltiesMissed { get; set; }
    
    /// <summary>
    /// Own goals scored
    /// </summary>
    public int OwnGoals { get; set; }
    
    /// <summary>
    /// Assists provided
    /// </summary>
    public int Assists { get; set; }
    
    /// <summary>
    /// Yellow cards received
    /// </summary>
    public int YellowCards { get; set; }
    
    /// <summary>
    /// Red cards received
    /// </summary>
    public int RedCards { get; set; }
    
    /// <summary>
    /// Whether the player was in the squad
    /// </summary>
    public bool Played { get; set; }
}

/// <summary>
/// Data transfer object for updating an existing player performance record.
/// </summary>
public class UpdatePlayerPerformanceDto
{
    /// <summary>
    /// Goals scored (excluding penalties)
    /// </summary>
    public int Goals { get; set; }
    
    /// <summary>
    /// Penalty goals scored
    /// </summary>
    public int PenaltyGoals { get; set; }
    
    /// <summary>
    /// Number of penalties missed
    /// </summary>
    public int PenaltiesMissed { get; set; }
    
    /// <summary>
    /// Own goals scored
    /// </summary>
    public int OwnGoals { get; set; }
    
    /// <summary>
    /// Assists provided
    /// </summary>
    public int Assists { get; set; }
    
    /// <summary>
    /// Yellow cards received
    /// </summary>
    public int YellowCards { get; set; }
    
    /// <summary>
    /// Red cards received
    /// </summary>
    public int RedCards { get; set; }
    
    /// <summary>
    /// Whether the player was in the squad
    /// </summary>
    public bool Played { get; set; }
}

/// <summary>
/// Data transfer object for recording multiple player performances in a match.
/// </summary>
public class BulkPlayerPerformanceDto
{
    /// <summary>
    /// ID of the match these performances are for
    /// </summary>
    public Guid MatchId { get; set; }
    
    /// <summary>
    /// List of individual player performances to record
    /// </summary>
    public List<CreatePlayerPerformanceDto> Performances { get; set; } = new();
}

/// <summary>
/// Data transfer object containing comprehensive admin statistics.
/// Provides counts and breakdowns of various system entities.
/// </summary>
public class AdminStatsDto
{
    /// <summary>
    /// Total number of registered users
    /// </summary>
    public int TotalUsers { get; set; }
    
    /// <summary>
    /// Number of users with admin privileges
    /// </summary>
    public int TotalAdmins { get; set; }
    
    /// <summary>
    /// Total number of football players in the system
    /// </summary>
    public int TotalPlayers { get; set; }
    
    /// <summary>
    /// Total number of clubs
    /// </summary>
    public int TotalClubs { get; set; }
    
    /// <summary>
    /// Total number of matches recorded
    /// </summary>
    public int TotalMatches { get; set; }
    
    /// <summary>
    /// Number of matches with completed results
    /// </summary>
    public int CompletedMatches { get; set; }
    
    /// <summary>
    /// Total number of fantasy pools
    /// </summary>
    public int TotalPools { get; set; }
    
    /// <summary>
    /// Total number of team lineups across all pools
    /// </summary>
    public int TotalLineups { get; set; }
    
    /// <summary>
    /// Breakdown of players by position
    /// </summary>
    public List<PositionStatsDto> PlayersByPosition { get; set; } = new();
    
    /// <summary>
    /// Breakdown of players by nationality
    /// </summary>
    public List<NationalityStatsDto> PlayersByNationality { get; set; } = new();
    
    /// <summary>
    /// Breakdown of players by club
    /// </summary>
    public List<ClubStatsDto> PlayersByClub { get; set; } = new();
}

/// <summary>
/// Data transfer object for player position statistics.
/// </summary>
public class PositionStatsDto
{
    /// <summary>
    /// Position category (GK, DEF, MID, FWD)
    /// </summary>
    public string Position { get; set; } = string.Empty;
    
    /// <summary>
    /// Number of players in this position
    /// </summary>
    public int Count { get; set; }
}

/// <summary>
/// Data transfer object for player nationality statistics.
/// </summary>
public class NationalityStatsDto
{
    /// <summary>
    /// Player nationality
    /// </summary>
    public string Nationality { get; set; } = string.Empty;
    
    /// <summary>
    /// Number of players of this nationality
    /// </summary>
    public int Count { get; set; }
}

/// <summary>
/// Data transfer object for club player statistics.
/// </summary>
public class ClubStatsDto
{
    /// <summary>
    /// Club name
    /// </summary>
    public string Club { get; set; } = string.Empty;
    
    /// <summary>
    /// Number of players in this club
    /// </summary>
    public int Count { get; set; }
}

/// <summary>
/// Data transfer object for batch processing match results.
/// </summary>
public class ProcessMatchesDto
{
    /// <summary>
    /// List of match results to process
    /// </summary>
    public List<MatchResultDto> MatchResults { get; set; } = new();
}

/// <summary>
/// Data transfer object for a match result with player performances.
/// Used for batch processing of match outcomes.
/// </summary>
public class MatchResultDto
{
    /// <summary>
    /// ID of the match
    /// </summary>
    public Guid MatchId { get; set; }
    
    /// <summary>
    /// Name of the home team
    /// </summary>
    public string HomeTeam { get; set; } = string.Empty;
    
    /// <summary>
    /// Name of the away team
    /// </summary>
    public string AwayTeam { get; set; } = string.Empty;
    
    /// <summary>
    /// Goals scored by home team
    /// </summary>
    public int HomeScore { get; set; }
    
    /// <summary>
    /// Goals scored by away team
    /// </summary>
    public int AwayScore { get; set; }
    
    /// <summary>
    /// List of individual player performances in this match
    /// </summary>
    public List<PlayerPerformanceDto> PlayerPerformances { get; set; } = new();
}

/// <summary>
/// Data transfer object for importing players in bulk.
/// </summary>
public class ImportPlayersDto
{
    /// <summary>
    /// List of players to import
    /// </summary>
    public List<CreatePlayerDto> Players { get; set; } = new();
    
    /// <summary>
    /// Whether to update existing players or skip them
    /// </summary>
    public bool OverwriteExisting { get; set; }
}

/// <summary>
/// Data transfer object for exporting filtered player data.
/// </summary>
public class ExportPlayersDto
{
    /// <summary>
    /// Optional list of specific player IDs to export
    /// </summary>
    public List<Guid>? PlayerIds { get; set; }
    
    /// <summary>
    /// Optional position filter
    /// </summary>
    public string? Position { get; set; }
    
    /// <summary>
    /// Optional nationality filter
    /// </summary>
    public string? Nationality { get; set; }
    
    /// <summary>
    /// Optional club filter
    /// </summary>
    public Guid? ClubId { get; set; }
}

/// <summary>
/// Data transfer object representing a football league.
/// Contains league details and associated statistics.
/// </summary>
public class LeagueDto
{
    /// <summary>
    /// Unique identifier for the league
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Full name of the league
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Abbreviated name of the league
    /// </summary>
    public string? ShortName { get; set; }
    
    /// <summary>
    /// Country where the league is based
    /// </summary>
    public string? Country { get; set; }
    
    /// <summary>
    /// URL to the league's logo
    /// </summary>
    public string? LogoUrl { get; set; }
    
    /// <summary>
    /// Google Calendar ID for match schedule sync
    /// </summary>
    public string? GoogleCalendarId { get; set; }
    
    /// <summary>
    /// When the league's schedule was last synced
    /// </summary>
    public DateTime? LastSyncAt { get; set; }
    
    /// <summary>
    /// Whether the league is currently active
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    /// Number of clubs in the league
    /// </summary>
    public int ClubCount { get; set; }
    
    /// <summary>
    /// Number of matches recorded for the league
    /// </summary>
    public int MatchCount { get; set; }
    
    /// <summary>
    /// When the league was added to the system
    /// </summary>
    public DateTime CreatedAt { get; set; }
}

/// <summary>
/// Data transfer object for creating a new league.
/// </summary>
public class CreateLeagueDto
{
    /// <summary>
    /// Full name of the league
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Abbreviated name of the league
    /// </summary>
    public string? ShortName { get; set; }
    
    /// <summary>
    /// Country where the league is based
    /// </summary>
    public string? Country { get; set; }
    
    /// <summary>
    /// URL to the league's logo
    /// </summary>
    public string? LogoUrl { get; set; }
    
    /// <summary>
    /// Google Calendar ID for match schedule sync
    /// </summary>
    public string? GoogleCalendarId { get; set; }
    
    /// <summary>
    /// Whether the league should start as active
    /// </summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Data transfer object for updating an existing league.
/// </summary>
public class UpdateLeagueDto
{
    /// <summary>
    /// Full name of the league
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Abbreviated name of the league
    /// </summary>
    public string? ShortName { get; set; }
    
    /// <summary>
    /// Country where the league is based
    /// </summary>
    public string? Country { get; set; }
    
    /// <summary>
    /// URL to the league's logo
    /// </summary>
    public string? LogoUrl { get; set; }
    
    /// <summary>
    /// Google Calendar ID for match schedule sync
    /// </summary>
    public string? GoogleCalendarId { get; set; }
    
    /// <summary>
    /// Whether the league is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Data transfer object for initiating a Google Calendar sync.
/// </summary>
public class GoogleCalendarSyncDto
{
    /// <summary>
    /// ID of the league to sync
    /// </summary>
    public Guid LeagueId { get; set; }
    
    /// <summary>
    /// Google Calendar ID to sync from
    /// </summary>
    public string GoogleCalendarId { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether to sync regardless of last sync time
    /// </summary>
    public bool ForceSync { get; set; } = false;
}

/// <summary>
/// Request DTOs for API endpoints
/// These DTOs represent the structure of incoming API requests
/// </summary>

/// <summary>
/// Request DTO for creating a new league via API.
/// </summary>
public class CreateLeagueRequestDto
{
    /// <summary>
    /// Full name of the league
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Abbreviated name of the league
    /// </summary>
    public string? ShortName { get; set; }
    
    /// <summary>
    /// Country where the league is based
    /// </summary>
    public string? Country { get; set; }
    
    /// <summary>
    /// URL to the league's logo
    /// </summary>
    public string? LogoUrl { get; set; }
    
    /// <summary>
    /// Google Calendar ID for match schedule sync
    /// </summary>
    public string? GoogleCalendarId { get; set; }
    
    /// <summary>
    /// Whether the league should start as active
    /// </summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Request DTO for updating an existing league via API.
/// </summary>
public class UpdateLeagueRequestDto
{
    /// <summary>
    /// Full name of the league
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Abbreviated name of the league
    /// </summary>
    public string? ShortName { get; set; }
    
    /// <summary>
    /// Country where the league is based
    /// </summary>
    public string? Country { get; set; }
    
    /// <summary>
    /// URL to the league's logo
    /// </summary>
    public string? LogoUrl { get; set; }
    
    /// <summary>
    /// Google Calendar ID for match schedule sync
    /// </summary>
    public string? GoogleCalendarId { get; set; }
    
    /// <summary>
    /// Whether the league is currently active
    /// </summary>
    public bool IsActive { get; set; } = true;
}

/// <summary>
/// Request DTO for initiating a Google Calendar sync via API.
/// </summary>
public class GoogleCalendarSyncRequestDto
{
    /// <summary>
    /// Google Calendar ID to sync from
    /// </summary>
    public string GoogleCalendarId { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether to sync regardless of last sync time
    /// </summary>
    public bool ForceSync { get; set; } = false;
    
    /// <summary>
    /// Optional start date for sync range
    /// </summary>
    public DateTime? StartDate { get; set; }
    
    /// <summary>
    /// Optional end date for sync range
    /// </summary>
    public DateTime? EndDate { get; set; }
}

/// <summary>
/// Request DTO for testing Google Calendar access.
/// </summary>
public class TestCalendarAccessRequestDto
{
    /// <summary>
    /// Google Calendar ID to test access for
    /// </summary>
    public string CalendarId { get; set; } = string.Empty;
}

/// <summary>
/// Response DTOs
/// These DTOs represent the structure of API responses
/// </summary>

/// <summary>
/// Response DTO for league information in admin context.
/// Contains detailed league data and statistics.
/// </summary>
public class AdminLeagueDto
{
    /// <summary>
    /// Unique identifier for the league
    /// </summary>
    public string Id { get; set; } = string.Empty;
    
    /// <summary>
    /// Full name of the league
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Abbreviated name of the league
    /// </summary>
    public string? ShortName { get; set; }
    
    /// <summary>
    /// Country where the league is based
    /// </summary>
    public string? Country { get; set; }
    
    /// <summary>
    /// URL to the league's logo
    /// </summary>
    public string? LogoUrl { get; set; }
    
    /// <summary>
    /// Google Calendar ID for match schedule sync
    /// </summary>
    public string? GoogleCalendarId { get; set; }
    
    /// <summary>
    /// When the league's schedule was last synced
    /// </summary>
    public DateTime? LastSyncAt { get; set; }
    
    /// <summary>
    /// Whether the league is currently active
    /// </summary>
    public bool IsActive { get; set; }
    
    /// <summary>
    /// Number of clubs in the league
    /// </summary>
    public int ClubCount { get; set; }
    
    /// <summary>
    /// Number of matches recorded for the league
    /// </summary>
    public int MatchCount { get; set; }
    
    /// <summary>
    /// When the league was added to the system
    /// </summary>
    public DateTime CreatedAt { get; set; }
}
