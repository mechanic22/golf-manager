using GolfManager.Core.Common;

namespace GolfManager.Core.Entities;

/// <summary>
/// League entity - Tenant root for multi-tenancy
/// Each league is an independent tenant with its own seasons, events, and golfers
/// </summary>
public class League : BaseEntity
{
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

    // Custom Domain Support (Future Feature)

    /// <summary>
    /// Custom domain for this league (e.g., "digikeygolf.com")
    /// </summary>
    public string? CustomDomain { get; set; }

    /// <summary>
    /// Whether to use custom domain for routing
    /// </summary>
    public bool UseCustomDomain { get; set; }

    /// <summary>
    /// DNS verification token for custom domain
    /// </summary>
    public string? CustomDomainVerificationToken { get; set; }

    /// <summary>
    /// When the custom domain was verified
    /// </summary>
    public DateTime? CustomDomainVerifiedAt { get; set; }

    // Navigation Properties

    /// <summary>
    /// Seasons in this league
    /// </summary>
    public ICollection<Season> Seasons { get; set; } = new List<Season>();

    /// <summary>
    /// Golfer memberships in this league
    /// </summary>
    public ICollection<LeagueGolfer> LeagueGolfers { get; set; } = new List<LeagueGolfer>();

    /// <summary>
    /// User memberships in this league
    /// </summary>
    public ICollection<UserLeague> UserLeagues { get; set; } = new List<UserLeague>();
}

