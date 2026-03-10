using GolfManager.Shared.DTOs.Event;

namespace GolfManager.Services.Event;

/// <summary>
/// Service for managing season events
/// </summary>
public interface IEventService
{
    /// <summary>
    /// Get all events for a season
    /// </summary>
    Task<List<EventResponse>> GetSeasonEventsAsync(string seasonId, string leagueId);

    /// <summary>
    /// Get an event by ID
    /// </summary>
    Task<EventResponse?> GetEventByIdAsync(string eventId, string leagueId);

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
}

