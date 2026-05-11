using System.Net.Http.Json;
using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.League;

namespace GolfManager.Web.Services;

/// <summary>
/// Service for handling league operations
/// </summary>
public class LeagueService : ILeagueService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<LeagueService> _logger;

    public LeagueService(HttpClient httpClient, ILogger<LeagueService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ApiResponse<LeagueResponse>?> CreateLeagueAsync(CreateLeagueRequest request)
    {
        try
        {
            _logger.LogInformation("Creating league: Name={Name}, Key={Key}", request.Name, request.Key);

            _logger.LogInformation("Sending POST to api/v1/leagues");
            var response = await _httpClient.PostAsJsonAsync("api/v1/leagues", request);

            _logger.LogInformation("Response status: {StatusCode}", response.StatusCode);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<LeagueResponse>>();
                _logger.LogInformation("League created successfully: {LeagueId}", result?.Data?.Id);
                return result;
            }

            // Try to read error response
            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("API returned error {StatusCode}: {ErrorContent}", response.StatusCode, errorContent);

            // For 401 Unauthorized, the response body is usually empty
            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                _logger.LogWarning("Unauthorized - token may be expired or invalid");
                return ApiResponse<LeagueResponse>.ErrorResponse(
                    "Unauthorized",
                    "Your session has expired. Please log in again.");
            }

            // Try to parse as ApiResponse if there's content
            if (!string.IsNullOrEmpty(errorContent))
            {
                try
                {
                    var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse<LeagueResponse>>();
                    if (errorResponse != null)
                    {
                        return errorResponse;
                    }
                }
                catch (Exception parseEx)
                {
                    _logger.LogWarning(parseEx, "Failed to parse error response as JSON");
                }
            }

            return ApiResponse<LeagueResponse>.ErrorResponse(
                $"Request failed with status {response.StatusCode}",
                errorContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception creating league: {Message}", ex.Message);
            return ApiResponse<LeagueResponse>.ErrorResponse("Request failed", ex.Message);
        }
    }

    public async Task<ApiResponse<List<LeagueResponse>>?> GetUserLeaguesAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/v1/leagues");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<List<LeagueResponse>>>();
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<ApiResponse<LeagueResponse>?> GetLeagueByKeyAsync(string key, string? anonymousAccessPassword = null)
    {
        try
        {
            var requestUri = $"api/v1/leagues/by-key/{key}";
            if (!string.IsNullOrWhiteSpace(anonymousAccessPassword))
            {
                requestUri = $"{requestUri}?anonymousAccessPassword={Uri.EscapeDataString(anonymousAccessPassword)}";
            }

            var response = await _httpClient.GetAsync(requestUri);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<LeagueResponse>>();
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                return ApiResponse<LeagueResponse>.ErrorResponse("Forbidden", errorContent);
            }

            return null;
        }
        catch
        {
            return null;
        }
    }

    public async Task<ApiResponse<bool>?> VerifyAnonymousAccessAsync(string key, string password)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"api/v1/leagues/by-key/{key}/anonymous-access",
                new VerifyAnonymousAccessRequest { Password = password });

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            try
            {
                var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
                if (errorResponse != null)
                {
                    return errorResponse;
                }
            }
            catch
            {
            }

            return ApiResponse<bool>.ErrorResponse($"Request failed with status {response.StatusCode}", errorContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying anonymous access");
            return ApiResponse<bool>.ErrorResponse("Request failed", ex.Message);
        }
    }

    public async Task<ApiResponse<LeagueResponse>?> UpdateLeagueAsync(string leagueId, UpdateLeagueRequest request)
    {
        try
        {
            _logger.LogInformation("Updating league: {LeagueId}", leagueId);

            var response = await _httpClient.PutAsJsonAsync($"api/v1/leagues/{leagueId}", request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<LeagueResponse>>();
                _logger.LogInformation("League updated successfully: {LeagueId}", leagueId);
                return result;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("API returned error {StatusCode}: {ErrorContent}", response.StatusCode, errorContent);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return ApiResponse<LeagueResponse>.ErrorResponse(
                    "Unauthorized",
                    "Your session has expired. Please log in again.");
            }

            if (!string.IsNullOrEmpty(errorContent))
            {
                try
                {
                    var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse<LeagueResponse>>();
                    if (errorResponse != null)
                    {
                        return errorResponse;
                    }
                }
                catch (Exception parseEx)
                {
                    _logger.LogWarning(parseEx, "Failed to parse error response as JSON");
                }
            }

            return ApiResponse<LeagueResponse>.ErrorResponse(
                $"Request failed with status {response.StatusCode}",
                errorContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Exception updating league: {Message}", ex.Message);
            return ApiResponse<LeagueResponse>.ErrorResponse("Request failed", ex.Message);
        }
    }

    public async Task<ApiResponse<List<LeagueMemberResponse>>?> GetLeagueMembersAsync(string leagueId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/leagues/{leagueId}/members");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<List<LeagueMemberResponse>>>();
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting league members");
            return null;
        }
    }

    public async Task<ApiResponse<LeagueMemberResponse>?> AddLeagueMemberAsync(string leagueId, AddLeagueMemberRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/v1/leagues/{leagueId}/members", request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<LeagueMemberResponse>>();
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            try
            {
                var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse<LeagueMemberResponse>>();
                if (errorResponse != null)
                {
                    return errorResponse;
                }
            }
            catch { }

            return ApiResponse<LeagueMemberResponse>.ErrorResponse($"Request failed with status {response.StatusCode}", errorContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding league member");
            return ApiResponse<LeagueMemberResponse>.ErrorResponse("Request failed", ex.Message);
        }
    }

    public async Task<ApiResponse<LeagueMemberResponse>?> UpdateLeagueMemberAsync(string leagueId, string userId, UpdateLeagueMemberRequest request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/v1/leagues/{leagueId}/members/{userId}", request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<LeagueMemberResponse>>();
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            try
            {
                var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse<LeagueMemberResponse>>();
                if (errorResponse != null)
                {
                    return errorResponse;
                }
            }
            catch { }

            return ApiResponse<LeagueMemberResponse>.ErrorResponse($"Request failed with status {response.StatusCode}", errorContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating league member");
            return ApiResponse<LeagueMemberResponse>.ErrorResponse("Request failed", ex.Message);
        }
    }

    public async Task<ApiResponse<bool>?> RemoveLeagueMemberAsync(string leagueId, string userId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/v1/leagues/{leagueId}/members/{userId}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            try
            {
                var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
                if (errorResponse != null)
                {
                    return errorResponse;
                }
            }
            catch { }

            return ApiResponse<bool>.ErrorResponse($"Request failed with status {response.StatusCode}", errorContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing league member");
            return ApiResponse<bool>.ErrorResponse("Request failed", ex.Message);
        }
    }

    public async Task<ApiResponse<LeagueResponse>?> VerifyLeagueCustomDomainAsync(string leagueId)
    {
        try
        {
            var response = await _httpClient.PostAsync($"api/v1/leagues/{leagueId}/custom-domain/verify", null);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<LeagueResponse>>();
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("API returned error {StatusCode}: {ErrorContent}", response.StatusCode, errorContent);

            if (response.StatusCode == System.Net.HttpStatusCode.Unauthorized)
            {
                return ApiResponse<LeagueResponse>.ErrorResponse(
                    "Unauthorized",
                    "Your session has expired. Please log in again.");
            }

            if (!string.IsNullOrEmpty(errorContent))
            {
                try
                {
                    var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse<LeagueResponse>>();
                    if (errorResponse != null)
                    {
                        return errorResponse;
                    }
                }
                catch (Exception parseEx)
                {
                    _logger.LogWarning(parseEx, "Failed to parse error response as JSON");
                }
            }

            return ApiResponse<LeagueResponse>.ErrorResponse(
                $"Request failed with status {response.StatusCode}",
                errorContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error verifying league custom domain");
            return ApiResponse<LeagueResponse>.ErrorResponse("Request failed", ex.Message);
        }
    }
}

