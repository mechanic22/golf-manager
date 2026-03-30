using System.ComponentModel.DataAnnotations;

namespace GolfManager.Shared.DTOs.OneTimeEvent;

/// <summary>
/// Request to add a player to a team
/// </summary>
public class AddPlayerRequest
{
    /// <summary>
    /// Player name
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string PlayerName { get; set; } = string.Empty;

    /// <summary>
    /// Player email (optional)
    /// </summary>
    [EmailAddress]
    [StringLength(100)]
    public string? Email { get; set; }

    /// <summary>
    /// Player handicap (optional)
    /// </summary>
    [Range(0, 54, ErrorMessage = "Handicap must be between 0 and 54")]
    public decimal? Handicap { get; set; }

    /// <summary>
    /// Is this player the captain?
    /// </summary>
    public bool IsCaptain { get; set; }
}

