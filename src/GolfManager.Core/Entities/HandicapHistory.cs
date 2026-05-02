using GolfManager.Core.Common;

namespace GolfManager.Core.Entities;

/// <summary>
/// Handicap history entity - Tracks handicap changes over time
/// </summary>
public class HandicapHistory : BaseEntity
{
    /// <summary>
    /// Golfer ID (required)
    /// </summary>
    public string GolferId { get; set; } = string.Empty;

    /// <summary>
    /// League ID (optional - null means global handicap)
    /// </summary>
    public string? LeagueId { get; set; }

    /// <summary>
    /// Season ID (optional - null means league or global handicap)
    /// </summary>
    public string? SeasonId { get; set; }

    /// <summary>
    /// Handicap index value
    /// </summary>
    public double HandicapIndex { get; set; }

    /// <summary>
    /// Effective date of this handicap
    /// </summary>
    public DateOnly EffectiveDate { get; set; }

    /// <summary>
    /// Method used to calculate this handicap
    /// </summary>
    public string? CalculationMethod { get; set; }

    /// <summary>
    /// Number of rounds used in calculation
    /// </summary>
    public int? RoundsUsed { get; set; }

    /// <summary>
    /// Additional notes about this handicap change
    /// </summary>
    public string? Notes { get; set; }

    // Navigation Properties

    /// <summary>
    /// Associated golfer
    /// </summary>
    public Golfer Golfer { get; set; } = null!;

    /// <summary>
    /// Associated league (if league handicap)
    /// </summary>
    public League? League { get; set; }

    /// <summary>
    /// Associated season (if season handicap)
    /// </summary>
    public Season? Season { get; set; }
}
