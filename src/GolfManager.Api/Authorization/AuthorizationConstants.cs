namespace GolfManager.Api.Authorization;

/// <summary>
/// Authorization constants for policies, roles, and claims
/// </summary>
public static class AuthorizationConstants
{
    /// <summary>
    /// Authorization policy names
    /// </summary>
    public static class Policies
    {
        /// <summary>
        /// User must be a member of the league (any role)
        /// </summary>
        public const string LeagueMember = "LeagueMember";

        /// <summary>
        /// User must be an admin of the league
        /// </summary>
        public const string LeagueAdmin = "LeagueAdmin";

        /// <summary>
        /// User must be a global admin
        /// </summary>
        public const string GlobalAdmin = "GlobalAdmin";

        /// <summary>
        /// User must be either a league admin or global admin
        /// </summary>
        public const string LeagueOrGlobalAdmin = "LeagueOrGlobalAdmin";

        /// <summary>
        /// Anonymous guest user with a valid scoped league session
        /// </summary>
        public const string GuestLeagueViewer = "GuestLeagueViewer";
    }

    /// <summary>
    /// Custom claim types
    /// </summary>
    public static class Claims
    {
        /// <summary>
        /// Claim for global admin status
        /// </summary>
        public const string IsGlobalAdmin = "is_global_admin";

        /// <summary>
        /// Claim for league ID (used for league-specific operations)
        /// </summary>
        public const string LeagueId = "league_id";

        /// <summary>
        /// Claim for league admin status
        /// </summary>
        public const string IsLeagueAdmin = "is_league_admin";

        /// <summary>
        /// Claim indicating this is an anonymous guest session
        /// </summary>
        public const string IsGuest = "is_guest";

        /// <summary>
        /// Claim holding the league key for a guest session
        /// </summary>
        public const string LeagueKey = "league_key";
    }

    /// <summary>
    /// Route parameter names
    /// </summary>
    public static class RouteParams
    {
        /// <summary>
        /// League ID route parameter
        /// </summary>
        public const string LeagueId = "leagueId";

        /// <summary>
        /// League key route parameter
        /// </summary>
        public const string LeagueKey = "leagueKey";
    }
}

