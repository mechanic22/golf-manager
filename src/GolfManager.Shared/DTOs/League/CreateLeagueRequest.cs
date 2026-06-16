using System.ComponentModel.DataAnnotations;

namespace GolfManager.Shared.DTOs.League;

/// <summary>
/// Request to create a new league
/// </summary>
public class CreateLeagueRequest
{
    /// <summary>
    /// URL-friendly slug/key (unique) - e.g., "digikey-golf"
    /// </summary>
    [Required]
    [StringLength(50, MinimumLength = 3)]
    [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Key must contain only lowercase letters, numbers, and hyphens")]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// League display name
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// League description
    /// </summary>
    [StringLength(500)]
    public string? Description { get; set; }

    /// <summary>
    /// League logo URL (absolute URL or local path, e.g. /img/logo.png)
    /// </summary>
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
    public bool UseCustomDomain { get; set; }
}

