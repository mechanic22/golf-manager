using GolfManager.Shared.DTOs.Player;

namespace GolfManager.Services.Player;

/// <summary>
/// Service for managing players within leagues
/// </summary>
public interface IPlayerService
{
    /// <summary>
    /// Get all players in a league
    /// </summary>
    Task<List<PlayerResponse>> GetLeaguePlayersAsync(string leagueId);

    /// <summary>
    /// Get a specific player in a league
    /// </summary>
    Task<PlayerResponse?> GetPlayerAsync(string leagueId, string playerId);

    /// <summary>
    /// Add a player to a league
    /// </summary>
    Task<PlayerResponse> AddPlayerToLeagueAsync(string leagueId, CreatePlayerRequest request);

    /// <summary>
    /// Update a player's league profile
    /// </summary>
    Task<PlayerResponse> UpdatePlayerAsync(string leagueId, string playerId, UpdatePlayerRequest request);

    /// <summary>
    /// Remove a player from a league
    /// </summary>
    Task<bool> RemovePlayerFromLeagueAsync(string leagueId, string playerId);

    /// <summary>
    /// Get players participating in a specific season
    /// </summary>
    Task<List<PlayerResponse>> GetSeasonPlayersAsync(string seasonId, string leagueId);
}

