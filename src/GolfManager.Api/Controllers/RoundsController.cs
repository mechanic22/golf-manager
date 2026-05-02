using GolfManager.Api.Authorization;
using GolfManager.Core.Services;
using GolfManager.Shared.DTOs.Common;
using GolfManager.Services.Round;
using GolfManager.Shared.DTOs.Round;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GolfManager.Api.Controllers;

/// <summary>
/// Controller for round management
/// </summary>
[ApiController]
[Route("api/v1/rounds")]
[Authorize]
public class RoundsController : ControllerBase
{
    private readonly IRoundService _roundService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<RoundsController> _logger;

    public RoundsController(
        IRoundService roundService,
        ICurrentUserService currentUserService,
        ILogger<RoundsController> logger)
    {
        _roundService = roundService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Get all rounds for a league golfer
    /// </summary>
    [HttpGet("golfer/{leagueGolferId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueMember)]
    public async Task<ActionResult<ApiResponse<List<RoundResponse>>>> GetLeagueGolferRounds(
        string leagueGolferId)
    {
        var leagueId = HttpContext.Items["LeagueId"] as string;
        if (string.IsNullOrEmpty(leagueId))
        {
            return BadRequest(ApiResponse<List<RoundResponse>>.ErrorResponse("League context required"));
        }

        var rounds = await _roundService.GetLeagueGolferRoundsAsync(leagueGolferId, leagueId);
        return Ok(ApiResponse<List<RoundResponse>>.SuccessResponse(rounds));
    }

    /// <summary>
    /// Get a specific round by ID
    /// </summary>
    [HttpGet("{roundId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueMember)]
    public async Task<ActionResult<ApiResponse<RoundResponse>>> GetRound(
        string roundId)
    {
        var leagueId = HttpContext.Items["LeagueId"] as string;
        if (string.IsNullOrEmpty(leagueId))
        {
            return BadRequest(ApiResponse<RoundResponse>.ErrorResponse("League context required"));
        }

        var round = await _roundService.GetRoundByIdAsync(roundId, leagueId);

        if (round == null)
        {
            return NotFound(ApiResponse<RoundResponse>.ErrorResponse("Round not found"));
        }

        return Ok(ApiResponse<RoundResponse>.SuccessResponse(round));
    }

    /// <summary>
    /// Get all rounds for a season event
    /// </summary>
    [HttpGet("event/{seasonEventId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueMember)]
    public async Task<ActionResult<ApiResponse<List<RoundResponse>>>> GetEventRounds(
        string seasonEventId)
    {
        var leagueId = HttpContext.Items["LeagueId"] as string;
        if (string.IsNullOrEmpty(leagueId))
        {
            return BadRequest(ApiResponse<List<RoundResponse>>.ErrorResponse("League context required"));
        }

        var rounds = await _roundService.GetEventRoundsAsync(seasonEventId, leagueId);
        return Ok(ApiResponse<List<RoundResponse>>.SuccessResponse(rounds));
    }

    /// <summary>
    /// Create a new round
    /// </summary>
    [HttpPost]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueAdmin)]
    public async Task<ActionResult<ApiResponse<RoundResponse>>> CreateRound(
        [FromBody] CreateRoundRequest request)
    {
        var leagueId = HttpContext.Items["LeagueId"] as string;
        if (string.IsNullOrEmpty(leagueId))
        {
            return BadRequest(ApiResponse<RoundResponse>.ErrorResponse("League context required"));
        }

        var userId = _currentUserService.UserId!;

        var round = await _roundService.CreateRoundAsync(request, leagueId, userId);

        _logger.LogInformation("Round {RoundId} created in league {LeagueId} by user {UserId}",
            round.Id, leagueId, userId);

        return CreatedAtAction(
            nameof(GetRound),
            new { roundId = round.Id },
            ApiResponse<RoundResponse>.SuccessResponse(round));
    }

    /// <summary>
    /// Update an existing round
    /// </summary>
    [HttpPut("{roundId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueAdmin)]
    public async Task<ActionResult<ApiResponse<RoundResponse>>> UpdateRound(
        string roundId,
        [FromBody] UpdateRoundRequest request)
    {
        var leagueId = HttpContext.Items["LeagueId"] as string;
        if (string.IsNullOrEmpty(leagueId))
        {
            return BadRequest(ApiResponse<RoundResponse>.ErrorResponse("League context required"));
        }

        var userId = _currentUserService.UserId!;

        var round = await _roundService.UpdateRoundAsync(roundId, request, leagueId, userId);

        _logger.LogInformation("Round {RoundId} updated in league {LeagueId} by user {UserId}",
            roundId, leagueId, userId);

        return Ok(ApiResponse<RoundResponse>.SuccessResponse(round));
    }

    /// <summary>
    /// Delete a round
    /// </summary>
    [HttpDelete("{roundId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueAdmin)]
    public async Task<ActionResult<ApiResponse<object>>> DeleteRound(
        string roundId)
    {
        var leagueId = HttpContext.Items["LeagueId"] as string;
        if (string.IsNullOrEmpty(leagueId))
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("League context required"));
        }

        var userId = _currentUserService.UserId!;

        await _roundService.DeleteRoundAsync(roundId, leagueId, userId);

        _logger.LogInformation("Round {RoundId} deleted from league {LeagueId} by user {UserId}",
            roundId, leagueId, userId);

        return Ok(ApiResponse<object>.SuccessResponse("Round deleted successfully"));
    }

    /// <summary>
    /// Record or update a hole score
    /// </summary>
    [HttpPost("{roundId}/holes")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueAdmin)]
    public async Task<ActionResult<ApiResponse<RoundResponse>>> RecordHoleScore(
        string roundId,
        [FromBody] RecordHoleScoreRequest request)
    {
        var leagueId = HttpContext.Items["LeagueId"] as string;
        if (string.IsNullOrEmpty(leagueId))
        {
            return BadRequest(ApiResponse<RoundResponse>.ErrorResponse("League context required"));
        }

        var userId = _currentUserService.UserId!;

        var round = await _roundService.RecordHoleScoreAsync(roundId, request, leagueId, userId);

        _logger.LogInformation("Hole score recorded for round {RoundId} in league {LeagueId} by user {UserId}",
            roundId, leagueId, userId);

        return Ok(ApiResponse<RoundResponse>.SuccessResponse(round));
    }
}

