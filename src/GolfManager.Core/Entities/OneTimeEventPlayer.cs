using GolfManager.Core.Common;

namespace GolfManager.Core.Entities;

/// <summary>
/// OneTimeEventPlayer - A player on a team for a one-time event
/// </summary>
public class OneTimeEventPlayer : BaseEntity
{
    /// <summary>
    /// Team ID
    /// </summary>
    public string TeamId { get; set; } = string.Empty;

    /// <summary>
    /// Event ID (denormalized for easier queries)
    /// </summary>
    public string EventId { get; set; } = string.Empty;

    /// <summary>
    /// User ID (if registered user)
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Player name
    /// </summary>
    public string PlayerName { get; set; } = string.Empty;

    /// <summary>
    /// Player email (optional)
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Player handicap
    /// </summary>
    public decimal? Handicap { get; set; }

    /// <summary>
    /// Player number within the team (1, 2, 3, 4)
    /// </summary>
    public int PlayerNumber { get; set; }

    /// <summary>
    /// Is this player the team captain?
    /// </summary>
    public bool IsCaptain { get; set; }

    // Navigation Properties

    /// <summary>
    /// Associated team
    /// </summary>
    public OneTimeEventTeam Team { get; set; } = null!;

    /// <summary>
    /// Associated event
    /// </summary>
    public OneTimeEvent Event { get; set; } = null!;

    /// <summary>
    /// Associated user (if registered)
    /// </summary>
    public User? User { get; set; }
}

