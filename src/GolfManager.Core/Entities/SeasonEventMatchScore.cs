using GolfManager.Core.Common;

namespace GolfManager.Core.Entities;

/// <summary>
/// Immutable calculated score for a match/team competition in a season event.
/// Populated when event is locked; never recalculated.
/// </summary>
public class SeasonEventMatchScore : BaseEntity
{
    public required string SeasonEventId { get; set; }
    public required string SeasonEventMatchId { get; set; }
    public required string LeagueId { get; set; }

    /// <summary>
    /// Home team/sub ID (references SeasonTeam.Id or SeasonGolfer.Id)
    /// </summary>
    public string? HomeTeamId { get; set; }

    /// <summary>
    /// Home team/sub display name (denormalized)
    /// </summary>
    public string? HomeTeamName { get; set; }

    /// <summary>
    /// Points awarded to home team/golfer
    /// </summary>
    public double? HomePoints { get; set; }

    /// <summary>
    /// Away team/sub ID (references SeasonTeam.Id or SeasonGolfer.Id)
    /// </summary>
    public string? AwayTeamId { get; set; }

    /// <summary>
    /// Away team/sub display name (denormalized)
    /// </summary>
    public string? AwayTeamName { get; set; }

    /// <summary>
    /// Points awarded to away team/golfer
    /// </summary>
    public double? AwayPoints { get; set; }

    /// <summary>
    /// True if both teams/golfers completed their rounds
    /// </summary>
    public bool IsComplete { get; set; }

    /// <summary>
    /// Starting hole number if applicable
    /// </summary>
    public int? StartingHole { get; set; }

    /// <summary>
    /// Starting flight/grouping if applicable
    /// </summary>
    public int? StartingFlight { get; set; }

    // Navigation properties
    public virtual SeasonEvent? SeasonEvent { get; set; }
    public virtual SeasonEventMatch? SeasonEventMatch { get; set; }
}
