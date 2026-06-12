using System.Security.Claims;
using GolfManager.Api.Authorization;
using GolfManager.Data;
using GolfManager.Services.League;
using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.League;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace GolfManager.Api.Controllers;

/// <summary>
/// Controller for managing leagues
/// </summary>
[ApiController]
[Route("api/v1/leagues")]
[Authorize]
public class LeaguesController : ControllerBase
{
    private readonly ILeagueService _leagueService;
    private readonly GolfManagerDbContext _context;
    private readonly ILogger<LeaguesController> _logger;

    public LeaguesController(
        ILeagueService leagueService,
        GolfManagerDbContext context,
        ILogger<LeaguesController> logger)
    {
        _leagueService = leagueService;
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Get all leagues for the current user
    /// </summary>
    [HttpGet]
    public async Task<ActionResult<ApiResponse<List<LeagueResponse>>>> GetUserLeagues()
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<List<LeagueResponse>>.ErrorResponse("User not authenticated", "User ID not found in token"));
        }

        var leagues = await _leagueService.GetUserLeaguesAsync(userId);
        return Ok(ApiResponse<List<LeagueResponse>>.SuccessResponse(leagues, $"Retrieved {leagues.Count} leagues"));
    }

    /// <summary>
    /// Get a specific league by ID
    /// </summary>
    [HttpGet("{leagueId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueMember)]
    public async Task<ActionResult<ApiResponse<LeagueResponse>>> GetLeagueById(string leagueId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var league = await _leagueService.GetLeagueByIdAsync(leagueId, userId);

        if (league == null)
        {
            return NotFound(ApiResponse<LeagueResponse>.ErrorResponse("League not found", $"League with ID {leagueId} not found"));
        }

        return Ok(ApiResponse<LeagueResponse>.SuccessResponse(league, "League retrieved successfully"));
    }

    /// <summary>
    /// Discover publicly listed leagues (anonymous, opt-in leagues only)
    /// </summary>
    [HttpGet("discover")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<List<LeagueResponse>>>> DiscoverLeagues([FromQuery] string? search = null)
    {
        var leagues = await _leagueService.GetPublicLeaguesAsync(search);
        return Ok(ApiResponse<List<LeagueResponse>>.SuccessResponse(leagues, $"Found {leagues.Count} leagues"));
    }

    /// <summary>
    /// Get a specific league by key
    /// </summary>
    [HttpGet("by-key/{leagueKey}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LeagueResponse>>> GetLeagueByKey(
        string leagueKey,
        [FromQuery] string? anonymousAccessPassword = null)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrWhiteSpace(anonymousAccessPassword)
            && Request.Headers.TryGetValue("X-League-Anonymous-Password", out var headerPassword))
        {
            anonymousAccessPassword = headerPassword.FirstOrDefault();
        }

        var league = await _leagueService.GetLeagueByKeyAsync(leagueKey, userId, anonymousAccessPassword);

        if (league == null)
        {
            return NotFound(ApiResponse<LeagueResponse>.ErrorResponse("League not found", $"League with key '{leagueKey}' not found"));
        }

        return Ok(ApiResponse<LeagueResponse>.SuccessResponse(league, "League retrieved successfully"));
    }

    /// <summary>
    /// Get read-only team standings for a guest session.
    /// League context is derived from the guest cookie claims — no route parameter needed.
    /// </summary>
    [HttpGet("guest/standings")]
    [Authorize(Policy = AuthorizationConstants.Policies.GuestLeagueViewer)]
    public async Task<ActionResult<ApiResponse<GuestStandingsResponse>>> GetGuestStandings()
    {
        var leagueId = User.FindFirst(AuthorizationConstants.Claims.LeagueId)?.Value;
        if (string.IsNullOrEmpty(leagueId))
            return BadRequest(ApiResponse<GuestStandingsResponse>.ErrorResponse("No league context in guest session"));

        var league = await _context.Leagues
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(l => l.Id == leagueId && !l.IsDeleted);

        if (league == null)
            return NotFound(ApiResponse<GuestStandingsResponse>.ErrorResponse("League not found"));

        if (string.IsNullOrEmpty(league.ActiveSeasonId))
        {
            return Ok(ApiResponse<GuestStandingsResponse>.SuccessResponse(new GuestStandingsResponse
            {
                LeagueName = league.Name,
                LogoUrl = league.LogoUrl
            }));
        }

        var season = await _context.Seasons
            .IgnoreQueryFilters()
            .AsNoTracking()
            .FirstOrDefaultAsync(s => s.Id == league.ActiveSeasonId && !s.IsDeleted);

        var teams = await _context.SeasonTeams
            .IgnoreQueryFilters()
            .AsNoTracking()
            .Where(t => t.SeasonId == league.ActiveSeasonId && t.LeagueId == leagueId && !t.IsDeleted)
            .OrderByDescending(t => t.SeasonPoints ?? 0)
            .ThenByDescending(t => t.Wins)
            .ThenBy(t => t.Losses)
            .ToListAsync();

        var rows = teams.Select((t, i) => new GuestTeamStandingRow
        {
            Rank = i + 1,
            Name = t.Name,
            Wins = t.Wins,
            Losses = t.Losses,
            Ties = t.Ties,
            SeasonPoints = t.SeasonPoints
        }).ToList();

        _logger.LogInformation("Guest standings served for league {LeagueId}", leagueId);

        return Ok(ApiResponse<GuestStandingsResponse>.SuccessResponse(new GuestStandingsResponse
        {
            LeagueName = league.Name,
            LogoUrl = league.LogoUrl,
            SeasonName = season?.Name,
            Teams = rows
        }));
    }

    /// <summary>
    /// Verify anonymous/public access password for a league.
    /// </summary>
    [HttpPost("by-key/{leagueKey}/anonymous-access")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<bool>>> VerifyAnonymousAccess(
        string leagueKey,
        [FromBody] VerifyAnonymousAccessRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Password))
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse("Password is required"));
        }

        var isValid = await _leagueService.VerifyAnonymousAccessAsync(leagueKey, request.Password);
        if (!isValid)
        {
            return Unauthorized(ApiResponse<bool>.ErrorResponse("Invalid password"));
        }

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Anonymous access verified"));
    }

    /// <summary>
    /// Create a new league
    /// </summary>
    [HttpPost]
    public async Task<ActionResult<ApiResponse<LeagueResponse>>> CreateLeague([FromBody] CreateLeagueRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<LeagueResponse>.ErrorResponse("User not authenticated", "User ID not found in token"));
        }

        try
        {
            var league = await _leagueService.CreateLeagueAsync(request, userId);
            _logger.LogInformation("League {LeagueKey} created by user {UserId}", league.Key, userId);

            return CreatedAtAction(
                nameof(GetLeagueById),
                new { leagueId = league.Id },
                ApiResponse<LeagueResponse>.SuccessResponse(league, "League created successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<LeagueResponse>.ErrorResponse("Invalid operation", ex.Message));
        }
    }

    /// <summary>
    /// Update a league
    /// </summary>
    [HttpPut("{leagueId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueAdmin)]
    public async Task<ActionResult<ApiResponse<LeagueResponse>>> UpdateLeague(string leagueId, [FromBody] UpdateLeagueRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<LeagueResponse>.ErrorResponse("User not authenticated", "User ID not found in token"));
        }

        try
        {
            var league = await _leagueService.UpdateLeagueAsync(leagueId, request, userId);
            _logger.LogInformation("League {LeagueId} updated by user {UserId}", leagueId, userId);

            return Ok(ApiResponse<LeagueResponse>.SuccessResponse(league, "League updated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<LeagueResponse>.ErrorResponse("League not found", ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<LeagueResponse>.ErrorResponse("Invalid operation", ex.Message));
        }
    }

    /// <summary>
    /// Verify a league custom domain
    /// </summary>
    [HttpPost("{leagueId}/custom-domain/verify")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueAdmin)]
    public async Task<ActionResult<ApiResponse<LeagueResponse>>> VerifyLeagueCustomDomain(string leagueId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<LeagueResponse>.ErrorResponse("User not authenticated", "User ID not found in token"));
        }

        try
        {
            var league = await _leagueService.VerifyCustomDomainAsync(leagueId, userId);
            return Ok(ApiResponse<LeagueResponse>.SuccessResponse(league, "Custom domain verified successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<LeagueResponse>.ErrorResponse("League not found", ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<LeagueResponse>.ErrorResponse("Domain verification failed", ex.Message));
        }
    }

    /// <summary>
    /// Delete a league
    /// </summary>
    [HttpDelete("{leagueId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueAdmin)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteLeague(string leagueId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<bool>.ErrorResponse("User not authenticated", "User ID not found in token"));
        }

        var result = await _leagueService.DeleteLeagueAsync(leagueId, userId);

        if (!result)
        {
            return NotFound(ApiResponse<bool>.ErrorResponse("League not found", $"League with ID {leagueId} not found"));
        }

        _logger.LogInformation("League {LeagueId} deleted by user {UserId}", leagueId, userId);
        return Ok(ApiResponse<bool>.SuccessResponse(true, "League deleted successfully"));
    }

    /// <summary>
    /// Get all members of a league
    /// </summary>
    [HttpGet("{leagueId}/members")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueMember)]
    public async Task<ActionResult<ApiResponse<List<LeagueMemberResponse>>>> GetLeagueMembers(string leagueId)
    {
        var members = await _leagueService.GetLeagueMembersAsync(leagueId);
        return Ok(ApiResponse<List<LeagueMemberResponse>>.SuccessResponse(members, $"Retrieved {members.Count} members"));
    }

    /// <summary>
    /// Add a member to a league
    /// </summary>
    [HttpPost("{leagueId}/members")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueAdmin)]
    public async Task<ActionResult<ApiResponse<LeagueMemberResponse>>> AddLeagueMember(string leagueId, [FromBody] AddLeagueMemberRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<LeagueMemberResponse>.ErrorResponse("User not authenticated", "User ID not found in token"));
        }

        try
        {
            var member = await _leagueService.AddLeagueMemberAsync(leagueId, request, userId);
            _logger.LogInformation("User {Email} added to league {LeagueId} by {UserId}", request.Email, leagueId, userId);
            return Ok(ApiResponse<LeagueMemberResponse>.SuccessResponse(member, "Member added successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<LeagueMemberResponse>.ErrorResponse("Not found", ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<LeagueMemberResponse>.ErrorResponse("Invalid operation", ex.Message));
        }
    }

    /// <summary>
    /// Remove a member from a league
    /// </summary>
    [HttpDelete("{leagueId}/members/{memberId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueAdmin)]
    public async Task<ActionResult<ApiResponse<bool>>> RemoveLeagueMember(string leagueId, string memberId)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<bool>.ErrorResponse("User not authenticated", "User ID not found in token"));
        }

        try
        {
            var result = await _leagueService.RemoveLeagueMemberAsync(leagueId, memberId, userId);

            if (!result)
            {
                return NotFound(ApiResponse<bool>.ErrorResponse("Member not found", $"Member {memberId} not found in league {leagueId}"));
            }

            _logger.LogInformation("User {MemberId} removed from league {LeagueId} by {UserId}", memberId, leagueId, userId);
            return Ok(ApiResponse<bool>.SuccessResponse(true, "Member removed successfully"));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse("Invalid operation", ex.Message));
        }
    }

    /// <summary>
    /// Update a league member's role
    /// </summary>
    [HttpPut("{leagueId}/members/{memberId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueAdmin)]
    public async Task<ActionResult<ApiResponse<LeagueMemberResponse>>> UpdateLeagueMember(string leagueId, string memberId, [FromBody] UpdateLeagueMemberRequest request)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(userId))
        {
            return Unauthorized(ApiResponse<LeagueMemberResponse>.ErrorResponse("User not authenticated", "User ID not found in token"));
        }

        try
        {
            var member = await _leagueService.UpdateLeagueMemberAsync(leagueId, memberId, request, userId);
            _logger.LogInformation("User {MemberId} role updated in league {LeagueId} by {UserId}", memberId, leagueId, userId);
            return Ok(ApiResponse<LeagueMemberResponse>.SuccessResponse(member, "Member role updated successfully"));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(ApiResponse<LeagueMemberResponse>.ErrorResponse("Not found", ex.Message));
        }
        catch (InvalidOperationException ex)
        {
            return BadRequest(ApiResponse<LeagueMemberResponse>.ErrorResponse("Invalid operation", ex.Message));
        }
    }
}

