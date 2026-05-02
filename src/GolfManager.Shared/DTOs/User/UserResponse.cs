namespace GolfManager.Shared.DTOs.User;

/// <summary>
/// User response for admin user list
/// </summary>
public class UserResponse
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string FullName => $"{FirstName} {LastName}".Trim();
    public bool IsGlobalAdmin { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }

    // League admin status (computed from UserLeagues)
    public int LeagueAdminCount { get; set; }
}
