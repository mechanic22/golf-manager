using System.Net.Http.Json;
using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.Event;

namespace GolfManager.Web.Services;

/// <summary>
/// Service for managing season events
/// </summary>
public class EventService : IEventService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<EventService> _logger;

    public EventService(HttpClient httpClient, ILogger<EventService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ApiResponse<List<EventResponse>>?> GetSeasonEventsAsync(string leagueId, string seasonId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<EventResponse>>>(
                $"api/v1/seasons/{seasonId}/events");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting events for season {SeasonId}", seasonId);
            return null;
        }
    }

    public async Task<ApiResponse<EventResponse>?> GetEventByIdAsync(string leagueId, string seasonId, string eventId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<EventResponse>>(
                $"api/v1/seasons/{seasonId}/events/{eventId}");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting event {EventId}", eventId);
            return null;
        }
    }

    public async Task<ApiResponse<EventResponse>?> CreateEventAsync(string leagueId, string seasonId, CreateEventRequest request)
    {
        try
        {
            var response = await _httpClient.PostAsJsonAsync(
                $"api/v1/seasons/{seasonId}/events", request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<EventResponse>>();
            }

            _logger.LogWarning("Failed to create event. Status: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error creating event");
            return null;
        }
    }

    public async Task<ApiResponse<EventResponse>?> UpdateEventAsync(string leagueId, string seasonId, string eventId, UpdateEventRequest request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync(
                $"api/v1/seasons/{seasonId}/events/{eventId}", request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<EventResponse>>();
            }

            _logger.LogWarning("Failed to update event {EventId}. Status: {StatusCode}", eventId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating event {EventId}", eventId);
            return null;
        }
    }

    public async Task<ApiResponse<bool>?> DeleteEventAsync(string leagueId, string seasonId, string eventId)
    {
        try
        {
            var response = await _httpClient.DeleteAsync(
                $"api/v1/seasons/{seasonId}/events/{eventId}");

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
            }

            _logger.LogWarning("Failed to delete event {EventId}. Status: {StatusCode}", eventId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting event {EventId}", eventId);
            return null;
        }
    }
}

