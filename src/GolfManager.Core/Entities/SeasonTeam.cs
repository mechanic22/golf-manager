using GolfManager.Core.Common;

namespace GolfManager.Core.Entities;

/// <summary>
/// SeasonTeam - A team within a season
/// </summary>
public class SeasonTeam : BaseEntity, ITenantEntity
{
    /// <summary>
    /// Season ID
    /// </summary>
    public string SeasonId { get; set; } = string.Empty;

    /// <summary>
    /// League ID (tenant isolation)
    /// </summary>
    public string LeagueId { get; set; } = string.Empty;

    /// <summary>
    /// Team name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Team avatar/logo URL
    /// </summary>
    public string? AvatarUrl { get; set; }

    /// <summary>
    /// Total season points
    /// </summary>
    public double? SeasonPoints { get; set; }

    /// <summary>
    /// Team wins
    /// </summary>
    public int Wins { get; set; }

    /// <summary>
    /// Team losses
    /// </summary>
    public int Losses { get; set; }

    /// <summary>
    /// Team ties
    /// </summary>
    public int Ties { get; set; }

    // Navigation Properties

    /// <summary>
    /// Associated season
    /// </summary>
    public Season Season { get; set; } = null!;

    /// <summary>
    /// Team members
    /// </summary>
    public ICollection<SeasonGolfer> Members { get; set; } = new List<SeasonGolfer>();
}

