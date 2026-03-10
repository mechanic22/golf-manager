using GolfManager.Shared.DTOs.Season;

namespace GolfManager.Services.Season;

/// <summary>
/// Service for managing seasons
/// </summary>
public interface ISeasonService
{
    /// <summary>
    /// Get all seasons for a league
    /// </summary>
    Task<List<SeasonResponse>> GetLeagueSeasonsAsync(string leagueId);

    /// <summary>
    /// Get a season by ID
    /// </summary>
    Task<SeasonResponse?> GetSeasonByIdAsync(string seasonId, string leagueId);

    /// <summary>
    /// Get a season by key
    /// </summary>
    Task<SeasonResponse?> GetSeasonByKeyAsync(string seasonKey, string leagueId);

    /// <summary>
    /// Create a new season
    /// </summary>
    Task<SeasonResponse> CreateSeasonAsync(CreateSeasonRequest request, string leagueId, string userId);

    /// <summary>
    /// Update an existing season
    /// </summary>
    Task<SeasonResponse> UpdateSeasonAsync(string seasonId, UpdateSeasonRequest request, string leagueId, string userId);

    /// <summary>
    /// Delete a season (soft delete)
    /// </summary>
    Task<bool> DeleteSeasonAsync(string seasonId, string leagueId, string userId);
}

