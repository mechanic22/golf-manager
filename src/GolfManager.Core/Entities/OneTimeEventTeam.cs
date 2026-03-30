using GolfManager.Core.Common;

namespace GolfManager.Core.Entities;

/// <summary>
/// OneTimeEventTeam - A team registered for a one-time event
/// </summary>
public class OneTimeEventTeam : BaseEntity
{
    /// <summary>
    /// Event ID
    /// </summary>
    public string EventId { get; set; } = string.Empty;

    /// <summary>
    /// Team name
    /// </summary>
    public string TeamName { get; set; } = string.Empty;

    /// <summary>
    /// Team number (assigned by system)
    /// </summary>
    public int TeamNumber { get; set; }

    // Captain Information

    /// <summary>
    /// Captain user ID (if registered user)
    /// </summary>
    public string? CaptainUserId { get; set; }

    /// <summary>
    /// Captain name
    /// </summary>
    public string CaptainName { get; set; } = string.Empty;

    /// <summary>
    /// Captain email
    /// </summary>
    public string CaptainEmail { get; set; } = string.Empty;

    /// <summary>
    /// Captain phone
    /// </summary>
    public string? CaptainPhone { get; set; }

    // Registration

    /// <summary>
    /// When the team registered
    /// </summary>
    public DateTime RegisteredAt { get; set; } = DateTime.UtcNow;

    /// <summary>
    /// Is the team checked in for the event?
    /// </summary>
    public bool IsCheckedIn { get; set; }

    /// <summary>
    /// When the team checked in
    /// </summary>
    public DateTime? CheckedInAt { get; set; }

    // Scoring

    /// <summary>
    /// Total gross score
    /// </summary>
    public int? TotalScore { get; set; }

    /// <summary>
    /// Total net score (with handicaps)
    /// </summary>
    public int? NetScore { get; set; }

    /// <summary>
    /// Leaderboard position
    /// </summary>
    public int? Position { get; set; }

    // Navigation Properties

    /// <summary>
    /// Associated event
    /// </summary>
    public OneTimeEvent Event { get; set; } = null!;

    /// <summary>
    /// Captain (if registered user)
    /// </summary>
    public User? Captain { get; set; }

    /// <summary>
    /// Team players
    /// </summary>
    public ICollection<OneTimeEventPlayer> Players { get; set; } = new List<OneTimeEventPlayer>();

    /// <summary>
    /// Rounds played by this team
    /// </summary>
    public ICollection<Round> Rounds { get; set; } = new List<Round>();
}

