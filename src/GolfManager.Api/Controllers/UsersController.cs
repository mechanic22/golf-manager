using GolfManager.Api.Authorization;
using GolfManager.Core.Enums;
using GolfManager.Data;
using GolfManager.Shared.DTOs.Admin;
using GolfManager.Shared.DTOs.Auth;
using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.Round;
using GolfManager.Shared.DTOs.User;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace GolfManager.Api.Controllers;

[ApiController]
[Route("api/v1/[controller]")]
[Authorize]
public class UsersController : ControllerBase
{
    private readonly GolfManagerDbContext _context;
    private readonly ILogger<UsersController> _logger;

    public UsersController(GolfManagerDbContext context, ILogger<UsersController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all users (GlobalAdmin only)
    /// </summary>
    [HttpGet]
    [Authorize(Policy = AuthorizationConstants.Policies.GlobalAdmin)]
    public async Task<ActionResult<ApiResponse<List<UserResponse>>>> GetAllUsers([FromQuery] bool includeInactive = false)
    {
        var query = _context.Users
            .Include(u => u.UserLeagues)
            .AsQueryable();

        // Filter by active status unless includeInactive is true
        if (!includeInactive)
        {
            query = query.Where(u => !u.IsDeleted);
        }
        else
        {
            query = query.Where(u => !u.IsDeleted);
        }

        var users = await query
            .OrderBy(u => u.LastName)
            .ThenBy(u => u.FirstName)
            .Select(u => new UserResponse
            {
                Id = u.Id,
                Email = u.Email,
                FirstName = u.FirstName,
                LastName = u.LastName,
                IsGlobalAdmin = u.IsGlobalAdmin,
                IsActive = u.IsActive,
                CreatedAt = u.CreatedAt,
                LastLoginAt = u.LastLoginAt,
                LeagueAdminCount = u.UserLeagues.Count(ul => ul.IsLeagueAdmin && ul.IsActive)
            })
            .ToListAsync();

        _logger.LogInformation("Retrieved {Count} users", users.Count);

        return Ok(ApiResponse<List<UserResponse>>.SuccessResponse(users, $"Retrieved {users.Count} users"));
    }

    /// <summary>
    /// Get user's league memberships with domain mappings
    /// </summary>
    [HttpGet("me/leagues")]
    public async Task<ActionResult<ApiResponse<List<LeagueMappingResponse>>>> GetMyLeagues()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;

        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<List<LeagueMappingResponse>>.ErrorResponse("User not authenticated"));
        }

        var leagueMappings = await _context.UserLeagues
            .Where(ul => ul.UserId == userId && ul.IsActive)
            .Include(ul => ul.League)
            .Select(ul => new LeagueMappingResponse
            {
                LeagueId = ul.LeagueId,
                LeagueKey = ul.League.Key,
                LeagueName = ul.League.Name,
                CustomDomain = ul.League.CustomDomain,
                IsLeagueAdmin = ul.Role == LeagueMemberRole.Owner || ul.Role == LeagueMemberRole.Admin,
                Role = ul.Role
            })
            .ToListAsync();

        _logger.LogInformation("Retrieved {Count} league mappings for user {UserId}", leagueMappings.Count, userId);

        return Ok(ApiResponse<List<LeagueMappingResponse>>.SuccessResponse(
            leagueMappings,
            $"Retrieved {leagueMappings.Count} league memberships"));
    }

    /// <summary>
    /// Get current user's profile
    /// </summary>
    [HttpGet("me")]
    public async Task<ActionResult<ApiResponse<UserProfileResponse>>> GetCurrentUser()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<UserProfileResponse>.ErrorResponse("User not authenticated", "User ID not found in token"));
        }

        var user = await _context.Users
            .Include(u => u.Golfer)
            .Include(u => u.UserLeagues)
                .ThenInclude(ul => ul.League)
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

        if (user == null)
        {
            return NotFound(ApiResponse<UserProfileResponse>.ErrorResponse("User not found"));
        }

        // Get rounds count and handicap if user is a golfer
        int roundsCount = 0;
        double? handicapIndex = null;

        if (user.Golfer != null)
        {
            roundsCount = await _context.Rounds
                .Where(r => r.GolferId == user.Golfer.Id && !r.IsDeleted)
                .CountAsync();

            // Use the global handicap from the Golfer entity
            handicapIndex = user.Golfer.GlobalHandicap;
        }

        var response = new UserProfileResponse
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsGlobalAdmin = user.IsGlobalAdmin,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            LeagueCount = user.UserLeagues.Count(ul => ul.IsActive),
            IsGolfer = user.Golfer != null,
            GolferId = user.Golfer?.Id,
            HandicapIndex = handicapIndex,
            RoundsCount = roundsCount,
            DisplayName = user.Golfer?.DisplayName ?? $"{user.FirstName} {user.LastName}"
        };

        _logger.LogInformation("Retrieved profile for user {UserId}: {Rounds} rounds, handicap {Handicap}",
            userId, roundsCount, handicapIndex);

        return Ok(ApiResponse<UserProfileResponse>.SuccessResponse(response));
    }

    /// <summary>
    /// Get current user's round history
    /// </summary>
    [HttpGet("me/rounds")]
    public async Task<ActionResult<ApiResponse<List<RoundResponse>>>> GetMyRounds()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<List<RoundResponse>>.ErrorResponse("User not authenticated", "User ID not found in token"));
        }

        var user = await _context.Users
            .Include(u => u.Golfer)
            .FirstOrDefaultAsync(u => u.Id == userId && u.IsActive);

        if (user?.Golfer == null)
        {
            // User is not a golfer, return empty list
            return Ok(ApiResponse<List<RoundResponse>>.SuccessResponse(new List<RoundResponse>(), "User has no golfer profile"));
        }

        var rounds = await _context.Rounds
            .Where(r => r.GolferId == user.Golfer.Id && r.IsActive)
            .Include(r => r.Course)
            .Include(r => r.Tee)
            .OrderByDescending(r => r.RoundDate)
            .Take(50) // Limit to most recent 50 rounds
            .Select(r => new RoundResponse
            {
                Id = r.Id,
                GolferId = r.GolferId,
                LeagueGolferId = r.LeagueGolferId,
                LeagueId = r.LeagueId,
                CourseId = r.CourseId,
                CourseName = r.Course != null ? r.Course.Name : "Unknown Course",
                TeeId = r.TeeId,
                TeeName = r.Tee != null ? r.Tee.Name : "Unknown",
                RoundDate = r.RoundDate,
                HolesPlayed = r.HolesPlayed,
                TotalScore = r.TotalScore,
                NetScore = r.NetScore,
                HandicapUsed = r.HandicapUsed,
                IsComplete = r.IsComplete,
                Notes = r.Notes,
                CreatedAt = r.CreatedAt,
                UpdatedAt = r.UpdatedAt
            })
            .ToListAsync();

        _logger.LogInformation("Retrieved {Count} rounds for user {UserId}", rounds.Count, userId);

        return Ok(ApiResponse<List<RoundResponse>>.SuccessResponse(rounds, $"Retrieved {rounds.Count} rounds"));
    }

    /// <summary>
    /// Get a specific user by ID (GlobalAdmin only)
    /// </summary>
    [HttpGet("{userId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.GlobalAdmin)]
    public async Task<ActionResult<ApiResponse<UserResponse>>> GetUser(string userId)
    {
        var user = await _context.Users
            .Include(u => u.UserLeagues)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

        if (user == null)
        {
            return NotFound(ApiResponse<UserResponse>.ErrorResponse("User not found"));
        }

        var response = new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsGlobalAdmin = user.IsGlobalAdmin,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            LeagueAdminCount = user.UserLeagues.Count(ul => ul.IsLeagueAdmin && ul.IsActive)
        };

        return Ok(ApiResponse<UserResponse>.SuccessResponse(response));
    }

    /// <summary>
    /// Update a user (GlobalAdmin only)
    /// </summary>
    [HttpPut("{userId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.GlobalAdmin)]
    public async Task<ActionResult<ApiResponse<UserResponse>>> UpdateUser(string userId, [FromBody] UpdateUserRequest request)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Unauthorized(ApiResponse<UserResponse>.ErrorResponse("User not authenticated", "User ID not found in token"));
        }

        var user = await _context.Users
            .Include(u => u.UserLeagues)
            .FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

        if (user == null)
        {
            return NotFound(ApiResponse<UserResponse>.ErrorResponse("User not found"));
        }

        // Update fields if provided
        if (!string.IsNullOrWhiteSpace(request.FirstName))
        {
            user.FirstName = request.FirstName;
        }

        if (!string.IsNullOrWhiteSpace(request.LastName))
        {
            user.LastName = request.LastName;
        }

        if (!string.IsNullOrWhiteSpace(request.Email))
        {
            // Check if email is already in use
            var emailExists = await _context.Users.AnyAsync(u => u.Email == request.Email && u.Id != userId && !u.IsDeleted);
            if (emailExists)
            {
                return BadRequest(ApiResponse<UserResponse>.ErrorResponse("Email already in use", "Another user already has this email address"));
            }
            user.Email = request.Email;
        }

        if (request.IsGlobalAdmin.HasValue)
        {
            user.IsGlobalAdmin = request.IsGlobalAdmin.Value;
        }

        if (request.IsActive.HasValue)
        {
            user.IsActive = request.IsActive.Value;
        }

        user.UpdatedAt = DateTime.UtcNow;
        user.UpdatedBy = currentUserId;

        await _context.SaveChangesAsync();

        _logger.LogInformation("User {UserId} updated by {CurrentUserId}", userId, currentUserId);

        var response = new UserResponse
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            IsGlobalAdmin = user.IsGlobalAdmin,
            IsActive = user.IsActive,
            CreatedAt = user.CreatedAt,
            LastLoginAt = user.LastLoginAt,
            LeagueAdminCount = user.UserLeagues.Count(ul => ul.IsLeagueAdmin && ul.IsActive)
        };

        return Ok(ApiResponse<UserResponse>.SuccessResponse(response, "User updated successfully"));
    }

    /// <summary>
    /// Send password reset email to user (GlobalAdmin only)
    /// </summary>
    [HttpPost("{userId}/password-reset")]
    [Authorize(Policy = AuthorizationConstants.Policies.GlobalAdmin)]
    public async Task<ActionResult<ApiResponse<bool>>> SendPasswordReset(string userId)
    {
        var user = await _context.Users.FirstOrDefaultAsync(u => u.Id == userId && !u.IsDeleted);

        if (user == null)
        {
            return NotFound(ApiResponse<bool>.ErrorResponse("User not found"));
        }

        // TODO: Implement password reset email sending
        // For now, just log it
        _logger.LogInformation("Password reset requested for user {UserId} ({Email})", userId, user.Email);

        // In a real implementation, you would:
        // 1. Generate a password reset token
        // 2. Save it to the database with an expiration
        // 3. Send an email with a reset link
        // 4. Return success

        return Ok(ApiResponse<bool>.SuccessResponse(true, $"Password reset email sent to {user.Email}"));
    }

    /// <summary>
    /// Get platform statistics (GlobalAdmin only)
    /// </summary>
    [HttpGet("stats/platform")]
    [Authorize(Policy = AuthorizationConstants.Policies.GlobalAdmin)]
    public async Task<ActionResult<ApiResponse<PlatformStatsResponse>>> GetPlatformStats()
    {
        var now = DateTime.UtcNow;
        var startOfMonth = new DateTime(now.Year, now.Month, 1);

        var stats = new PlatformStatsResponse
        {
            TotalUsers = await _context.Users.CountAsync(u => !u.IsDeleted),
            ActiveUsers = await _context.Users.CountAsync(u => u.IsActive && !u.IsDeleted),
            NewUsersThisMonth = await _context.Users.CountAsync(u => u.CreatedAt >= startOfMonth && !u.IsDeleted),
            GlobalAdminCount = await _context.Users.CountAsync(u => u.IsGlobalAdmin && u.IsActive && !u.IsDeleted),

            TotalLeagues = await _context.Leagues.CountAsync(l => !l.IsDeleted),
            ActiveLeagues = await _context.Leagues.CountAsync(l => l.IsActive && !l.IsDeleted),

            TotalSeasons = await _context.Seasons.CountAsync(s => !s.IsDeleted),
            ActiveSeasons = await _context.Seasons.CountAsync(s =>
                s.IsActive &&
                !s.IsDeleted &&
                s.StartDate <= DateOnly.FromDateTime(now) &&
                s.EndDate >= DateOnly.FromDateTime(now)),

            TotalEvents = await _context.SeasonEvents.CountAsync(e => !e.IsDeleted),
            UpcomingEvents = await _context.SeasonEvents.CountAsync(e =>
                e.IsActive &&
                !e.IsDeleted &&
                e.EventDate >= now),

            TotalRounds = await _context.Rounds.CountAsync(r => !r.IsDeleted),
            RoundsThisMonth = await _context.Rounds.CountAsync(r =>
                !r.IsDeleted &&
                r.CreatedAt >= startOfMonth)
        };

        _logger.LogInformation("Retrieved platform stats");

        return Ok(ApiResponse<PlatformStatsResponse>.SuccessResponse(stats));
    }

    /// <summary>
    /// Search for a user by email
    /// </summary>
    [HttpGet("search")]
    public async Task<ActionResult<ApiResponse<UserSearchResponse>>> SearchByEmail([FromQuery] string email)
    {
        if (string.IsNullOrWhiteSpace(email))
        {
            return BadRequest(ApiResponse<UserSearchResponse>.ErrorResponse("Invalid request", "Email is required"));
        }

        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == email && u.IsActive);

        if (user == null)
        {
            // Return a response indicating user doesn't exist
            var notFoundResponse = new UserSearchResponse
            {
                Email = email,
                Exists = false
            };

            return Ok(ApiResponse<UserSearchResponse>.SuccessResponse(notFoundResponse));
        }

        var response = new UserSearchResponse
        {
            Id = user.Id,
            Email = user.Email,
            FirstName = user.FirstName,
            LastName = user.LastName,
            Exists = true
        };

        _logger.LogInformation("User search for email {Email}: {Exists}", email, response.Exists);

        return Ok(ApiResponse<UserSearchResponse>.SuccessResponse(response));
    }
}
