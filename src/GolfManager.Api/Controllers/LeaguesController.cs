using System.Security.Claims;
using GolfManager.Api.Authorization;
using GolfManager.Services.League;
using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.League;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
    private readonly ILogger<LeaguesController> _logger;

    public LeaguesController(
        ILeagueService leagueService,
        ILogger<LeaguesController> logger)
    {
        _leagueService = leagueService;
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
    /// Get a specific league by key
    /// </summary>
    [HttpGet("by-key/{leagueKey}")]
    [AllowAnonymous]
    public async Task<ActionResult<ApiResponse<LeagueResponse>>> GetLeagueByKey(string leagueKey)
    {
        var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        var league = await _leagueService.GetLeagueByKeyAsync(leagueKey, userId);

        if (league == null)
        {
            return NotFound(ApiResponse<LeagueResponse>.ErrorResponse("League not found", $"League with key '{leagueKey}' not found"));
        }

        return Ok(ApiResponse<LeagueResponse>.SuccessResponse(league, "League retrieved successfully"));
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

        var league = await _leagueService.CreateLeagueAsync(request, userId);
        _logger.LogInformation("League {LeagueKey} created by user {UserId}", league.Key, userId);

        return CreatedAtAction(
            nameof(GetLeagueById),
            new { leagueId = league.Id },
            ApiResponse<LeagueResponse>.SuccessResponse(league, "League created successfully"));
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

        var league = await _leagueService.UpdateLeagueAsync(leagueId, request, userId);
        _logger.LogInformation("League {LeagueId} updated by user {UserId}", leagueId, userId);

        return Ok(ApiResponse<LeagueResponse>.SuccessResponse(league, "League updated successfully"));
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
}

