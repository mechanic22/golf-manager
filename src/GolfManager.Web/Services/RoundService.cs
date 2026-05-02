using System.Net.Http.Json;
using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.Round;

namespace GolfManager.Web.Services;

/// <summary>
/// Service for round operations
/// </summary>
public class RoundService : IRoundService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<RoundService> _logger;

    public RoundService(HttpClient httpClient, ILogger<RoundService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<RoundResponse?> GetRoundByIdAsync(string leagueId, string roundId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<RoundResponse>>(
                $"api/v1/rounds/{roundId}");

            return response?.Data;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting round {RoundId} in league {LeagueId}", roundId, leagueId);
            return null;
        }
    }

    public async Task<List<RoundResponse>> GetGolferRoundsAsync(string leagueId, string golferId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<RoundResponse>>>(
                $"api/v1/rounds?golferId={golferId}");

            return response?.Data ?? new List<RoundResponse>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting rounds for golfer {GolferId} in league {LeagueId}", golferId, leagueId);
            return new List<RoundResponse>();
        }
    }

    public async Task<ApiResponse<RoundResponse>> CreateRoundAsync(string leagueId, CreateRoundRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/v1/rounds",
                request);

            return await response.Content.ReadFromJsonAsync<ApiResponse<RoundResponse>>()
                ?? ApiResponse<RoundResponse>.ErrorResponse("Failed to create round");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating round in league {LeagueId}", leagueId);
            return ApiResponse<RoundResponse>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<RoundResponse>> UpdateRoundAsync(string leagueId, string roundId, UpdateRoundRequest request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync(
                $"api/v1/rounds/{roundId}",
                request);

            return await response.Content.ReadFromJsonAsync<ApiResponse<RoundResponse>>()
                ?? ApiResponse<RoundResponse>.ErrorResponse("Failed to update round");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating round {RoundId} in league {LeagueId}", roundId, leagueId);
            return ApiResponse<RoundResponse>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<bool>> DeleteRoundAsync(string leagueId, string roundId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync(
                $"api/v1/rounds/{roundId}");

            if (response.IsSuccessStatusCode)
            {
                return ApiResponse<bool>.SuccessResponse(true);
            }

            return ApiResponse<bool>.ErrorResponse("Failed to delete round");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting round {RoundId} in league {LeagueId}", roundId, leagueId);
            return ApiResponse<bool>.ErrorResponse(ex.Message);
        }
    }

    public async Task<ApiResponse<RoundResponse>> RecordHoleScoreAsync(string leagueId, string roundId, RecordHoleScoreRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"api/v1/rounds/{roundId}/holes",
                request);

            return await response.Content.ReadFromJsonAsync<ApiResponse<RoundResponse>>()
                ?? ApiResponse<RoundResponse>.ErrorResponse("Failed to record hole score");
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recording hole score for round {RoundId} in league {LeagueId}", roundId, leagueId);
            return ApiResponse<RoundResponse>.ErrorResponse(ex.Message);
        }
    }
}

