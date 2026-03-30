using GolfManager.Shared.DTOs.Auth;

namespace GolfManager.Web.Services;

/// <summary>
/// Service for handling authentication operations
/// </summary>
public interface IAuthService
{
    /// <summary>
    /// Register a new user
    /// </summary>
    Task<AuthResponse?> RegisterAsync(RegisterRequest request);

    /// <summary>
    /// Login a user
    /// </summary>
    Task<AuthResponse?> LoginAsync(LoginRequest request);

    /// <summary>
    /// Logout the current user
    /// </summary>
    Task LogoutAsync();

    /// <summary>
    /// Initialize the auth service (restore session from storage)
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Check if user is authenticated
    /// </summary>
    bool IsAuthenticated { get; }

    /// <summary>
    /// Get current user's email
    /// </summary>
    string? UserEmail { get; }

    /// <summary>
    /// Get current user's ID
    /// </summary>
    string? UserId { get; }

    /// <summary>
    /// Get current access token
    /// </summary>
    string? AccessToken { get; }

    /// <summary>
    /// Check if current user is a global admin
    /// </summary>
    bool IsGlobalAdmin { get; }
}

