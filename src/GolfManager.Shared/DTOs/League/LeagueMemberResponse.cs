namespace GolfManager.Shared.DTOs.League;

/// <summary>
/// Response containing league member information
/// </summary>
public class LeagueMemberResponse
{
    /// <summary>
    /// User ID
    /// </summary>
    public string UserId { get; set; } = string.Empty;

    /// <summary>
    /// User's email
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// User's first name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// User's last name
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// User's full name
    /// </summary>
    public string FullName => $"{FirstName} {LastName}".Trim();

    /// <summary>
    /// Whether the user is a league admin
    /// </summary>
    public bool IsLeagueAdmin { get; set; }

    /// <summary>
    /// When the user joined the league
    /// </summary>
    public DateTime JoinedAt { get; set; }

    /// <summary>
    /// Associated player ID (if the user has a player profile in this league)
    /// </summary>
    public string? PlayerId { get; set; }

    /// <summary>
    /// Player display name (if applicable)
    /// </summary>
    public string? PlayerDisplayName { get; set; }
}

