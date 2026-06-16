using GolfManager.Shared.DTOs.Season;

namespace GolfManager.Shared.DTOs.League;

public class GuestStandingsResponse
{
    public string LeagueName { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? SeasonName { get; set; }
    public List<GuestTeamStandingRow> Teams { get; set; } = new();
    public List<PlayerStandingResponse> Players { get; set; } = new();
    /// <summary>
    /// SeasonGolferId of the currently signed-in user (null for guests or when not enrolled).
    /// Used by the UI to highlight the user's own row.
    /// </summary>
    public string? CurrentUserSeasonGolferId { get; set; }
}

public class GuestTeamStandingRow
{
    public int Rank { get; set; }
    public string Name { get; set; } = string.Empty;
    public int Wins { get; set; }
    public int Losses { get; set; }
    public int Ties { get; set; }
    public double? SeasonPoints { get; set; }
}
