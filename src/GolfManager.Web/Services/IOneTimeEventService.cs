using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.OneTimeEvent;

namespace GolfManager.Web.Services;

/// <summary>
/// Service for handling one-time event operations
/// </summary>
public interface IOneTimeEventService
{
    // Event operations
    Task<ApiResponse<List<OneTimeEventListResponse>>?> GetEventsAsync(bool? publicOnly = null, bool? upcomingOnly = null, string? organizerId = null);
    Task<ApiResponse<OneTimeEventResponse>?> GetEventByIdAsync(string eventId);
    Task<ApiResponse<OneTimeEventResponse>?> GetEventByKeyAsync(string eventKey);
    Task<ApiResponse<OneTimeEventResponse>?> CreateEventAsync(CreateOneTimeEventRequest request);
    Task<ApiResponse<OneTimeEventResponse>?> UpdateEventAsync(string eventId, UpdateOneTimeEventRequest request);
    Task<ApiResponse<OneTimeEventResponse>?> PublishEventAsync(string eventId);
    Task<ApiResponse<OneTimeEventResponse>?> CancelEventAsync(string eventId);
    Task<ApiResponse<bool>?> DeleteEventAsync(string eventId);

    // Team operations
    Task<ApiResponse<List<OneTimeEventTeamResponse>>?> GetEventTeamsAsync(string eventId);
    Task<ApiResponse<OneTimeEventTeamResponse>?> GetTeamByIdAsync(string teamId);
    Task<ApiResponse<OneTimeEventTeamResponse>?> RegisterTeamAsync(string eventId, RegisterTeamRequest request);
    Task<ApiResponse<OneTimeEventTeamResponse>?> UpdateTeamAsync(string teamId, UpdateTeamRequest request);
    Task<ApiResponse<bool>?> RemoveTeamAsync(string teamId);
    Task<ApiResponse<OneTimeEventTeamResponse>?> CheckInTeamAsync(string teamId);

    // Player operations
    Task<ApiResponse<OneTimeEventPlayerResponse>?> AddPlayerAsync(string teamId, AddPlayerRequest request);
    Task<ApiResponse<OneTimeEventPlayerResponse>?> UpdatePlayerAsync(string playerId, UpdatePlayerRequest request);
    Task<ApiResponse<bool>?> RemovePlayerAsync(string playerId);
}

