using System.Net.Http.Json;
using GolfManager.Shared.DTOs.Admin;
using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.Round;
using GolfManager.Shared.DTOs.User;

namespace GolfManager.Web.Services;

/// <summary>
/// Service for handling user operations
/// </summary>
public class UserService : IUserService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<UserService> _logger;

    public UserService(HttpClient httpClient, ILogger<UserService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ApiResponse<UserSearchResponse>?> SearchByEmailAsync(string email)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/users/search?email={Uri.EscapeDataString(email)}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<UserSearchResponse>>();
            }

            _logger.LogWarning("Failed to search for user by email: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error searching for user by email");
            return null;
        }
    }

    public async Task<ApiResponse<List<UserResponse>>?> GetAllUsersAsync(bool includeInactive = false)
    {
        try
        {
            _logger.LogInformation("Fetching all users from API");
            var url = includeInactive ? "api/v1/users?includeInactive=true" : "api/v1/users";
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<UserResponse>>>(url);
            _logger.LogInformation("Fetched {Count} users", response?.Data?.Count ?? 0);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching users");
            return ApiResponse<List<UserResponse>>.ErrorResponse("Failed to load users", ex.Message);
        }
    }

    public async Task<ApiResponse<UserResponse>?> GetUserAsync(string userId)
    {
        try
        {
            _logger.LogInformation("Fetching user {UserId} from API", userId);
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<UserResponse>>($"api/v1/users/{userId}");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user {UserId}", userId);
            return ApiResponse<UserResponse>.ErrorResponse("Failed to load user", ex.Message);
        }
    }

    public async Task<ApiResponse<UserResponse>?> UpdateUserAsync(string userId, UpdateUserRequest request)
    {
        try
        {
            _logger.LogInformation("Updating user {UserId}", userId);
            var response = await _httpClient.PutAsJsonAsync($"api/v1/users/{userId}", request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<UserResponse>>();
                _logger.LogInformation("User {UserId} updated successfully", userId);
                return result;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to update user {UserId}: {StatusCode} - {Error}", userId, response.StatusCode, errorContent);
            return ApiResponse<UserResponse>.ErrorResponse("Failed to update user", errorContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating user {UserId}", userId);
            return ApiResponse<UserResponse>.ErrorResponse("Failed to update user", ex.Message);
        }
    }

    public async Task<ApiResponse<bool>?> SendPasswordResetAsync(string userId)
    {
        try
        {
            _logger.LogInformation("Sending password reset for user {UserId}", userId);
            var response = await _httpClient.PostAsync($"api/v1/users/{userId}/password-reset", null);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
                _logger.LogInformation("Password reset sent for user {UserId}", userId);
                return result;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to send password reset for user {UserId}: {StatusCode} - {Error}", userId, response.StatusCode, errorContent);
            return ApiResponse<bool>.ErrorResponse("Failed to send password reset", errorContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error sending password reset for user {UserId}", userId);
            return ApiResponse<bool>.ErrorResponse("Failed to send password reset", ex.Message);
        }
    }

    public async Task<ApiResponse<PlatformStatsResponse>?> GetPlatformStatsAsync()
    {
        try
        {
            _logger.LogInformation("Fetching platform stats from API");
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<PlatformStatsResponse>>("api/v1/users/stats/platform");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching platform stats");
            return ApiResponse<PlatformStatsResponse>.ErrorResponse("Failed to load platform stats", ex.Message);
        }
    }

    public async Task<ApiResponse<UserProfileResponse>?> GetCurrentUserAsync()
    {
        try
        {
            _logger.LogInformation("Fetching current user profile from API");
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<UserProfileResponse>>("api/v1/users/me");
            _logger.LogInformation("Fetched user profile: {Email}", response?.Data?.Email);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user profile");
            return ApiResponse<UserProfileResponse>.ErrorResponse("Failed to load profile", ex.Message);
        }
    }

    public async Task<ApiResponse<List<RoundResponse>>?> GetMyRoundsAsync()
    {
        try
        {
            _logger.LogInformation("Fetching current user's rounds from API");
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<RoundResponse>>>("api/v1/users/me/rounds");
            _logger.LogInformation("Fetched {Count} rounds", response?.Data?.Count ?? 0);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching user rounds");
            return ApiResponse<List<RoundResponse>>.ErrorResponse("Failed to load rounds", ex.Message);
        }
    }
}

