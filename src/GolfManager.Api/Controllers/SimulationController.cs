using GolfManager.Api.Authorization;
using GolfManager.Services.Simulation;
using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.Simulation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GolfManager.Api.Controllers;

/// <summary>
/// Controller for season simulation and testing
/// </summary>
[ApiController]
[Route("api/v1/leagues/{leagueId}/seasons/{seasonId}/simulation")]
[Authorize(Policy = AuthorizationConstants.Policies.LeagueAdmin)]
public class SimulationController : ControllerBase
{
    private readonly ISeasonSimulationService _simulationService;
    private readonly ILogger<SimulationController> _logger;

    public SimulationController(
        ISeasonSimulationService simulationService,
        ILogger<SimulationController> logger)
    {
        _simulationService = simulationService;
        _logger = logger;
    }

    /// <summary>
    /// Seed a season with players, teams, and events
    /// </summary>
    [HttpPost("seed")]
    public async Task<ActionResult<ApiResponse<SimulationResult>>> SeedSeason(
        string leagueId,
        string seasonId,
        [FromBody] SeedSeasonRequest? request = null)
    {
        _logger.LogInformation("Seeding season {SeasonId} in league {LeagueId}", seasonId, leagueId);

        var playerCount = request?.PlayerCount ?? 60;
        var playersPerTeam = request?.PlayersPerTeam ?? 3;
        var weekCount = request?.WeekCount ?? 16;

        var result = await _simulationService.SeedSeasonAsync(
            leagueId,
            seasonId,
            playerCount,
            playersPerTeam,
            weekCount);

        if (!result.Success)
        {
            return BadRequest(ApiResponse<SimulationResult>.ErrorResponse(
                "Failed to seed season",
                result.Errors.ToArray()));
        }

        return Ok(ApiResponse<SimulationResult>.SuccessResponse(result, result.Message));
    }

    /// <summary>
    /// Simulate scores for a specific event
    /// </summary>
    [HttpPost("events/{eventId}/simulate")]
    public async Task<ActionResult<ApiResponse<SimulationResult>>> SimulateEvent(
        string leagueId,
        string seasonId,
        string eventId)
    {
        _logger.LogInformation("Simulating event {EventId}", eventId);

        var result = await _simulationService.SimulateEventScoresAsync(leagueId, eventId);

        if (!result.Success)
        {
            return BadRequest(ApiResponse<SimulationResult>.ErrorResponse(
                "Failed to simulate event",
                result.Errors.ToArray()));
        }

        return Ok(ApiResponse<SimulationResult>.SuccessResponse(result, result.Message));
    }

    /// <summary>
    /// Simulate the next unplayed event
    /// </summary>
    [HttpPost("simulate-next")]
    public async Task<ActionResult<ApiResponse<SimulationResult>>> SimulateNextEvent(
        string leagueId,
        string seasonId)
    {
        _logger.LogInformation("Simulating next event for season {SeasonId}", seasonId);

        var result = await _simulationService.SimulateNextEventAsync(leagueId, seasonId);

        if (!result.Success)
        {
            return BadRequest(ApiResponse<SimulationResult>.ErrorResponse(
                "Failed to simulate next event",
                result.Errors.ToArray()));
        }

        return Ok(ApiResponse<SimulationResult>.SuccessResponse(result, result.Message));
    }

    /// <summary>
    /// Simulate all remaining events in the season
    /// </summary>
    [HttpPost("simulate-all")]
    public async Task<ActionResult<ApiResponse<SimulationResult>>> SimulateAllEvents(
        string leagueId,
        string seasonId)
    {
        _logger.LogInformation("Simulating all events for season {SeasonId}", seasonId);

        var result = await _simulationService.SimulateAllEventsAsync(leagueId, seasonId);

        if (!result.Success)
        {
            return BadRequest(ApiResponse<SimulationResult>.ErrorResponse(
                "Failed to simulate all events",
                result.Errors.ToArray()));
        }

        return Ok(ApiResponse<SimulationResult>.SuccessResponse(result, result.Message));
    }

    /// <summary>
    /// Clear all simulation data for a season
    /// </summary>
    [HttpDelete("clear")]
    public async Task<ActionResult<ApiResponse<SimulationResult>>> ClearSeasonData(
        string leagueId,
        string seasonId)
    {
        _logger.LogInformation("Clearing simulation data for season {SeasonId}", seasonId);

        var result = await _simulationService.ClearSeasonDataAsync(leagueId, seasonId);

        if (!result.Success)
        {
            return BadRequest(ApiResponse<SimulationResult>.ErrorResponse(
                "Failed to clear season data",
                result.Errors.ToArray()));
        }

        return Ok(ApiResponse<SimulationResult>.SuccessResponse(result, result.Message));
    }
}

/// <summary>
/// Request for seeding a season
/// </summary>
public class SeedSeasonRequest
{
    public int PlayerCount { get; set; } = 60;
    public int PlayersPerTeam { get; set; } = 3;
    public int WeekCount { get; set; } = 16;
}

