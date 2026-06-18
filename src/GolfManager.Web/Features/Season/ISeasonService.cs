using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.Season;

namespace GolfManager.Web.Features.Season;

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

    /// <summary>
    /// Bulk configure a season from pasted calendar and team roster text.
    /// </summary>
    Task<ApiResponse<SeasonSetupResponse>?> SetupSeasonAsync(string leagueId, string seasonId, SeasonSetupRequest request);

    // ── Teams ────────────────────────────────────────────────────────────────
    Task<ApiResponse<List<SeasonTeamResponse>>?> GetSeasonTeamsAsync(string leagueId, string seasonId);
    Task<ApiResponse<SeasonTeamResponse>?> CreateSeasonTeamAsync(string leagueId, string seasonId, CreateSeasonTeamRequest request);
    Task<ApiResponse<SeasonTeamResponse>?> UpdateSeasonTeamAsync(string leagueId, string seasonId, string teamId, UpdateSeasonTeamRequest request);
    Task<bool> DeleteSeasonTeamAsync(string leagueId, string seasonId, string teamId);
    Task<bool> AssignPlayerToTeamAsync(string leagueId, string seasonId, string seasonGolferId, AssignPlayerToTeamRequest request);

    // ── Players ──────────────────────────────────────────────────────────────
    Task<bool> RemovePlayerFromSeasonAsync(string leagueId, string seasonId, string seasonGolferId);
    Task<bool> UpdateSeasonPlayerPaymentAsync(string leagueId, string seasonId, string seasonGolferId, UpdateSeasonPlayerPaymentRequest request);

    // ── Standings ─────────────────────────────────────────────────────────────
    Task<ApiResponse<List<PlayerStandingResponse>>?> GetSeasonStandingsAsync(string leagueId, string seasonId);

    // ── Stats ─────────────────────────────────────────────────────────────────
    Task<ApiResponse<PlayerSeasonHoleStatsResponse>?> GetPlayerHoleStatsAsync(string leagueId, string seasonId, string leagueGolferId);
    Task<ApiResponse<PlayerSeasonHoleStatsResponse>?> GetPlayerCareerHoleStatsAsync(string leagueId, string leagueGolferId);
}

