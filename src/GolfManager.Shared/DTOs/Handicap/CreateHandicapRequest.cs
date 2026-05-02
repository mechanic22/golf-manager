using System.ComponentModel.DataAnnotations;

namespace GolfManager.Shared.DTOs.Handicap;

/// <summary>
/// Request to create/update a handicap
/// </summary>
public class CreateHandicapRequest
{
    /// <summary>
    /// League ID (optional - null means global handicap)
    /// </summary>
    public string? LeagueId { get; set; }

    /// <summary>
    /// Season ID (optional - null means league/global handicap)
    /// </summary>
    public string? SeasonId { get; set; }

    /// <summary>
    /// Handicap index value
    /// </summary>
    [Required]
    [Range(-10, 54, ErrorMessage = "Handicap must be between -10 and 54")]
    public double HandicapIndex { get; set; }

    /// <summary>
    /// Effective date (defaults to today if not provided)
    /// </summary>
    public DateOnly? EffectiveDate { get; set; }

    /// <summary>
    /// Calculation method (e.g., "Manual", "Bob's", "USGA", "Scratch")
    /// </summary>
    [MaxLength(50)]
    public string? CalculationMethod { get; set; }

    /// <summary>
    /// Number of rounds used in calculation
    /// </summary>
    [Range(0, 100)]
    public int? RoundsUsed { get; set; }

    /// <summary>
    /// Additional notes
    /// </summary>
    [MaxLength(500)]
    public string? Notes { get; set; }
}
