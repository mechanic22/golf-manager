using System.ComponentModel.DataAnnotations;
using GolfManager.Core.Enums;

namespace GolfManager.Shared.DTOs.Event;

/// <summary>
/// Request to create a new event
/// </summary>
public class CreateEventRequest
{
    /// <summary>
    /// Event date
    /// </summary>
    [Required]
    public DateTime EventDate { get; set; }

    /// <summary>
    /// Course ID (optional - may be TBD)
    /// </summary>
    public string? CourseId { get; set; }

    /// <summary>
    /// Tee ID (optional)
    /// </summary>
    public string? TeeId { get; set; }

    /// <summary>
    /// Number of holes played (9 or 18)
    /// </summary>
    public HolesPlayed HolesPlayed { get; set; } = HolesPlayed.Eighteen;

    /// <summary>
    /// Event type (regular, playoff, championship, etc.)
    /// </summary>
    public SeasonEventType EventType { get; set; } = SeasonEventType.Regular;

    /// <summary>
    /// Scoring format (stroke play, match play, stableford, etc.)
    /// </summary>
    public ScoringFormat ScoringFormat { get; set; } = ScoringFormat.StrokePlay;

    /// <summary>
    /// Event name/title
    /// </summary>
    [StringLength(100)]
    public string? Name { get; set; }

    /// <summary>
    /// Event description/notes
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// Optional game-of-the-day title.
    /// </summary>
    [StringLength(150)]
    public string? GameOfDayTitle { get; set; }

    /// <summary>
    /// Optional season golfer id selected as game-of-the-day winner.
    /// </summary>
    [StringLength(50)]
    public string? GameOfDayWinnerSeasonGolferId { get; set; }

    /// <summary>
    /// Optional winner display name fallback for non-rostered winners.
    /// </summary>
    [StringLength(120)]
    public string? GameOfDayWinnerDisplayName { get; set; }
}

