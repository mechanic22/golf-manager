using System.ComponentModel.DataAnnotations;

namespace GolfManager.Shared.DTOs.User;

/// <summary>
/// Request to update a user (Global Admin only)
/// </summary>
public class UpdateUserRequest
{
    /// <summary>
    /// User's first name
    /// </summary>
    [StringLength(100, MinimumLength = 1)]
    public string? FirstName { get; set; }

    /// <summary>
    /// User's last name
    /// </summary>
    [StringLength(100, MinimumLength = 1)]
    public string? LastName { get; set; }

    /// <summary>
    /// User's email address
    /// </summary>
    [EmailAddress]
    [StringLength(255)]
    public string? Email { get; set; }

    /// <summary>
    /// Whether the user should be a global admin
    /// </summary>
    public bool? IsGlobalAdmin { get; set; }

    /// <summary>
    /// Whether the user account is active
    /// </summary>
    public bool? IsActive { get; set; }
}
