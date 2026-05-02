using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.League;

namespace GolfManager.Web.Services;

/// <summary>
/// Service for handling league operations
/// </summary>
public interface ILeagueService
{
    /// <summary>
    /// Create a new league
    /// </summary>
    Task<ApiResponse<LeagueResponse>?> CreateLeagueAsync(CreateLeagueRequest request);

    /// <summary>
    /// Get all leagues for the current user
    /// </summary>
    Task<ApiResponse<List<LeagueResponse>>?> GetUserLeaguesAsync();

    /// <summary>
    /// Get a league by its key
    /// </summary>
    Task<ApiResponse<LeagueResponse>?> GetLeagueByKeyAsync(string key);

    /// <summary>
    /// Update a league
    /// </summary>
    Task<ApiResponse<LeagueResponse>?> UpdateLeagueAsync(string leagueId, UpdateLeagueRequest request);

    /// <summary>
    /// Get all members of a league
    /// </summary>
    Task<ApiResponse<List<LeagueMemberResponse>>?> GetLeagueMembersAsync(string leagueId);

    /// <summary>
    /// Add a member to a league
    /// </summary>
    Task<ApiResponse<LeagueMemberResponse>?> AddLeagueMemberAsync(string leagueId, AddLeagueMemberRequest request);

    /// <summary>
    /// Update a league member's role
    /// </summary>
    Task<ApiResponse<LeagueMemberResponse>?> UpdateLeagueMemberAsync(string leagueId, string userId, UpdateLeagueMemberRequest request);

    /// <summary>
    /// Verify a league's custom domain using DNS lookup and TXT validation
    /// </summary>
    Task<ApiResponse<LeagueResponse>?> VerifyLeagueCustomDomainAsync(string leagueId);

    /// <summary>
    /// Remove a member from a league
    /// </summary>
    Task<ApiResponse<bool>?> RemoveLeagueMemberAsync(string leagueId, string userId);
}

