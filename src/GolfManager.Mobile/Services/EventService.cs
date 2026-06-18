using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.Event;

namespace GolfManager.Mobile.Services;

public class EventService : IEventService
{
    private readonly IApiService _api;

    public EventService(IApiService api) => _api = api;

    public Task<PagedResponse<EventResponse>?> GetEventsAsync(string seasonId, string leagueKey)
        => _api.GetAsync<PagedResponse<EventResponse>>($"/api/v1/seasons/{seasonId}/events", leagueKey);
}
