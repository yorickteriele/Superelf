namespace Superelf.API.DTOs;

/// <summary>
/// Data transfer object representing a fantasy football pool.
/// Contains all the information about a pool including its participants.
/// </summary>
public class PoolDto
{
    /// <summary>
    /// Unique identifier for the pool
    /// </summary>
    public Guid Id { get; set; }
    
    /// <summary>
    /// Display name of the pool
    /// </summary>
    public string Name { get; set; } = string.Empty;
    
    /// <summary>
    /// Unique invite code used to join the pool
    /// </summary>
    public string Code { get; set; } = string.Empty;
    
    /// <summary>
    /// When the pool was created
    /// </summary>
    public DateTime CreateTime { get; set; }
    
    /// <summary>
    /// Username of the pool creator/owner
    /// </summary>
    public string OwnerName { get; set; } = string.Empty;
    
    /// <summary>
    /// List of all participants in the pool
    /// </summary>
    public List<PoolParticipantDto> Participants { get; set; } = new();
    
    /// <summary>
    /// Whether players can currently modify their team selections
    /// </summary>
    public bool AllowSelectionEditing { get; set; } = true;
}

/// <summary>
/// Data transfer object representing a participant in a fantasy football pool.
/// Contains information about their selection status and joker usage.
/// </summary>
public class PoolParticipantDto
{
    /// <summary>
    /// Unique identifier of the participant
    /// </summary>
    public string UserId { get; set; } = string.Empty;
    
    /// <summary>
    /// Display name of the participant
    /// </summary>
    public string UserName { get; set; } = string.Empty;
    
    /// <summary>
    /// Whether the participant has completed their team selection
    /// </summary>
    public bool SelectionComplete { get; set; }
    
    /// <summary>
    /// Number of players this participant has selected
    /// </summary>
    public int SelectedPlayers { get; set; }
    
    /// <summary>
    /// Whether the participant has used their joker
    /// </summary>
    public bool HasJoker { get; set; }
}

/// <summary>
/// Data transfer object for creating a new pool.
/// </summary>
public class CreatePoolDto
{
    /// <summary>
    /// Display name for the new pool
    /// </summary>
    public string Name { get; set; } = string.Empty;
}

/// <summary>
/// Data transfer object for joining an existing pool.
/// </summary>
public class JoinPoolDto
{
    /// <summary>
    /// The invite code of the pool to join
    /// </summary>
    public string PoolCode { get; set; } = string.Empty;
}

/// <summary>
/// Data transfer object for changing a pool's name.
/// </summary>
public class EditPoolNameDto
{
    /// <summary>
    /// The new name for the pool
    /// </summary>
    public string NewName { get; set; } = string.Empty;
}

/// <summary>
/// Data transfer object for toggling whether team selections can be edited.
/// </summary>
public class ToggleSelectionEditingDto
{
    /// <summary>
    /// Whether to allow participants to modify their team selections
    /// </summary>
    public bool AllowSelectionEditing { get; set; }
}

// Scoreboard DTOs are defined in Application layer to avoid duplication