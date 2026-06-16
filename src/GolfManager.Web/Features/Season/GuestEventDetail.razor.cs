using Microsoft.AspNetCore.Components;
using GolfManager.Shared.DTOs.League;

namespace GolfManager.Web.Features.Season;

public partial class GuestEventDetail : ComponentBase
{
    [Parameter]
    public string? LeagueKey { get; set; }

    [Parameter]
    public string? EventId { get; set; }

    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private ILeagueService LeagueService { get; set; } = null!;
    [Inject] private AppState AppState { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<GuestEventDetail> Logger { get; set; } = null!;

    private GuestEventRow? ev;
    private bool isLoading = true;
    private string? errorMessage;

    protected override async Task OnInitializedAsync()
    {
        await AuthService.InitializeAsync();

        if (!AuthService.IsAuthenticated)
        {
            Navigation.NavigateTo("/login");
            return;
        }

        if (AuthService.IsGuest && AuthService.GuestLeagueKey != LeagueKey)
        {
            Navigation.NavigateTo($"/league/{LeagueKey}/guest");
            return;
        }

        AppState.SetCurrentLeague(LeagueKey);
        await LoadEvent();
    }

    private async Task LoadEvent()
    {
        try
        {
            if (string.IsNullOrWhiteSpace(EventId))
            {
                errorMessage = "Invalid event.";
                return;
            }

            var response = await LeagueService.GetGuestEventDetailAsync(EventId);

            if (response?.Success == true && response.Data != null)
                ev = response.Data;
            else
                errorMessage = response?.Message ?? "Unable to load event.";
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading event detail {EventId}", EventId);
            errorMessage = "An unexpected error occurred.";
        }
        finally
        {
            isLoading = false;
        }
    }

    private void NavigateToMatch(string matchupId) =>
        Navigation.NavigateTo($"/league/{LeagueKey}/match/{matchupId}");
}
