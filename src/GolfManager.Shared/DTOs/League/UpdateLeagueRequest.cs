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
}

