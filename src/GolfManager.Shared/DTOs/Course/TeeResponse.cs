namespace GolfManager.Shared.DTOs.Course;

public class TeeResponse
{
    public string Id { get; set; } = string.Empty;
    public string CourseId { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string HtmlColorCode { get; set; } = "#FFFFFF";
    public double RatingOut { get; set; }
    public int SlopeOut { get; set; }
    public double RatingIn { get; set; }
    public int SlopeIn { get; set; }
    public int YardsOut { get; set; }
    public int YardsIn { get; set; }
    public int ParOut { get; set; }
    public int ParIn { get; set; }
    public double TotalRating => RatingOut + RatingIn;
    public int TotalYards => YardsOut + YardsIn;
    public int TotalPar => ParOut + ParIn;

    /// <summary>
    /// Per-hole details (populated when ?includeHoles=true)
    /// </summary>
    public List<HoleTeeResponse>? Holes { get; set; }
}
