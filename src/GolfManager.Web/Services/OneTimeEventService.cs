using System.Net.Http.Json;
using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.OneTimeEvent;

namespace GolfManager.Web.Services;

/// <summary>
/// Service for handling one-time event operations
/// </summary>
public class OneTimeEventService : IOneTimeEventService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<OneTimeEventService> _logger;

    public OneTimeEventService(HttpClient httpClient, ILogger<OneTimeEventService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    // Event operations
    public async Task<ApiResponse<List<OneTimeEventListResponse>>?> GetEventsAsync(bool? publicOnly = null, bool? upcomingOnly = null, string? organizerId = null)
    {
        try
        {
            var queryParams = new List<string>();
            if (publicOnly.HasValue) queryParams.Add($"publicOnly={publicOnly.Value}");
            if (upcomingOnly.HasValue) queryParams.Add($"upcomingOnly={upcomingOnly.Value}");
            if (!string.IsNullOrEmpty(organizerId)) queryParams.Add($"organizerId={organizerId}");

            var query = queryParams.Any() ? "?" + string.Join("&", queryParams) : "";
            var response = await _httpClient.GetAsync($"api/v1/events/one-time{query}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<List<OneTimeEventListResponse>>>();
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting events");
            return null;
        }
    }

    public async Task<ApiResponse<OneTimeEventResponse>?> GetEventByIdAsync(string eventId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/events/one-time/{eventId}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<OneTimeEventResponse>>();
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting event by ID");
            return null;
        }
    }

    public async Task<ApiResponse<OneTimeEventResponse>?> GetEventByKeyAsync(string eventKey)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/events/one-time/by-key/{eventKey}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<OneTimeEventResponse>>();
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting event by key");
            return null;
        }
    }

    public async Task<ApiResponse<OneTimeEventResponse>?> CreateEventAsync(CreateOneTimeEventRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/events/one-time", request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<OneTimeEventResponse>>();
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            try
            {
                var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse<OneTimeEventResponse>>();
                if (errorResponse != null) return errorResponse;
            }
            catch { }

            return ApiResponse<OneTimeEventResponse>.ErrorResponse($"Request failed with status {response.StatusCode}", errorContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating event");
            return ApiResponse<OneTimeEventResponse>.ErrorResponse("Request failed", ex.Message);
        }
    }

    public async Task<ApiResponse<OneTimeEventResponse>?> UpdateEventAsync(string eventId, UpdateOneTimeEventRequest request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/v1/events/one-time/{eventId}", request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<OneTimeEventResponse>>();
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            try
            {
                var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse<OneTimeEventResponse>>();
                if (errorResponse != null) return errorResponse;
            }
            catch { }

            return ApiResponse<OneTimeEventResponse>.ErrorResponse($"Request failed with status {response.StatusCode}", errorContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating event");
            return ApiResponse<OneTimeEventResponse>.ErrorResponse("Request failed", ex.Message);
        }
    }

    public async Task<ApiResponse<OneTimeEventResponse>?> PublishEventAsync(string eventId)
    {
        try
        {
            var response = await _httpClient.PostAsync($"api/v1/events/one-time/{eventId}/publish", null);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<OneTimeEventResponse>>();
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error publishing event");
            return null;
        }
    }

    public async Task<ApiResponse<OneTimeEventResponse>?> CancelEventAsync(string eventId)
    {
        try
        {
            var response = await _httpClient.PostAsync($"api/v1/events/one-time/{eventId}/cancel", null);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<OneTimeEventResponse>>();
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error cancelling event");
            return null;
        }
    }

    public async Task<ApiResponse<bool>?> DeleteEventAsync(string eventId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/v1/events/one-time/{eventId}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting event");
            return null;
        }
    }

    // Team operations
    public async Task<ApiResponse<List<OneTimeEventTeamResponse>>?> GetEventTeamsAsync(string eventId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/events/one-time/{eventId}/teams");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<List<OneTimeEventTeamResponse>>>();
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting event teams");
            return null;
        }
    }

    public async Task<ApiResponse<OneTimeEventTeamResponse>?> GetTeamByIdAsync(string teamId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/events/one-time/teams/{teamId}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<OneTimeEventTeamResponse>>();
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting team");
            return null;
        }
    }

    public async Task<ApiResponse<OneTimeEventTeamResponse>?> RegisterTeamAsync(string eventId, RegisterTeamRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/v1/events/one-time/{eventId}/teams", request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<OneTimeEventTeamResponse>>();
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            try
            {
                var errorResponse = await response.Content.ReadFromJsonAsync<ApiResponse<OneTimeEventTeamResponse>>();
                if (errorResponse != null) return errorResponse;
            }
            catch { }

            return ApiResponse<OneTimeEventTeamResponse>.ErrorResponse($"Request failed with status {response.StatusCode}", errorContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error registering team");
            return ApiResponse<OneTimeEventTeamResponse>.ErrorResponse("Request failed", ex.Message);
        }
    }

    public async Task<ApiResponse<OneTimeEventTeamResponse>?> UpdateTeamAsync(string teamId, UpdateTeamRequest request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/v1/events/one-time/teams/{teamId}", request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<OneTimeEventTeamResponse>>();
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating team");
            return null;
        }
    }


    public async Task<ApiResponse<bool>?> RemoveTeamAsync(string teamId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/v1/events/one-time/teams/{teamId}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing team");
            return null;
        }
    }

    public async Task<ApiResponse<OneTimeEventTeamResponse>?> CheckInTeamAsync(string teamId)
    {
        try
        {
            var response = await _httpClient.PostAsync($"api/v1/events/one-time/teams/{teamId}/check-in", null);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<OneTimeEventTeamResponse>>();
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error checking in team");
            return null;
        }
    }

    // Player operations
    public async Task<ApiResponse<OneTimeEventPlayerResponse>?> AddPlayerAsync(string teamId, AddPlayerRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/v1/events/one-time/teams/{teamId}/players", request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<OneTimeEventPlayerResponse>>();
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding player");
            return null;
        }
    }

    public async Task<ApiResponse<OneTimeEventPlayerResponse>?> UpdatePlayerAsync(string playerId, UpdatePlayerRequest request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/v1/events/one-time/players/{playerId}", request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<OneTimeEventPlayerResponse>>();
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating player");
            return null;
        }
    }

    public async Task<ApiResponse<bool>?> RemovePlayerAsync(string playerId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/v1/events/one-time/players/{playerId}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
            }

            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing player");
            return null;
        }
    }
}
