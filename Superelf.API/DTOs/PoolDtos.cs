namespace Superelf.API.DTOs;

public class PoolDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Code { get; set; } = string.Empty;
    public DateTime CreateTime { get; set; }
    public string OwnerName { get; set; } = string.Empty;
    public List<PoolParticipantDto> Participants { get; set; } = new();
    public bool AllowSelectionEditing { get; set; } = true;
}

public class PoolParticipantDto
{
    public string UserId { get; set; } = string.Empty;
    public string UserName { get; set; } = string.Empty;
    public bool SelectionComplete { get; set; }
    public int SelectedPlayers { get; set; }
    public bool HasJoker { get; set; }
}

public class CreatePoolDto
{
    public string Name { get; set; } = string.Empty;
}

public class JoinPoolDto
{
    public string PoolCode { get; set; } = string.Empty;
}

public class EditPoolNameDto
{
    public string NewName { get; set; } = string.Empty;
}

public class ToggleSelectionEditingDto
{
    public bool AllowSelectionEditing { get; set; }
}

// Scoreboard DTOs are defined in Application layer to avoid duplication