using System.ComponentModel.DataAnnotations;

namespace GolfManager.Shared.DTOs.League;

/// <summary>
/// Request to add a member to a league
/// Supports both adding existing users and creating new users
/// </summary>
public class AddLeagueMemberRequest
{
    /// <summary>
    /// Email of the user to add to the league
    /// </summary>
    [Required]
    [EmailAddress]
    [StringLength(255)]
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// First name - required if user doesn't exist and needs to be created
    /// </summary>
    [StringLength(100)]
    public string? FirstName { get; set; }

    /// <summary>
    /// Last name - required if user doesn't exist and needs to be created
    /// </summary>
    [StringLength(100)]
    public string? LastName { get; set; }

    /// <summary>
    /// Whether the user should be a league admin
    /// </summary>
    public bool IsLeagueAdmin { get; set; } = false;

    /// <summary>
    /// Optional invitation message
    /// </summary>
    [StringLength(500)]
    public string? InvitationMessage { get; set; }
}

