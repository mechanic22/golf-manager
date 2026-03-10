using GolfManager.Core.Common;

namespace GolfManager.Core.Entities;

/// <summary>
/// Season entity - A season within a league
/// </summary>
public class Season : BaseEntity, ITenantEntity
{
    /// <summary>
    /// League ID (tenant isolation)
    /// </summary>
    public string LeagueId { get; set; } = string.Empty;

    /// <summary>
    /// URL-friendly key (e.g., "2024", "spring-2024")
    /// </summary>
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Season display name
    /// </summary>
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Season start date
    /// </summary>
    public DateOnly StartDate { get; set; }

    /// <summary>
    /// Season end date (optional)
    /// </summary>
    public DateOnly? EndDate { get; set; }

    /// <summary>
    /// Is the season locked (prevent changes)?
    /// </summary>
    public bool IsLocked { get; set; }

    // Navigation Properties

    /// <summary>
    /// Associated league
    /// </summary>
    public League League { get; set; } = null!;

    /// <summary>
    /// Events in this season
    /// </summary>
    public ICollection<SeasonEvent> Events { get; set; } = new List<SeasonEvent>();

    /// <summary>
    /// Teams in this season
    /// </summary>
    public ICollection<SeasonTeam> Teams { get; set; } = new List<SeasonTeam>();

    /// <summary>
    /// Golfer participations in this season
    /// </summary>
    public ICollection<SeasonGolfer> SeasonGolfers { get; set; } = new List<SeasonGolfer>();
}

