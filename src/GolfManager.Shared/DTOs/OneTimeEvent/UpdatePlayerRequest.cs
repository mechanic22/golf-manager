using System.ComponentModel.DataAnnotations;

namespace GolfManager.Shared.DTOs.OneTimeEvent;

/// <summary>
/// Request to update a player on a team
/// </summary>
public class UpdatePlayerRequest
{
    /// <summary>
    /// Player name
    /// </summary>
    [StringLength(100, MinimumLength = 2)]
    public string? PlayerName { get; set; }

    /// <summary>
    /// Player email
    /// </summary>
    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; }

    /// <summary>
    /// Player handicap
    /// </summary>
    [Range(0, 54, ErrorMessage = "Handicap must be between 0 and 54")]
    public decimal? Handicap { get; set; }
}

