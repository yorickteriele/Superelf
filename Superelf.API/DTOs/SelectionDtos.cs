namespace Superelf.API.DTOs;

/// <summary>
/// Data transfer object representing a user's team selection in a pool.
/// Contains the complete team lineup and available players for selection.
/// </summary>
public class SelectionDto
{
    /// <summary>
    /// ID of the pool this selection belongs to
    /// </summary>
    public Guid PoolId { get; set; }
    
    /// <summary>
    /// Whether the team selection is complete (all required positions filled)
    /// </summary>
    public bool Complete { get; set; }
    
    /// <summary>
    /// Team formation (e.g., "4-3-3", "4-4-2")
    /// </summary>
    public string Formation { get; set; } = "4-3-3";
    
    /// <summary>
    /// Total number of players currently selected
    /// </summary>
    public int TotalPlayers { get; set; }
    
    /// <summary>
    /// Count of different nationalities in the selected team
    /// </summary>
    public int UniqueNationalities { get; set; }
    
    /// <summary>
    /// Whether a joker has been set for this selection
    /// </summary>
    public bool HasJoker { get; set; }
    
    /// <summary>
    /// ID of the player set as joker
    /// </summary>
    public Guid? JokerPlayerId { get; set; }
    
    // Starting lineup positions
    public SelectedPlayerDto? SelectedBasisGoalkeeper { get; set; }
    public List<SelectedPlayerDto> SelectedBasisDefenders { get; set; } = new();
    public List<SelectedPlayerDto> SelectedBasisMidfielders { get; set; } = new();
    public List<SelectedPlayerDto> SelectedBasisForwards { get; set; } = new();
    
    // Reserve positions
    public SelectedPlayerDto? SelectedReserveGoalkeeper { get; set; }
    public SelectedPlayerDto? SelectedReserveDefender { get; set; }
    public SelectedPlayerDto? SelectedReserveMidfielder { get; set; }
    public SelectedPlayerDto? SelectedReserveForward { get; set; }
    
    // Available players for selection
    public List<FootballPlayerDto> Goalkeepers { get; set; } = new();
    public List<FootballPlayerDto> Defenders { get; set; } = new();
    public List<FootballPlayerDto> Midfielders { get; set; } = new();
    public List<FootballPlayerDto> Forwards { get; set; } = new();
}

/// <summary>
/// Data transfer object representing a selected player in a team.
/// Contains player info and their specific position in the lineup.
/// </summary>
public class SelectedPlayerDto
{
    /// <summary>
    /// ID of the selected player
    /// </summary>
    public Guid PlayerId { get; set; }
    
    /// <summary>
    /// Detailed player information
    /// </summary>
    public FootballPlayerDto Player { get; set; } = new();
    
    /// <summary>
    /// Numerical position in the formation (e.g., 1 for first defender)
    /// </summary>
    public int SpecificPosition { get; set; }
    
    /// <summary>
    /// Whether this player is set as the joker
    /// </summary>
    public bool IsJoker { get; set; }
    
    /// <summary>
    /// Human readable position name (e.g., "Left Back", "Striker")
    /// </summary>
    public string PositionName { get; set; } = string.Empty;
}

/// <summary>
/// Data transfer object representing a football player's basic information.
/// Used for both selected players and available players.
/// </summary>
public class FootballPlayerDto
{
    /// <summary>
    /// Unique identifier for the player
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Player's full name
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Player's general position (GK, DEF, MID, FWD)
    /// </summary>
    public string Position { get; set; } = string.Empty;
    
    /// <summary>
    /// Player's nationality
    /// </summary>
    public string Nationality { get; set; } = string.Empty;
    
    /// <summary>
    /// Player's current club name
    /// </summary>
    public string? Club { get; set; }
    
    /// <summary>
    /// ID of the player's current club
    /// </summary>
    public Guid? ClubId { get; set; }
    
    /// <summary>
    /// URL to the player's photo
    /// </summary>
    public string? PhotoUrl { get; set; }
    
    /// <summary>
    /// When the player was added to the system
    /// </summary>
    public DateTime CreatedAt { get; set; }
    
    /// <summary>
    /// Specific position number in the formation
    /// </summary>
    public int? SpecificPosition { get; set; }
    
    /// <summary>
    /// Whether the player is currently a joker
    /// </summary>
    public bool IsJoker { get; set; }
}

/// <summary>
/// Data transfer object representing a single line in the lineup display.
/// </summary>
public class LineupLineDto
{
    /// <summary>
    /// Unique identifier for the lineup line
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Player in this lineup position
    /// </summary>
    public FootballPlayerDto Player { get; set; } = new();
    
    /// <summary>
    /// Whether this is a reserve position
    /// </summary>
    public bool IsReserve { get; set; }
    
    /// <summary>
    /// Position number in the formation
    /// </summary>
    public int SpecificPosition { get; set; }
    
    /// <summary>
    /// Whether this player is the joker
    /// </summary>
    public bool IsJoker { get; set; }
    
    /// <summary>
    /// Human readable position name
    /// </summary>
    public string PositionName { get; set; } = string.Empty;
}

/// <summary>
/// Data transfer object for submitting player selections.
/// Used when adding or removing players from a team.
/// </summary>
public class SubmitSelectionDto
{
    /// <summary>
    /// Position category being modified (GK, DEF, MID, FWD)
    /// </summary>
    public string Position { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether this is a reserve position
    /// </summary>
    public bool IsReserve { get; set; }
    
    /// <summary>
    /// IDs of the players being selected
    /// </summary>
    public List<Guid> SelectedPlayers { get; set; } = new();
    
    /// <summary>
    /// Whether this selection is for the joker
    /// </summary>
    public bool IsJoker { get; set; }
    
    /// <summary>
    /// Which position slot was clicked (for specific position assignment)
    /// </summary>
    public int? SlotIndex { get; set; }
}

/// <summary>
/// Data transfer object for setting a player as the joker.
/// </summary>
public class SetJokerDto
{
    /// <summary>
    /// ID of the player to set as joker
    /// </summary>
    public Guid PlayerId { get; set; }
}

/// <summary>
/// Data transfer object for the player selection table.
/// Contains available players and selection constraints.
/// </summary>
public class SelectionTableDto
{
    /// <summary>
    /// ID of the pool this selection is for
    /// </summary>
    public Guid PoolId { get; set; }
    
    /// <summary>
    /// Position category being displayed (GK, DEF, MID, FWD)
    /// </summary>
    public string Position { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether this is for reserve positions
    /// </summary>
    public bool IsReserve { get; set; }
    
    /// <summary>
    /// Maximum number of players that can be selected
    /// </summary>
    public int MaxSelection { get; set; }
    
    /// <summary>
    /// List of available players to select from
    /// </summary>
    public List<FootballPlayerDto> Players { get; set; } = new();
    
    /// <summary>
    /// Current count of different nationalities
    /// </summary>
    public int UniqueNationalities { get; set; }
    
    /// <summary>
    /// Total number of players currently selected
    /// </summary>
    public int TotalSelectedPlayers { get; set; }
    
    /// <summary>
    /// Number of players still needed
    /// </summary>
    public int RemainingPlayers { get; set; }
    
    /// <summary>
    /// IDs of players already selected in other positions
    /// </summary>
    public List<Guid> AlreadySelectedPlayers { get; set; } = new();
    
    /// <summary>
    /// List of nationalities currently represented in the team
    /// </summary>
    public List<string> SelectedNationalities { get; set; } = new();
}
