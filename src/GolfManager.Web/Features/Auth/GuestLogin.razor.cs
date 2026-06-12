using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Features.Auth;

public partial class GuestLogin : ComponentBase
{
    [Parameter]
    public string? LeagueKey { get; set; }

    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<GuestLogin> Logger { get; set; } = null!;

    private string? password;
    private string? errorMessage;
    private bool isLoading = false;
    private bool loginSuccessful = false;

    protected override async Task OnInitializedAsync()
    {
        await AuthService.InitializeAsync();

        if (AuthService.IsAuthenticated && !AuthService.IsGuest)
        {
            // Regular authenticated user doesn't need guest login
            Navigation.NavigateTo($"/league/{LeagueKey}", true);
            return;
        }

        if (AuthService.IsGuest && AuthService.GuestLeagueKey == LeagueKey)
        {
            Navigation.NavigateTo($"/league/{LeagueKey}", true);
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
                // Give the user a moment to see the success message before redirecting
                await Task.Delay(500);
                Navigation.NavigateTo($"/league/{LeagueKey}", true);
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
