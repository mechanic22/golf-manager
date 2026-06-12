using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.Player;

namespace GolfManager.Web.Features.Profile;

/// <summary>
/// Service for player operations in the Web client
/// </summary>
public interface IPlayerService
{
    /// <summary>
    /// Get all players in a league
    /// </summary>
    Task<ApiResponse<List<PlayerResponse>>?> GetLeaguePlayersAsync(string leagueId);

    /// <summary>
    /// Get a specific player in a league
    /// </summary>
    Task<ApiResponse<PlayerResponse>?> GetPlayerAsync(string leagueId, string playerId);

    /// <summary>
    /// Add a player to a league
    /// </summary>
    Task<ApiResponse<PlayerResponse>?> AddPlayerToLeagueAsync(string leagueId, CreatePlayerRequest request);

    /// <summary>
    /// Add a player to a season (and league membership if needed)
    /// </summary>
    Task<ApiResponse<PlayerResponse>?> AddPlayerToSeasonAsync(string leagueId, string seasonId, CreatePlayerRequest request);

    /// <summary>
    /// Update a player's league profile
    /// </summary>
    Task<ApiResponse<PlayerResponse>?> UpdatePlayerAsync(string leagueId, string playerId, GolfManager.Shared.DTOs.Player.UpdatePlayerRequest request);

    /// <summary>
    /// Remove a player from a league
    /// </summary>
    Task<ApiResponse<bool>?> RemovePlayerFromLeagueAsync(string leagueId, string playerId);

    /// <summary>
    /// Get players participating in a specific season
    /// </summary>
    Task<ApiResponse<List<PlayerResponse>>?> GetSeasonPlayersAsync(string leagueId, string seasonId);
}

