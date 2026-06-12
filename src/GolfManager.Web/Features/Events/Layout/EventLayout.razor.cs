using GolfManager.Web.Layout;
using MaterialComponents.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace GolfManager.Web.Features.Events.Layout;

public partial class EventLayout : LayoutComponentBase, IDisposable
{
    [Inject] private ILeagueService LeagueService { get; set; } = null!;
    [Inject] private ISeasonService SeasonService { get; set; } = null!;
    [Inject] private IEventService EventService { get; set; } = null!;
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<EventLayout> Logger { get; set; } = null!;

    private EventLayoutContext? context;
    private bool isLoading = true;
    private string leagueKey = string.Empty;
    private string seasonKey = string.Empty;
    private string eventKey = string.Empty;
    private string activeTab = "team";

    protected override async Task OnInitializedAsync()
    {
        Navigation.LocationChanged += OnLocationChanged;
        await LoadFromUrlAsync();
    }

    private async void OnLocationChanged(object? sender, LocationChangedEventArgs e)
    {
        var prevTab = activeTab;
        UpdateActiveTab();
        if (activeTab != prevTab)
            await InvokeAsync(StateHasChanged);
    }

    private async Task LoadFromUrlAsync()
    {
        var url = Navigation.ToBaseRelativePath(Navigation.Uri);
        var parts = url.Split('/', StringSplitOptions.RemoveEmptyEntries);

        // Expected: league/{key}/season/{skey}/event/{ekey}/...
        if (parts.Length < 6
            || !string.Equals(parts[0], "league", StringComparison.OrdinalIgnoreCase)
            || !string.Equals(parts[2], "season", StringComparison.OrdinalIgnoreCase)
            || !string.Equals(parts[4], "event", StringComparison.OrdinalIgnoreCase))
        {
            isLoading = false;
            return;
        }

        var newLeagueKey = parts[1];
        var newSeasonKey = parts[3];
        var newEventKey = parts[5];

        // Skip reload when only the sub-page changed (same event)
        if (newLeagueKey == leagueKey && newSeasonKey == seasonKey && newEventKey == eventKey && context != null)
        {
            UpdateActiveTab();
            return;
        }

        leagueKey = newLeagueKey;
        seasonKey = newSeasonKey;
        eventKey = newEventKey;
        isLoading = true;
        context = null;
        StateHasChanged();

        try
        {
            var leagueResponse = await LeagueService.GetLeagueByKeyAsync(leagueKey);
            if (leagueResponse?.Success != true || leagueResponse.Data == null)
            {
                Logger.LogWarning("League not found for key {Key}", leagueKey);
                return;
            }

            var league = leagueResponse.Data;

            var seasonResponse = await SeasonService.GetSeasonByKeyAsync(league.Id, seasonKey);
            if (seasonResponse?.Success != true || seasonResponse.Data == null)
            {
                Logger.LogWarning("Season not found for key {Key}", seasonKey);
                return;
            }

            var season = seasonResponse.Data;

            var eventResponse = await EventService.GetEventByIdAsync(league.Id, season.Id, eventKey);
            if (eventResponse?.Success != true || eventResponse.Data == null)
            {
                Logger.LogWarning("Event not found for key {Key}", eventKey);
                return;
            }

            var isAdmin = league.IsCurrentUserAdmin == true || AuthService.IsGlobalAdmin;

            context = new EventLayoutContext
            {
                League = league,
                Season = season,
                Event = eventResponse.Data,
                IsAdmin = isAdmin
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading event layout for {League}/{Season}/{Event}", leagueKey, seasonKey, eventKey);
        }
        finally
        {
            isLoading = false;
            UpdateActiveTab();
        }
    }

    private void UpdateActiveTab()
    {
        var url = Navigation.ToBaseRelativePath(Navigation.Uri);
        var parts = url.Split('/', StringSplitOptions.RemoveEmptyEntries);

        // URL: league/{key}/season/{skey}/event/{ekey}/{tab?}
        // parts[6] is the tab segment (if present)
        activeTab = (parts.Length > 6 ? parts[6] : "team") switch
        {
            "individual"  => "individual",
            "scores"      => "scores",
            "matchups"    => "matchups",
            "scorecards"  => "scorecards",
            _             => "team"
        };
    }

    private List<MaterialTab> GetNavigationTabs()
    {
        var tabs = new List<MaterialTab>
        {
            new() { Value = "team",       Label = "Team"       },
            new() { Value = "individual", Label = "Individual" }
        };

        if (context?.IsAdmin == true)
        {
            tabs.Add(new() { Value = "scores",     Label = "Score Entry" });
            tabs.Add(new() { Value = "matchups",   Label = "Matchups"    });
            tabs.Add(new() { Value = "scorecards", Label = "Scorecards"  });
        }

        return tabs;
    }

    private void HandleTabChange(string tabValue)
    {
        var route = tabValue switch
        {
            "individual"  => $"/league/{leagueKey}/season/{seasonKey}/event/{eventKey}/individual",
            "scores"      => $"/league/{leagueKey}/season/{seasonKey}/event/{eventKey}/scores",
            "matchups"    => $"/league/{leagueKey}/season/{seasonKey}/event/{eventKey}/matchups",
            "scorecards"  => $"/league/{leagueKey}/season/{seasonKey}/event/{eventKey}/scorecards",
            _             => $"/league/{leagueKey}/season/{seasonKey}/event/{eventKey}"
        };
        Navigation.NavigateTo(route);
    }

    public void Dispose()
    {
        Navigation.LocationChanged -= OnLocationChanged;
    }
}
