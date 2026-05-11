using GolfManager.Web.Components.Icons;
using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Pages;

public partial class CreateEvent : ComponentBase
{
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private IAuthorizationService AuthorizationService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<CreateEvent> Logger { get; set; } = null!;

    private bool canCreate = false;
    private bool isSubmitting = false;
    private string? errorMessage;

    private string eventName = string.Empty;
    private string description = string.Empty;
    private DateTime eventDate = DateTime.Today.AddDays(7);
    private string teeTimeString = "09:00";
    private string courseName = string.Empty;
    private string format = "individual";
    private int? maxPlayers;
    private DateTime? registrationDeadline;

    protected override async Task OnInitializedAsync()
    {
        if (!AuthService.IsAuthenticated)
        {
            Navigation.NavigateTo("/login");
            return;
        }

        // Only Global Admins can create standalone events
        canCreate = AuthorizationService.IsGlobalAdmin();

        await Task.CompletedTask;
    }

    private async Task HandleSubmit()
    {
        if (isSubmitting) return;

        try
        {
            isSubmitting = true;
            errorMessage = null;

            // TODO: Call API to create event
            Logger.LogInformation("Creating event: {Name}", eventName);

            await Task.Delay(1000); // Simulate API call

            // Navigate to events list
            Navigation.NavigateTo("/events");
        }
        catch (Exception ex)
        {
            errorMessage = $"Failed to create event: {ex.Message}";
            Logger.LogError(ex, "Error creating event");
        }
        finally
        {
            isSubmitting = false;
        }
    }
}
