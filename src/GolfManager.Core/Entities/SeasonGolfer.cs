using GolfManager.Core.Common;

namespace GolfManager.Core.Entities;

/// <summary>
/// SeasonGolfer - Golfer participation in a specific season
/// </summary>
public class SeasonGolfer : BaseEntity, ITenantEntity
{
    /// <summary>
    /// Season ID
    /// </summary>
    public string SeasonId { get; set; } = string.Empty;

    /// <summary>
    /// League ID (tenant isolation)
    /// </summary>
    public string LeagueId { get; set; } = string.Empty;

    /// <summary>
    /// LeagueGolfer ID
    /// </summary>
    public string LeagueGolferId { get; set; } = string.Empty;

    /// <summary>
    /// Global Golfer ID (denormalized for queries)
    /// </summary>
    public string GolferId { get; set; } = string.Empty;

    /// <summary>
    /// Team assignment (optional)
    /// </summary>
    public string? TeamId { get; set; }

    /// <summary>
    /// Season-specific handicap
    /// </summary>
    public double? SeasonHandicap { get; set; }

    /// <summary>
    /// Total events participated in
    /// </summary>
    public int TotalEvents { get; set; }

    /// <summary>
    /// Average score for the season
    /// </summary>
    public double? AverageScore { get; set; }

    /// <summary>
    /// Total points earned in the season
    /// </summary>
    public double? TotalPoints { get; set; }

    /// <summary>
    /// When the golfer joined this season
    /// </summary>
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties

    /// <summary>
    /// Associated season
    /// </summary>
    public Season Season { get; set; } = null!;

    /// <summary>
    /// Associated league golfer
    /// </summary>
    public LeagueGolfer LeagueGolfer { get; set; } = null!;

    /// <summary>
    /// Associated global golfer
    /// </summary>
    public Golfer Golfer { get; set; } = null!;

    /// <summary>
    /// Associated team (if assigned)
    /// </summary>
    public SeasonTeam? Team { get; set; }
}

