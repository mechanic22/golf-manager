using GolfManager.Core.Enums;

namespace GolfManager.Shared.DTOs.OneTimeEvent;

/// <summary>
/// Lightweight response for listing one-time events
/// </summary>
public class OneTimeEventListResponse
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
    /// Event date
    /// </summary>
    public DateTime EventDate { get; set; }

    /// <summary>
    /// Organizer name
    /// </summary>
    public string OrganizerName { get; set; } = string.Empty;

    /// <summary>
    /// Organization name
    /// </summary>
    public string? OrganizationName { get; set; }

    /// <summary>
    /// Course name
    /// </summary>
    public string? CourseName { get; set; }

    /// <summary>
    /// Scoring format
    /// </summary>
    public ScoringFormat Format { get; set; }

    /// <summary>
    /// Team size
    /// </summary>
    public int TeamSize { get; set; }

    /// <summary>
    /// Total number of rounds
    /// </summary>
    public int TotalRounds { get; set; }

    /// <summary>
    /// Event access type
    /// </summary>
    public EventAccessType AccessType { get; set; }

    /// <summary>
    /// Event status
    /// </summary>
    public EventStatus Status { get; set; }

    /// <summary>
    /// Number of teams registered
    /// </summary>
    public int RegisteredTeamsCount { get; set; }

    /// <summary>
    /// Maximum number of teams
    /// </summary>
    public int? MaxTeams { get; set; }

    /// <summary>
    /// Spots remaining (null if unlimited)
    /// </summary>
    public int? SpotsRemaining { get; set; }

    /// <summary>
    /// Registration deadline
    /// </summary>
    public DateTime? RegistrationDeadline { get; set; }

    /// <summary>
    /// Is registration open?
    /// </summary>
    public bool IsRegistrationOpen { get; set; }
}

