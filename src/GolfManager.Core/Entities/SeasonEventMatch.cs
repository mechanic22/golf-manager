using GolfManager.Core.Common;

namespace GolfManager.Core.Entities;

/// <summary>
/// SeasonEventMatch - Team match within a season event (match play)
/// </summary>
public class SeasonEventMatch : BaseEntity, ITenantEntity
{
    /// <summary>
    /// Season event ID
    /// </summary>
    public string SeasonEventId { get; set; } = string.Empty;

    /// <summary>
    /// League ID (tenant isolation)
    /// </summary>
    public string LeagueId { get; set; } = string.Empty;

    /// <summary>
    /// Scorecard ID (optional)
    /// </summary>
    public string? ScorecardId { get; set; }

    /// <summary>
    /// Home team ID
    /// </summary>
    public string? HomeTeamId { get; set; }

    /// <summary>
    /// Away team ID
    /// </summary>
    public string? AwayTeamId { get; set; }

    /// <summary>
    /// Season golfer ID acting as home team substitute for this weekly match
    /// </summary>
    public string? HomeSubSeasonGolferId { get; set; }

    /// <summary>
    /// Season golfer ID acting as away team substitute for this weekly match
    /// </summary>
    public string? AwaySubSeasonGolferId { get; set; }

    /// <summary>
    /// Points earned by home team
    /// </summary>
    public double? HomePoints { get; set; }

    /// <summary>
    /// Points earned by away team
    /// </summary>
    public double? AwayPoints { get; set; }

    /// <summary>
    /// Starting hole number for this match
    /// </summary>
    public int? StartingHole { get; set; }

    /// <summary>
    /// Starting flight number for this match
    /// </summary>
    public int? StartingFlight { get; set; }

    /// <summary>
    /// Is the match completed?
    /// </summary>
    public bool IsComplete { get; set; }

    // Navigation Properties

    /// <summary>
    /// Associated season event
    /// </summary>
    public SeasonEvent SeasonEvent { get; set; } = null!;

    /// <summary>
    /// Associated scorecard (if any)
    /// </summary>
    public Scorecard? Scorecard { get; set; }

    /// <summary>
    /// Home team
    /// </summary>
    public SeasonTeam? HomeTeam { get; set; }

    /// <summary>
    /// Away team
    /// </summary>
    public SeasonTeam? AwayTeam { get; set; }
}
