using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Pages.Auth;

public partial class Login : ComponentBase
{
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private AppState AppState { get; set; } = null!;

    private LoginRequest loginRequest = new();
    private string? errorMessage;
    private bool isLoading = false;

    protected override async Task OnInitializedAsync()
    {
        await AuthService.InitializeAsync();

        if (AuthService.IsAuthenticated)
        {
            Navigation.NavigateTo(GetAuthenticatedHomeRoute(), true);
        }
    }

    private async Task HandleLogin()
    {
        isLoading = true;
        errorMessage = null;

        try
        {
            var result = await AuthService.LoginAsync(loginRequest);
            
            if (result != null)
            {
                // Login successful, navigate to app home
                Navigation.NavigateTo(GetAuthenticatedHomeRoute(), true);
            }
            else
            {
                errorMessage = "Invalid email or password. Please try again.";
            }
        }
        catch (Exception ex)
        {
            errorMessage = $"An error occurred: {ex.Message}";
        }
        finally
        {
            isLoading = false;
        }
    }

    private string GetAuthenticatedHomeRoute()
    {
        if (!string.IsNullOrWhiteSpace(AppState.CurrentLeagueKey))
        {
            return $"/league/{AppState.CurrentLeagueKey}";
        }

        return "/dashboard";
    }
}
