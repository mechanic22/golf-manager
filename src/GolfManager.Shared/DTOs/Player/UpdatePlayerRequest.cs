using System.ComponentModel.DataAnnotations;

namespace GolfManager.Shared.DTOs.Player;

/// <summary>
/// Request to update a player's league profile
/// </summary>
public class UpdatePlayerRequest
{
    /// <summary>
    /// Display name in this league
    /// </summary>
    [StringLength(100, MinimumLength = 1)]
    public string? DisplayName { get; set; }

    /// <summary>
    /// Nickname in this league
    /// </summary>
    [StringLength(50)]
    public string? Nickname { get; set; }

    /// <summary>
    /// League-specific handicap
    /// </summary>
    [Range(0, 54)]
    public double? LeagueHandicap { get; set; }
}

