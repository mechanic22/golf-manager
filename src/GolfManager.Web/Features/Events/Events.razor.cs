using MaterialComponents.Models;
using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Features.Events;

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
            var allTask = EventService.GetEventsAsync(publicOnly: true, upcomingOnly: false);
            var myTask = EventService.GetEventsAsync(organizerId: AuthService.UserId);

            await Task.WhenAll(allTask, myTask);

            var allResponse = await allTask;
            var myResponse = await myTask;

            upcomingEvents = allResponse?.Data ?? [];
            myEvents = myResponse?.Data ?? [];
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading events");
            myEvents = [];
            upcomingEvents = [];
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
        isLoading = true;
        try
        {
            var response = await EventService.GetEventsAsync(publicOnly: true, upcomingOnly: false);
            var all = response?.Data ?? [];
            upcomingEvents = string.IsNullOrWhiteSpace(searchQuery)
                ? all
                : all.Where(e =>
                    e.Name.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ||
                    (e.CourseName?.Contains(searchQuery, StringComparison.OrdinalIgnoreCase) ?? false))
                  .ToList();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error searching events");
        }
        finally
        {
            isLoading = false;
        }
    }

    private static RenderFragment CreateIcon() => builder =>
    {
        builder.OpenComponent<MaterialIcon>(0);
        builder.AddAttribute(1, "Icon", MaterialIcons.Add);
        builder.CloseComponent();
    };

    private static RenderFragment SearchIcon() => builder =>
    {
        builder.OpenComponent<MaterialIcon>(0);
        builder.AddAttribute(1, "Icon", MaterialIcons.Search);
        builder.CloseComponent();
    };
}
