using GolfManager.Shared.DTOs.Admin;
using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.Round;
using GolfManager.Shared.DTOs.User;

namespace GolfManager.Web.Services;

/// <summary>
/// Service for user operations
/// </summary>
public interface IUserService
{
    /// <summary>
    /// Search for a user by email
    /// </summary>
    Task<ApiResponse<UserSearchResponse>?> SearchByEmailAsync(string email);

    /// <summary>
    /// Get all users (GlobalAdmin only)
    /// </summary>
    Task<ApiResponse<List<UserResponse>>?> GetAllUsersAsync(bool includeInactive = false);

    /// <summary>
    /// Get a specific user by ID (GlobalAdmin only)
    /// </summary>
    Task<ApiResponse<UserResponse>?> GetUserAsync(string userId);

    /// <summary>
    /// Update a user (GlobalAdmin only)
    /// </summary>
    Task<ApiResponse<UserResponse>?> UpdateUserAsync(string userId, UpdateUserRequest request);

    /// <summary>
    /// Send password reset email (GlobalAdmin only)
    /// </summary>
    Task<ApiResponse<bool>?> SendPasswordResetAsync(string userId);

    /// <summary>
    /// Get platform statistics (GlobalAdmin only)
    /// </summary>
    Task<ApiResponse<PlatformStatsResponse>?> GetPlatformStatsAsync();

    /// <summary>
    /// Get current user's profile
    /// </summary>
    Task<ApiResponse<UserProfileResponse>?> GetCurrentUserAsync();

    /// <summary>
    /// Get current user's round history
    /// </summary>
    Task<ApiResponse<List<RoundResponse>>?> GetMyRoundsAsync();
}

