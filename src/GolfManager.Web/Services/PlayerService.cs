using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.Player;
using System.Net.Http.Json;

namespace GolfManager.Web.Services;

/// <summary>
/// Implementation of player service for Web client
/// </summary>
public class PlayerService : IPlayerService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PlayerService> _logger;

    public PlayerService(HttpClient httpClient, ILogger<PlayerService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ApiResponse<List<PlayerResponse>>?> GetLeaguePlayersAsync(string leagueId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<PlayerResponse>>>(
                "api/v1/players");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting players for league {LeagueId}", leagueId);
            return ApiResponse<List<PlayerResponse>>.ErrorResponse("Failed to load players", ex.Message);
        }
    }

    public async Task<ApiResponse<PlayerResponse>?> GetPlayerAsync(string leagueId, string playerId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<PlayerResponse>>(
                $"api/v1/players/{playerId}");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting player {PlayerId} in league {LeagueId}", playerId, leagueId);
            return ApiResponse<PlayerResponse>.ErrorResponse("Failed to load player", ex.Message);
        }
    }

    public async Task<ApiResponse<PlayerResponse>?> AddPlayerToLeagueAsync(string leagueId, CreatePlayerRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                "api/v1/players",
                request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to add player to league {LeagueId}: {Error}", leagueId, errorContent);
                return ApiResponse<PlayerResponse>.ErrorResponse("Failed to add player", errorContent);
            }

            return await response.Content.ReadFromJsonAsync<ApiResponse<PlayerResponse>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding player to league {LeagueId}", leagueId);
            return ApiResponse<PlayerResponse>.ErrorResponse("Failed to add player", ex.Message);
        }
    }

    public async Task<ApiResponse<PlayerResponse>?> AddPlayerToSeasonAsync(string leagueId, string seasonId, CreatePlayerRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"api/v1/seasons/{seasonId}/players",
                request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to add player to season {SeasonId} in league {LeagueId}: {Error}", seasonId, leagueId, errorContent);
                return ApiResponse<PlayerResponse>.ErrorResponse("Failed to add player to season", errorContent);
            }

            return await response.Content.ReadFromJsonAsync<ApiResponse<PlayerResponse>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error adding player to season {SeasonId} in league {LeagueId}", seasonId, leagueId);
            return ApiResponse<PlayerResponse>.ErrorResponse("Failed to add player to season", ex.Message);
        }
    }

    public async Task<ApiResponse<PlayerResponse>?> UpdatePlayerAsync(string leagueId, string playerId, GolfManager.Shared.DTOs.Player.UpdatePlayerRequest request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync(
                $"api/v1/players/{playerId}",
                request);

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to update player {PlayerId} in league {LeagueId}: {Error}", playerId, leagueId, errorContent);
                return ApiResponse<PlayerResponse>.ErrorResponse("Failed to update player", errorContent);
            }

            return await response.Content.ReadFromJsonAsync<ApiResponse<PlayerResponse>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating player {PlayerId} in league {LeagueId}", playerId, leagueId);
            return ApiResponse<PlayerResponse>.ErrorResponse("Failed to update player", ex.Message);
        }
    }

    public async Task<ApiResponse<bool>?> RemovePlayerFromLeagueAsync(string leagueId, string playerId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync(
                $"api/v1/players/{playerId}");

            if (!response.IsSuccessStatusCode)
            {
                var errorContent = await response.Content.ReadAsStringAsync();
                _logger.LogWarning("Failed to remove player {PlayerId} from league {LeagueId}: {Error}", playerId, leagueId, errorContent);
                return ApiResponse<bool>.ErrorResponse("Failed to remove player", errorContent);
            }

            return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error removing player {PlayerId} from league {LeagueId}", playerId, leagueId);
            return ApiResponse<bool>.ErrorResponse("Failed to remove player", ex.Message);
        }
    }

    public async Task<ApiResponse<List<PlayerResponse>>?> GetSeasonPlayersAsync(string leagueId, string seasonId)
    {
        try
        {
            _logger.LogInformation("Fetching players for season {SeasonId} in league {LeagueId}", seasonId, leagueId);
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<PlayerResponse>>>($"api/v1/seasons/{seasonId}/players");
            _logger.LogInformation("Fetched {Count} season players", response?.Data?.Count ?? 0);
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error fetching season players for season {SeasonId}", seasonId);
            return ApiResponse<List<PlayerResponse>>.ErrorResponse("Failed to load season players", ex.Message);
        }
    }
}

