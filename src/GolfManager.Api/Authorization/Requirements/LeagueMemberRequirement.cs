using Microsoft.AspNetCore.Authorization;

namespace GolfManager.Api.Authorization.Requirements;

/// <summary>
/// Requirement that user must be a member of the league
/// </summary>
public class LeagueMemberRequirement : IAuthorizationRequirement
{
    public LeagueMemberRequirement()
    {
    }
}

