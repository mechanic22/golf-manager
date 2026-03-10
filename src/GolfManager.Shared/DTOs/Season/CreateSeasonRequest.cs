using System.ComponentModel.DataAnnotations;

namespace GolfManager.Shared.DTOs.Season;

/// <summary>
/// Request to create a new season
/// </summary>
public class CreateSeasonRequest
{
    /// <summary>
    /// URL-friendly key (e.g., "2024", "spring-2024")
    /// </summary>
    [Required]
    [RegularExpression(@"^[a-z0-9-]+$", ErrorMessage = "Key must contain only lowercase letters, numbers, and hyphens")]
    [StringLength(50, MinimumLength = 2)]
    public string Key { get; set; } = string.Empty;

    /// <summary>
    /// Season display name
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string Name { get; set; } = string.Empty;

    /// <summary>
    /// Season start date
    /// </summary>
    [Required]
    public DateOnly StartDate { get; set; }

    /// <summary>
    /// Season end date (optional)
    /// </summary>
    public DateOnly? EndDate { get; set; }
}

