using GolfManager.Core.Entities;
using GolfManager.Data;
using GolfManager.Shared.DTOs.League;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace GolfManager.Services.League;

/// <summary>
/// Service for managing leagues
/// </summary>
public class LeagueService : ILeagueService
{
    private readonly GolfManagerDbContext _context;
    private readonly ILogger<LeagueService> _logger;

    public LeagueService(
        GolfManagerDbContext context,
        ILogger<LeagueService> logger)
    {
        _context = context;
        _logger = logger;
    }

    public async Task<List<LeagueResponse>> GetUserLeaguesAsync(string userId)
    {
        // Use IgnoreQueryFilters() because we're explicitly filtering by userId
        var userLeagues = await _context.UserLeagues
            .IgnoreQueryFilters()
            .Include(ul => ul.League)
            .Where(ul => ul.UserId == userId && ul.IsActive && ul.League.IsActive)
            .Select(ul => ul.League)
            .ToListAsync();

        var responses = new List<LeagueResponse>();
        foreach (var league in userLeagues)
        {
            responses.Add(await MapToResponseAsync(league, userId));
        }

        return responses;
    }

    public async Task<LeagueResponse?> GetLeagueByIdAsync(string leagueId, string? userId = null)
    {
        var league = await _context.Leagues
            .FirstOrDefaultAsync(l => l.Id == leagueId && l.IsActive);

        if (league == null)
        {
            return null;
        }

        return await MapToResponseAsync(league, userId);
    }

    public async Task<LeagueResponse?> GetLeagueByKeyAsync(string leagueKey, string? userId = null)
    {
        var league = await _context.Leagues
            .FirstOrDefaultAsync(l => l.Key == leagueKey && l.IsActive);

        if (league == null)
        {
            return null;
        }

        return await MapToResponseAsync(league, userId);
    }

    public async Task<LeagueResponse> CreateLeagueAsync(CreateLeagueRequest request, string userId)
    {
        // Check if league key already exists
        var existingLeague = await _context.Leagues
            .FirstOrDefaultAsync(l => l.Key == request.Key);

        if (existingLeague != null)
        {
            throw new InvalidOperationException($"League with key '{request.Key}' already exists");
        }

        // Create the league
        var league = new Core.Entities.League
        {
            Id = Guid.NewGuid().ToString(),
            Key = request.Key,
            Name = request.Name,
            Description = request.Description,
            LogoUrl = request.LogoUrl,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId,
            IsActive = true
        };

        _context.Leagues.Add(league);

        // Add the creator as a league admin
        var userLeague = new UserLeague
        {
            Id = Guid.NewGuid().ToString(),
            UserId = userId,
            LeagueId = league.Id,
            IsLeagueAdmin = true,
            JoinedAt = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow,
            CreatedBy = userId,
            IsActive = true
        };

        _context.UserLeagues.Add(userLeague);

        await _context.SaveChangesAsync();

        _logger.LogInformation("Created league {LeagueKey} ({LeagueId}) by user {UserId}", league.Key, league.Id, userId);

        return await MapToResponseAsync(league, userId);
    }

    public async Task<LeagueResponse> UpdateLeagueAsync(string leagueId, UpdateLeagueRequest request, string userId)
    {
        var league = await _context.Leagues
            .FirstOrDefaultAsync(l => l.Id == leagueId && l.IsActive);

        if (league == null)
        {
            throw new KeyNotFoundException($"League with ID {leagueId} not found");
        }

        // Update fields if provided
        if (!string.IsNullOrEmpty(request.Name))
        {
            league.Name = request.Name;
        }

        if (request.Description != null)
        {
            league.Description = request.Description;
        }

        if (request.LogoUrl != null)
        {
            league.LogoUrl = request.LogoUrl;
        }

        league.UpdatedAt = DateTime.UtcNow;
        league.UpdatedBy = userId;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated league {LeagueId} by user {UserId}", leagueId, userId);

        return await MapToResponseAsync(league, userId);
    }

    public async Task<bool> DeleteLeagueAsync(string leagueId, string userId)
    {
        var league = await _context.Leagues
            .FirstOrDefaultAsync(l => l.Id == leagueId && l.IsActive);

        if (league == null)
        {
            return false;
        }

        // Soft delete
        league.IsActive = false;
        league.UpdatedAt = DateTime.UtcNow;
        league.UpdatedBy = userId;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Deleted league {LeagueId} by user {UserId}", leagueId, userId);

        return true;
    }

    private async Task<LeagueResponse> MapToResponseAsync(Core.Entities.League league, string? userId = null)
    {
        // Use IgnoreQueryFilters() for counts
        var memberCount = await _context.UserLeagues
            .IgnoreQueryFilters()
            .CountAsync(ul => ul.LeagueId == league.Id && ul.IsActive);

        var playerCount = await _context.LeagueGolfers
            .IgnoreQueryFilters()
            .CountAsync(lg => lg.LeagueId == league.Id && lg.IsActive);

        var seasonCount = await _context.Seasons
            .IgnoreQueryFilters()
            .CountAsync(s => s.LeagueId == league.Id && s.IsActive);

        bool isCurrentUserAdmin = false;
        if (!string.IsNullOrEmpty(userId))
        {
            isCurrentUserAdmin = await _context.UserLeagues
                .IgnoreQueryFilters()
                .AnyAsync(ul => ul.UserId == userId && ul.LeagueId == league.Id && ul.IsLeagueAdmin && ul.IsActive);
        }

        return new LeagueResponse
        {
            Id = league.Id,
            Key = league.Key,
            Name = league.Name,
            Description = league.Description,
            LogoUrl = league.LogoUrl,
            ActiveSeasonId = league.ActiveSeasonId,
            MemberCount = memberCount,
            PlayerCount = playerCount,
            SeasonCount = seasonCount,
            IsCurrentUserAdmin = isCurrentUserAdmin,
            CreatedAt = league.CreatedAt,
            UpdatedAt = league.UpdatedAt
        };
    }
}
