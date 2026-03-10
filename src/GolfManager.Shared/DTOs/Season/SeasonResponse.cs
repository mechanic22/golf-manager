namespace GolfManager.Shared.DTOs.Season;

/// <summary>
/// Season response DTO
/// </summary>
public class SeasonResponse
{
    /// <summary>
    /// Season ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// League ID
    /// </summary>
    public string LeagueId { get; set; } = string.Empty;

    /// <summary>
    /// URL-friendly key
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
    /// Is the season locked?
    /// </summary>
    public bool IsLocked { get; set; }

    /// <summary>
    /// Number of events in the season
    /// </summary>
    public int EventCount { get; set; }

    /// <summary>
    /// Number of golfers in the season
    /// </summary>
    public int GolferCount { get; set; }

    /// <summary>
    /// When the season was created
    /// </summary>
    public DateTime CreatedAt { get; set; }

    /// <summary>
    /// When the season was last updated
    /// </summary>
    public DateTime? UpdatedAt { get; set; }
}

