using GolfManager.Core.Enums;

namespace GolfManager.Shared.DTOs.Event;

/// <summary>
/// Event response DTO
/// </summary>
public class EventResponse
{
    /// <summary>
    /// Event ID
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

    /// <summary>
    /// Event date
    /// </summary>
    public DateTime EventDate { get; set; }

    /// <summary>
    /// Course ID (optional)
    /// </summary>
    public string? CourseId { get; set; }

    /// <summary>
    /// Tee ID (optional)
    /// </summary>
    public string? TeeId { get; set; }

    /// <summary>
    /// Number of holes played
    /// </summary>
    public HolesPlayed HolesPlayed { get; set; }

    /// <summary>
    /// Event type
    /// </summary>
    public SeasonEventType EventType { get; set; }

    /// <summary>
    /// Scoring format
    /// </summary>
    public ScoringFormat ScoringFormat { get; set; }

    /// <summary>
    /// Event name/title
    /// </summary>
    public string? Name { get; set; }

    /// <summary>
    /// Event description/notes
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Is the event locked?
    /// </summary>
    public bool IsLocked { get; set; }

    /// <summary>
    /// When the event was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the event was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

