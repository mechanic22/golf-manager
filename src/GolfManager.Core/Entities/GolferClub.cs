using GolfManager.Core.Common;
using GolfManager.Core.Enums;

namespace GolfManager.Core.Entities;

/// <summary>
/// GolferClub - A golf club in a golfer's bag
/// </summary>
public class GolferClub : BaseEntity
{
    /// <summary>
    /// Golfer ID
    /// </summary>
    public string GolferId { get; set; } = string.Empty;

    /// <summary>
    /// Club type (Driver, 3-Wood, 5-Iron, etc.)
    /// </summary>
    public ClubType ClubType { get; set; }

    /// <summary>
    /// Club brand/manufacturer
    /// </summary>
    public string? Brand { get; set; }

    /// <summary>
    /// Club model
    /// </summary>
    public string? Model { get; set; }

    /// <summary>
    /// Average distance with this club (yards)
    /// </summary>
    public int? AverageDistance { get; set; }

    /// <summary>
    /// Club notes
    /// </summary>
    public string? Notes { get; set; }

    /// <summary>
    /// Is this club currently in the bag?
    /// </summary>
    public bool IsInBag { get; set; } = true;

    // Navigation Properties

    /// <summary>
    /// Associated golfer
    /// </summary>
    public Golfer Golfer { get; set; } = null!;
}

