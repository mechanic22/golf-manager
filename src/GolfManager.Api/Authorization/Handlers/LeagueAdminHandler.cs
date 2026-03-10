using System.Security.Claims;
using GolfManager.Api.Authorization.Requirements;
using GolfManager.Services.League;
using Microsoft.AspNetCore.Authorization;

namespace GolfManager.Api.Authorization.Handlers;

/// <summary>
/// Handler for LeagueAdminRequirement
/// Validates that the user is an admin of the league specified in the route
/// </summary>
public class LeagueAdminHandler : AuthorizationHandler<LeagueAdminRequirement>
{
    private readonly ILeagueAuthorizationService _leagueAuthService;
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly ILogger<LeagueAdminHandler> _logger;

    public LeagueAdminHandler(
        ILeagueAuthorizationService leagueAuthService,
        IHttpContextAccessor httpContextAccessor,
        ILogger<LeagueAdminHandler> logger)
    {
        _leagueAuthService = leagueAuthService;
        _httpContextAccessor = httpContextAccessor;
        _logger = logger;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        LeagueAdminRequirement requirement)
    {
        // Get user ID from claims
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            _logger.LogWarning("User ID not found in claims");
            return;
        }

        // Check if user is global admin
        var isGlobalAdmin = context.User.FindFirst(AuthorizationConstants.Claims.IsGlobalAdmin)?.Value == "true";
        if (isGlobalAdmin)
        {
            _logger.LogInformation("User {UserId} is global admin, granting admin access", userId);
            context.Succeed(requirement);
            return;
        }

        // Get league ID from route
        var leagueId = GetLeagueIdFromRoute();
        if (string.IsNullOrEmpty(leagueId))
        {
            _logger.LogWarning("League ID not found in route");
            return;
        }

        // Check if user is an admin of the league
        var isAdmin = await _leagueAuthService.IsUserLeagueAdminAsync(userId, leagueId);
        if (isAdmin)
        {
            _logger.LogInformation("User {UserId} is an admin of league {LeagueId}", userId, leagueId);
            context.Succeed(requirement);
        }
        else
        {
            _logger.LogWarning("User {UserId} is not an admin of league {LeagueId}", userId, leagueId);
        }
    }

    private string? GetLeagueIdFromRoute()
    {
        var httpContext = _httpContextAccessor.HttpContext;
        if (httpContext == null)
        {
            return null;
        }

        // Try to get leagueId from route values
        if (httpContext.Request.RouteValues.TryGetValue(AuthorizationConstants.RouteParams.LeagueId, out var leagueIdObj))
        {
            return leagueIdObj?.ToString();
        }

        // Try to get leagueKey from route values
        if (httpContext.Request.RouteValues.TryGetValue(AuthorizationConstants.RouteParams.LeagueKey, out var leagueKeyObj))
        {
            return leagueKeyObj?.ToString();
        }

        return null;
    }
}

