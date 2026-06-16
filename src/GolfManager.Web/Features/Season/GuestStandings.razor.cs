using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Features.Season;

public partial class GuestStandings : ComponentBase
{
    [Parameter]
    public string? LeagueKey { get; set; }

    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private ILeagueService LeagueService { get; set; } = null!;
    [Inject] private AppState AppState { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<GuestStandings> Logger { get; set; } = null!;

    private bool isLoading = true;
    private bool noSeason = false;
    private string? leagueName;
    private string? logoUrl;

    protected override async Task OnInitializedAsync()
    {
        await AuthService.InitializeAsync();

        if (!AuthService.IsAuthenticated)
        {
            Navigation.NavigateTo($"/league/{LeagueKey}/guest");
            return;
        }

        if (AuthService.IsGuest && AuthService.GuestLeagueKey != LeagueKey)
        {
            Navigation.NavigateTo($"/league/{LeagueKey}/guest");
            return;
        }

        AppState.SetCurrentLeague(LeagueKey);
        await ResolveAndRedirect();
    }

    private async Task ResolveAndRedirect()
    {
        try
        {
            var response = await LeagueService.GetLeagueByKeyAsync(LeagueKey!);
            if (response?.Success == true && response.Data != null)
            {
                var league = response.Data;
                leagueName = league.Name;
                logoUrl = league.LogoUrl;
                AppState.UpdateCurrentLeagueLogoUrl(league.LogoUrl);

                if (!string.IsNullOrEmpty(league.ActiveSeasonKey))
                {
                    Navigation.NavigateTo(
                        $"/league/{LeagueKey}/season/{league.ActiveSeasonKey}/standings",
                        replace: true);
                    return;
                }
            }

            noSeason = true;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error resolving league standings for {LeagueKey}", LeagueKey);
            noSeason = true;
        }
        finally
        {
            isLoading = false;
        }
    }
}
