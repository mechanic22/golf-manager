namespace GolfManager.Shared.DTOs.Event;

/// <summary>
/// Calculated scoring summary for a season event.
/// </summary>
public class EventScoreboardResponse
{
    public string EventId { get; set; } = string.Empty;
    public string SeasonId { get; set; } = string.Empty;
    public string LeagueId { get; set; } = string.Empty;
    public string? EventName { get; set; }
    public DateTime EventDate { get; set; }
    public List<EventMatchScoreResponse> Matches { get; set; } = new();
    public List<EventPlayerScoreResponse> Players { get; set; } = new();
}

public class EventMatchScoreResponse
{
    public string MatchupId { get; set; } = string.Empty;
    public string? HomeTeamId { get; set; }
    public string? HomeTeamName { get; set; }
    public string? HomeSubSeasonGolferId { get; set; }
    public string? HomeSubDisplayName { get; set; }
    public double? HomePoints { get; set; }
    public string? AwayTeamId { get; set; }
    public string? AwayTeamName { get; set; }
    public string? AwaySubSeasonGolferId { get; set; }
    public string? AwaySubDisplayName { get; set; }
    public double? AwayPoints { get; set; }
    public int? StartingHole { get; set; }
    public int? StartingFlight { get; set; }
    public bool IsComplete { get; set; }
    public List<EventTeamMemberScoreResponse> HomeMembers { get; set; } = new();
    public List<EventTeamMemberScoreResponse> AwayMembers { get; set; } = new();
}

public class EventTeamMemberScoreResponse
{
    public string SeasonGolferId { get; set; } = string.Empty;
    public string LeagueGolferId { get; set; } = string.Empty;
    public string GolferId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public double? Handicap { get; set; }
    public int? RawScore { get; set; }
    public double? NetScore { get; set; }
    public bool IsSubstitute { get; set; }
}

public class EventPlayerScoreResponse
{
    public string SeasonGolferId { get; set; } = string.Empty;
    public string LeagueGolferId { get; set; } = string.Empty;
    public string GolferId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public string? TeamId { get; set; }
    public string? TeamName { get; set; }
    public int? RawScore { get; set; }
    public double? Handicap { get; set; }
    public double? NetScore { get; set; }
    public double? EventPoints { get; set; }
    public int? EventPosition { get; set; }
    public int? MissCount { get; set; }
    public double? MissScore { get; set; }
}
