using GolfManager.Api.Authorization;
using GolfManager.Services.Handicap;
using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.Handicap;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace GolfManager.Api.Controllers;

/// <summary>
/// Controller for handicap management
/// </summary>
[ApiController]
[Route("api/v1/golfers/{golferId}/handicap")]
[Authorize]
public class HandicapController : ControllerBase
{
    private readonly IHandicapService _handicapService;
    private readonly ILogger<HandicapController> _logger;

    public HandicapController(
        IHandicapService handicapService,
        ILogger<HandicapController> logger)
    {
        _handicapService = handicapService;
        _logger = logger;
    }

    /// <summary>
    /// Get handicap history for a golfer
    /// </summary>
    /// <param name="golferId">Golfer ID</param>
    /// <param name="leagueId">Optional league ID filter</param>
    /// <param name="seasonId">Optional season ID filter</param>
    /// <param name="limit">Maximum records to return (default 50, max 200)</param>
    [HttpGet("history")]
    public async Task<ActionResult<ApiResponse<List<HandicapHistoryResponse>>>> GetHandicapHistory(
        string golferId,
        [FromQuery] string? leagueId = null,
        [FromQuery] string? seasonId = null,
        [FromQuery] int limit = 50)
    {
        // Cap limit at 200
        limit = Math.Min(limit, 200);

        var response = await _handicapService.GetHandicapHistoryAsync(golferId, leagueId, seasonId, limit);

        if (response == null || !response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Create/update a handicap (manual entry)
    /// </summary>
    /// <param name="golferId">Golfer ID</param>
    /// <param name="request">Handicap details</param>
    [HttpPost]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueAdmin)]
    public async Task<ActionResult<ApiResponse<HandicapHistoryResponse>>> CreateHandicap(
        string golferId,
        [FromBody] CreateHandicapRequest request)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(currentUserId))
        {
            return Unauthorized(ApiResponse<HandicapHistoryResponse>.ErrorResponse("User not authenticated"));
        }

        var response = await _handicapService.CreateHandicapAsync(golferId, request, currentUserId);

        if (response == null || !response.Success)
        {
            return BadRequest(response);
        }

        return Ok(response);
    }

    /// <summary>
    /// Get current handicap for a golfer
    /// </summary>
    /// <param name="golferId">Golfer ID</param>
    /// <param name="leagueId">Optional league ID</param>
    /// <param name="seasonId">Optional season ID</param>
    [HttpGet("current")]
    public async Task<ActionResult<ApiResponse<double?>>> GetCurrentHandicap(
        string golferId,
        [FromQuery] string? leagueId = null,
        [FromQuery] string? seasonId = null)
    {
        var handicap = await _handicapService.GetCurrentHandicapAsync(golferId, leagueId, seasonId);

        return Ok(ApiResponse<double?>.SuccessResponse(handicap, 
            handicap.HasValue ? $"Current handicap: {handicap.Value:F1}" : "No handicap set"));
    }

    /// <summary>
    /// Calculate a golfer's handicap index from their recorded rounds.
    /// Supports World Handicap System (WHS), Bob's League, and Scratch methods.
    /// Set persist=false for a preview without saving.
    /// </summary>
    [HttpPost("calculate")]
    public async Task<ActionResult<ApiResponse<HandicapCalculationResponse>>> CalculateHandicap(
        string golferId,
        [FromBody] CalculateHandicapRequest request)
    {
        var currentUserId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        if (string.IsNullOrEmpty(currentUserId))
            return Unauthorized(ApiResponse<HandicapCalculationResponse>.ErrorResponse("User not authenticated"));

        var response = await _handicapService.CalculateHandicapAsync(golferId, request, currentUserId);

        if (!response.Success)
            return BadRequest(response);

        return Ok(response);
    }
}
