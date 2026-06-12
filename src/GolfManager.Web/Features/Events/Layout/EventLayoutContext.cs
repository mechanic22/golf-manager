using GolfManager.Shared.DTOs.Event;
using GolfManager.Shared.DTOs.League;
using GolfManager.Shared.DTOs.Season;

namespace GolfManager.Web.Features.Events.Layout;

/// <summary>
/// Cascaded from EventLayout to all event pages so they don't re-fetch league/season/event data.
/// </summary>
public class EventLayoutContext
{
    public required LeagueResponse League { get; init; }
    public required SeasonResponse Season { get; init; }
    public required EventResponse Event { get; init; }
    public bool IsAdmin { get; init; }
}
