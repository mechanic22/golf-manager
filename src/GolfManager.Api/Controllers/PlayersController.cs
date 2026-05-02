using GolfManager.Api.Authorization;
using GolfManager.Services.Player;
using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.Player;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GolfManager.Api.Controllers;

/// <summary>
/// Controller for managing players within leagues
/// </summary>
[ApiController]
[Route("api/v1/players")]
[Authorize]
public class PlayersController : ControllerBase
{
    private readonly IPlayerService _playerService;
    private readonly ILogger<PlayersController> _logger;

    public PlayersController(
        IPlayerService playerService,
        ILogger<PlayersController> logger)
    {
        _playerService = playerService;
        _logger = logger;
    }

    /// <summary>
    /// Get all players in a league
    /// </summary>
    [HttpGet]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueMember)]
    public async Task<ActionResult<ApiResponse<List<PlayerResponse>>>> GetPlayers()
    {
        var leagueId = HttpContext.Items["LeagueId"] as string;
        if (string.IsNullOrEmpty(leagueId))
        {
            return BadRequest(ApiResponse<List<PlayerResponse>>.ErrorResponse("League context required"));
        }

        var players = await _playerService.GetLeaguePlayersAsync(leagueId);
        return Ok(ApiResponse<List<PlayerResponse>>.SuccessResponse(players, $"Retrieved {players.Count} players"));
    }

    /// <summary>
    /// Get a specific player in a league
    /// </summary>
    [HttpGet("{playerId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueMember)]
    public async Task<ActionResult<ApiResponse<PlayerResponse>>> GetPlayer(string playerId)
    {
        var leagueId = HttpContext.Items["LeagueId"] as string;
        if (string.IsNullOrEmpty(leagueId))
        {
            return BadRequest(ApiResponse<PlayerResponse>.ErrorResponse("League context required"));
        }

        var player = await _playerService.GetPlayerAsync(leagueId, playerId);

        if (player == null)
        {
            return NotFound(ApiResponse<PlayerResponse>.ErrorResponse("Player not found"));
        }

        return Ok(ApiResponse<PlayerResponse>.SuccessResponse(player, "Player retrieved successfully"));
    }

    /// <summary>
    /// Add a player to a league
    /// </summary>
    [HttpPost]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueAdmin)]
    public async Task<ActionResult<ApiResponse<PlayerResponse>>> AddPlayer(
        [FromBody] CreatePlayerRequest request)
    {
        var leagueId = HttpContext.Items["LeagueId"] as string;
        if (string.IsNullOrEmpty(leagueId))
        {
            return BadRequest(ApiResponse<PlayerResponse>.ErrorResponse("League context required"));
        }

        var player = await _playerService.AddPlayerToLeagueAsync(leagueId, request);

        _logger.LogInformation("Player {DisplayName} added to league {LeagueId}", request.DisplayName, leagueId);

        return CreatedAtAction(
            nameof(GetPlayer),
            new { playerId = player.Id },
            ApiResponse<PlayerResponse>.SuccessResponse(player, "Player added successfully"));
    }

    /// <summary>
    /// Update a player's league profile
    /// </summary>
    [HttpPut("{playerId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueAdmin)]
    public async Task<ActionResult<ApiResponse<PlayerResponse>>> UpdatePlayer(
        string playerId,
        [FromBody] UpdatePlayerRequest request)
    {
        var leagueId = HttpContext.Items["LeagueId"] as string;
        if (string.IsNullOrEmpty(leagueId))
        {
            return BadRequest(ApiResponse<PlayerResponse>.ErrorResponse("League context required"));
        }

        var player = await _playerService.UpdatePlayerAsync(leagueId, playerId, request);

        _logger.LogInformation("Player {PlayerId} updated in league {LeagueId}", playerId, leagueId);

        return Ok(ApiResponse<PlayerResponse>.SuccessResponse(player, "Player updated successfully"));
    }

    /// <summary>
    /// Remove a player from a league
    /// </summary>
    [HttpDelete("{playerId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueAdmin)]
    public async Task<ActionResult<ApiResponse<bool>>> RemovePlayer(string playerId)
    {
        var leagueId = HttpContext.Items["LeagueId"] as string;
        if (string.IsNullOrEmpty(leagueId))
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse("League context required"));
        }

        var result = await _playerService.RemovePlayerFromLeagueAsync(leagueId, playerId);

        if (!result)
        {
            return NotFound(ApiResponse<bool>.ErrorResponse("Player not found"));
        }

        _logger.LogInformation("Player {PlayerId} removed from league {LeagueId}", playerId, leagueId);

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Player removed successfully"));
    }
}

