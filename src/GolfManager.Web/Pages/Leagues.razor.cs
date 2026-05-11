using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Pages;

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
            // Load all leagues from API
            var response = await LeagueService.GetUserLeaguesAsync();
            if (response != null && response.Success && response.Data != null)
            {
                // All returned leagues are user's leagues (API filters by user)
                myLeagues = response.Data;
                publicLeagues = new List<LeagueResponse>(); // TODO: Implement public leagues endpoint
            }

            // Any authenticated user can create a league and becomes its owner.
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
        // TODO: Implement search when API endpoint exists
        Logger.LogInformation("Searching for: {SearchQuery}", searchQuery);
    }

    private async Task RequestToJoin(LeagueResponse league)
    {
        // TODO: Implement join request when API endpoint exists
        Logger.LogInformation("Requesting to join league: {LeagueKey}", league.Key);
    }

    // Icon helpers
    private static RenderFragment CreateIcon() => _ => { };
    private static RenderFragment SearchIcon() => _ => { };
}
