using GolfManager.Core.Common;

namespace GolfManager.Core.Entities;

/// <summary>
/// Scorecard - Physical or digital scorecard for a round
/// </summary>
public class Scorecard : BaseEntity
{
    /// <summary>
    /// Round ID (one-to-one)
    /// </summary>
    public string RoundId { get; set; } = string.Empty;

    /// <summary>
    /// Scorecard image URL (if uploaded)
    /// </summary>
    public string? ImageUrl { get; set; }

    /// <summary>
    /// Weather conditions
    /// </summary>
    public string? Weather { get; set; }

    /// <summary>
    /// Temperature (Fahrenheit)
    /// </summary>
    public int? Temperature { get; set; }

    /// <summary>
    /// Wind conditions
    /// </summary>
    public string? Wind { get; set; }

    /// <summary>
    /// Course conditions notes
    /// </summary>
    public string? CourseConditions { get; set; }

    /// <summary>
    /// Playing partners (comma-separated names)
    /// </summary>
    public string? PlayingPartners { get; set; }

    /// <summary>
    /// Additional notes
    /// </summary>
    public string? Notes { get; set; }

    // Navigation Properties

    /// <summary>
    /// Associated round
    /// </summary>
    public Round Round { get; set; } = null!;
}

