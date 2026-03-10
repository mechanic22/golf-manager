using GolfManager.Core.Common;

namespace GolfManager.Core.Entities;

/// <summary>
/// Tee entity - A set of tees on a course (e.g., Blue, White, Red)
/// </summary>
public class Tee : BaseEntity
{
    /// <summary>
    /// Course ID
    /// </summary>
    public string CourseId { get; set; } = string.Empty;

    /// <summary>
    /// Tee name (e.g., "Blue", "White", "Red", "Gold")
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// HTML color code for display (e.g., "#0000FF" for blue)
    /// </summary>
    public string HtmlColorCode { get; set; } = "#FFFFFF";

    /// <summary>
    /// Course rating for front 9
    /// </summary>
    public double RatingOut { get; set; }

    /// <summary>
    /// Slope rating for front 9
    /// </summary>
    public int SlopeOut { get; set; }

    /// <summary>
    /// Course rating for back 9
    /// </summary>
    public double RatingIn { get; set; }

    /// <summary>
    /// Slope rating for back 9
    /// </summary>
    public int SlopeIn { get; set; }

    /// <summary>
    /// Total yardage for front 9
    /// </summary>
    public int YardsOut { get; set; }

    /// <summary>
    /// Total yardage for back 9
    /// </summary>
    public int YardsIn { get; set; }

    /// <summary>
    /// Par for front 9
    /// </summary>
    public int ParOut { get; set; }

    /// <summary>
    /// Par for back 9
    /// </summary>
    public int ParIn { get; set; }

    // Computed Properties

    /// <summary>
    /// Total course rating (Out + In)
    /// </summary>
    public double TotalRating => RatingOut + RatingIn;

    /// <summary>
    /// Average slope rating
    /// </summary>
    public int AverageSlope => (SlopeOut + SlopeIn) / 2;

    /// <summary>
    /// Total yardage (Out + In)
    /// </summary>
    public int TotalYards => YardsOut + YardsIn;

    /// <summary>
    /// Total par (Out + In)
    /// </summary>
    public int TotalPar => ParOut + ParIn;

    // Navigation Properties

    /// <summary>
    /// Associated course
    /// </summary>
    public Course Course { get; set; } = null!;

    /// <summary>
    /// Hole-specific tee information
    /// </summary>
    public ICollection<HoleTee> HoleTees { get; set; } = new List<HoleTee>();

    /// <summary>
    /// Rounds played from this tee
    /// </summary>
    public ICollection<Round> Rounds { get; set; } = new List<Round>();
}

