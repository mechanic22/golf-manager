using System.ComponentModel.DataAnnotations;

namespace GolfManager.Shared.DTOs.User;

/// <summary>
/// Request for a user to update their own profile
/// </summary>
public class UpdateProfileRequest
{
    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string FirstName { get; set; } = string.Empty;

    [Required]
    [StringLength(100, MinimumLength = 1)]
    public string LastName { get; set; } = string.Empty;
}
