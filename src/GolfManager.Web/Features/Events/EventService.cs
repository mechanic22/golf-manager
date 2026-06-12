using System.Net.Http.Json;
using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.Event;

namespace GolfManager.Web.Features.Events;

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
            // API returns PagedResponse<EventResponse>; unwrap to a flat list for callers
            var paged = await _httpClient.GetFromJsonAsync<ApiResponse<PagedResponse<EventResponse>>>(
                $"api/v1/seasons/{seasonId}/events?pageSize=200");
            if (paged == null) return null;
            return new ApiResponse<List<EventResponse>>
            {
                Success = paged.Success,
                Message = paged.Message,
                Data = paged.Data?.Items,
                Errors = paged.Errors
            };
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

    public async Task<ApiResponse<EventScoreboardResponse>?> GetEventScoreboardAsync(string leagueId, string seasonId, string eventId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<EventScoreboardResponse>>(
                $"api/v1/seasons/{seasonId}/events/{eventId}/scoreboard");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting scoreboard for event {EventId}", eventId);
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

    public async Task<ApiResponse<List<EventMatchupResponse>>?> GetEventMatchupsAsync(string leagueId, string seasonId, string eventId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<List<EventMatchupResponse>>>(
                $"api/v1/seasons/{seasonId}/events/{eventId}/matchups");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting event matchups for event {EventId}", eventId);
            return null;
        }
    }

    public async Task<ApiResponse<List<EventMatchupResponse>>?> AutoSetupEventMatchupsAsync(string leagueId, string seasonId, string eventId)
    {
        try
        {
            var response = await _httpClient.PostAsync(
                $"api/v1/seasons/{seasonId}/events/{eventId}/matchups/auto-setup",
                content: null);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<List<EventMatchupResponse>>>();
            }

            _logger.LogWarning("Failed to auto-setup matchups for event {EventId}. Status: {StatusCode}", eventId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error auto-setting matchups for event {EventId}", eventId);
            return null;
        }
    }

    public async Task<ApiResponse<EventResponse>?> ScheduleNextWeekFromEventAsync(string leagueId, string seasonId, string eventId)
    {
        try
        {
            var response = await _httpClient.PostAsync(
                $"api/v1/seasons/{seasonId}/events/{eventId}/schedule-next-week",
                content: null);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<EventResponse>>();
            }

            _logger.LogWarning("Failed to schedule next week from event {EventId}. Status: {StatusCode}", eventId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error scheduling next week from event {EventId}", eventId);
            return null;
        }
    }

    public async Task<ApiResponse<EventMatchupResponse>?> UpdateEventMatchupAsync(string leagueId, string seasonId, string eventId, string matchupId, UpdateEventMatchupRequest request)
    {
        try
        {
            var response = await _httpClient.PutAsJsonAsync(
                $"api/v1/seasons/{seasonId}/events/{eventId}/matchups/{matchupId}", request);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<EventMatchupResponse>>();
            }

            _logger.LogWarning("Failed to update matchup {MatchupId} for event {EventId}. Status: {StatusCode}", matchupId, eventId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error updating matchup {MatchupId} for event {EventId}", matchupId, eventId);
            return null;
        }
    }

    public async Task<ApiResponse<int>?> RecalculateEventHandicapsAsync(string leagueId, string seasonId, string eventId)
    {
        try
        {
            var response = await _httpClient.PostAsync(
                $"api/v1/seasons/{seasonId}/events/{eventId}/handicaps/recalculate",
                content: null);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<int>>();
            }

            _logger.LogWarning("Failed to recalculate handicaps for event {EventId}. Status: {StatusCode}", eventId, response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recalculating handicaps for event {EventId}", eventId);
            return null;
        }
    }

    public async Task<ApiResponse<bool>?> RecalculateEventGolferHandicapAsync(string leagueId, string seasonId, string eventId, string golferId)
    {
        try
        {
            var response = await _httpClient.PostAsync(
                $"api/v1/seasons/{seasonId}/events/{eventId}/handicaps/recalculate/{golferId}",
                content: null);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<bool>>();
            }

            _logger.LogWarning(
                "Failed to recalculate handicap for golfer {GolferId} in event {EventId}. Status: {StatusCode}",
                golferId,
                eventId,
                response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recalculating handicap for golfer {GolferId} in event {EventId}", golferId, eventId);
            return null;
        }
    }

    public async Task<ApiResponse<int>?> RecalculateOverallStandingsAsync(string leagueId, string seasonId, string eventId)
    {
        try
        {
            var response = await _httpClient.PostAsync(
                $"api/v1/seasons/{seasonId}/events/{eventId}/overall/recalculate",
                content: null);

            if (response.IsSuccessStatusCode)
            {
                return await response.Content.ReadFromJsonAsync<ApiResponse<int>>();
            }

            _logger.LogWarning("Failed to recalculate overall standings for season {SeasonId} from event {EventId}. Status: {StatusCode}",
                seasonId,
                eventId,
                response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error recalculating overall standings for season {SeasonId} from event {EventId}", seasonId, eventId);
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

    public async Task<ApiResponse<MyMatchupResponse?>?> GetMyMatchupAsync(string leagueId, string seasonId, string eventId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<MyMatchupResponse?>>(
                $"api/v1/seasons/{seasonId}/events/{eventId}/my-matchup");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting my matchup for event {EventId}", eventId);
            return null;
        }
    }

    public async Task<ApiResponse<MatchDetailResponse>?> GetMatchDetailAsync(string leagueId, string seasonId, string eventId, string matchupId)
    {
        try
        {
            var response = await _httpClient.GetFromJsonAsync<ApiResponse<MatchDetailResponse>>(
                $"api/v1/seasons/{seasonId}/events/{eventId}/matchups/{matchupId}/detail");
            return response;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting match detail for matchup {MatchupId}", matchupId);
            return null;
        }
    }
}

