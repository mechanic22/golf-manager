using System.Net.Http.Json;
using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.Season;

namespace GolfManager.Web.Features.Season;

/// <summary>
/// Service for handling season operations
/// </summary>
public class SeasonService : ISeasonService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SeasonService> _logger;

    public SeasonService(HttpClient httpClient, ILogger<SeasonService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ApiResponse<List<SeasonResponse>>?> GetLeagueSeasonsAsync(string leagueId)
    {
        try
        {
            var response = await _httpClient.GetAsync("api/v1/seasons");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<List<SeasonResponse>>>();
            }

            _logger.LogWarning("Failed to get league seasons: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting league seasons");
            return null;
        }
    }

    public async Task<ApiResponse<SeasonResponse>?> GetSeasonByIdAsync(string leagueId, string seasonId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/seasons/{seasonId}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<SeasonResponse>>();
            }

            _logger.LogWarning("Failed to get season by ID: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting season by ID");
            return null;
        }
    }

    public async Task<ApiResponse<SeasonResponse>?> GetSeasonByKeyAsync(string leagueId, string seasonKey)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/seasons/by-key/{seasonKey}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<SeasonResponse>>();
            }

            if (response.StatusCode == System.Net.HttpStatusCode.Forbidden)
            {
                var error = await response.Content.ReadAsStringAsync();
                return ApiResponse<SeasonResponse>.ErrorResponse("Forbidden", error);
            }

            _logger.LogWarning("Failed to get season by key: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting season by key");
            return null;
        }
    }

    public async Task<ApiResponse<SeasonResponse>?> CreateSeasonAsync(string leagueId, CreateSeasonRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync("api/v1/seasons", request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<SeasonResponse>>();
            }

            _logger.LogWarning("Failed to create season: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating season");
            return null;
        }
    }

    public async Task<ApiResponse<SeasonResponse>?> UpdateSeasonAsync(string leagueId, string seasonId, UpdateSeasonRequest request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/v1/seasons/{seasonId}", request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<SeasonResponse>>();
            }

            _logger.LogWarning("Failed to update season: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating season");
            return null;
        }
    }

    public async Task<bool> DeleteSeasonAsync(string leagueId, string seasonId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/v1/seasons/{seasonId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting season");
            return false;
        }
    }

    public async Task<ApiResponse<SeasonSetupResponse>?> SetupSeasonAsync(string leagueId, string seasonId, SeasonSetupRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/v1/seasons/{seasonId}/setup", request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<SeasonSetupResponse>>();
            }

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogWarning("Failed to bulk configure season {SeasonId}: {StatusCode} {Error}", seasonId, response.StatusCode, error);
            return ApiResponse<SeasonSetupResponse>.ErrorResponse("Failed to configure season", error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error configuring season {SeasonId}", seasonId);
            return ApiResponse<SeasonSetupResponse>.ErrorResponse("Failed to configure season", ex.Message);
        }
    }

    // ── Teams ────────────────────────────────────────────────────────────────

    public async Task<ApiResponse<List<SeasonTeamResponse>>?> GetSeasonTeamsAsync(string leagueId, string seasonId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/seasons/{seasonId}/teams");
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<ApiResponse<List<SeasonTeamResponse>>>();
            _logger.LogWarning("Failed to get season teams: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting season teams");
            return null;
        }
    }

    public async Task<ApiResponse<SeasonTeamResponse>?> CreateSeasonTeamAsync(string leagueId, string seasonId, CreateSeasonTeamRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync($"api/v1/seasons/{seasonId}/teams", request);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<ApiResponse<SeasonTeamResponse>>();
            var error = await response.Content.ReadAsStringAsync();
            return ApiResponse<SeasonTeamResponse>.ErrorResponse("Failed to create team", error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating season team");
            return null;
        }
    }

    public async Task<ApiResponse<SeasonTeamResponse>?> UpdateSeasonTeamAsync(string leagueId, string seasonId, string teamId, UpdateSeasonTeamRequest request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/v1/seasons/{seasonId}/teams/{teamId}", request);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<ApiResponse<SeasonTeamResponse>>();
            var error = await response.Content.ReadAsStringAsync();
            return ApiResponse<SeasonTeamResponse>.ErrorResponse("Failed to update team", error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating season team");
            return null;
        }
    }

    public async Task<bool> DeleteSeasonTeamAsync(string leagueId, string seasonId, string teamId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/v1/seasons/{seasonId}/teams/{teamId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting season team");
            return false;
        }
    }

    public async Task<bool> AssignPlayerToTeamAsync(string leagueId, string seasonId, string seasonGolferId, AssignPlayerToTeamRequest request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/v1/seasons/{seasonId}/players/{seasonGolferId}/team", request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error assigning player to team");
            return false;
        }
    }

    public async Task<bool> RemovePlayerFromSeasonAsync(string leagueId, string seasonId, string seasonGolferId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync($"api/v1/seasons/{seasonId}/players/{seasonGolferId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing player from season");
            return false;
        }
    }

    public async Task<bool> UpdateSeasonPlayerPaymentAsync(string leagueId, string seasonId, string seasonGolferId, UpdateSeasonPlayerPaymentRequest request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync($"api/v1/seasons/{seasonId}/players/{seasonGolferId}/payment", request);
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating season player payment status");
            return false;
        }
    }

    public async Task<ApiResponse<List<PlayerStandingResponse>>?> GetSeasonStandingsAsync(string leagueId, string seasonId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/seasons/{seasonId}/standings");

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<ApiResponse<List<PlayerStandingResponse>>>();

            _logger.LogWarning("Failed to get season standings: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting season standings");
            return null;
        }
    }

    public async Task<ApiResponse<PlayerSeasonHoleStatsResponse>?> GetPlayerHoleStatsAsync(string leagueId, string seasonId, string leagueGolferId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/seasons/{seasonId}/golfers/{leagueGolferId}/hole-stats");

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<ApiResponse<PlayerSeasonHoleStatsResponse>>();

            _logger.LogWarning("Failed to get player hole stats: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting player hole stats for golfer {LeagueGolferId}", leagueGolferId);
            return null;
        }
    }

    public async Task<ApiResponse<PlayerSeasonHoleStatsResponse>?> GetPlayerCareerHoleStatsAsync(string leagueId, string leagueGolferId)
    {
        try
        {
            var response = await _httpClient.GetAsync($"api/v1/seasons/golfers/{leagueGolferId}/career-hole-stats");

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<ApiResponse<PlayerSeasonHoleStatsResponse>>();

            _logger.LogWarning("Failed to get player career hole stats: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting player career hole stats for golfer {LeagueGolferId}", leagueGolferId);
            return null;
        }
    }
}

