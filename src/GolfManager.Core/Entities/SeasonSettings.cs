using GolfManager.Core.Common;
using GolfManager.Core.Enums;

namespace GolfManager.Core.Entities;

/// <summary>
/// Season settings entity - Configuration for a season
/// </summary>
public class SeasonSettings : BaseEntity, ITenantEntity
{
    /// <summary>
    /// Season ID
    /// </summary>
    public string SeasonId { get; set; } = string.Empty;

    /// <summary>
    /// League ID (tenant isolation)
    /// </summary>
    public string LeagueId { get; set; } = string.Empty;

    // Handicap Settings

    /// <summary>
    /// Type of handicap system to use
    /// </summary>
    public HandicapType HandicapType { get; set; } = HandicapType.None;

    /// <summary>
    /// Maximum handicap allowed
    /// </summary>
    public int? MaxHandicap { get; set; }

    /// <summary>
    /// Maximum score per hole for handicap calculation
    /// </summary>
    public MaxScoreForHandicap MaxScoreForHandicap { get; set; } = MaxScoreForHandicap.None;

    // Scoring Settings

    /// <summary>
    /// Individual scoring system
    /// </summary>
    public IndividualScoringType IndividualScoringType { get; set; } = IndividualScoringType.None;

    /// <summary>
    /// Team scoring system
    /// </summary>
    public TeamScoringType TeamScoringType { get; set; } = TeamScoringType.None;

    /// <summary>
    /// How to handle missing players
    /// </summary>
    public MissingPlayerType MissingPlayerType { get; set; } = MissingPlayerType.None;

    /// <summary>
    /// How to handle missing teams
    /// </summary>
    public MissingTeamType MissingTeamType { get; set; } = MissingTeamType.NoPoints;

    // Defaults

    /// <summary>
    /// Default course ID for events
    /// </summary>
    public string? DefaultCourseId { get; set; }

    /// <summary>
    /// Default start time for events
    /// </summary>
    public TimeOnly? DefaultStartTime { get; set; }

    // Navigation Properties

    /// <summary>
    /// Associated season
    /// </summary>
    public Season Season { get; set; } = null!;

    /// <summary>
    /// Default course (if set)
    /// </summary>
    public Course? DefaultCourse { get; set; }
}

