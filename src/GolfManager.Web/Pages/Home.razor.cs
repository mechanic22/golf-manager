using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Pages;

public partial class Home : ComponentBase
{
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private AppState AppState { get; set; } = null!;

    protected override async Task OnInitializedAsync()
    {
        await AuthService.InitializeAsync();

        if (AuthService.IsAuthenticated)
        {
            Navigation.NavigateTo(GetAuthenticatedHomeRoute(), true);
        }
    }

    private void NavigateToRegister()
    {
        Navigation.NavigateTo("/register");
    }

    private void NavigateToLogin()
    {
        Navigation.NavigateTo("/login");
    }

    private void NavigateToDashboard()
    {
        Navigation.NavigateTo(GetAuthenticatedHomeRoute());
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
