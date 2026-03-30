using System.Security.Cryptography;
using GolfManager.Core.Entities;
using GolfManager.Data;
using GolfManager.Services.Auth;
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
    private readonly IPasswordHasher _passwordHasher;
    private readonly ILogger<LeagueService> _logger;

    public LeagueService(
        GolfManagerDbContext context,
        IPasswordHasher passwordHasher,
        ILogger<LeagueService> logger)
    {
        _context = context;
        _passwordHasher = passwordHasher;
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

    public async Task<List<LeagueMemberResponse>> GetLeagueMembersAsync(string leagueId)
    {
        var members = await _context.UserLeagues
            .IgnoreQueryFilters()
            .Include(ul => ul.User)
            .Where(ul => ul.LeagueId == leagueId && ul.IsActive)
            .OrderBy(ul => ul.User.LastName)
            .ThenBy(ul => ul.User.FirstName)
            .ToListAsync();

        var responses = new List<LeagueMemberResponse>();

        foreach (var member in members)
        {
            // Try to find associated player/golfer
            var golfer = await _context.LeagueGolfers
                .IgnoreQueryFilters()
                .Include(lg => lg.Golfer)
                .FirstOrDefaultAsync(lg => lg.LeagueId == leagueId && lg.Golfer.UserId == member.UserId && lg.IsActive);

            responses.Add(new LeagueMemberResponse
            {
                UserId = member.UserId,
                Email = member.User.Email,
                FirstName = member.User.FirstName,
                LastName = member.User.LastName,
                IsLeagueAdmin = member.IsLeagueAdmin,
                JoinedAt = member.JoinedAt,
                PlayerId = golfer?.GolferId,
                PlayerDisplayName = golfer?.Golfer?.DisplayName
            });
        }

        return responses;
    }

    public async Task<LeagueMemberResponse> AddLeagueMemberAsync(string leagueId, AddLeagueMemberRequest request, string currentUserId)
    {
        // Verify league exists
        var league = await _context.Leagues
            .FirstOrDefaultAsync(l => l.Id == leagueId && l.IsActive);

        if (league == null)
        {
            throw new KeyNotFoundException($"League with ID {leagueId} not found");
        }

        // Find user by email
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email && u.IsActive);

        // If user doesn't exist, create them
        if (user == null)
        {
            // Validate that FirstName and LastName are provided for new users
            if (string.IsNullOrWhiteSpace(request.FirstName) || string.IsNullOrWhiteSpace(request.LastName))
            {
                throw new InvalidOperationException($"User with email {request.Email} not found. FirstName and LastName are required to create a new user.");
            }

            // Generate a random password for the new user
            var randomPassword = GenerateRandomPassword();

            user = new User
            {
                Id = Guid.NewGuid().ToString(),
                Email = request.Email,
                PasswordHash = _passwordHasher.HashPassword(randomPassword),
                FirstName = request.FirstName,
                LastName = request.LastName,
                IsGlobalAdmin = false,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = currentUserId,
                IsActive = true
            };

            _context.Users.Add(user);

            _logger.LogInformation("Created new user {Email} ({UserId}) while adding to league {LeagueId}", user.Email, user.Id, leagueId);

            // TODO: Send invitation email with temporary password or password reset link
        }

        // Check if user is already a member
        var existingMembership = await _context.UserLeagues
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(ul => ul.UserId == user.Id && ul.LeagueId == leagueId);

        if (existingMembership != null)
        {
            if (existingMembership.IsActive)
            {
                throw new InvalidOperationException($"User {request.Email} is already a member of this league");
            }
            else
            {
                // Reactivate the membership
                existingMembership.IsActive = true;
                existingMembership.IsLeagueAdmin = request.IsLeagueAdmin;
                existingMembership.UpdatedAt = DateTime.UtcNow;
                existingMembership.UpdatedBy = currentUserId;
            }
        }
        else
        {
            // Create new membership
            var userLeague = new UserLeague
            {
                Id = Guid.NewGuid().ToString(),
                UserId = user.Id,
                LeagueId = leagueId,
                IsLeagueAdmin = request.IsLeagueAdmin,
                JoinedAt = DateTime.UtcNow,
                CreatedAt = DateTime.UtcNow,
                CreatedBy = currentUserId,
                IsActive = true
            };

            _context.UserLeagues.Add(userLeague);
        }

        await _context.SaveChangesAsync();

        _logger.LogInformation("Added user {UserId} to league {LeagueId} by {CurrentUserId}", user.Id, leagueId, currentUserId);

        return new LeagueMemberResponse
        {
            UserId = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsLeagueAdmin = request.IsLeagueAdmin,
            JoinedAt = DateTime.UtcNow
        };
    }

    /// <summary>
    /// Generate a random password for new users
    /// </summary>
    private static string GenerateRandomPassword()
    {
        const string chars = "ABCDEFGHJKLMNPQRSTUVWXYZabcdefghjkmnpqrstuvwxyz23456789!@#$%";
        var random = RandomNumberGenerator.Create();
        var bytes = new byte[16];
        random.GetBytes(bytes);

        return new string(bytes.Select(b => chars[b % chars.Length]).ToArray());
    }

    public async Task<bool> RemoveLeagueMemberAsync(string leagueId, string userId, string currentUserId)
    {
        var membership = await _context.UserLeagues
            .IgnoreQueryFilters()
            .FirstOrDefaultAsync(ul => ul.UserId == userId && ul.LeagueId == leagueId && ul.IsActive);

        if (membership == null)
        {
            return false;
        }

        // Prevent removing the last admin
        var adminCount = await _context.UserLeagues
            .IgnoreQueryFilters()
            .CountAsync(ul => ul.LeagueId == leagueId && ul.IsLeagueAdmin && ul.IsActive);

        if (membership.IsLeagueAdmin && adminCount <= 1)
        {
            throw new InvalidOperationException("Cannot remove the last admin from the league");
        }

        // Soft delete
        membership.IsActive = false;
        membership.UpdatedAt = DateTime.UtcNow;
        membership.UpdatedBy = currentUserId;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Removed user {UserId} from league {LeagueId} by {CurrentUserId}", userId, leagueId, currentUserId);

        return true;
    }

    public async Task<LeagueMemberResponse> UpdateLeagueMemberAsync(string leagueId, string userId, UpdateLeagueMemberRequest request, string currentUserId)
    {
        var membership = await _context.UserLeagues
            .IgnoreQueryFilters()
            .Include(ul => ul.User)
            .FirstOrDefaultAsync(ul => ul.UserId == userId && ul.LeagueId == leagueId && ul.IsActive);

        if (membership == null)
        {
            throw new KeyNotFoundException($"User {userId} is not a member of league {leagueId}");
        }

        // If demoting from admin, check that there's at least one other admin
        if (membership.IsLeagueAdmin && !request.IsLeagueAdmin)
        {
            var adminCount = await _context.UserLeagues
                .IgnoreQueryFilters()
                .CountAsync(ul => ul.LeagueId == leagueId && ul.IsLeagueAdmin && ul.IsActive);

            if (adminCount <= 1)
            {
                throw new InvalidOperationException("Cannot demote the last admin of the league");
            }
        }

        membership.IsLeagueAdmin = request.IsLeagueAdmin;
        membership.UpdatedAt = DateTime.UtcNow;
        membership.UpdatedBy = currentUserId;

        await _context.SaveChangesAsync();

        _logger.LogInformation("Updated user {UserId} role in league {LeagueId} by {CurrentUserId}", userId, leagueId, currentUserId);

        return new LeagueMemberResponse
        {
            UserId = membership.UserId,
            Email = membership.User.Email,
            FirstName = membership.User.FirstName,
            LastName = membership.User.LastName,
            IsLeagueAdmin = membership.IsLeagueAdmin,
            JoinedAt = membership.JoinedAt
        };
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
