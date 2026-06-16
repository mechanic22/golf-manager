namespace GolfManager.Shared.DTOs.Season;

public class PlayerSeasonHoleStatsResponse
{
    public int TotalHolesPlayed { get; set; }
    public List<HoleStatEntry> Holes { get; set; } = [];
}

public class HoleStatEntry
{
    public int HoleNumber { get; set; }
    public int Par { get; set; }
    public int HolesPlayed { get; set; }
    public double? AverageOverPar { get; set; }
    public int EagleOrBetter { get; set; }
    public int Birdies { get; set; }
    public int Pars { get; set; }
    public int Bogeys { get; set; }
    public int DoubleBogeys { get; set; }
    public int TriplePlus { get; set; }
}
