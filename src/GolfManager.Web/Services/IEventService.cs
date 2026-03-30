using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.Event;

namespace GolfManager.Web.Services;

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
    /// Create a new event
    /// </summary>
    Task<ApiResponse<EventResponse>?> CreateEventAsync(string leagueId, string seasonId, CreateEventRequest request);

    /// <summary>
    /// Update an existing event
    /// </summary>
    Task<ApiResponse<EventResponse>?> UpdateEventAsync(string leagueId, string seasonId, string eventId, UpdateEventRequest request);

    /// <summary>
    /// Delete an event
    /// </summary>
    Task<ApiResponse<bool>?> DeleteEventAsync(string leagueId, string seasonId, string eventId);
}

