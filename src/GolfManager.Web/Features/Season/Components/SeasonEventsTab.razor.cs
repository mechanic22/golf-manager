using GolfManager.Shared.DTOs.Event;
using GolfManager.Web.Layout;
using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Features.Season.Components;

public partial class SeasonEventsTab : ComponentBase
{
    [Inject] private IEventService EventService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<SeasonEventsTab> Logger { get; set; } = null!;

    [Parameter] public string LeagueKey { get; set; } = string.Empty;
    [Parameter] public string SeasonKey { get; set; } = string.Empty;
    [CascadingParameter] private SeasonLayoutContext? SeasonCtx { get; set; }

    private string LeagueId => SeasonCtx!.League.Id;
    private string SeasonId => SeasonCtx!.Season.Id;

    private string? _loadedSeasonId;
    private List<EventResponse> events = new();
    private string statusFilter = "all";

    private List<EventResponse> FilteredEvents => statusFilter switch
    {
        "upcoming" => events.Where(e => e.EventDate.Date >= DateTime.Today).ToList(),
        "completed" => events.Where(e => e.IsLocked).ToList(),
        _ => events
    };

    protected override async Task OnParametersSetAsync()
    {
        if (SeasonCtx?.Season.Id != null && SeasonCtx.Season.Id != _loadedSeasonId)
        {
            _loadedSeasonId = SeasonCtx.Season.Id;
            await LoadEvents();
        }
    }

    private async Task LoadEvents()
    {
        try
        {
            var response = await EventService.GetSeasonEventsAsync(LeagueId, SeasonId);
            events = response?.Success == true && response.Data != null
                ? response.Data.OrderByDescending(e => e.EventDate).ToList()
                : new();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading events for season {SeasonId}", SeasonId);
            events = new();
        }
    }
}
