using System.Security.Claims;
using GolfManager.Services.Auth;
using GolfManager.Services.OneTimeEvent;
using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.OneTimeEvent;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace GolfManager.Api.Controllers;

/// <summary>
/// Controller for managing one-time events
/// </summary>
[ApiController]
[Route("api/v1/events/one-time")]
public class OneTimeEventsController : ControllerBase
{
    private readonly IOneTimeEventService _eventService;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<OneTimeEventsController> _logger;

    public OneTimeEventsController(
        IOneTimeEventService eventService,
        ICurrentUserService currentUserService,
        ILogger<OneTimeEventsController> logger)
    {
        _eventService = eventService;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Get all one-time events (with optional filters)
    /// </summary>
    /// <param name="publicOnly">Filter to only public events</param>
    /// <param name="upcomingOnly">Filter to only upcoming events</param>
    /// <param name="organizerId">Filter by organizer user ID</param>
    /// <returns>List of one-time events</returns>
    [HttpGet]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<List<OneTimeEventListResponse>>), StatusCodes.Status200OK)]
    public async Task<ActionResult<ApiResponse<List<OneTimeEventListResponse>>>> GetEvents(
        [FromQuery] bool? publicOnly = null,
        [FromQuery] bool? upcomingOnly = null,
        [FromQuery] string? organizerId = null)
    {
        var events = await _eventService.GetEventsAsync(publicOnly, upcomingOnly, organizerId);
        return Ok(ApiResponse<List<OneTimeEventListResponse>>.SuccessResponse(
            events,
            $"Retrieved {events.Count} events"));
    }

    /// <summary>
    /// Get a one-time event by ID
    /// </summary>
    /// <param name="eventId">The event ID</param>
    /// <returns>Event details</returns>
    [HttpGet("{eventId}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<OneTimeEventResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<OneTimeEventResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<OneTimeEventResponse>>> GetEventById(string eventId)
    {
        var eventResponse = await _eventService.GetEventByIdAsync(eventId);

        if (eventResponse == null)
        {
            return NotFound(ApiResponse<OneTimeEventResponse>.ErrorResponse(
                "Event not found",
                $"Event with ID {eventId} not found"));
        }

        return Ok(ApiResponse<OneTimeEventResponse>.SuccessResponse(
            eventResponse,
            "Event retrieved successfully"));
    }

    /// <summary>
    /// Get a one-time event by key
    /// </summary>
    /// <param name="eventKey">The URL-friendly event key</param>
    /// <returns>Event details</returns>
    [HttpGet("by-key/{eventKey}")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(ApiResponse<OneTimeEventResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<OneTimeEventResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<OneTimeEventResponse>>> GetEventByKey(string eventKey)
    {
        var eventResponse = await _eventService.GetEventByKeyAsync(eventKey);

        if (eventResponse == null)
        {
            return NotFound(ApiResponse<OneTimeEventResponse>.ErrorResponse(
                "Event not found",
                $"Event with key '{eventKey}' not found"));
        }

        return Ok(ApiResponse<OneTimeEventResponse>.SuccessResponse(
            eventResponse,
            "Event retrieved successfully"));
    }

    /// <summary>
    /// Create a new one-time event
    /// </summary>
    /// <param name="request">Event creation details</param>
    /// <returns>Created event details</returns>
    [HttpPost]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<OneTimeEventResponse>), StatusCodes.Status201Created)]
    [ProducesResponseType(typeof(ApiResponse<OneTimeEventResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<ActionResult<ApiResponse<OneTimeEventResponse>>> CreateEvent(
        [FromBody] CreateOneTimeEventRequest request)
    {
        var userId = _currentUserService.UserId!;
        var eventResponse = await _eventService.CreateEventAsync(request, userId);

        _logger.LogInformation("One-time event {EventKey} ({EventId}) created by user {UserId}",
            eventResponse.Key, eventResponse.Id, userId);

        return CreatedAtAction(
            nameof(GetEventById),
            new { eventId = eventResponse.Id },
            ApiResponse<OneTimeEventResponse>.SuccessResponse(
                eventResponse,
                "Event created successfully"));
    }

    /// <summary>
    /// Update an existing one-time event
    /// </summary>
    /// <param name="eventId">The event ID</param>
    /// <param name="request">Updated event details</param>
    /// <returns>Updated event details</returns>
    [HttpPut("{eventId}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<OneTimeEventResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<OneTimeEventResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<OneTimeEventResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<OneTimeEventResponse>>> UpdateEvent(
        string eventId,
        [FromBody] UpdateOneTimeEventRequest request)
    {
        var userId = _currentUserService.UserId!;

        // Check if user is the organizer
        var isOrganizer = await _eventService.IsOrganizerAsync(eventId, userId);
        if (!isOrganizer)
        {
            return Forbid();
        }

        var eventResponse = await _eventService.UpdateEventAsync(eventId, request, userId);

        _logger.LogInformation("One-time event {EventId} updated by user {UserId}",
            eventId, userId);

        return Ok(ApiResponse<OneTimeEventResponse>.SuccessResponse(
            eventResponse,
            "Event updated successfully"));
    }

    /// <summary>
    /// Publish an event (change status from Draft to Published)
    /// </summary>
    /// <param name="eventId">The event ID</param>
    /// <returns>Updated event details</returns>
    [HttpPost("{eventId}/publish")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<OneTimeEventResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<OneTimeEventResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<OneTimeEventResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<OneTimeEventResponse>>> PublishEvent(string eventId)
    {
        var userId = _currentUserService.UserId!;

        // Check if user is the organizer
        var isOrganizer = await _eventService.IsOrganizerAsync(eventId, userId);
        if (!isOrganizer)
        {
            return Forbid();
        }

        var eventResponse = await _eventService.PublishEventAsync(eventId, userId);

        _logger.LogInformation("One-time event {EventId} published by user {UserId}",
            eventId, userId);

        return Ok(ApiResponse<OneTimeEventResponse>.SuccessResponse(
            eventResponse,
            "Event published successfully"));
    }

    /// <summary>
    /// Cancel an event
    /// </summary>
    /// <param name="eventId">The event ID</param>
    /// <returns>Updated event details</returns>
    [HttpPost("{eventId}/cancel")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<OneTimeEventResponse>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(ApiResponse<OneTimeEventResponse>), StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<OneTimeEventResponse>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<OneTimeEventResponse>>> CancelEvent(string eventId)
    {
        var userId = _currentUserService.UserId!;

        // Check if user is the organizer
        var isOrganizer = await _eventService.IsOrganizerAsync(eventId, userId);
        if (!isOrganizer)
        {
            return Forbid();
        }

        var eventResponse = await _eventService.CancelEventAsync(eventId, userId);

        _logger.LogInformation("One-time event {EventId} cancelled by user {UserId}",
            eventId, userId);

        return Ok(ApiResponse<OneTimeEventResponse>.SuccessResponse(
            eventResponse,
            "Event cancelled successfully"));
    }

    /// <summary>
    /// Delete an event (soft delete)
    /// </summary>
    /// <param name="eventId">The event ID</param>
    /// <returns>Success status</returns>
    [HttpDelete("{eventId}")]
    [Authorize]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(ApiResponse<bool>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteEvent(string eventId)
    {
        var userId = _currentUserService.UserId!;

        // Check if user is the organizer
        var isOrganizer = await _eventService.IsOrganizerAsync(eventId, userId);
        if (!isOrganizer)
        {
            return Forbid();
        }

        var result = await _eventService.DeleteEventAsync(eventId, userId);

        if (!result)
        {
            return NotFound(ApiResponse<bool>.ErrorResponse(
                "Event not found",
                $"Event with ID {eventId} not found"));
        }

        _logger.LogInformation("One-time event {EventId} deleted by user {UserId}",
            eventId, userId);

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Event deleted successfully"));
    }
}
