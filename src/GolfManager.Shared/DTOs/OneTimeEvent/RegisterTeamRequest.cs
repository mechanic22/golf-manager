using System.ComponentModel.DataAnnotations;

namespace GolfManager.Shared.DTOs.OneTimeEvent;

/// <summary>
/// Request to register a team for a one-time event
/// </summary>
public class RegisterTeamRequest
{
    /// <summary>
    /// Team name
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string TeamName { get; set; } = string.Empty;

    /// <summary>
    /// Captain name
    /// </summary>
    [Required]
    [StringLength(100, MinimumLength = 2)]
    public string CaptainName { get; set; } = string.Empty;

    /// <summary>
    /// Captain email
    /// </summary>
    [Required]
    [EmailAddress]
    [StringLength(100)]
    public string CaptainEmail { get; set; } = string.Empty;

    /// <summary>
    /// Captain phone
    /// </summary>
    [Phone]
    [StringLength(20)]
    public string? CaptainPhone { get; set; }

    /// <summary>
    /// Registration code (required for private events)
    /// </summary>
    [StringLength(50)]
    public string? RegistrationCode { get; set; }

    /// <summary>
    /// Team players (including captain)
    /// </summary>
    [Required]
    [MinLength(1, ErrorMessage = "At least one player is required")]
    public List<PlayerInfo> Players { get; set; } = new();

    /// <summary>
    /// Player information for registration
    /// </summary>
    public class PlayerInfo
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
}

