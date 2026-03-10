namespace GolfManager.Services.League;

/// <summary>
/// Service for checking league membership and authorization
/// </summary>
public interface ILeagueAuthorizationService
{
    /// <summary>
    /// Check if a user is a member of a league
    /// </summary>
    Task<bool> IsUserMemberOfLeagueAsync(string userId, string leagueId);

    /// <summary>
    /// Check if a user is an admin of a league
    /// </summary>
    Task<bool> IsUserLeagueAdminAsync(string userId, string leagueId);

    /// <summary>
    /// Get the league ID from a league key
    /// </summary>
    Task<string?> GetLeagueIdByKeyAsync(string leagueKey);
}

