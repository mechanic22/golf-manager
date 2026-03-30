using GolfManager.Shared.DTOs.OneTimeEvent;

namespace GolfManager.Services.OneTimeEvent;

/// <summary>
/// Service for managing one-time events
/// </summary>
public interface IOneTimeEventService
{
    /// <summary>
    /// Get all one-time events (with optional filters)
    /// </summary>
    Task<List<OneTimeEventListResponse>> GetEventsAsync(
        bool? publicOnly = null,
        bool? upcomingOnly = null,
        string? organizerId = null);

    /// <summary>
    /// Get a one-time event by ID
    /// </summary>
    Task<OneTimeEventResponse?> GetEventByIdAsync(string eventId);

    /// <summary>
    /// Get a one-time event by key
    /// </summary>
    Task<OneTimeEventResponse?> GetEventByKeyAsync(string eventKey);

    /// <summary>
    /// Create a new one-time event
    /// </summary>
    Task<OneTimeEventResponse> CreateEventAsync(CreateOneTimeEventRequest request, string userId);

    /// <summary>
    /// Update an existing one-time event
    /// </summary>
    Task<OneTimeEventResponse> UpdateEventAsync(string eventId, UpdateOneTimeEventRequest request, string userId);

    /// <summary>
    /// Publish an event (change status from Draft to Published)
    /// </summary>
    Task<OneTimeEventResponse> PublishEventAsync(string eventId, string userId);

    /// <summary>
    /// Cancel an event
    /// </summary>
    Task<OneTimeEventResponse> CancelEventAsync(string eventId, string userId);

    /// <summary>
    /// Delete an event (soft delete)
    /// </summary>
    Task<bool> DeleteEventAsync(string eventId, string userId);

    /// <summary>
    /// Check if a user is the organizer of an event
    /// </summary>
    Task<bool> IsOrganizerAsync(string eventId, string userId);
}

