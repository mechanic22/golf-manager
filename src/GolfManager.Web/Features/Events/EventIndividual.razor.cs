using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Features.Events;

public partial class EventIndividual : ComponentBase
{
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private IEventService EventService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<EventIndividual> Logger { get; set; } = null!;

    [Parameter] public string LeagueKey { get; set; } = string.Empty;
    [Parameter] public string SeasonKey { get; set; } = string.Empty;
    [Parameter] public string EventKey { get; set; } = string.Empty;

    [CascadingParameter] public EventLayoutContext? EventContext { get; set; }

    private List<EventPlayerScoreResponse> players = new();
    private List<EventPlayerScoreResponse> substitutes = new();
    private bool isLoading = true;

    protected override async Task OnInitializedAsync()
    {
        if (!AuthService.IsAuthenticated)
        {
            Navigation.NavigateTo("/login");
            return;
        }

        await LoadData();
    }

    private async Task LoadData()
    {
        isLoading = true;
        try
        {
            if (EventContext == null) return;

            var result = await EventService.GetEventScoreboardAsync(
                EventContext.League.Id,
                EventContext.Season.Id,
                EventContext.Event.Id);

            if (result?.Success == true && result.Data != null)
            {
                var all = result.Data.Players
                    .OrderBy(p => p.EventPosition ?? int.MaxValue)
                    .ThenBy(p => p.NetScore ?? double.MaxValue)
                    .ToList();
                players = all.Where(p => !p.IsSubstitute).ToList();
                substitutes = all.Where(p => p.IsSubstitute).ToList();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading individual standings for {EventKey}", EventKey);
        }
        finally
        {
            isLoading = false;
        }
    }
}
