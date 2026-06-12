using GolfManager.Core.Entities;
using GolfManager.Data;
using GolfManager.Core.Enums;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GolfManager.Api.Middleware;

/// <summary>
/// Middleware to validate league context from client header
/// </summary>
public class LeagueContextMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<LeagueContextMiddleware> _logger;

    public LeagueContextMiddleware(RequestDelegate next, ILogger<LeagueContextMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context, GolfManagerDbContext dbContext)
    {
        var requestPath = context.Request.Path.Value ?? string.Empty;
        var isAuthEndpoint = requestPath.StartsWith("/api/v1/auth", StringComparison.OrdinalIgnoreCase);

        // Read league context from header (sent by frontend)
        var leagueKey = context.Request.Headers["X-League-Context"].FirstOrDefault()?.Trim();
        var userId = context.User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var leagueContextHeaderProvided = context.Request.Headers.ContainsKey("X-League-Context");

        _logger.LogDebug("League context header: {LeagueKey}, User: {UserId}", leagueKey ?? "none", userId ?? "anonymous");

        // If there is no explicit league header, try resolving from the custom domain host
        League? hostResolvedLeague = null;
        if (string.IsNullOrEmpty(leagueKey))
        {
            var host = context.Request.Host.Host?.Trim().ToLowerInvariant();
            if (!string.IsNullOrEmpty(host))
            {
                hostResolvedLeague = await dbContext.Leagues
                    .AsNoTracking()
                    .FirstOrDefaultAsync(l =>
                        !l.IsDeleted &&
                        l.UseCustomDomain &&
                        l.CustomDomainVerifiedAt != null &&
                        l.CustomDomain != null &&
                        l.CustomDomain.ToLower() == host);

                if (hostResolvedLeague != null)
                {
                    leagueKey = hostResolvedLeague.Key;
                    _logger.LogInformation("Resolved league context from host {Host} => {LeagueKey}", host, leagueKey);
                }
                else
                {
                    _logger.LogDebug("No league resolved from host {Host}", host);
                }
            }
        }

        if (!string.IsNullOrEmpty(leagueKey))
        {
            // For authenticated users, validate membership against the explicit league header.
            if (!string.IsNullOrEmpty(userId))
            {
                var normalizedLeagueKey = leagueKey.ToLowerInvariant();
                var membership = await dbContext.UserLeagues
                    .IgnoreQueryFilters()
                    .Include(ul => ul.League)
                    .FirstOrDefaultAsync(ul =>
                        ul.UserId == userId &&
                        !ul.IsDeleted &&
                        ul.League.IsActive &&
                        ul.League.Key.ToLower() == normalizedLeagueKey &&
                        ul.IsActive &&
                        !ul.League.IsDeleted);

                if (membership != null)
                {
                    context.Items["LeagueId"] = membership.LeagueId;
                    context.Items["LeagueKey"] = membership.League.Key;
                    context.Items["IsLeagueAdmin"] = membership.Role == LeagueMemberRole.Owner || membership.Role == LeagueMemberRole.Admin;

                    _logger.LogInformation(
                        "League context validated: User {UserId} accessing league {LeagueKey}",
                        userId, leagueKey);
                }
                else
                {
                    // Global admins can access any league even without a UserLeague record.
                    var isGlobalAdmin = context.User.FindFirst("IsGlobalAdmin")?.Value == "true";
                    if (isGlobalAdmin)
                    {
                        var targetLeague = hostResolvedLeague ?? await dbContext.Leagues
                            .IgnoreQueryFilters()
                            .AsNoTracking()
                            .FirstOrDefaultAsync(l => !l.IsDeleted && l.Key.ToLower() == normalizedLeagueKey);

                        if (targetLeague != null)
                        {
                            context.Items["LeagueId"] = targetLeague.Id;
                            context.Items["LeagueKey"] = targetLeague.Key;
                            context.Items["IsLeagueAdmin"] = true;
                            _logger.LogInformation(
                                "Global admin {UserId} granted access to league {LeagueKey}",
                                userId, leagueKey);
                        }
                    }
                    else if (hostResolvedLeague != null)
                    {
                        context.Items["LeagueId"] = hostResolvedLeague.Id;
                        context.Items["LeagueKey"] = hostResolvedLeague.Key;
                        _logger.LogDebug("Host-based league context set for league {LeagueKey}", hostResolvedLeague.Key);
                    }
                    else if (leagueContextHeaderProvided && !isAuthEndpoint)
                    {
                        _logger.LogWarning(
                            "Forbidden: User {UserId} attempted to access league {LeagueKey} without membership",
                            userId, leagueKey);

                        context.Response.StatusCode = 403;
                        await context.Response.WriteAsJsonAsync(new
                        {
                            error = "Forbidden",
                            message = "You are not a member of this league"
                        });
                        return;
                    }
                }
            }
            else
            {
                // Anonymous/public endpoints (e.g., /api/v1/leagues/by-key/{key})
                // must not be blocked just because a league header is present.
                _logger.LogDebug("League context provided but user not authenticated: {LeagueKey}", leagueKey);
            }
        }

        await _next(context);
    }
}

/// <summary>
/// Extension method to register the middleware
/// </summary>
public static class LeagueContextMiddlewareExtensions
{
    public static IApplicationBuilder UseLeagueContext(this IApplicationBuilder builder)
    {
        return builder.UseMiddleware<LeagueContextMiddleware>();
    }
}
