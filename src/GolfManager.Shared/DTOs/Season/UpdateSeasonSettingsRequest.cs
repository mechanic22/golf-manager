using System.ComponentModel.DataAnnotations;
using GolfManager.Core.Enums;

namespace GolfManager.Shared.DTOs.Season;

/// <summary>
/// Request to update season settings
/// </summary>
public class UpdateSeasonSettingsRequest
{
    // Handicap Settings

    /// <summary>
    /// Type of handicap system
    /// </summary>
    public HandicapType? HandicapType { get; set; }

    /// <summary>
    /// Maximum handicap allowed
    /// </summary>
    [Range(0, 54, ErrorMessage = "Max handicap must be between 0 and 54")]
    public int? MaxHandicap { get; set; }

    /// <summary>
    /// Maximum score per hole for handicap calculation
    /// </summary>
    public MaxScoreForHandicap? MaxScoreForHandicap { get; set; }

    // Scoring Settings

    /// <summary>
    /// Individual scoring system
    /// </summary>
    public IndividualScoringType? IndividualScoringType { get; set; }

    /// <summary>
    /// Team scoring system
    /// </summary>
    public TeamScoringType? TeamScoringType { get; set; }

    /// <summary>
    /// How to handle missing players
    /// </summary>
    public MissingPlayerType? MissingPlayerType { get; set; }

    /// <summary>
    /// How to handle missing teams
    /// </summary>
    public MissingTeamType? MissingTeamType { get; set; }

    // Defaults

    /// <summary>
    /// Default course ID
    /// </summary>
    [StringLength(50)]
    public string? DefaultCourseId { get; set; }

    /// <summary>
    /// Default start time for events
    /// </summary>
    public TimeOnly? DefaultStartTime { get; set; }
}

