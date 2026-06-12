using GolfManager.Shared.DTOs.League;

namespace GolfManager.Services.League;

/// <summary>
/// Service for managing leagues
/// </summary>
public interface ILeagueService
{
    /// <summary>
    /// Get all leagues for the current user
    /// </summary>
    Task<List<LeagueResponse>> GetUserLeaguesAsync(string userId);

    /// <summary>
    /// Get a specific league by ID
    /// </summary>
    Task<LeagueResponse?> GetLeagueByIdAsync(string leagueId, string? userId = null);

    /// <summary>
    /// Get a specific league by key
    /// </summary>
    Task<LeagueResponse?> GetLeagueByKeyAsync(string leagueKey, string? userId = null, string? anonymousAccessPassword = null);

    /// <summary>
    /// Get publicly discoverable leagues (anonymous, optional search filter)
    /// </summary>
    Task<List<LeagueResponse>> GetPublicLeaguesAsync(string? search = null);

    /// <summary>
    /// Verify whether a supplied anonymous/public password grants access for a league key.
    /// </summary>
    Task<bool> VerifyAnonymousAccessAsync(string leagueKey, string password);

    /// <summary>
    /// Create a new league
    /// </summary>
    Task<LeagueResponse> CreateLeagueAsync(CreateLeagueRequest request, string userId);

    /// <summary>
    /// Update a league
    /// </summary>
    Task<LeagueResponse> UpdateLeagueAsync(string leagueId, UpdateLeagueRequest request, string userId);

    /// <summary>
    /// Delete a league (soft delete)
    /// </summary>
    Task<bool> DeleteLeagueAsync(string leagueId, string userId);

    /// <summary>
    /// Verify the league custom domain by DNS resolution and TXT record
    /// </summary>
    Task<LeagueResponse> VerifyCustomDomainAsync(string leagueId, string userId);

    /// <summary>
    /// Get all members of a league
    /// </summary>
    Task<List<LeagueMemberResponse>> GetLeagueMembersAsync(string leagueId);

    /// <summary>
    /// Add a member to a league
    /// </summary>
    Task<LeagueMemberResponse> AddLeagueMemberAsync(string leagueId, AddLeagueMemberRequest request, string currentUserId);

    /// <summary>
    /// Remove a member from a league
    /// </summary>
    Task<bool> RemoveLeagueMemberAsync(string leagueId, string userId, string currentUserId);

    /// <summary>
    /// Update a league member's role
    /// </summary>
    Task<LeagueMemberResponse> UpdateLeagueMemberAsync(string leagueId, string userId, UpdateLeagueMemberRequest request, string currentUserId);
}

