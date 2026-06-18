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

    /// <summary>
    /// Bulk configure a season from pasted calendar and team roster text.
    /// </summary>
    Task<SeasonSetupResponse> SetupSeasonAsync(string seasonId, SeasonSetupRequest request, string leagueId, string userId);

    // ── Teams ────────────────────────────────────────────────────────────────

    Task<List<SeasonTeamResponse>> GetSeasonTeamsAsync(string seasonId, string leagueId);
    Task<SeasonTeamResponse> CreateSeasonTeamAsync(string seasonId, CreateSeasonTeamRequest request, string leagueId, string userId);
    Task<SeasonTeamResponse> UpdateSeasonTeamAsync(string seasonId, string teamId, UpdateSeasonTeamRequest request, string leagueId, string userId);
    Task<bool> DeleteSeasonTeamAsync(string seasonId, string teamId, string leagueId, string userId);
    Task AssignPlayerToTeamAsync(string seasonId, string seasonGolferId, AssignPlayerToTeamRequest request, string leagueId, string userId);

    // ── Players ──────────────────────────────────────────────────────────────

    Task<bool> RemovePlayerFromSeasonAsync(string seasonId, string seasonGolferId, string leagueId, string userId);
    Task UpdateSeasonPlayerPaymentAsync(string seasonId, string seasonGolferId, UpdateSeasonPlayerPaymentRequest request, string leagueId, string userId);

    // ── Stats ─────────────────────────────────────────────────────────────────

    Task<PlayerSeasonHoleStatsResponse?> GetPlayerSeasonHoleStatsAsync(string seasonId, string leagueGolferId, string leagueId);
    Task<PlayerSeasonHoleStatsResponse?> GetPlayerCareerHoleStatsAsync(string leagueGolferId, string leagueId);
}

