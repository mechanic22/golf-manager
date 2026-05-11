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
    /// Optional custom welcome headline for this league.
    /// </summary>
    public string? WelcomeHeadline { get; set; }

    /// <summary>
    /// Optional custom welcome supporting text for this league.
    /// </summary>
    public string? WelcomeSubhead { get; set; }

    /// <summary>
    /// Optional tone copy used for empty states.
    /// </summary>
    public string? EmptyStateMessage { get; set; }

    /// <summary>
    /// Optional commissioner/admin display name.
    /// </summary>
    public string? CommissionerName { get; set; }

    /// <summary>
    /// Optional announcement headline shown on tenant home surfaces.
    /// </summary>
    public string? AnnouncementTitle { get; set; }

    /// <summary>
    /// Optional announcement body shown on tenant home surfaces.
    /// </summary>
    public string? AnnouncementBody { get; set; }

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
    /// Custom domain for this league
    /// </summary>
    public string? CustomDomain { get; set; }

    /// <summary>
    /// Whether the custom domain is enabled for this league
    /// </summary>
    public bool UseCustomDomain { get; set; }

    /// <summary>
    /// DNS verification token for custom domains
    /// </summary>
    public string? CustomDomainVerificationToken { get; set; }

    /// <summary>
    /// When the custom domain was verified
    /// </summary>
    public DateTime? CustomDomainVerifiedAt { get; set; }

    /// <summary>
    /// Whether anonymous/public pages require a password.
    /// </summary>
    public bool RequireAnonymousPassword { get; set; }

    /// <summary>
    /// Whether an anonymous/public password has been configured.
    /// </summary>
    public bool HasAnonymousPassword { get; set; }

    /// <summary>
    /// When anonymous password settings were last updated.
    /// </summary>
    public DateTime? AnonymousPasswordUpdatedAt { get; set; }

    /// <summary>
    /// When the league was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the league was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

