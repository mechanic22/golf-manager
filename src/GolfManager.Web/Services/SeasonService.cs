using System.Net.Http.Json;
using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.Season;

namespace GolfManager.Web.Services;

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
            var response = await _httpClient.GetAsync($"api/v1/leagues/{leagueId}/seasons");

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
            var response = await _httpClient.GetAsync($"api/v1/leagues/{leagueId}/seasons/{seasonId}");

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
            var response = await _httpClient.GetAsync($"api/v1/leagues/{leagueId}/seasons/by-key/{seasonKey}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<SeasonResponse>>();
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
            var response = await _httpClient.PostAsJsonAsync($"api/v1/leagues/{leagueId}/seasons", request);

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
            var response = await _httpClient.PutAsJsonAsync($"api/v1/leagues/{leagueId}/seasons/{seasonId}", request);

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
            var response = await _httpClient.DeleteAsync($"api/v1/leagues/{leagueId}/seasons/{seasonId}");
            return response.IsSuccessStatusCode;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting season");
            return false;
        }
    }
}

