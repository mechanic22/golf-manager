namespace GolfManager.Shared.DTOs.OneTimeEvent;

/// <summary>
/// Response containing one-time event player information
/// </summary>
public class OneTimeEventPlayerResponse
{
    /// <summary>
    /// Player ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Team ID
    /// </summary>
    public string TeamId { get; set; } = string.Empty;

    /// <summary>
    /// Event ID
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
    /// Player email
    /// </summary>
    public string? Email { get; set; }

    /// <summary>
    /// Player handicap
    /// </summary>
    public decimal? Handicap { get; set; }

    /// <summary>
    /// Player number (position in team)
    /// </summary>
    public int PlayerNumber { get; set; }

    /// <summary>
    /// Is this player the captain?
    /// </summary>
    public bool IsCaptain { get; set; }
}

