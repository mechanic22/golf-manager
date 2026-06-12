namespace GolfManager.Shared.DTOs.Event;

public class MyMatchupResponse
{
    public int? StartingHole { get; set; }
    public int? StartingFlight { get; set; }
    public string? MyTeamName { get; set; }
    public string? OpponentName { get; set; }
}
