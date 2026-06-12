using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.Round;

namespace GolfManager.Web.Features.Profile;

/// <summary>
/// Service interface for round operations
/// </summary>
public interface IRoundService
{
    /// <summary>
    /// Get a round by ID
    /// </summary>
    Task<RoundResponse?> GetRoundByIdAsync(string leagueId, string roundId);

    /// <summary>
    /// Get all rounds for a golfer in a league
    /// </summary>
    Task<List<RoundResponse>> GetGolferRoundsAsync(string leagueId, string golferId);

    /// <summary>
    /// Create a new round
    /// </summary>
    Task<ApiResponse<RoundResponse>> CreateRoundAsync(string leagueId, CreateRoundRequest request);

    /// <summary>
    /// Update an existing round
    /// </summary>
    Task<ApiResponse<RoundResponse>> UpdateRoundAsync(string leagueId, string roundId, UpdateRoundRequest request);

    /// <summary>
    /// Delete a round
    /// </summary>
    Task<ApiResponse<bool>> DeleteRoundAsync(string leagueId, string roundId);

    /// <summary>
    /// Record or update a hole score
    /// </summary>
    Task<ApiResponse<RoundResponse>> RecordHoleScoreAsync(string leagueId, string roundId, RecordHoleScoreRequest request);
}

