using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.Season;

namespace GolfManager.Web.Services;

/// <summary>
/// Service for season operations
/// </summary>
public interface ISeasonService
{
    /// <summary>
    /// Get all seasons for a league
    /// </summary>
    Task<ApiResponse<List<SeasonResponse>>?> GetLeagueSeasonsAsync(string leagueId);

    /// <summary>
    /// Get a season by ID
    /// </summary>
    Task<ApiResponse<SeasonResponse>?> GetSeasonByIdAsync(string leagueId, string seasonId);

    /// <summary>
    /// Get a season by key
    /// </summary>
    Task<ApiResponse<SeasonResponse>?> GetSeasonByKeyAsync(string leagueId, string seasonKey);

    /// <summary>
    /// Create a new season
    /// </summary>
    Task<ApiResponse<SeasonResponse>?> CreateSeasonAsync(string leagueId, CreateSeasonRequest request);

    /// <summary>
    /// Update an existing season
    /// </summary>
    Task<ApiResponse<SeasonResponse>?> UpdateSeasonAsync(string leagueId, string seasonId, UpdateSeasonRequest request);

    /// <summary>
    /// Delete a season
    /// </summary>
    Task<bool> DeleteSeasonAsync(string leagueId, string seasonId);
}

