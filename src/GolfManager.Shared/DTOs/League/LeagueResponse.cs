namespace GolfManager.Shared.DTOs.League;

/// <summary>
/// Response containing league information
/// </summary>
public class LeagueResponse
{
    /// <summary>
    /// League ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// URL-friendly slug/key (unique) - e.g., "digikey-golf"
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// League display name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// League description
    /// </summary>
    public string? Description { get; set; }

    /// <summary>
    /// League logo URL
    /// </summary>
    public string? LogoUrl { get; set; }

    /// <summary>
    /// Currently active season ID
    /// </summary>
    public string? ActiveSeasonId { get; set; }

    /// <summary>
    /// Total number of members in the league
    /// </summary>
    public int MemberCount { get; set; }

    /// <summary>
    /// Total number of players in the league
    /// </summary>
    public int PlayerCount { get; set; }

    /// <summary>
    /// Total number of seasons in the league
    /// </summary>
    public int SeasonCount { get; set; }

    /// <summary>
    /// Whether the current user is an admin of this league
    /// </summary>
    public bool IsCurrentUserAdmin { get; set; }

    /// <summary>
    /// When the league was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the league was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

