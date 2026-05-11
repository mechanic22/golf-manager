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

        // Resolve league ID from route values or middleware-populated context items.
        var leagueId = await GetLeagueIdForAuthorizationAsync();
        if (string.IsNullOrEmpty(leagueId))
        {
            _logger.LogWarning("League ID not found in route or context items");
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

    private async Task<string?> GetLeagueIdForAuthorizationAsync()
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

        // Try to get middleware-resolved league ID from request context items.
        if (httpContext.Items.TryGetValue("LeagueId", out var leagueIdItem))
        {
            return leagueIdItem as string;
        }

        // Try to get leagueKey from route values and convert to ID
        if (httpContext.Request.RouteValues.TryGetValue(AuthorizationConstants.RouteParams.LeagueKey, out var leagueKeyObj))
        {
            var leagueKey = leagueKeyObj?.ToString();
            if (!string.IsNullOrEmpty(leagueKey))
            {
                return await _leagueAuthService.GetLeagueIdByKeyAsync(leagueKey);
            }
        }

        return null;
    }
}

