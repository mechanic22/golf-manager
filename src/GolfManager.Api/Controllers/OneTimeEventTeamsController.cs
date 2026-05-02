using GolfManager.Core.Services;
using GolfManager.Services.OneTimeEvent;
using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.OneTimeEvent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GolfManager.Api.Controllers;

/// <summary>
/// Controller for managing one-time event team registrations
/// </summary>
[ApiController]
[Route("api/v1/events/one-time")]
public class OneTimeEventTeamsController : ControllerBase
{
    private readonly ITeamRegistrationService _teamService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<OneTimeEventTeamsController> _logger;

    public OneTimeEventTeamsController(
        ITeamRegistrationService teamService,
        ICurrentUserService currentUserService,
        ILogger<OneTimeEventTeamsController> logger)
    {
        _teamService = teamService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Get all teams for an event
    /// </summary>
    /// <param name="eventId">The event ID</param>
    /// <returns>List of teams registered for the event</returns>
    [HttpGet("{eventId}/teams")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<List<OneTimeEventTeamResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<OneTimeEventTeamResponse>>>> GetEventTeams(string eventId)
    {
        var teams = await _teamService.GetEventTeamsAsync(eventId);
        return Ok(ApiResponse<List<OneTimeEventTeamResponse>>.SuccessResponse(
            teams,
            $"Retrieved {teams.Count} teams"));
    }

    /// <summary>
    /// Get a specific team by ID
    /// </summary>
    /// <param name="teamId">The team ID</param>
    /// <returns>Team details</returns>
    [HttpGet("teams/{teamId}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<OneTimeEventTeamResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<OneTimeEventTeamResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<OneTimeEventTeamResponse>>> GetTeamById(string teamId)
    {
        var team = await _teamService.GetTeamByIdAsync(teamId);

        if (team == null)
        {
            return NotFound(ApiResponse<OneTimeEventTeamResponse>.ErrorResponse(
                "Team not found",
                $"Team with ID {teamId} not found"));
        }

        return Ok(ApiResponse<OneTimeEventTeamResponse>.SuccessResponse(
            team,
            "Team retrieved successfully"));
    }

    /// <summary>
    /// Register a team for an event
    /// </summary>
    /// <param name="eventId">The event ID</param>
    /// <param name="request">Team registration details</param>
    /// <returns>Registered team details</returns>
    [HttpPost("{eventId}/teams")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<OneTimeEventTeamResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<OneTimeEventTeamResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(typeof(ApiResponse<OneTimeEventTeamResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<OneTimeEventTeamResponse>>> RegisterTeam(
        string eventId,
        [FromBody] RegisterTeamRequest request)
    {
        // Get userId if authenticated, null if anonymous
        var userId = _currentUserService.UserId;

        var team = await _teamService.RegisterTeamAsync(eventId, request, userId);

        _logger.LogInformation("Team {TeamName} ({TeamId}) registered for event {EventId}",
            team.TeamName, team.Id, eventId);

        return CreatedAtAction(
            nameof(GetTeamById),
            new { teamId = team.Id },
            ApiResponse<OneTimeEventTeamResponse>.SuccessResponse(
                team,
                "Team registered successfully"));
    }

    /// <summary>
    /// Update team information
    /// </summary>
    /// <param name="teamId">The team ID</param>
    /// <param name="request">Updated team details</param>
    /// <returns>Updated team details</returns>
    [HttpPut("teams/{teamId}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<OneTimeEventTeamResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<OneTimeEventTeamResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<OneTimeEventTeamResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<OneTimeEventTeamResponse>>> UpdateTeam(
        string teamId,
        [FromBody] UpdateTeamRequest request)
    {
        var userId = _currentUserService.UserId!;

        // Check if user is the team captain
        var isCaptain = await _teamService.IsTeamCaptainAsync(teamId, userId);
        if (!isCaptain)
        {
            return Forbid();
        }

        var team = await _teamService.UpdateTeamAsync(teamId, request, userId);

        _logger.LogInformation("Team {TeamId} updated by user {UserId}",
            teamId, userId);

        return Ok(ApiResponse<OneTimeEventTeamResponse>.SuccessResponse(
            team,
            "Team updated successfully"));
    }

    /// <summary>
    /// Remove a team from an event
    /// </summary>
    /// <param name="teamId">The team ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("teams/{teamId}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> RemoveTeam(string teamId)
    {
        var userId = _currentUserService.UserId!;

        // Check if user is the team captain
        var isCaptain = await _teamService.IsTeamCaptainAsync(teamId, userId);
        if (!isCaptain)
        {
            return Forbid();
        }

        var result = await _teamService.RemoveTeamAsync(teamId, userId);

        if (!result)
        {
            return NotFound(ApiResponse<bool>.ErrorResponse(
                "Team not found",
                $"Team with ID {teamId} not found"));
        }

        _logger.LogInformation("Team {TeamId} removed by user {UserId}",
            teamId, userId);

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Team removed successfully"));
    }

    /// <summary>
    /// Check in a team on event day (organizer only)
    /// </summary>
    /// <param name="teamId">The team ID</param>
    /// <returns>Updated team details with check-in status</returns>
    [HttpPost("teams/{teamId}/check-in")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<OneTimeEventTeamResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<OneTimeEventTeamResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<OneTimeEventTeamResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<OneTimeEventTeamResponse>>> CheckInTeam(string teamId)
    {
        var userId = _currentUserService.UserId!;

        var team = await _teamService.CheckInTeamAsync(teamId, userId);

        _logger.LogInformation("Team {TeamId} checked in by user {UserId}",
            teamId, userId);

        return Ok(ApiResponse<OneTimeEventTeamResponse>.SuccessResponse(
            team,
            "Team checked in successfully"));
    }

    /// <summary>
    /// Add a player to a team
    /// </summary>
    /// <param name="teamId">The team ID</param>
    /// <param name="request">Player details</param>
    /// <returns>Added player details</returns>
    [HttpPost("teams/{teamId}/players")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<OneTimeEventPlayerResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<OneTimeEventPlayerResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<OneTimeEventPlayerResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<OneTimeEventPlayerResponse>>> AddPlayer(
        string teamId,
        [FromBody] AddPlayerRequest request)
    {
        var userId = _currentUserService.UserId!;

        // Check if user is the team captain
        var isCaptain = await _teamService.IsTeamCaptainAsync(teamId, userId);
        if (!isCaptain)
        {
            return Forbid();
        }

        var player = await _teamService.AddPlayerAsync(teamId, request, userId);

        _logger.LogInformation("Player {PlayerName} ({PlayerId}) added to team {TeamId} by user {UserId}",
            player.PlayerName, player.Id, teamId, userId);

        return Ok(ApiResponse<OneTimeEventPlayerResponse>.SuccessResponse(
            player,
            "Player added successfully"));
    }

    /// <summary>
    /// Update a player's information
    /// </summary>
    /// <param name="playerId">The player ID</param>
    /// <param name="request">Updated player details</param>
    /// <returns>Updated player details</returns>
    [HttpPut("players/{playerId}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<OneTimeEventPlayerResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<OneTimeEventPlayerResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<OneTimeEventPlayerResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<OneTimeEventPlayerResponse>>> UpdatePlayer(
        string playerId,
        [FromBody] UpdatePlayerRequest request)
    {
        var userId = _currentUserService.UserId!;

        var player = await _teamService.UpdatePlayerAsync(playerId, request, userId);

        _logger.LogInformation("Player {PlayerId} updated by user {UserId}",
            playerId, userId);

        return Ok(ApiResponse<OneTimeEventPlayerResponse>.SuccessResponse(
            player,
            "Player updated successfully"));
    }

    /// <summary>
    /// Remove a player from a team
    /// </summary>
    /// <param name="playerId">The player ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("players/{playerId}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> RemovePlayer(string playerId)
    {
        var userId = _currentUserService.UserId!;

        var result = await _teamService.RemovePlayerAsync(playerId, userId);

        if (!result)
        {
            return NotFound(ApiResponse<bool>.ErrorResponse(
                "Player not found",
                $"Player with ID {playerId} not found"));
        }

        _logger.LogInformation("Player {PlayerId} removed by user {UserId}",
            playerId, userId);

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Player removed successfully"));
    }
}

