using Microsoft.AspNetCore.Authorization;

namespace GolfManager.Api.Authorization.Requirements;

/// <summary>
/// Requirement that user must be an admin of the league
/// </summary>
public class LeagueAdminRequirement : IAuthorizationRequirement
{
    public LeagueAdminRequirement()
    {
    }
}

