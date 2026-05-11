using System.Net.Http.Json;
using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.Season;

namespace GolfManager.Web.Services;

/// <summary>
/// Service for handling season settings operations
/// </summary>
public class SeasonSettingsService : ISeasonSettingsService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<SeasonSettingsService> _logger;

    public SeasonSettingsService(HttpClient httpClient, ILogger<SeasonSettingsService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<SeasonSettingsResponse?> GetSeasonSettingsAsync(string leagueId, string seasonId)
    {
        try
        {
            var responseMessage = await _httpClient.GetAsync($"api/v1/seasons/{seasonId}/settings");
            if (responseMessage.StatusCode == System.Net.HttpStatusCode.NotFound)
            {
                return null;
            }

            responseMessage.EnsureSuccessStatusCode();

            var response = await responseMessage.Content.ReadFromJsonAsync<ApiResponse<SeasonSettingsResponse>>();

            return response?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting season settings for season {SeasonId}", seasonId);
            return null;
        }
    }

    public async Task<ApiResponse<SeasonSettingsResponse>> UpdateSeasonSettingsAsync(
        string leagueId,
        string seasonId,
        UpdateSeasonSettingsRequest request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync(
                $"api/v1/seasons/{seasonId}/settings",
                request);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<SeasonSettingsResponse>>();
                return result ?? ApiResponse<SeasonSettingsResponse>.ErrorResponse("Failed to parse response");
            }

            var error = await response.Content.ReadAsStringAsync();
            return ApiResponse<SeasonSettingsResponse>.ErrorResponse(error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating season settings for season {SeasonId}", seasonId);
            return ApiResponse<SeasonSettingsResponse>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<SeasonSettingsResponse>> CreateDefaultSettingsAsync(string leagueId, string seasonId)
    {
        try
        {
            var response = await _httpClient.PostAsync(
                $"api/v1/seasons/{seasonId}/settings/default",
                null);

            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<SeasonSettingsResponse>>();
                return result ?? ApiResponse<SeasonSettingsResponse>.ErrorResponse("Failed to parse response");
            }

            var error = await response.Content.ReadAsStringAsync();
            return ApiResponse<SeasonSettingsResponse>.ErrorResponse(error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating default settings for season {SeasonId}", seasonId);
            return ApiResponse<SeasonSettingsResponse>.ErrorResponse(ex.Message);
        }
    }
}

