using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Features.Auth;

public partial class Login : ComponentBase
{
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private AppState AppState { get; set; } = null!;

    [SupplyParameterFromQuery(Name = "returnUrl")]
    public string? ReturnUrl { get; set; }

    [SupplyParameterFromQuery(Name = "error")]
    public string? OAuthError { get; set; }

    private LoginRequest loginRequest = new();
    private string? errorMessage;
    private bool isLoading = false;
    private string guestLeagueKey = string.Empty;
    private bool showGuestForm = false;

    protected override async Task OnInitializedAsync()
    {
        await AuthService.InitializeAsync();

        if (AuthService.IsAuthenticated)
        {
            Navigation.NavigateTo(GetPostLoginRoute(), replace: true);
            return;
        }

        if (!string.IsNullOrEmpty(OAuthError))
        {
            errorMessage = OAuthError switch
            {
                "oauth_failed" => "Google sign-in failed. Please try again.",
                "no_email" => "Google did not provide an email address.",
                _ => "Sign-in failed. Please try again."
            };
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
                Navigation.NavigateTo(GetPostLoginRoute(), replace: true);
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

    private void HandleGuestAccess()
    {
        if (!string.IsNullOrWhiteSpace(guestLeagueKey))
        {
            Navigation.NavigateTo($"/league/{guestLeagueKey.Trim().ToLower()}/guest");
        }
    }

    private string GetPostLoginRoute()
    {
        // Don't send back to home/root — always go to dashboard unless a real deep link was captured
        if (!string.IsNullOrWhiteSpace(ReturnUrl) && ReturnUrl != "/" && ReturnUrl != "")
            return ReturnUrl;

        if (!string.IsNullOrWhiteSpace(AppState.CurrentLeagueKey))
            return $"/league/{AppState.CurrentLeagueKey}";

        return "/dashboard";
    }
}
