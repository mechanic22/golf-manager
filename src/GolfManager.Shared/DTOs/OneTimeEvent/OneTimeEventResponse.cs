using GolfManager.Core.Enums;

namespace GolfManager.Shared.DTOs.OneTimeEvent;

/// <summary>
/// Response containing one-time event information
/// </summary>
public class OneTimeEventResponse
{
    /// <summary>
    /// Event ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// URL-friendly key
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Event name/title
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Event description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// Event date
    /// </summary>
    public DateTime EventDate { get; set; }

    // Organizer Information

    /// <summary>
    /// User ID of the event organizer
    /// </summary>
    public string OrganizerId { get; set; } = string.Empty;

    /// <summary>
    /// Organizer name (from User entity)
    /// </summary>
    public string OrganizerName { get; set; } = string.Empty;

    /// <summary>
    /// Organization name
    /// </summary>
    public string? OrganizationName { get; set; }

    /// <summary>
    /// Organizer contact email
    /// </summary>
    public string? OrganizerEmail { get; set; }

    /// <summary>
    /// Organizer contact phone
    /// </summary>
    public string? OrganizerPhone { get; set; }

    // Venue Information

    /// <summary>
    /// Course ID
    /// </summary>
    public string? CourseId { get; set; }

    /// <summary>
    /// Course name (from Course entity)
    /// </summary>
    public string? CourseName { get; set; }

    /// <summary>
    /// Tee ID
    /// </summary>
    public string? TeeId { get; set; }

    /// <summary>
    /// Tee name (from Tee entity)
    /// </summary>
    public string? TeeName { get; set; }

    /// <summary>
    /// Number of holes played
    /// </summary>
    public HolesPlayed HolesPlayed { get; set; }

    // Tournament Settings

    /// <summary>
    /// Scoring format
    /// </summary>
    public ScoringFormat Format { get; set; }

    /// <summary>
    /// Team size
    /// </summary>
    public int TeamSize { get; set; }

    /// <summary>
    /// Whether handicaps are used
    /// </summary>
    public bool UseHandicaps { get; set; }

    /// <summary>
    /// Maximum number of teams
    /// </summary>
    public int? MaxTeams { get; set; }

    /// <summary>
    /// Total number of rounds
    /// </summary>
    public int TotalRounds { get; set; }

    // Access Control

    /// <summary>
    /// Event access type
    /// </summary>
    public EventAccessType AccessType { get; set; }

    /// <summary>
    /// Registration deadline
    /// </summary>
    public DateTime? RegistrationDeadline { get; set; }

    // Status

    /// <summary>
    /// Event status
    /// </summary>
    public EventStatus Status { get; set; }

    /// <summary>
    /// Is the event locked (scores finalized)?
    /// </summary>
    public bool IsLocked { get; set; }

    // Statistics

    /// <summary>
    /// Number of teams registered
    /// </summary>
    public int RegisteredTeamsCount { get; set; }

    /// <summary>
    /// Number of teams checked in
    /// </summary>
    public int CheckedInTeamsCount { get; set; }

    /// <summary>
    /// Spots remaining (null if unlimited)
    /// </summary>
    public int? SpotsRemaining { get; set; }

    // Timestamps

    /// <summary>
    /// When the event was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the event was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

