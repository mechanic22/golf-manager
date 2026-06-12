using Microsoft.AspNetCore.Components;
using GolfManager.Shared.DTOs.League;

namespace GolfManager.Web.Features.Season;

public partial class GuestStandings : ComponentBase
{
    [Parameter]
    public string? LeagueKey { get; set; }

    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private ILeagueService LeagueService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<GuestStandings> Logger { get; set; } = null!;

    private GuestStandingsResponse? standings;
    private bool isLoading = true;
    private string? leagueKey;

    protected override async Task OnInitializedAsync()
    {
        leagueKey = LeagueKey;

        await AuthService.InitializeAsync();

        // Verify user is guest or has permission to view
        if (!AuthService.IsGuest && !AuthService.IsAuthenticated)
        {
            Navigation.NavigateTo($"/league/{LeagueKey}/guest");
            return;
        }

        if (AuthService.IsGuest && AuthService.GuestLeagueKey != LeagueKey)
        {
            // Guest logged in to a different league
            Navigation.NavigateTo($"/league/{LeagueKey}/guest");
            return;
        }

        await LoadStandings();
    }

    private async Task LoadStandings()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(LeagueKey))
            {
                Logger.LogWarning("LeagueKey is empty");
                return;
            }

            var response = await LeagueService.GetGuestStandingsAsync(LeagueKey);

            if (response?.Success == true && response.Data != null)
            {
                standings = response.Data;
                Logger.LogInformation("Loaded standings for league: {LeagueKey}", LeagueKey);
            }
            else
            {
                Logger.LogWarning("Failed to load standings. Response: {@Response}", response);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading standings for league: {LeagueKey}", LeagueKey);
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task HandleSignIn()
    {
        await AuthService.LogoutAsync();
        Navigation.NavigateTo("/login");
    }
}
