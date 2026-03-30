namespace GolfManager.Shared.DTOs.OneTimeEvent;

/// <summary>
/// Response containing one-time event team information
/// </summary>
public class OneTimeEventTeamResponse
{
    /// <summary>
    /// Team ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Event ID
    /// </summary>
    public string EventId { get; set; } = string.Empty;

    /// <summary>
    /// Team name
    /// </summary>
    public string TeamName { get; set; } = string.Empty;

    /// <summary>
    /// Team number (assigned sequentially)
    /// </summary>
    public int TeamNumber { get; set; }

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

    /// <summary>
    /// When the team registered
    /// </summary>
    public DateTime RegisteredAt { get; set; }

    /// <summary>
    /// Is the team checked in?
    /// </summary>
    public bool IsCheckedIn { get; set; }

    /// <summary>
    /// When the team checked in
    /// </summary>
    public DateTime? CheckedInAt { get; set; }

    /// <summary>
    /// Total score (gross)
    /// </summary>
    public int? TotalScore { get; set; }

    /// <summary>
    /// Net score (after handicap)
    /// </summary>
    public int? NetScore { get; set; }

    /// <summary>
    /// Team position/rank
    /// </summary>
    public int? Position { get; set; }

    /// <summary>
    /// Team players
    /// </summary>
    public List<OneTimeEventPlayerResponse> Players { get; set; } = new();
}

