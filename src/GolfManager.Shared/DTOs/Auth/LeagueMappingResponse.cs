using GolfManager.Core.Enums;

namespace GolfManager.Shared.DTOs.Auth;

/// <summary>
/// League mapping for a user (returned after login)
/// </summary>
public class LeagueMappingResponse
{
    /// <summary>
    /// League ID
    /// </summary>
    public string LeagueId { get; set; } = string.Empty;
    
    /// <summary>
    /// League key (used in URLs)
    /// </summary>
    public string LeagueKey { get; set; } = string.Empty;
    
    /// <summary>
    /// League name
    /// </summary>
    public string LeagueName { get; set; } = string.Empty;
    
    /// <summary>
    /// Custom domain for this league (e.g., "digikeygolf.com" or "localhost:7001")
    /// </summary>
    public string? CustomDomain { get; set; }
    
    /// <summary>
    /// Whether user is a league admin
    /// </summary>
    public bool IsLeagueAdmin { get; set; }

    /// <summary>
    /// User's role in this league.
    /// </summary>
    public LeagueMemberRole Role { get; set; } = LeagueMemberRole.Member;

    /// <summary>
    /// League logo URL (local path or absolute URL)
    /// </summary>
    public string? LogoUrl { get; set; }
}
