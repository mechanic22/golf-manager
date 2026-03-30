using System.Net.Http.Json;
using GolfManager.Shared.DTOs.Common;
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
}

