using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Pages;

public partial class Events : ComponentBase
{
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private IAuthorizationService AuthorizationService { get; set; } = null!;
    [Inject] private IOneTimeEventService EventService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<Events> Logger { get; set; } = null!;

    private List<OneTimeEventListResponse>? myEvents;
    private List<OneTimeEventListResponse>? upcomingEvents;
    private bool isLoading = true;
    private bool scrollToUpcoming = false;
    private string searchQuery = string.Empty;

    protected override async Task OnInitializedAsync()
    {
        if (!AuthService.IsAuthenticated)
        {
            Navigation.NavigateTo("/login");
            return;
        }

        await AuthorizationService.InitializeAsync();
        await LoadData();
    }

    private async Task LoadData()
    {
        isLoading = true;
        try
        {
            // TODO: Load one-time events from API
            // For now, show empty lists
            myEvents = new List<OneTimeEventListResponse>();
            upcomingEvents = new List<OneTimeEventListResponse>();
            Logger.LogInformation("Events page - API integration pending");
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading events");
            myEvents = new List<OneTimeEventListResponse>();
            upcomingEvents = new List<OneTimeEventListResponse>();
        }
        finally
        {
            isLoading = false;
        }
    }

    private string GetEventStatusBadgeClass(OneTimeEventListResponse evt)
    {
        if (evt.EventDate < DateTime.Now)
            return "px-2 py-1 bg-gray-100 text-gray-700 text-xs font-medium rounded";
        if (evt.EventDate < DateTime.Now.AddDays(7))
            return "px-2 py-1 bg-yellow-100 text-yellow-700 text-xs font-medium rounded";
        return "px-2 py-1 bg-green-100 text-green-700 text-xs font-medium rounded";
    }

    private string GetEventStatusText(OneTimeEventListResponse evt)
    {
        if (evt.EventDate < DateTime.Now)
            return "Completed";
        if (evt.EventDate < DateTime.Now.AddDays(7))
            return "Soon";
        return "Upcoming";
    }

    private async Task SearchEvents()
    {
        // TODO: Implement search when API endpoint exists
        Logger.LogInformation("Searching for: {SearchQuery}", searchQuery);
    }

    // Icon helpers
    private static RenderFragment CreateIcon() => _ => { };
    private static RenderFragment SearchIcon() => _ => { };
}
