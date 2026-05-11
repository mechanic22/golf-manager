namespace GolfManager.Shared.DTOs.Season;

/// <summary>
/// Response DTO for a season team
/// </summary>
public class SeasonTeamResponse
{
    public string Id { get; set; } = string.Empty;
    public string SeasonId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? AvatarUrl { get; set; }
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Ties { get; set; }
    public double? SeasonPoints { get; set; }
    public List<SeasonTeamMemberResponse> Members { get; set; } = new();
}

/// <summary>
/// A team member within a season team
/// </summary>
public class SeasonTeamMemberResponse
{
    public string SeasonGolferId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public double? LeagueHandicap { get; set; }
}
