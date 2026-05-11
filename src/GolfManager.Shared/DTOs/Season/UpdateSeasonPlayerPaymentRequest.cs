using System.ComponentModel.DataAnnotations;

namespace GolfManager.Shared.DTOs.Season;

/// <summary>
/// Request to mark a season player paid/unpaid.
/// </summary>
public class UpdateSeasonPlayerPaymentRequest
{
    /// <summary>
    /// True when the player has paid season dues.
    /// </summary>
    [Required]
    public bool IsPaidForSeason { get; set; }
}
