using GolfManager.Core.Common;

namespace GolfManager.Core.Entities;

/// <summary>
/// Immutable calculated score for a golfer in a season event.
/// Populated when event is locked; never recalculated.
/// </summary>
public class SeasonEventPlayerScore : BaseEntity
{
    public required string SeasonEventId { get; set; }
    public required string SeasonGolferId { get; set; }
    public required string LeagueId { get; set; }

    /// <summary>
    /// Player's gross score (before handicap adjustment)
    /// </summary>
    public int? RawScore { get; set; }

    /// <summary>
    /// Player's handicap used for calculation
    /// </summary>
    public double? Handicap { get; set; }

    /// <summary>
    /// Player's net score (after handicap adjustment)
    /// </summary>
    public double? NetScore { get; set; }

    /// <summary>
    /// Points awarded for this player's performance
    /// </summary>
    public double? EventPoints { get; set; }

    /// <summary>
    /// True if player did not submit a score and a miss score was calculated
    /// </summary>
    public bool IsMissing { get; set; }

    /// <summary>
    /// Score used when player missed (field average, par, etc.)
    /// </summary>
    public double? MissScore { get; set; }

    /// <summary>
    /// Denormalized player name for display (avoids join on lookup)
    /// </summary>
    public required string DisplayName { get; set; }

    /// <summary>
    /// Team ID if player is on a team
    /// </summary>
    public string? TeamId { get; set; }

    /// <summary>
    /// Team name for display (denormalized)
    /// </summary>
    public string? TeamName { get; set; }

    // Navigation properties
    public virtual SeasonEvent? SeasonEvent { get; set; }
    public virtual SeasonGolfer? SeasonGolfer { get; set; }
}
