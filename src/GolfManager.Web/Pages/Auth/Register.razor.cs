using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Pages.Auth;

public partial class Register : ComponentBase
{
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;

    private RegisterRequest registerRequest = new();
    private string? errorMessage;
    private bool isLoading = false;

    private async Task HandleRegister()
    {
        isLoading = true;
        errorMessage = null;

        try
        {
            var result = await AuthService.RegisterAsync(registerRequest);
            
            if (result != null)
            {
                // Registration successful, navigate to dashboard
                Navigation.NavigateTo("/dashboard");
            }
            else
            {
                errorMessage = "Registration failed. Please check your information and try again.";
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
}
