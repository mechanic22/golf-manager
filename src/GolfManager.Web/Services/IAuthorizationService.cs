namespace GolfManager.Web.Services;

/// <summary>
/// Service for checking user permissions and authorization
/// </summary>
public interface IAuthorizationService
{
    /// <summary>
    /// Initialize the authorization service (loads user's league memberships)
    /// </summary>
    Task InitializeAsync();

    /// <summary>
    /// Check if the current user is a global admin
    /// </summary>
    bool IsGlobalAdmin();

    /// <summary>
    /// Check if the current user is an admin of a specific league
    /// </summary>
    bool IsLeagueAdmin(string leagueId);

    /// <summary>
    /// Check if the current user is a member of a specific league
    /// </summary>
    bool IsLeagueMember(string leagueId);

    /// <summary>
    /// Check if the current user can manage a league (Global Admin OR League Admin)
    /// </summary>
    bool CanManageLeague(string leagueId);

    /// <summary>
    /// Check if the current user can view a league (Global Admin OR League Member)
    /// </summary>
    bool CanViewLeague(string leagueId);

    /// <summary>
    /// Check if the current user can manage a season (Global Admin OR League Admin OR Season Admin)
    /// Future: Will include Season Admin role
    /// </summary>
    bool CanManageSeason(string leagueId, string seasonId);

    /// <summary>
    /// Check if the current user can enter scores (Global Admin OR League Admin OR Season Admin OR Moderator)
    /// Future: Will include Season Admin and Moderator roles
    /// </summary>
    bool CanEnterScores(string leagueId, string seasonId);

    /// <summary>
    /// Check if the current user can view a season (Global Admin OR League Member)
    /// </summary>
    bool CanViewSeason(string leagueId, string seasonId);

    /// <summary>
    /// Get all league IDs the current user is a member of
    /// </summary>
    List<string> GetUserLeagueIds();

    /// <summary>
    /// Get all league IDs the current user is an admin of
    /// </summary>
    List<string> GetUserAdminLeagueIds();
}

