using System.ComponentModel.DataAnnotations;
using GolfManager.Core.Enums;

namespace GolfManager.Shared.DTOs.OneTimeEvent;

/// <summary>
/// Request to create a new one-time event
/// </summary>
public class CreateOneTimeEventRequest
{
    /// <summary>
    /// URL-friendly key for the event (e.g., "summer-scramble-2024")
    /// </summary>
    [Required]
    [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Key must contain only lowercase letters, numbers, and hyphens")]
    [StringLength(50, MinimumLength = 3)]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Event name/title
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 3)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Event description
    /// </summary>
    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Event date
    /// </summary>
    [Required]
    public DateTime EventDate { get; set; }

    // Organizer Information

    /// <summary>
    /// Organization name (optional)
    /// </summary>
    [StringLength(100)]
    public string? OrganizationName { get; set; }

    /// <summary>
    /// Organizer contact email
    /// </summary>
    [EmailAddress]
    [StringLength(100)]
    public string? OrganizerEmail { get; set; }

    /// <summary>
    /// Organizer contact phone
    /// </summary>
    [Phone]
    [StringLength(20)]
    public string? OrganizerPhone { get; set; }

    // Venue Information

    /// <summary>
    /// Course ID (optional - may be TBD)
    /// </summary>
    [StringLength(50)]
    public string? CourseId { get; set; }

    /// <summary>
    /// Tee ID (optional)
    /// </summary>
    [StringLength(50)]
    public string? TeeId { get; set; }

    /// <summary>
    /// Number of holes played (9, 18, Front, Back)
    /// </summary>
    [Required]
    public HolesPlayed HolesPlayed { get; set; } = HolesPlayed.Eighteen;

    // Tournament Settings

    /// <summary>
    /// Scoring format (StrokePlay, Scramble, BestBall, etc.)
    /// </summary>
    [Required]
    public ScoringFormat Format { get; set; } = ScoringFormat.Scramble;

    /// <summary>
    /// Team size (1 = individual, 2+ = team)
    /// </summary>
    [Required]
    [Range(1, 6, ErrorMessage = "Team size must be between 1 and 6")]
    public int TeamSize { get; set; } = 4;

    /// <summary>
    /// Whether to use handicaps for scoring
    /// </summary>
    public bool UseHandicaps { get; set; } = true;

    /// <summary>
    /// Maximum number of teams allowed (null = unlimited)
    /// </summary>
    [Range(1, 500, ErrorMessage = "Max teams must be between 1 and 500")]
    public int? MaxTeams { get; set; }

    /// <summary>
    /// Total number of rounds for this event (default: 1)
    /// </summary>
    [Required]
    [Range(1, 10, ErrorMessage = "Total rounds must be between 1 and 10")]
    public int TotalRounds { get; set; } = 1;

    // Access Control

    /// <summary>
    /// Event access type (Public, Private, InviteOnly)
    /// </summary>
    [Required]
    public EventAccessType AccessType { get; set; } = EventAccessType.Public;

    /// <summary>
    /// Registration code (required for Private events)
    /// </summary>
    [StringLength(50)]
    public string? RegistrationCode { get; set; }

    /// <summary>
    /// Registration deadline
    /// </summary>
    public DateTime? RegistrationDeadline { get; set; }
}

