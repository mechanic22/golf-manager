using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Features.League;

public partial class Leagues : ComponentBase
{
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private IAuthorizationService AuthorizationService { get; set; } = null!;
    [Inject] private ILeagueService LeagueService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<Leagues> Logger { get; set; } = null!;

    private List<LeagueResponse>? myLeagues;
    private List<LeagueResponse>? publicLeagues;
    private bool isLoading = true;
    private bool canCreateLeague = false;
    private bool scrollToDiscover = false;
    private string searchQuery = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        if (!AuthService.IsAuthenticated)
        {
            Navigation.NavigateTo("/login");
            return;
        }

        await AuthorizationService.InitializeAsync();
        await LoadData();
    }

    private async Task LoadData()
    {
        isLoading = true;
        try
        {
            var myTask = LeagueService.GetUserLeaguesAsync();
            var publicTask = LeagueService.DiscoverLeaguesAsync();

            await Task.WhenAll(myTask, publicTask);

            var myResponse = await myTask;
            var publicResponse = await publicTask;

            myLeagues = myResponse?.Data ?? [];
            publicLeagues = (publicResponse?.Data ?? [])
                .Where(p => myLeagues.All(m => m.Id != p.Id))
                .ToList();

            canCreateLeague = AuthService.IsAuthenticated;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading leagues");
        }
        finally
        {
            isLoading = false;
        }
    }

    private int GetActiveSeasonCount(LeagueResponse league)
    {
        return league.SeasonCount;
    }

    private async Task SearchLeagues()
    {
        isLoading = true;
        try
        {
            var response = await LeagueService.DiscoverLeaguesAsync(
                string.IsNullOrWhiteSpace(searchQuery) ? null : searchQuery);
            var results = response?.Data ?? [];
            publicLeagues = results.Where(p => myLeagues?.All(m => m.Id != p.Id) != false).ToList();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error searching leagues");
        }
        finally
        {
            isLoading = false;
        }
    }

    private void RequestToJoin(LeagueResponse league)
    {
        // Navigate to the league page — the user can request access from there
        Navigation.NavigateTo($"/league/{league.Key}");
    }

    // Icon helpers
    private static RenderFragment CreateIcon() => _ => { };
    private static RenderFragment SearchIcon() => _ => { };
}
