namespace GolfManager.Shared.DTOs.Event;

/// <summary>
/// Matchup response for a season event.
/// </summary>
public class EventMatchupResponse
{
    public string Id { get; set; } = string.Empty;
    public string SeasonEventId { get; set; } = string.Empty;
    public string? HomeTeamId { get; set; }
    public string? HomeTeamName { get; set; }
    public string? AwayTeamId { get; set; }
    public string? AwayTeamName { get; set; }
    public string? HomeSubSeasonGolferId { get; set; }
    public string? AwaySubSeasonGolferId { get; set; }
    public double? HomePoints { get; set; }
    public double? AwayPoints { get; set; }
    public int? StartingHole { get; set; }
    public int? StartingFlight { get; set; }
    public bool IsComplete { get; set; }
}
