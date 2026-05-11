using GolfManager.Data;
using GolfManager.Core.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GolfManager.Services.League;

/// <summary>
/// Service for checking league membership and authorization
/// </summary>
public class LeagueAuthorizationService : ILeagueAuthorizationService
{
    private readonly GolfManagerDbContext _context;
    private readonly ILogger<LeagueAuthorizationService> _logger;

    public LeagueAuthorizationService(
        GolfManagerDbContext context,
        ILogger<LeagueAuthorizationService> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Check if a user is a member of a league
    /// </summary>
    public async Task<bool> IsUserMemberOfLeagueAsync(string userId, string leagueId)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(leagueId))
        {
            return false;
        }

        // Use IgnoreQueryFilters() because we're checking authorization BEFORE setting the tenant context
        var membership = await _context.UserLeagues
            .IgnoreQueryFilters()
            .Where(ul => ul.UserId == userId && ul.LeagueId == leagueId && ul.IsActive)
            .FirstOrDefaultAsync();

        return membership != null;
    }

    /// <summary>
    /// Check if a user is an admin of a league
    /// </summary>
    public async Task<bool> IsUserLeagueAdminAsync(string userId, string leagueId)
    {
        if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(leagueId))
        {
            return false;
        }

        // Use IgnoreQueryFilters() because we're checking authorization BEFORE setting the tenant context
        var membership = await _context.UserLeagues
            .IgnoreQueryFilters()
            .Where(ul =>
                ul.UserId == userId
                && ul.LeagueId == leagueId
                && ul.IsActive
                && (ul.Role == LeagueMemberRole.Owner || ul.Role == LeagueMemberRole.Admin))
            .FirstOrDefaultAsync();

        return membership != null;
    }

    /// <summary>
    /// Get the league ID from a league key
    /// </summary>
    public async Task<string?> GetLeagueIdByKeyAsync(string leagueKey)
    {
        if (string.IsNullOrEmpty(leagueKey))
        {
            return null;
        }

        var league = await _context.Leagues
            .Where(l => l.Key == leagueKey && l.IsActive)
            .Select(l => l.Id)
            .FirstOrDefaultAsync();

        return league;
    }
}

