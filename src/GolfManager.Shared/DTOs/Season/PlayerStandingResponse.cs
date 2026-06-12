namespace GolfManager.Shared.DTOs.Season;

public class PlayerStandingResponse
{
    public string SeasonGolferId { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public double? LeagueHandicap { get; set; }
    public double? SeasonPoints { get; set; }
    public int RoundCount { get; set; }
    public double? AverageNetScore { get; set; }
    public int? BestRawScore { get; set; }
}
