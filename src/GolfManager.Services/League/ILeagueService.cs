using GolfManager.Shared.DTOs.League;

namespace GolfManager.Services.League;

/// <summary>
/// Service for managing leagues
/// </summary>
public interface ILeagueService
{
    /// <summary>
    /// Get all leagues for the current user
    /// </summary>
    Task<List<LeagueResponse>> GetUserLeaguesAsync(string userId);

    /// <summary>
    /// Get a specific league by ID
    /// </summary>
    Task<LeagueResponse?> GetLeagueByIdAsync(string leagueId, string? userId = null);

    /// <summary>
    /// Get a specific league by key
    /// </summary>
    Task<LeagueResponse?> GetLeagueByKeyAsync(string leagueKey, string? userId = null);

    /// <summary>
    /// Create a new league
    /// </summary>
    Task<LeagueResponse> CreateLeagueAsync(CreateLeagueRequest request, string userId);

    /// <summary>
    /// Update a league
    /// </summary>
    Task<LeagueResponse> UpdateLeagueAsync(string leagueId, UpdateLeagueRequest request, string userId);

    /// <summary>
    /// Delete a league (soft delete)
    /// </summary>
    Task<bool> DeleteLeagueAsync(string leagueId, string userId);
}

