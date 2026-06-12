using System.ComponentModel.DataAnnotations;

namespace GolfManager.Shared.DTOs.League;

/// <summary>
/// Request to update a league
/// </summary>
public class UpdateLeagueRequest
{
    /// <summary>
    /// League display name
    /// </summary>
    [StringLength(100, MinimumLength = 1)]
    public string? Name { get; set; }

    /// <summary>
    /// League description
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// League logo URL
    /// </summary>
    [Url]
    [StringLength(500)]
    public string? LogoUrl { get; set; }

    /// <summary>
    /// Optional custom welcome headline for this league.
    /// </summary>
    [StringLength(200)]
    public string? WelcomeHeadline { get; set; }

    /// <summary>
    /// Optional custom welcome supporting text for this league.
    /// </summary>
    [StringLength(500)]
    public string? WelcomeSubhead { get; set; }

    /// <summary>
    /// Optional tone copy used for empty states.
    /// </summary>
    [StringLength(500)]
    public string? EmptyStateMessage { get; set; }

    /// <summary>
    /// Optional commissioner/admin display name.
    /// </summary>
    [StringLength(100)]
    public string? CommissionerName { get; set; }

    /// <summary>
    /// Optional announcement headline shown on dashboard.
    /// </summary>
    [StringLength(150)]
    public string? AnnouncementTitle { get; set; }

    /// <summary>
    /// Optional announcement body shown on dashboard.
    /// </summary>
    [StringLength(1000)]
    public string? AnnouncementBody { get; set; }

    /// <summary>
    /// Custom domain for this league (e.g. "digikeygolf.com")
    /// </summary>
    [StringLength(200)]
    public string? CustomDomain { get; set; }

    /// <summary>
    /// Whether to use the custom domain for this league
    /// </summary>
    public bool? UseCustomDomain { get; set; }

    /// <summary>
    /// Whether this league appears in public discovery search. Default: false (opt-in).
    /// </summary>
    public bool? IsPubliclyDiscoverable { get; set; }

    /// <summary>
    /// Whether anonymous/public pages should require a password.
    /// </summary>
    public bool? RequireAnonymousPassword { get; set; }

    /// <summary>
    /// Optional plain-text password to set for anonymous/public access.
    /// </summary>
    [StringLength(128, MinimumLength = 4)]
    public string? AnonymousAccessPassword { get; set; }

    /// <summary>
    /// Clears any existing anonymous/public access password hash.
    /// </summary>
    public bool? ClearAnonymousAccessPassword { get; set; }
}

