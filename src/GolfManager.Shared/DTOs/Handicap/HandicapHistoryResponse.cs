namespace GolfManager.Shared.DTOs.Handicap;

/// <summary>
/// Response for handicap history entry
/// </summary>
public class HandicapHistoryResponse
{
    /// <summary>
    /// Handicap history ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Golfer ID
    /// </summary>
    public string GolferId { get; set; } = string.Empty;

    /// <summary>
    /// League ID (null for global handicap)
    /// </summary>
    public string? LeagueId { get; set; }

    /// <summary>
    /// Season ID (null for league/global handicap)
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
    /// Calculation method used
    /// </summary>
    public string? CalculationMethod { get; set; }

    /// <summary>
    /// Number of rounds used in calculation
    /// </summary>
    public int? RoundsUsed { get; set; }

    /// <summary>
    /// Additional notes
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// When this entry was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// Who created this entry
    /// </summary>
    public string? CreatedBy { get; set; }
}
