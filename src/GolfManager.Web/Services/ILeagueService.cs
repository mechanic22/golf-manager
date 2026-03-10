using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.League;

namespace GolfManager.Web.Services;

/// <summary>
/// Service for handling league operations
/// </summary>
public interface ILeagueService
{
    /// <summary>
    /// Create a new league
    /// </summary>
    Task<ApiResponse<LeagueResponse>?> CreateLeagueAsync(CreateLeagueRequest request);

    /// <summary>
    /// Get all leagues for the current user
    /// </summary>
    Task<ApiResponse<List<LeagueResponse>>?> GetUserLeaguesAsync();

    /// <summary>
    /// Get a league by its key
    /// </summary>
    Task<ApiResponse<LeagueResponse>?> GetLeagueByKeyAsync(string key);
}

