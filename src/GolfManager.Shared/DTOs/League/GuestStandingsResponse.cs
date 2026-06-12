namespace GolfManager.Shared.DTOs.League;

public class GuestStandingsResponse
{
    public string LeagueName { get; set; } = string.Empty;
    public string? LogoUrl { get; set; }
    public string? SeasonName { get; set; }
    public List<GuestTeamStandingRow> Teams { get; set; } = new();
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
