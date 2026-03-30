using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.Season;

namespace GolfManager.Web.Services;

/// <summary>
/// Service interface for season settings operations
/// </summary>
public interface ISeasonSettingsService
{
    /// <summary>
    /// Get settings for a season
    /// </summary>
    Task<SeasonSettingsResponse?> GetSeasonSettingsAsync(string leagueId, string seasonId);

    /// <summary>
    /// Update season settings
    /// </summary>
    Task<ApiResponse<SeasonSettingsResponse>> UpdateSeasonSettingsAsync(
        string leagueId,
        string seasonId,
        UpdateSeasonSettingsRequest request);

    /// <summary>
    /// Create default settings for a season
    /// </summary>
    Task<ApiResponse<SeasonSettingsResponse>> CreateDefaultSettingsAsync(string leagueId, string seasonId);
}

