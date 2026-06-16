namespace GolfManager.Shared.DTOs.League;

public class GuestEventsResponse
{
    public string LeagueName { get; set; } = string.Empty;
    public string? SeasonName { get; set; }
    public List<GuestEventRow> Events { get; set; } = new();
}

public class GuestEventRow
{
    public string Id { get; set; } = string.Empty;
    public string? Name { get; set; }
    public DateTime EventDate { get; set; }
    public bool IsComplete { get; set; }
    public List<GuestMatchupRow> Matchups { get; set; } = new();
}

public class GuestMatchupRow
{
    public string Id { get; set; } = string.Empty;
    public string? HomeTeamName { get; set; }
    public string? AwayTeamName { get; set; }
    public double? HomePoints { get; set; }
    public double? AwayPoints { get; set; }
    public bool IsComplete { get; set; }
    public List<GuestMatchupMemberRow> HomeMembers { get; set; } = new();
    public List<GuestMatchupMemberRow> AwayMembers { get; set; } = new();
}

public class GuestMatchupMemberRow
{
    public string DisplayName { get; set; } = string.Empty;
    public double? Handicap { get; set; }
    public int? RawScore { get; set; }
    public double? NetScore { get; set; }
    public bool IsSubstitute { get; set; }
}
