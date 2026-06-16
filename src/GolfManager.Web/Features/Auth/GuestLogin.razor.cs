using GolfManager.Web.Features.League;
using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Features.Auth;

public partial class GuestLogin : ComponentBase
{
    [Parameter]
    public string? LeagueKey { get; set; }

    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private ILeagueService LeagueService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<GuestLogin> Logger { get; set; } = null!;

    private string? password;
    private string? errorMessage;
    private bool isLoading = false;
    private bool loginSuccessful = false;

    private string? leagueLogoUrl;
    private string? leagueName;

    protected override async Task OnInitializedAsync()
    {
        await AuthService.InitializeAsync();

        if (AuthService.IsAuthenticated && !AuthService.IsGuest)
        {
            Navigation.NavigateTo($"/league/{LeagueKey}", true);
            return;
        }

        if (AuthService.IsGuest && AuthService.GuestLeagueKey == LeagueKey)
        {
            Navigation.NavigateTo($"/league/{LeagueKey}/standings", true);
            return;
        }

        await LoadLeagueMetadata();
    }

    private async Task LoadLeagueMetadata()
    {
        if (string.IsNullOrWhiteSpace(LeagueKey))
            return;

        try
        {
            var response = await LeagueService.GetLeagueByKeyAsync(LeagueKey);
            if (response?.Success == true && response.Data != null)
            {
                leagueLogoUrl = response.Data.LogoUrl;
                leagueName = response.Data.Name;
            }
        }
        catch (Exception ex)
        {
            Logger.LogWarning(ex, "Could not load league metadata for guest login page");
        }
    }

    private async Task HandleGuestLogin()
    {
        if (string.IsNullOrWhiteSpace(LeagueKey) || string.IsNullOrWhiteSpace(password))
        {
            errorMessage = "League key and password are required.";
            return;
        }

        isLoading = true;
        errorMessage = null;

        try
        {
            var result = await AuthService.LoginAsGuestAsync(LeagueKey, password);

            if (result)
            {
                loginSuccessful = true;
                await Task.Delay(500);
                Navigation.NavigateTo($"/league/{LeagueKey}/standings", true);
            }
            else
            {
                errorMessage = "Invalid password or league not found. Please try again.";
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error during guest login");
            errorMessage = $"An error occurred: {ex.Message}";
        }
        finally
        {
            isLoading = false;
        }
    }
}
