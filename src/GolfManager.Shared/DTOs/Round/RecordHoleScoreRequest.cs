using System.ComponentModel.DataAnnotations;

namespace GolfManager.Shared.DTOs.Round;

/// <summary>
/// Request to record or update a hole score
/// </summary>
public class RecordHoleScoreRequest
{
    /// <summary>
    /// Hole number (1-18)
    /// </summary>
    [Required]
    [Range(1, 18)]
    public int HoleNumber { get; set; }

    /// <summary>
    /// Gross score (actual strokes)
    /// </summary>
    [Range(1, 20)]
    public int? GrossScore { get; set; }

    /// <summary>
    /// Putts on this hole
    /// </summary>
    [Range(0, 10)]
    public int? Putts { get; set; }

    /// <summary>
    /// Fairway hit? (null for par 3s)
    /// </summary>
    public bool? FairwayHit { get; set; }

    /// <summary>
    /// Green in regulation?
    /// </summary>
    public bool? GreenInRegulation { get; set; }

    /// <summary>
    /// Number of penalty strokes
    /// </summary>
    [Range(0, 10)]
    public int? Penalties { get; set; }

    /// <summary>
    /// Hole notes
    /// </summary>
    public string? Notes { get; set; }
}

