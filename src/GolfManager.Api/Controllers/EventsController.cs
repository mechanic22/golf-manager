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
[Route("api/v1/seasons/{seasonId}/events")]
public class EventsController : BaseLeagueController
{
    private readonly IEventService _eventService;
    private readonly IEventScoringService _scoringService;
    private readonly IHandicapRecalculationQueue _handicapQueue;
    private readonly ISeasonPointsRecalculationQueue _seasonPointsQueue;
    private readonly ICurrentUserService _currentUserService;
    private readonly ILogger<EventsController> _logger;

    public EventsController(
        IEventService eventService,
        IEventScoringService scoringService,
        IHandicapRecalculationQueue handicapQueue,
        ISeasonPointsRecalculationQueue seasonPointsQueue,
        ICurrentUserService currentUserService,
        ILogger<EventsController> logger)
    {
        _eventService = eventService;
        _scoringService = scoringService;
        _handicapQueue = handicapQueue;
        _seasonPointsQueue = seasonPointsQueue;
        _currentUserService = currentUserService;
        _logger = logger;
    }

    /// <summary>
    /// Get all events for a season
    /// </summary>
    [HttpGet]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueMember)]
    public async Task<ActionResult<ApiResponse<PagedResponse<EventResponse>>>> GetSeasonEvents(
        string seasonId, [FromQuery] int page = 1, [FromQuery] int pageSize = 25)
    {
        var leagueId = LeagueId;
        if (string.IsNullOrEmpty(leagueId))
            return BadRequest(ApiResponse<PagedResponse<EventResponse>>.ErrorResponse("League context required"));

        var events = await _eventService.GetSeasonEventsAsync(seasonId, leagueId, page, pageSize);
        return Ok(ApiResponse<PagedResponse<EventResponse>>.SuccessResponse(events));
    }

    /// <summary>
    /// Get an event by ID
    /// </summary>
    [HttpGet("{eventId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueMember)]
    public async Task<ActionResult<ApiResponse<EventResponse>>> GetEventById(string seasonId, string eventId)
    {
        var leagueId = LeagueId;
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
    /// Get calculated team and individual scoreboard for an event.
    /// </summary>
    [HttpGet("{eventId}/scoreboard")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueMember)]
    public async Task<ActionResult<ApiResponse<EventScoreboardResponse>>> GetEventScoreboard(string seasonId, string eventId)
    {
        var leagueId = LeagueId;
        if (string.IsNullOrEmpty(leagueId))
        {
            return BadRequest(ApiResponse<EventScoreboardResponse>.ErrorResponse("League context required"));
        }

        var scoreboard = await _eventService.GetEventScoreboardAsync(seasonId, eventId, leagueId);
        return Ok(ApiResponse<EventScoreboardResponse>.SuccessResponse(scoreboard));
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
        var leagueId = LeagueId;
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
        var leagueId = LeagueId;
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
    /// Get hole-by-hole match detail for a specific matchup
    /// </summary>
    [HttpGet("{eventId}/matchups/{matchupId}/detail")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueMember)]
    public async Task<ActionResult<ApiResponse<MatchDetailResponse?>>> GetMatchDetail(string seasonId, string eventId, string matchupId)
    {
        var leagueId = LeagueId;
        if (string.IsNullOrEmpty(leagueId))
            return BadRequest(ApiResponse<MatchDetailResponse?>.ErrorResponse("League context required"));

        var detail = await _eventService.GetMatchDetailAsync(seasonId, eventId, matchupId, leagueId);
        if (detail == null)
            return NotFound(ApiResponse<MatchDetailResponse?>.ErrorResponse("Match not found"));

        return Ok(ApiResponse<MatchDetailResponse?>.SuccessResponse(detail));
    }

    /// <summary>
    /// Get event matchups
    /// </summary>
    [HttpGet("{eventId}/matchups")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueMember)]
    public async Task<ActionResult<ApiResponse<List<EventMatchupResponse>>>> GetEventMatchups(string seasonId, string eventId)
    {
        var leagueId = LeagueId;
        if (string.IsNullOrEmpty(leagueId))
        {
            return BadRequest(ApiResponse<List<EventMatchupResponse>>.ErrorResponse("League context required"));
        }

        var matchups = await _eventService.GetEventMatchupsAsync(seasonId, eventId, leagueId);
        return Ok(ApiResponse<List<EventMatchupResponse>>.SuccessResponse(matchups));
    }

    /// <summary>
    /// Get the current user's matchup (hole assignment + opponent) for an event
    /// </summary>
    [HttpGet("{eventId}/my-matchup")]
    [Authorize]
    public async Task<ActionResult<ApiResponse<MyMatchupResponse?>>> GetMyMatchup(string seasonId, string eventId)
    {
        var leagueId = LeagueId;
        if (string.IsNullOrEmpty(leagueId))
            return BadRequest(ApiResponse<MyMatchupResponse?>.ErrorResponse("League context required"));

        var userId = _currentUserService.UserId;
        if (string.IsNullOrEmpty(userId))
            return Ok(ApiResponse<MyMatchupResponse?>.SuccessResponse((MyMatchupResponse?)null));

        var matchup = await _eventService.GetMyMatchupForEventAsync(seasonId, eventId, leagueId, userId);
        return Ok(ApiResponse<MyMatchupResponse?>.SuccessResponse(matchup));
    }

    /// <summary>
    /// Auto setup matchups from current standings
    /// </summary>
    [HttpPost("{eventId}/matchups/auto-setup")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueAdmin)]
    public async Task<ActionResult<ApiResponse<List<EventMatchupResponse>>>> AutoSetupMatchups(string seasonId, string eventId)
    {
        var leagueId = LeagueId;
        if (string.IsNullOrEmpty(leagueId))
        {
            return BadRequest(ApiResponse<List<EventMatchupResponse>>.ErrorResponse("League context required"));
        }

        var userId = _currentUserService.UserId!;
        var matchups = await _eventService.AutoSetupEventMatchupsFromStandingsAsync(seasonId, eventId, leagueId, userId);

        return Ok(ApiResponse<List<EventMatchupResponse>>.SuccessResponse(matchups));
    }

    /// <summary>
    /// Schedule the next week's event from this event and auto-setup matchups from standings.
    /// </summary>
    [HttpPost("{eventId}/schedule-next-week")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueAdmin)]
    public async Task<ActionResult<ApiResponse<EventResponse>>> ScheduleNextWeek(string seasonId, string eventId)
    {
        var leagueId = LeagueId;
        if (string.IsNullOrEmpty(leagueId))
        {
            return BadRequest(ApiResponse<EventResponse>.ErrorResponse("League context required"));
        }

        var userId = _currentUserService.UserId!;
        var nextEvent = await _eventService.ScheduleNextWeekFromEventAsync(seasonId, eventId, leagueId, userId);

        return Ok(ApiResponse<EventResponse>.SuccessResponse(nextEvent, "Next week scheduled and matchups generated."));
    }

    /// <summary>
    /// Update one event matchup
    /// </summary>
    [HttpPut("{eventId}/matchups/{matchupId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueAdmin)]
    public async Task<ActionResult<ApiResponse<EventMatchupResponse>>> UpdateEventMatchup(
        string seasonId,
        string eventId,
        string matchupId,
        [FromBody] UpdateEventMatchupRequest request)
    {
        var leagueId = LeagueId;
        if (string.IsNullOrEmpty(leagueId))
        {
            return BadRequest(ApiResponse<EventMatchupResponse>.ErrorResponse("League context required"));
        }

        var userId = _currentUserService.UserId!;
        var matchup = await _eventService.UpdateEventMatchupAsync(seasonId, eventId, matchupId, request, leagueId, userId);

        return Ok(ApiResponse<EventMatchupResponse>.SuccessResponse(matchup));
    }

    /// <summary>
    /// Recalculate handicaps for all golfers with rounds in this event.
    /// </summary>
    [HttpPost("{eventId}/handicaps/recalculate")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueAdmin)]
    public async Task<ActionResult<ApiResponse<int>>> RecalculateEventHandicaps(string seasonId, string eventId)
    {
        var leagueId = LeagueId;
        if (string.IsNullOrEmpty(leagueId))
        {
            return BadRequest(ApiResponse<int>.ErrorResponse("League context required"));
        }

        var userId = _currentUserService.UserId!;
        await _handicapQueue.QueueEventAsync(leagueId, seasonId, eventId, userId);
        await _seasonPointsQueue.QueueSeasonAsync(leagueId, seasonId, userId);

        return Ok(ApiResponse<int>.SuccessResponse(0, "Handicap recalculation queued in background."));
    }

    /// <summary>
    /// Recalculate handicap for one golfer in the context of this event.
    /// </summary>
    [HttpPost("{eventId}/handicaps/recalculate/{golferId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueAdmin)]
    public async Task<ActionResult<ApiResponse<bool>>> RecalculateEventGolferHandicap(string seasonId, string eventId, string golferId)
    {
        var leagueId = LeagueId;
        if (string.IsNullOrEmpty(leagueId))
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse("League context required"));
        }

        var userId = _currentUserService.UserId!;
        await _handicapQueue.QueueGolferAsync(leagueId, seasonId, eventId, golferId, userId);
        await _seasonPointsQueue.QueueSeasonAsync(leagueId, seasonId, userId);

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Handicap recalculation queued in background."));
    }

    /// <summary>
    /// Recalculate overall season standings (wins/losses/points) from event results.
    /// </summary>
    [HttpPost("{eventId}/overall/recalculate")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueAdmin)]
    public async Task<ActionResult<ApiResponse<int>>> RecalculateOverallStandings(string seasonId, string eventId)
    {
        var leagueId = LeagueId;
        if (string.IsNullOrEmpty(leagueId))
        {
            return BadRequest(ApiResponse<int>.ErrorResponse("League context required"));
        }

        var userId = _currentUserService.UserId!;
        var updated = await _scoringService.RecalculateSeasonTeamStandingsAsync(seasonId, leagueId, userId);

        return Ok(ApiResponse<int>.SuccessResponse(updated, "Overall standings recalculated."));
    }

    /// <summary>
    /// Delete an event
    /// </summary>
    [HttpDelete("{eventId}")]
    [Authorize(Policy = AuthorizationConstants.Policies.LeagueAdmin)]
    public async Task<ActionResult<ApiResponse<bool>>> DeleteEvent(string seasonId, string eventId)
    {
        var leagueId = LeagueId;
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

