using System.ComponentModel.DataAnnotations;
using GolfManager.Core.Enums;

namespace GolfManager.Shared.DTOs.Event;

/// <summary>
/// Request to update an existing event
/// </summary>
public class UpdateEventRequest
{
    /// <summary>
    /// Event date
    /// </summary>
    public DateTime? EventDate { get; set; }

    /// <summary>
    /// Course ID
    /// </summary>
    public string? CourseId { get; set; }

    /// <summary>
    /// Tee ID
    /// </summary>
    public string? TeeId { get; set; }

    /// <summary>
    /// Number of holes played
    /// </summary>
    public HolesPlayed? HolesPlayed { get; set; }

    /// <summary>
    /// Event type
    /// </summary>
    public SeasonEventType? EventType { get; set; }

    /// <summary>
    /// Scoring format
    /// </summary>
    public ScoringFormat? ScoringFormat { get; set; }

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
    /// Is the event locked?
    /// </summary>
    public bool? IsLocked { get; set; }
}

