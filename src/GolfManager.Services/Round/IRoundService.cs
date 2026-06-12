using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.Round;

namespace GolfManager.Services.Round;

/// <summary>
/// Service interface for round management
/// </summary>
public interface IRoundService
{
    /// <summary>
    /// Get all rounds for a league golfer
    /// </summary>
    Task<PagedResponse<RoundResponse>> GetLeagueGolferRoundsAsync(string leagueGolferId, string leagueId, int page = 1, int pageSize = 25);

    /// <summary>
    /// Get a specific round by ID
    /// </summary>
    Task<RoundResponse?> GetRoundByIdAsync(string roundId, string leagueId);

    /// <summary>
    /// Get all rounds for a season event
    /// </summary>
    Task<List<RoundResponse>> GetEventRoundsAsync(string seasonEventId, string leagueId);

    /// <summary>
    /// Create a new round
    /// </summary>
    Task<RoundResponse> CreateRoundAsync(CreateRoundRequest request, string leagueId, string userId);

    /// <summary>
    /// Update an existing round
    /// </summary>
    Task<RoundResponse> UpdateRoundAsync(string roundId, UpdateRoundRequest request, string leagueId, string userId);

    /// <summary>
    /// Delete a round
    /// </summary>
    Task DeleteRoundAsync(string roundId, string leagueId, string userId);

    /// <summary>
    /// Record or update a hole score
    /// </summary>
    Task<RoundResponse> RecordHoleScoreAsync(string roundId, RecordHoleScoreRequest request, string leagueId, string userId);

    /// <summary>
    /// Calculate and update round totals
    /// </summary>
    Task<RoundResponse> CalculateRoundTotalsAsync(string roundId, string leagueId);
}

