using System.Net.Http.Json;
using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.Handicap;

namespace GolfManager.Web.Features.Profile;

/// <summary>
/// Service for handicap operations
/// </summary>
public class HandicapService : IHandicapService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<HandicapService> _logger;

    public HandicapService(HttpClient httpClient, ILogger<HandicapService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ApiResponse<List<HandicapHistoryResponse>>?> GetHandicapHistoryAsync(
        string golferId, 
        string? leagueId = null, 
        string? seasonId = null,
        int limit = 50)
    {
        try
        {
            var query = $"?limit={limit}";
            if (!string.IsNullOrEmpty(leagueId))
                query += $"&leagueId={leagueId}";
            if (!string.IsNullOrEmpty(seasonId))
                query += $"&seasonId={seasonId}";

            _logger.LogInformation("Fetching handicap history for golfer {GolferId}", golferId);
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<HandicapHistoryResponse>>>(
                $"api/v1/golfers/{golferId}/handicap/history{query}");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching handicap history for golfer {GolferId}", golferId);
            return ApiResponse<List<HandicapHistoryResponse>>.ErrorResponse("Failed to load handicap history", ex.Message);
        }
    }

    public async Task<ApiResponse<HandicapHistoryResponse>?> CreateHandicapAsync(
        string golferId, 
        CreateHandicapRequest request)
    {
        try
        {
            _logger.LogInformation("Creating handicap for golfer {GolferId}", golferId);
            var response = await _httpClient.PostAsJsonAsync($"api/v1/golfers/{golferId}/handicap", request);
            
            if (response.IsSuccessStatusCode)
            {
                var result = await response.Content.ReadFromJsonAsync<ApiResponse<HandicapHistoryResponse>>();
                _logger.LogInformation("Handicap created successfully for golfer {GolferId}", golferId);
                return result;
            }

            var errorContent = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to create handicap: {StatusCode} - {Error}", response.StatusCode, errorContent);
            return ApiResponse<HandicapHistoryResponse>.ErrorResponse("Failed to create handicap", errorContent);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating handicap for golfer {GolferId}", golferId);
            return ApiResponse<HandicapHistoryResponse>.ErrorResponse("Failed to create handicap", ex.Message);
        }
    }

    public async Task<ApiResponse<HandicapCalculationResponse>?> GetHandicapBreakdownAsync(
        string golferId,
        string? leagueId = null,
        string? seasonId = null)
    {
        try
        {
            var request = new CalculateHandicapRequest
            {
                LeagueId = leagueId,
                SeasonId = seasonId,
                Method = HandicapCalculationMethod.WorldHandicapSystem,
                Persist = false
            };

            var response = await _httpClient.PostAsJsonAsync($"api/v1/golfers/{golferId}/handicap/calculate", request);
            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<ApiResponse<HandicapCalculationResponse>>();

            var error = await response.Content.ReadAsStringAsync();
            _logger.LogError("Failed to fetch handicap breakdown: {StatusCode} - {Error}", response.StatusCode, error);
            return ApiResponse<HandicapCalculationResponse>.ErrorResponse("Failed to load handicap breakdown", error);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching handicap breakdown for golfer {GolferId}", golferId);
            return ApiResponse<HandicapCalculationResponse>.ErrorResponse("Failed to load handicap breakdown", ex.Message);
        }
    }

    public async Task<ApiResponse<double?>?> GetCurrentHandicapAsync(
        string golferId, 
        string? leagueId = null, 
        string? seasonId = null)
    {
        try
        {
            var query = "";
            if (!string.IsNullOrEmpty(leagueId))
                query += $"?leagueId={leagueId}";
            if (!string.IsNullOrEmpty(seasonId))
                query += string.IsNullOrEmpty(query) ? $"?seasonId={seasonId}" : $"&seasonId={seasonId}";

            _logger.LogInformation("Fetching current handicap for golfer {GolferId}", golferId);
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<double?>>(
                $"api/v1/golfers/{golferId}/handicap/current{query}");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching current handicap for golfer {GolferId}", golferId);
            return ApiResponse<double?>.ErrorResponse("Failed to load current handicap", ex.Message);
        }
    }
}
