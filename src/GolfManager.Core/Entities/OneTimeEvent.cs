using GolfManager.Core.Common;
using GolfManager.Core.Enums;

namespace GolfManager.Core.Entities;

/// <summary>
/// OneTimeEvent - A standalone tournament/event (not part of a league/season)
/// </summary>
public class OneTimeEvent : BaseEntity
{
    /// <summary>
    /// Unique URL-friendly key for the event (e.g., "summer-scramble-2024")
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
    /// Organization name (optional)
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
    /// Course ID (optional - may be TBD)
    /// </summary>
    public string? CourseId { get; set; }

    /// <summary>
    /// Tee ID (optional)
    /// </summary>
    public string? TeeId { get; set; }

    /// <summary>
    /// Number of holes played (9, 18, Front, Back)
    /// </summary>
    public HolesPlayed HolesPlayed { get; set; } = HolesPlayed.Eighteen;

    // Tournament Settings

    /// <summary>
    /// Scoring format (StrokePlay, Scramble, BestBall, etc.)
    /// </summary>
    public ScoringFormat Format { get; set; } = ScoringFormat.StrokePlay;

    /// <summary>
    /// Team size (1 = individual, 2+ = team)
    /// </summary>
    public int TeamSize { get; set; } = 1;

    /// <summary>
    /// Whether to use handicaps for scoring
    /// </summary>
    public bool UseHandicaps { get; set; } = true;

    /// <summary>
    /// Maximum number of teams allowed (null = unlimited)
    /// </summary>
    public int? MaxTeams { get; set; }

    /// <summary>
    /// Total number of rounds for this event (default: 1)
    /// For single-round events, leave as 1
    /// For multi-round events (2-day scrambles, 36-hole tournaments), set to 2+
    /// </summary>
    public int TotalRounds { get; set; } = 1;

    // Access Control

    /// <summary>
    /// Event access type (Public, Private, InviteOnly)
    /// </summary>
    public EventAccessType AccessType { get; set; } = EventAccessType.Public;

    /// <summary>
    /// Registration code (required for Private events)
    /// </summary>
    public string? RegistrationCode { get; set; }

    /// <summary>
    /// Registration deadline
    /// </summary>
    public DateTime? RegistrationDeadline { get; set; }

    // Status

    /// <summary>
    /// Event status (Draft, Published, InProgress, Completed, Cancelled)
    /// </summary>
    public EventStatus Status { get; set; } = EventStatus.Draft;

    /// <summary>
    /// Is the event locked (scores finalized)?
    /// </summary>
    public bool IsLocked { get; set; }

    // Payment Information (for future use)

    /// <summary>
    /// Event tier (Basic, Premium, Enterprise)
    /// </summary>
    public string? Tier { get; set; }

    /// <summary>
    /// Payment status
    /// </summary>
    public string? PaymentStatus { get; set; }

    /// <summary>
    /// Stripe payment intent ID
    /// </summary>
    public string? StripePaymentIntentId { get; set; }

    // Navigation Properties

    /// <summary>
    /// Event organizer (user)
    /// </summary>
    public User Organizer { get; set; } = null!;

    /// <summary>
    /// Associated course (if set)
    /// </summary>
    public Course? Course { get; set; }

    /// <summary>
    /// Associated tee (if set)
    /// </summary>
    public Tee? Tee { get; set; }

    /// <summary>
    /// Teams registered for this event
    /// </summary>
    public ICollection<OneTimeEventTeam> Teams { get; set; } = new List<OneTimeEventTeam>();
}

