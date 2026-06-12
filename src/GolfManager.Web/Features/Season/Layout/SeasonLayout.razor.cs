using GolfManager.Web.Layout;
using MaterialComponents.Models;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Routing;

namespace GolfManager.Web.Features.Season.Layout;

public partial class SeasonLayout : LayoutComponentBase, IDisposable
{
    [Inject] private ILeagueService LeagueService { get; set; } = null!;
    [Inject] private ISeasonService SeasonService { get; set; } = null!;
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<SeasonLayout> Logger { get; set; } = null!;

    private SeasonLayoutContext? context;
    private bool isLoading = true;
    private string leagueKey = string.Empty;
    private string seasonKey = string.Empty;
    private string activeTab = "overview";

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

        // Expected: league/{key}/season/{key}/...
        if (parts.Length < 4
            || !string.Equals(parts[0], "league", StringComparison.OrdinalIgnoreCase)
            || !string.Equals(parts[2], "season", StringComparison.OrdinalIgnoreCase))
        {
            isLoading = false;
            return;
        }

        var newLeagueKey = parts[1];
        var newSeasonKey = parts[3];

        // Skip reload when only the sub-page changed (same season)
        if (newLeagueKey == leagueKey && newSeasonKey == seasonKey && context != null)
        {
            UpdateActiveTab();
            return;
        }

        leagueKey = newLeagueKey;
        seasonKey = newSeasonKey;
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

            var isAdmin = league.IsCurrentUserAdmin || AuthService.IsGlobalAdmin;

            context = new SeasonLayoutContext
            {
                League = league,
                Season = seasonResponse.Data,
                IsAdmin = isAdmin
            };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading season layout for {League}/{Season}", leagueKey, seasonKey);
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
        activeTab = (parts.Length > 4 ? parts[4] : "overview") switch
        {
            "events"    => "events",
            "teams"     => "teams",
            "standings" => "standings",
            "settings"  => "settings",
            "overview"  => "overview",
            _           => "overview"
        };
    }

    private List<MaterialTab> GetNavigationTabs()
    {
        var tabs = new List<MaterialTab>
        {
            new() { Value = "overview",  Label = "Overview"  },
            new() { Value = "events",    Label = "Events"    },
            new() { Value = "teams",     Label = "Teams"     },
            new() { Value = "standings", Label = "Standings" }
        };

        if (context?.IsAdmin == true)
            tabs.Add(new() { Value = "settings", Label = "Settings" });

        return tabs;
    }

    private void HandleTabChange(string tabValue)
    {
        var route = tabValue switch
        {
            "events"    => $"/league/{leagueKey}/season/{seasonKey}/events",
            "teams"     => $"/league/{leagueKey}/season/{seasonKey}/teams",
            "standings" => $"/league/{leagueKey}/season/{seasonKey}/standings",
            "settings"  => $"/league/{leagueKey}/season/{seasonKey}/settings",
            _           => $"/league/{leagueKey}/season/{seasonKey}"
        };
        Navigation.NavigateTo(route);
    }

    public void Dispose()
    {
        Navigation.LocationChanged -= OnLocationChanged;
    }
}
