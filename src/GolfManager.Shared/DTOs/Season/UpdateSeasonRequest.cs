using System.ComponentModel.DataAnnotations;

namespace GolfManager.Shared.DTOs.Season;

/// <summary>
/// Request to update an existing season
/// </summary>
public class UpdateSeasonRequest
{
    /// <summary>
    /// Season display name
    /// </summary>
    [StringLength(100, MinimumLength = 2)]
    public string? Name { get; set; }

    /// <summary>
    /// Season start date
    /// </summary>
    public DateOnly? StartDate { get; set; }

    /// <summary>
    /// Season end date
    /// </summary>
    public DateOnly? EndDate { get; set; }

    /// <summary>
    /// Is the season locked?
    /// </summary>
    public bool? IsLocked { get; set; }
}

