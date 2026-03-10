using GolfManager.Core.Common;

namespace GolfManager.Core.Entities;

/// <summary>
/// LeagueGolfer - Golfer's profile within a specific league
/// Allows golfers to have different display names, handicaps, etc. per league
/// </summary>
public class LeagueGolfer : BaseEntity, ITenantEntity
{
    /// <summary>
    /// Global golfer ID
    /// </summary>
    public string GolferId { get; set; } = string.Empty;

    /// <summary>
    /// League ID (tenant isolation)
    /// </summary>
    public string LeagueId { get; set; } = string.Empty;

    /// <summary>
    /// Display name in this league (may differ from global)
    /// </summary>
    public string DisplayName { get; set; } = string.Empty;

    /// <summary>
    /// Nickname in this league
    /// </summary>
    public string? Nickname { get; set; }

    /// <summary>
    /// League-specific handicap
    /// </summary>
    public double? LeagueHandicap { get; set; }

    /// <summary>
    /// When the league handicap was last updated
    /// </summary>
    public DateTime? LeagueHandicapUpdatedAt { get; set; }

    /// <summary>
    /// Total rounds played in this league
    /// </summary>
    public int TotalRounds { get; set; }

    /// <summary>
    /// Average score in this league
    /// </summary>
    public double? AverageScore { get; set; }

    /// <summary>
    /// Best score in this league
    /// </summary>
    public int? BestScore { get; set; }

    /// <summary>
    /// When the golfer joined this league
    /// </summary>
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;

    // Navigation Properties

    /// <summary>
    /// Associated global golfer
    /// </summary>
    public Golfer Golfer { get; set; } = null!;

    /// <summary>
    /// Associated league
    /// </summary>
    public League League { get; set; } = null!;

    /// <summary>
    /// Season participations
    /// </summary>
    public ICollection<SeasonGolfer> SeasonGolfers { get; set; } = new List<SeasonGolfer>();

    /// <summary>
    /// Rounds played in this league
    /// </summary>
    public ICollection<Round> Rounds { get; set; } = new List<Round>();
}

