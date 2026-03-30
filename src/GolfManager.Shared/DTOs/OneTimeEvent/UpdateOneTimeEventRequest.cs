using System.ComponentModel.DataAnnotations;
using GolfManager.Core.Enums;

namespace GolfManager.Shared.DTOs.OneTimeEvent;

/// <summary>
/// Request to update an existing one-time event
/// </summary>
public class UpdateOneTimeEventRequest
{
    /// <summary>
    /// Event name/title
    /// </summary>
    [StringLength(100, MinimumLength = 3)]
    public string? Name { get; set; }

    /// <summary>
    /// Event description
    /// </summary>
    [StringLength(1000)]
    public string? Description { get; set; }

    /// <summary>
    /// Event date
    /// </summary>
    public DateTime? EventDate { get; set; }

    /// <summary>
    /// Organization name
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

    /// <summary>
    /// Course ID
    /// </summary>
    [StringLength(50)]
    public string? CourseId { get; set; }

    /// <summary>
    /// Tee ID
    /// </summary>
    [StringLength(50)]
    public string? TeeId { get; set; }

    /// <summary>
    /// Number of holes played
    /// </summary>
    public HolesPlayed? HolesPlayed { get; set; }

    /// <summary>
    /// Scoring format
    /// </summary>
    public ScoringFormat? Format { get; set; }

    /// <summary>
    /// Team size
    /// </summary>
    [Range(1, 6, ErrorMessage = "Team size must be between 1 and 6")]
    public int? TeamSize { get; set; }

    /// <summary>
    /// Whether to use handicaps
    /// </summary>
    public bool? UseHandicaps { get; set; }

    /// <summary>
    /// Maximum number of teams
    /// </summary>
    [Range(1, 500, ErrorMessage = "Max teams must be between 1 and 500")]
    public int? MaxTeams { get; set; }

    /// <summary>
    /// Total number of rounds
    /// </summary>
    [Range(1, 10, ErrorMessage = "Total rounds must be between 1 and 10")]
    public int? TotalRounds { get; set; }

    /// <summary>
    /// Event access type
    /// </summary>
    public EventAccessType? AccessType { get; set; }

    /// <summary>
    /// Registration code
    /// </summary>
    [StringLength(50)]
    public string? RegistrationCode { get; set; }

    /// <summary>
    /// Registration deadline
    /// </summary>
    public DateTime? RegistrationDeadline { get; set; }
}

