namespace GolfManager.Shared.DTOs.User;

/// <summary>
/// Full user profile response
/// </summary>
public class UserProfileResponse
{
    public string Id { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string DisplayName { get; set; } = string.Empty;
    public bool IsGlobalAdmin { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? LastLoginAt { get; set; }
    
    // Golfer info
    public bool IsGolfer { get; set; }
    public string? GolferId { get; set; }
    public double? HandicapIndex { get; set; }

    // Stats
    public int LeagueCount { get; set; }
    public int RoundsCount { get; set; }
}
