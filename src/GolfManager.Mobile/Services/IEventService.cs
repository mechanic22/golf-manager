using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.Event;

namespace GolfManager.Mobile.Services;

public interface IEventService
{
    Task<PagedResponse<EventResponse>?> GetEventsAsync(string seasonId, string leagueKey);
}
