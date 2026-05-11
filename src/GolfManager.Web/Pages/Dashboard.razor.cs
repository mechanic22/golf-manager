using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Pages;

public partial class Dashboard : ComponentBase
{
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private ILeagueService LeagueService { get; set; } = null!;
    [Inject] private IOneTimeEventService EventService { get; set; } = null!;
    [Inject] private IUserService UserService { get; set; } = null!;
    [Inject] private AppState AppState { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<Dashboard> Logger { get; set; } = null!;

    private List<LeagueResponse>? leagues;
    private List<OneTimeEventListResponse>? myEvents;
    private UserProfileResponse? currentUser;
    private LeagueResponse? featuredLeague;
    private bool isLoading = true;
    private bool isLoadingEvents = true;
    private bool showCreateModal = false;
    private bool showCreateEventModal = false;

    protected override async Task OnInitializedAsync()
    {
        // Check if user is authenticated
        if (!AuthService.IsAuthenticated)
        {
            Navigation.NavigateTo("/login");
            return;
        }

        await LoadUserProfile();
        await LoadLeagues();
        await LoadEvents();
    }

    private async Task LoadUserProfile()
    {
        try
        {
            var result = await UserService.GetCurrentUserAsync();
            currentUser = (result != null && result.Success && result.Data != null)
                ? result.Data
                : null;

            Logger.LogInformation("Loaded user profile: IsGolfer={IsGolfer}, GolferId={GolferId}",
                currentUser?.IsGolfer, currentUser?.GolferId);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading user profile");
            currentUser = null;
        }
    }

    private async Task LoadLeagues()
    {
        isLoading = true;
        try
        {
            // Load leagues from API
            var result = await LeagueService.GetUserLeaguesAsync();
            leagues = (result != null && result.Success && result.Data != null)
                ? result.Data
                : new List<LeagueResponse>();

            ResolveFeaturedLeague();

            Logger.LogInformation("Loaded {Count} leagues from API", leagues.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading leagues from API");
            Console.WriteLine($"Error loading leagues: {ex.Message}");
            leagues = new List<LeagueResponse>();
        }
        finally
        {
            isLoading = false;
        }
    }

    private void ShowCreateLeagueModal()
    {
        showCreateModal = true;
    }

    private async Task HandleLeagueCreated(LeagueResponse league)
    {
        Logger.LogInformation("League created successfully: {LeagueId}", league.Id);
        await LoadLeagues(); // Reload the leagues list
    }

    private void NavigateToLeague(string key)
    {
        Navigation.NavigateTo($"/league/{key}");
    }

    private async Task LoadEvents()
    {
        isLoadingEvents = true;
        try
        {
            if (currentUser != null)
            {
                var result = await EventService.GetEventsAsync(upcomingOnly: true, organizerId: currentUser.Id);
                if (result != null && result.Success && result.Data != null)
                {
                    myEvents = result.Data;
                    Logger.LogInformation("Loaded {Count} upcoming events for user {UserId}", myEvents.Count, currentUser.Id);
                }
                else
                {
                    myEvents = new List<OneTimeEventListResponse>();
                    Logger.LogWarning("Failed to load events: {Message}", result?.Message ?? "Unknown error");
                }
            }
            else
            {
                myEvents = new List<OneTimeEventListResponse>();
                Logger.LogWarning("Cannot load events: current user is null");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading events");
            myEvents = new List<OneTimeEventListResponse>();
        }
        finally
        {
            isLoadingEvents = false;
        }
    }

    private void ShowCreateEventModal()
    {
        showCreateEventModal = true;
    }

    private async Task HandleEventCreated(OneTimeEventResponse eventResponse)
    {
        Logger.LogInformation("Event created successfully: {EventId}", eventResponse.Id);
        // Navigate to the new event
        Navigation.NavigateTo($"/event/{eventResponse.Key}");
    }

    private MaterialChipColor GetStatusColor(EventStatus status)
    {
        return status switch
        {
            EventStatus.Draft => MaterialChipColor.Default,
            EventStatus.Published => MaterialChipColor.Primary,
            EventStatus.InProgress => MaterialChipColor.Info,
            EventStatus.Completed => MaterialChipColor.Success,
            EventStatus.Cancelled => MaterialChipColor.Error,
            _ => MaterialChipColor.Default
        };
    }

    private async Task HandleLogout()
    {
        await AuthService.LogoutAsync();
        Navigation.NavigateTo("/");
    }

    private string GetFirstName()
    {
        // Use the first name from auth service
        return AuthService.UserEmail?.Split('@')[0] ?? "Player";
    }

    private void ResolveFeaturedLeague()
    {
        if (leagues == null || leagues.Count == 0)
        {
            featuredLeague = null;
            return;
        }

        featuredLeague = leagues.FirstOrDefault(l =>
                !string.IsNullOrWhiteSpace(AppState.CurrentLeagueKey) &&
                string.Equals(l.Key, AppState.CurrentLeagueKey, StringComparison.OrdinalIgnoreCase))
            ?? leagues.FirstOrDefault();
    }

    private string GetWelcomeHeadline()
    {
        if (!string.IsNullOrWhiteSpace(featuredLeague?.WelcomeHeadline))
        {
            return featuredLeague.WelcomeHeadline!;
        }

        if (!string.IsNullOrWhiteSpace(featuredLeague?.Name))
        {
            return $"Welcome back, {GetFirstName()} - {featuredLeague.Name}";
        }

        return $"Welcome back, {GetFirstName()}";
    }

    private string GetWelcomeSubhead()
    {
        if (!string.IsNullOrWhiteSpace(featuredLeague?.WelcomeSubhead))
        {
            return featuredLeague.WelcomeSubhead!;
        }

        if (!string.IsNullOrWhiteSpace(featuredLeague?.Description))
        {
            return featuredLeague.Description!;
        }

        if (!string.IsNullOrWhiteSpace(featuredLeague?.Name))
        {
            return $"Today in {featuredLeague.Name}: events, standings, and your next round.";
        }

        return "Here is what is happening in your golf leagues.";
    }

    private string GetUpcomingEventsEmptyMessage()
    {
        if (!string.IsNullOrWhiteSpace(featuredLeague?.EmptyStateMessage))
        {
            return featuredLeague.EmptyStateMessage!;
        }

        if (!string.IsNullOrWhiteSpace(featuredLeague?.Name))
        {
            return $"No upcoming events in {featuredLeague.Name} yet. Create one to set the tone for the week.";
        }

        return "No upcoming events yet. Create one to get started.";
    }

    private bool HasFeaturedAnnouncement()
    {
        return !string.IsNullOrWhiteSpace(featuredLeague?.AnnouncementTitle)
            || !string.IsNullOrWhiteSpace(featuredLeague?.AnnouncementBody);
    }

    private string GetAnnouncementTitle()
    {
        if (!string.IsNullOrWhiteSpace(featuredLeague?.AnnouncementTitle))
        {
            return featuredLeague.AnnouncementTitle!;
        }

        if (!string.IsNullOrWhiteSpace(featuredLeague?.CommissionerName))
        {
            return $"Message from {featuredLeague.CommissionerName}";
        }

        return "League Announcement";
    }
}
