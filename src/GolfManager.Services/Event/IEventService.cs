using GolfManager.Shared.DTOs.Event;

namespace GolfManager.Services.Event;

/// <summary>
/// Service for managing season events
/// </summary>
public interface IEventService
{
    /// <summary>

    /// <summary>
    /// Recalculate season team standings fields from all event scoreboards.
    /// </summary>
    Task<int> RecalculateSeasonTeamStandingsAsync(string seasonId, string leagueId, string userId);
    /// Get all events for a season
    /// </summary>
    Task<List<EventResponse>> GetSeasonEventsAsync(string seasonId, string leagueId);

    /// <summary>
    /// Get an event by ID
    /// </summary>
    Task<EventResponse?> GetEventByIdAsync(string eventId, string leagueId);

    /// <summary>
    /// Get calculated team and individual scoring summary for an event.
    /// </summary>
    Task<EventScoreboardResponse> GetEventScoreboardAsync(string seasonId, string eventId, string leagueId);

    /// <summary>
    /// Create a new event
    /// </summary>
    Task<EventResponse> CreateEventAsync(CreateEventRequest request, string seasonId, string leagueId, string userId);

    /// <summary>
    /// Update an existing event
    /// </summary>
    Task<EventResponse> UpdateEventAsync(string eventId, UpdateEventRequest request, string leagueId, string userId);

    /// <summary>
    /// Delete an event
    /// </summary>
    Task<bool> DeleteEventAsync(string eventId, string leagueId, string userId);

    /// <summary>
    /// Get matchups for an event
    /// </summary>
    Task<List<EventMatchupResponse>> GetEventMatchupsAsync(string seasonId, string eventId, string leagueId);

    /// <summary>
    /// Auto-setup matchups from standings
    /// </summary>
    Task<List<EventMatchupResponse>> AutoSetupEventMatchupsFromStandingsAsync(string seasonId, string eventId, string leagueId, string userId);

    /// <summary>
    /// Create or reuse next week's event based on a source event and auto-setup its matchups from current standings.
    /// </summary>
    Task<EventResponse> ScheduleNextWeekFromEventAsync(string seasonId, string eventId, string leagueId, string userId);

    /// <summary>
    /// Update an existing matchup
    /// </summary>
    Task<EventMatchupResponse> UpdateEventMatchupAsync(string seasonId, string eventId, string matchupId, UpdateEventMatchupRequest request, string leagueId, string userId);

    /// <summary>
    /// Recalculate handicaps for all golfers who posted rounds for an event.
    /// </summary>
    Task<int> RecalculateEventHandicapsAsync(string seasonId, string eventId, string leagueId, string userId);

    /// <summary>
    /// Recalculate handicap for one golfer after event score changes.
    /// </summary>
    Task<bool> RecalculateEventGolferHandicapAsync(string seasonId, string eventId, string golferId, string leagueId, string userId);
}

