using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.Event;

namespace GolfManager.Web.Features.Events;

/// <summary>
/// Service for managing season events
/// </summary>
public interface IEventService
{
    /// <summary>
    /// Get all events for a season
    /// </summary>
    Task<ApiResponse<List<EventResponse>>?> GetSeasonEventsAsync(string leagueId, string seasonId);

    /// <summary>
    /// Get a specific event by ID
    /// </summary>
    Task<ApiResponse<EventResponse>?> GetEventByIdAsync(string leagueId, string seasonId, string eventId);

    /// <summary>
    /// Get calculated team and individual scoreboard for an event.
    /// </summary>
    Task<ApiResponse<EventScoreboardResponse>?> GetEventScoreboardAsync(string leagueId, string seasonId, string eventId);

    /// <summary>
    /// Create a new event
    /// </summary>
    Task<ApiResponse<EventResponse>?> CreateEventAsync(string leagueId, string seasonId, CreateEventRequest request);

    /// <summary>
    /// Update an existing event
    /// </summary>
    Task<ApiResponse<EventResponse>?> UpdateEventAsync(string leagueId, string seasonId, string eventId, UpdateEventRequest request);

    /// <summary>
    /// Get matchups for an event
    /// </summary>
    Task<ApiResponse<List<EventMatchupResponse>>?> GetEventMatchupsAsync(string leagueId, string seasonId, string eventId);

    /// <summary>
    /// Auto setup matchups from standings
    /// </summary>
    Task<ApiResponse<List<EventMatchupResponse>>?> AutoSetupEventMatchupsAsync(string leagueId, string seasonId, string eventId);

    /// <summary>
    /// Create or reuse next week's event from a source event and auto-generate matchups.
    /// </summary>
    Task<ApiResponse<EventResponse>?> ScheduleNextWeekFromEventAsync(string leagueId, string seasonId, string eventId);

    /// <summary>
    /// Update one matchup in an event
    /// </summary>
    Task<ApiResponse<EventMatchupResponse>?> UpdateEventMatchupAsync(string leagueId, string seasonId, string eventId, string matchupId, UpdateEventMatchupRequest request);

    /// <summary>
    /// Recalculate handicaps for golfers who played this event
    /// </summary>
    Task<ApiResponse<int>?> RecalculateEventHandicapsAsync(string leagueId, string seasonId, string eventId);

    /// <summary>
    /// Recalculate one golfer handicap for this event
    /// </summary>
    Task<ApiResponse<bool>?> RecalculateEventGolferHandicapAsync(string leagueId, string seasonId, string eventId, string golferId);

    /// <summary>
    /// Recalculate overall season standings from event results.
    /// </summary>
    Task<ApiResponse<int>?> RecalculateOverallStandingsAsync(string leagueId, string seasonId, string eventId);

    /// <summary>
    /// Delete an event
    /// </summary>
    Task<ApiResponse<bool>?> DeleteEventAsync(string leagueId, string seasonId, string eventId);

    /// <summary>
    /// Get the current user's matchup (hole assignment + opponent) for an event
    /// </summary>
    Task<ApiResponse<MyMatchupResponse?>?> GetMyMatchupAsync(string leagueId, string seasonId, string eventId);

    /// <summary>
    /// Get hole-by-hole match detail for a specific matchup.
    /// </summary>
    Task<ApiResponse<MatchDetailResponse>?> GetMatchDetailAsync(string leagueId, string seasonId, string eventId, string matchupId);
}

