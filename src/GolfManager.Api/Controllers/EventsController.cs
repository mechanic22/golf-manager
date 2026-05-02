using GolfManager.Api.Authorization;
using GolfManager.Core.Services;
using GolfManager.Services.Event;
using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.Event;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GolfManager.Api.Controllers;

/// <summary>
/// Controller for managing season events
/// </summary>
[ApiController]
[Route("api/v1/seasons/{seasonId}/events")]
[Authorize]
public class EventsController : ControllerBase
{
    private readonly IEventService _eventService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<EventsController> _logger;

    public EventsController(
        IEventService eventService,
        ICurrentUserService currentUserService,
        ILogger<EventsController> logger)
    {
        _eventService = eventService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Get all events for a season
    /// </summary>
    [HttpGet]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueMember)]
    public async Task<ActionResult<ApiResponse<List<EventResponse>>>> GetSeasonEvents(string seasonId)
    {
        var leagueId = HttpContext.Items["LeagueId"] as string;
        if (string.IsNullOrEmpty(leagueId))
        {
            return BadRequest(ApiResponse<List<EventResponse>>.ErrorResponse("League context required"));
        }

        var events = await _eventService.GetSeasonEventsAsync(seasonId, leagueId);
        return Ok(ApiResponse<List<EventResponse>>.SuccessResponse(events));
    }

    /// <summary>
    /// Get an event by ID
    /// </summary>
    [HttpGet("{eventId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueMember)]
    public async Task<ActionResult<ApiResponse<EventResponse>>> GetEventById(string seasonId, string eventId)
    {
        var leagueId = HttpContext.Items["LeagueId"] as string;
        if (string.IsNullOrEmpty(leagueId))
        {
            return BadRequest(ApiResponse<EventResponse>.ErrorResponse("League context required"));
        }

        var seasonEvent = await _eventService.GetEventByIdAsync(eventId, leagueId);

        if (seasonEvent == null)
        {
            return NotFound(ApiResponse<EventResponse>.ErrorResponse("Event not found"));
        }

        return Ok(ApiResponse<EventResponse>.SuccessResponse(seasonEvent));
    }

    /// <summary>
    /// Create a new event
    /// </summary>
    [HttpPost]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueAdmin)]
    public async Task<ActionResult<ApiResponse<EventResponse>>> CreateEvent(
        string seasonId,
        [FromBody] CreateEventRequest request)
    {
        var leagueId = HttpContext.Items["LeagueId"] as string;
        if (string.IsNullOrEmpty(leagueId))
        {
            return BadRequest(ApiResponse<EventResponse>.ErrorResponse("League context required"));
        }

        var userId = _currentUserService.UserId!;
        var seasonEvent = await _eventService.CreateEventAsync(request, seasonId, leagueId, userId);

        _logger.LogInformation("Event created in season {SeasonId} by user {UserId}",
            seasonId, userId);

        return CreatedAtAction(
            nameof(GetEventById),
            new { seasonId, eventId = seasonEvent.Id },
            ApiResponse<EventResponse>.SuccessResponse(seasonEvent));
    }

    /// <summary>
    /// Update an event
    /// </summary>
    [HttpPut("{eventId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueAdmin)]
    public async Task<ActionResult<ApiResponse<EventResponse>>> UpdateEvent(
        string seasonId,
        string eventId,
        [FromBody] UpdateEventRequest request)
    {
        var leagueId = HttpContext.Items["LeagueId"] as string;
        if (string.IsNullOrEmpty(leagueId))
        {
            return BadRequest(ApiResponse<EventResponse>.ErrorResponse("League context required"));
        }

        var userId = _currentUserService.UserId!;
        var seasonEvent = await _eventService.UpdateEventAsync(eventId, request, leagueId, userId);

        _logger.LogInformation("Event {EventId} updated by user {UserId}",
            eventId, userId);

        return Ok(ApiResponse<EventResponse>.SuccessResponse(seasonEvent));
    }

    /// <summary>
    /// Delete an event
    /// </summary>
    [HttpDelete("{eventId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueAdmin)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteEvent(string seasonId, string eventId)
    {
        var leagueId = HttpContext.Items["LeagueId"] as string;
        if (string.IsNullOrEmpty(leagueId))
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse("League context required"));
        }

        var userId = _currentUserService.UserId!;
        var result = await _eventService.DeleteEventAsync(eventId, leagueId, userId);

        if (!result)
        {
            return NotFound(ApiResponse<bool>.ErrorResponse("Event not found"));
        }

        _logger.LogInformation("Event {EventId} deleted by user {UserId}",
            eventId, userId);

        return Ok(ApiResponse<bool>.SuccessResponse(true));
    }
}

