using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.Handicap;

namespace GolfManager.Web.Features.Profile;

/// <summary>
/// Service for handicap operations
/// </summary>
public interface IHandicapService
{
    /// <summary>
    /// Get handicap history for a golfer
    /// </summary>
    Task<ApiResponse<List<HandicapHistoryResponse>>?> GetHandicapHistoryAsync(
        string golferId, 
        string? leagueId = null, 
        string? seasonId = null,
        int limit = 50);

    /// <summary>
    /// Create/update a handicap
    /// </summary>
    Task<ApiResponse<HandicapHistoryResponse>?> CreateHandicapAsync(
        string golferId, 
        CreateHandicapRequest request);

    /// <summary>
    /// Get current handicap
    /// </summary>
    Task<ApiResponse<double?>?> GetCurrentHandicapAsync(
        string golferId,
        string? leagueId = null,
        string? seasonId = null);

    /// <summary>
    /// Calculate handicap breakdown (preview only — does not persist)
    /// </summary>
    Task<ApiResponse<HandicapCalculationResponse>?> GetHandicapBreakdownAsync(
        string golferId,
        string? leagueId = null,
        string? seasonId = null);
}
