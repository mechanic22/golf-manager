namespace GolfManager.Shared.DTOs.User;

/// <summary>
/// Response for user search by email
/// </summary>
public class UserSearchResponse
{
    /// <summary>
    /// User ID
    /// </summary>
    public string Id { get; set; } = string.Empty;

    /// <summary>
    /// Email address
    /// </summary>
    public string Email { get; set; } = string.Empty;

    /// <summary>
    /// First name
    /// </summary>
    public string FirstName { get; set; } = string.Empty;

    /// <summary>
    /// Last name
    /// </summary>
    public string LastName { get; set; } = string.Empty;

    /// <summary>
    /// Whether the user exists in the system
    /// </summary>
    public bool Exists { get; set; }
}

