namespace GolfManager.Shared.DTOs.Round;

/// <summary>
/// Response containing hole score information
/// </summary>
public class RoundHoleResponse
{
    public int HoleNumber { get; set; }

    /// <summary>
    /// Gross score (actual strokes)
    /// </summary>
    public int? GrossScore { get; set; }

    /// <summary>
    /// Net score (after handicap strokes)
    /// </summary>
    public int? NetScore { get; set; }

    /// <summary>
    /// Putts on this hole
    /// </summary>
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
    public int? Penalties { get; set; }

    /// <summary>
    /// Hole notes
    /// </summary>
    public string? Notes { get; set; }
}

