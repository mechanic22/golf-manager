using System.ComponentModel.DataAnnotations;

namespace GolfManager.Shared.DTOs.Player;

/// <summary>
/// Request to add a player to a league
/// </summary>
public class CreatePlayerRequest
{
    /// <summary>
    /// User ID of the player to add
    /// If null, a new user and golfer will be created
    /// </summary>
    public string? UserId { get; set; }

    /// <summary>
    /// Email address (required if creating new user)
    /// </summary>
    [EmailAddress]
    public string? Email { get; set; }

    /// <summary>
    /// First name (required if creating new user)
    /// </summary>
    public string? FirstName { get; set; }

    /// <summary>
    /// Last name (required if creating new user)
    /// </summary>
    public string? LastName { get; set; }

    /// <summary>
    /// Display name in this league
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string DisplayName { get; set; } = string.Empty;

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

