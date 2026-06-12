using GolfManager.Shared.DTOs.Auth;
using GolfManager.Shared.DTOs.League;

namespace GolfManager.Web.Features.Auth;

/// <summary>
/// Service for handling authentication operations including guest access
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
    /// Login as guest to view a league's standings
    /// </summary>
    Task<bool> LoginAsGuestAsync(string leagueKey, string password);

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
    /// Get current user's first name
    /// </summary>
    string? UserFirstName { get; }

    /// <summary>
    /// Get current user's last name
    /// </summary>
    string? UserLastName { get; }

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

    /// <summary>
    /// Check if logged in as guest
    /// </summary>
    bool IsGuest { get; }

    /// <summary>
    /// Get guest league key (when IsGuest is true)
    /// </summary>
    string? GuestLeagueKey { get; }

    /// <summary>
    /// Get guest league ID (when IsGuest is true)
    /// </summary>
    string? GuestLeagueId { get; }
}

