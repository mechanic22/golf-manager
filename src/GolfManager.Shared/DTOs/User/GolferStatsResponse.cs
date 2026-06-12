namespace GolfManager.Shared.DTOs.User;

/// <summary>
/// Aggregated statistics for a golfer's own profile view
/// </summary>
public class GolferStatsResponse
{
    public int RoundsPlayed { get; set; }
    public double? AverageGrossScore { get; set; }
    public double? AverageNetScore { get; set; }
    public int? LowGrossScore { get; set; }
    public int? LowNetScore { get; set; }
    public double? CurrentHandicap { get; set; }
    public int RoundsThisYear { get; set; }
    public int RoundsLast30Days { get; set; }
    public List<CourseStatEntry> CourseStats { get; set; } = [];
}

public class CourseStatEntry
{
    public string CourseName { get; set; } = string.Empty;
    public int Rounds { get; set; }
    public double AverageScore { get; set; }
    public int BestScore { get; set; }
}
