using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.Handicap;

namespace GolfManager.Services.Handicap;

/// <summary>
/// Service for managing golfer handicaps
/// </summary>
public interface IHandicapService
{
    /// <summary>
    /// Get handicap history for a golfer
    /// </summary>
    Task<ApiResponse<List<HandicapHistoryResponse>>> GetHandicapHistoryAsync(
        string golferId, 
        string? leagueId = null, 
        string? seasonId = null,
        int limit = 50);

    /// <summary>
    /// Create or update a handicap (manual entry)
    /// </summary>
    Task<ApiResponse<HandicapHistoryResponse>> CreateHandicapAsync(
        string golferId, 
        CreateHandicapRequest request,
        string currentUserId);

    /// <summary>
    /// Get current handicap for a golfer at different scopes
    /// </summary>
    Task<double?> GetCurrentHandicapAsync(
        string golferId, 
        string? leagueId = null, 
        string? seasonId = null);

    /// <summary>
    /// Calculate a handicap index from the golfer's rounds using the chosen algorithm.
    /// Optionally persists the result to handicap history.
    /// </summary>
    Task<ApiResponse<HandicapCalculationResponse>> CalculateHandicapAsync(
        string golferId,
        CalculateHandicapRequest request,
        string currentUserId);
}
