namespace Superelf.API.DTOs;

public class SelectionDto
{
    public Guid PoolId { get; set; }
    public bool Complete { get; set; }
    public string Formation { get; set; } = "4-3-3";
    public int TotalPlayers { get; set; }
    public int UniqueNationalities { get; set; }
    public bool HasJoker { get; set; }
    public Guid? JokerPlayerId { get; set; }
    
    public SelectedPlayerDto? SelectedBasisGoalkeeper { get; set; }
    public List<SelectedPlayerDto> SelectedBasisDefenders { get; set; } = new();
    public List<SelectedPlayerDto> SelectedBasisMidfielders { get; set; } = new();
    public List<SelectedPlayerDto> SelectedBasisForwards { get; set; } = new();
    
    public SelectedPlayerDto? SelectedReserveGoalkeeper { get; set; }
    public SelectedPlayerDto? SelectedReserveDefender { get; set; }
    public SelectedPlayerDto? SelectedReserveMidfielder { get; set; }
    public SelectedPlayerDto? SelectedReserveForward { get; set; }
    
    public List<FootballPlayerDto> Goalkeepers { get; set; } = new();
    public List<FootballPlayerDto> Defenders { get; set; } = new();
    public List<FootballPlayerDto> Midfielders { get; set; } = new();
    public List<FootballPlayerDto> Forwards { get; set; } = new();
}

public class SelectedPlayerDto
{
    public Guid PlayerId { get; set; }
    public FootballPlayerDto Player { get; set; } = new();
    public int SpecificPosition { get; set; }
    public bool IsJoker { get; set; }
    public string PositionName { get; set; } = string.Empty; // Human readable position name
}

public class FootballPlayerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string Nationality { get; set; } = string.Empty;
    public string? Club { get; set; }
    public Guid? ClubId { get; set; }
    public string? PhotoUrl { get; set; }
    public DateTime CreatedAt { get; set; }
    public int? SpecificPosition { get; set; } // New field for specific position
    public bool IsJoker { get; set; } // New field to indicate if player is joker
}

public class LineupLineDto
{
    public Guid Id { get; set; }
    public FootballPlayerDto Player { get; set; } = new();
    public bool IsReserve { get; set; }
    public int SpecificPosition { get; set; }
    public bool IsJoker { get; set; }
    public string PositionName { get; set; } = string.Empty; // Human readable position name
}

public class SubmitSelectionDto
{
    public string Position { get; set; } = string.Empty;
    public bool IsReserve { get; set; }
    public List<Guid> SelectedPlayers { get; set; } = new();
    public bool IsJoker { get; set; }
    public int? SlotIndex { get; set; } // New field to track which slot was clicked
}

public class SetJokerDto
{
    public Guid PlayerId { get; set; }
}

public class SelectionTableDto
{
    public Guid PoolId { get; set; }
    public string Position { get; set; } = string.Empty;
    public bool IsReserve { get; set; }
    public int MaxSelection { get; set; }
    public List<FootballPlayerDto> Players { get; set; } = new();
    public int UniqueNationalities { get; set; }
    public int TotalSelectedPlayers { get; set; }
    public int RemainingPlayers { get; set; }
    public List<Guid> AlreadySelectedPlayers { get; set; } = new();
    public List<string> SelectedNationalities { get; set; } = new();
}
