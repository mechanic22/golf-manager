using GolfManager.Shared.DTOs.Common;
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
}

