namespace GolfManager.Shared.DTOs.Auth;

public class AuthResponse
{
    public string UserId { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public bool IsGlobalAdmin { get; set; }
    public string AccessToken { get; set; } = string.Empty;
    public string RefreshToken { get; set; } = string.Empty;
    public DateTime ExpiresAt { get; set; }

    /// <summary>
    /// User's league memberships with domain mappings
    /// </summary>
    public List<LeagueMappingResponse> LeagueMappings { get; set; } = new();
}

