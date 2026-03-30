using GolfManager.Shared.DTOs.Season;

namespace GolfManager.Services.Season;

/// <summary>
/// Service interface for managing season settings
/// </summary>
public interface ISeasonSettingsService
{
    /// <summary>
    /// Get settings for a season
    /// </summary>
    Task<SeasonSettingsResponse?> GetSeasonSettingsAsync(string seasonId, string leagueId);

    /// <summary>
    /// Update season settings
    /// </summary>
    Task<SeasonSettingsResponse> UpdateSeasonSettingsAsync(
        string seasonId,
        string leagueId,
        UpdateSeasonSettingsRequest request);

    /// <summary>
    /// Create default settings for a new season
    /// </summary>
    Task<SeasonSettingsResponse> CreateDefaultSettingsAsync(string seasonId, string leagueId);
}

