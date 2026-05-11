namespace GolfManager.Shared.DTOs.Handicap;

public class HandicapCalculationResponse
{
    public string GolferId { get; set; } = string.Empty;
    public double HandicapIndex { get; set; }
    public HandicapCalculationMethod Method { get; set; }
    public int RoundsUsed { get; set; }
    public int RoundsConsidered { get; set; }
    public List<ScoreDifferentialDetail> Differentials { get; set; } = new();
    public string? Notes { get; set; }
    public bool Persisted { get; set; }
}

public class ScoreDifferentialDetail
{
    public string RoundId { get; set; } = string.Empty;
    public DateTime RoundDate { get; set; }
    public string CourseName { get; set; } = string.Empty;
    public string TeeName { get; set; } = string.Empty;
    public int GrossScore { get; set; }
    public double CourseRating { get; set; }
    public int SlopeRating { get; set; }
    public double Differential { get; set; }
    public bool UsedInCalculation { get; set; }
}
