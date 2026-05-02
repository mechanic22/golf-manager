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
    /// League logo URL
    /// </summary>
    [Url]
    [StringLength(500)]
    public string? LogoUrl { get; set; }

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

