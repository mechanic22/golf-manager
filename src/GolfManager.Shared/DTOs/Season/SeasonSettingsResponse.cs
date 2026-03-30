using GolfManager.Core.Enums;

namespace GolfManager.Shared.DTOs.Season;

/// <summary>
/// Season settings response DTO
/// </summary>
public class SeasonSettingsResponse
{
    /// <summary>
    /// Settings ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Season ID
    /// </summary>
    public string SeasonId { get; set; } = string.Empty;

    /// <summary>
    /// League ID
    /// </summary>
    public string LeagueId { get; set; } = string.Empty;

    // Handicap Settings

    /// <summary>
    /// Type of handicap system
    /// </summary>
    public HandicapType HandicapType { get; set; }

    /// <summary>
    /// Maximum handicap allowed
    /// </summary>
    public int? MaxHandicap { get; set; }

    /// <summary>
    /// Maximum score per hole for handicap calculation
    /// </summary>
    public MaxScoreForHandicap MaxScoreForHandicap { get; set; }

    // Scoring Settings

    /// <summary>
    /// Individual scoring system
    /// </summary>
    public IndividualScoringType IndividualScoringType { get; set; }

    /// <summary>
    /// Team scoring system
    /// </summary>
    public TeamScoringType TeamScoringType { get; set; }

    /// <summary>
    /// How to handle missing players
    /// </summary>
    public MissingPlayerType MissingPlayerType { get; set; }

    /// <summary>
    /// How to handle missing teams
    /// </summary>
    public MissingTeamType MissingTeamType { get; set; }

    // Defaults

    /// <summary>
    /// Default course ID
    /// </summary>
    public string? DefaultCourseId { get; set; }

    /// <summary>
    /// Default course name
    /// </summary>
    public string? DefaultCourseName { get; set; }

    /// <summary>
    /// Default start time for events
    /// </summary>
    public TimeOnly? DefaultStartTime { get; set; }

    /// <summary>
    /// When the settings were created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the settings were last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

