using GolfManager.Core.Common;

namespace GolfManager.Core.Entities;

/// <summary>
/// HoleTee - Tee-specific information for a hole
/// Links a specific tee to a hole with yardage, par, handicap
/// </summary>
public class HoleTee : BaseEntity
{
    /// <summary>
    /// Tee ID
    /// </summary>
    public string TeeId { get; set; } = string.Empty;

    /// <summary>
    /// Hole number (1-18)
    /// </summary>
    public int HoleNumber { get; set; }

    /// <summary>
    /// Par for this hole from this tee
    /// </summary>
    public int Par { get; set; }

    /// <summary>
    /// Yardage for this hole from this tee
    /// </summary>
    public int Yardage { get; set; }

    /// <summary>
    /// Handicap stroke index (1-18, where 1 is hardest)
    /// </summary>
    public int Handicap { get; set; }

    // Navigation Properties

    /// <summary>
    /// Associated tee
    /// </summary>
    public Tee Tee { get; set; } = null!;

    /// <summary>
    /// Associated hole
    /// </summary>
    public Hole Hole { get; set; } = null!;
}

