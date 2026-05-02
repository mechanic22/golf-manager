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
    /// <param name="golferId">Golfer ID</param>
    /// <param name="leagueId">Optional league ID filter (null = global)</param>
    /// <param name="seasonId">Optional season ID filter</param>
    /// <param name="limit">Maximum number of records to return (default 50)</param>
    Task<ApiResponse<List<HandicapHistoryResponse>>> GetHandicapHistoryAsync(
        string golferId, 
        string? leagueId = null, 
        string? seasonId = null,
        int limit = 50);

    /// <summary>
    /// Create or update a handicap (manual entry)
    /// </summary>
    /// <param name="golferId">Golfer ID</param>
    /// <param name="request">Handicap details</param>
    /// <param name="currentUserId">User making the change</param>
    Task<ApiResponse<HandicapHistoryResponse>> CreateHandicapAsync(
        string golferId, 
        CreateHandicapRequest request,
        string currentUserId);

    /// <summary>
    /// Get current handicap for a golfer at different scopes
    /// </summary>
    /// <param name="golferId">Golfer ID</param>
    /// <param name="leagueId">Optional league ID</param>
    /// <param name="seasonId">Optional season ID</param>
    Task<double?> GetCurrentHandicapAsync(
        string golferId, 
        string? leagueId = null, 
        string? seasonId = null);
}
