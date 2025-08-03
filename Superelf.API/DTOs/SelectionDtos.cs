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
    
    public Guid? SelectedBasisGoalkeeper { get; set; }
    public List<Guid> SelectedBasisDefenders { get; set; } = new();
    public List<Guid> SelectedBasisMidfielders { get; set; } = new();
    public List<Guid> SelectedBasisForwards { get; set; } = new();
    
    public Guid? SelectedReserveGoalkeeper { get; set; }
    public Guid? SelectedReserveDefender { get; set; }
    public Guid? SelectedReserveMidfielder { get; set; }
    public Guid? SelectedReserveForward { get; set; }
    
    public List<FootballPlayerDto> Goalkeepers { get; set; } = new();
    public List<FootballPlayerDto> Defenders { get; set; } = new();
    public List<FootballPlayerDto> Midfielders { get; set; } = new();
    public List<FootballPlayerDto> Forwards { get; set; } = new();
}

public class FootballPlayerDto
{
    public Guid Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Position { get; set; } = string.Empty;
    public string Nationality { get; set; } = string.Empty;
    public string? Club { get; set; }
}

public class SubmitSelectionDto
{
    public string Position { get; set; } = string.Empty;
    public bool IsReserve { get; set; }
    public List<Guid> SelectedPlayers { get; set; } = new();
    public bool IsJoker { get; set; }
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
