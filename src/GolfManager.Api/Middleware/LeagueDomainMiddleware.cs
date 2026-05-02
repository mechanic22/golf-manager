using GolfManager.Core.Entities;
using GolfManager.Data;
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
        // Read league context from header (sent by frontend)
        var leagueKey = context.Request.Headers["X-League-Context"].FirstOrDefault();
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
            var membership = await dbContext.UserLeagues
                .Include(ul => ul.League)
                .FirstOrDefaultAsync(ul =>
                    ul.UserId == userId &&
                    ul.League.Key == leagueKey &&
                    ul.IsActive &&
                    !ul.League.IsDeleted);

            if (membership != null)
            {
                context.Items["LeagueId"] = membership.LeagueId;
                context.Items["LeagueKey"] = membership.League.Key;
                context.Items["IsLeagueAdmin"] = membership.IsLeagueAdmin;

                _logger.LogInformation(
                    "League context validated: User {UserId} accessing league {LeagueKey}",
                    userId, leagueKey);
            }
            else if (leagueContextHeaderProvided)
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
            else if (hostResolvedLeague != null)
            {
                context.Items["LeagueId"] = hostResolvedLeague.Id;
                context.Items["LeagueKey"] = hostResolvedLeague.Key;
                _logger.LogDebug("Host-based league context set for league {LeagueKey}", hostResolvedLeague.Key);
            }
        }
        else if (!string.IsNullOrEmpty(leagueKey) && string.IsNullOrEmpty(userId))
        {
            _logger.LogDebug("League context provided but user not authenticated: {LeagueKey}", leagueKey);
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
