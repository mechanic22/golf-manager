using Microsoft.AspNetCore.Components;
using GolfManager.Shared.DTOs.Player;

namespace GolfManager.Web.Pages.Season;

public partial class ScoreEntry : ComponentBase
{
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private IAuthorizationService AuthorizationService { get; set; } = null!;
    [Inject] private ILeagueService LeagueService { get; set; } = null!;
    [Inject] private ISeasonService SeasonService { get; set; } = null!;
    [Inject] private IEventService EventService { get; set; } = null!;
    [Inject] private IPlayerService PlayerService { get; set; } = null!;
    [Inject] private IRoundService RoundService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<ScoreEntry> Logger { get; set; } = null!;

    [Parameter]
    public string LeagueKey { get; set; } = string.Empty;

    [Parameter]
    public string SeasonKey { get; set; } = string.Empty;

    [Parameter]
    public string EventKey { get; set; } = string.Empty;

    private LeagueResponse? league;
    private SeasonResponse? season;
    private EventResponse? seasonEvent;
    private List<PlayerResponse> golfers = new();
    private bool isLoading = true;
    private bool accessDenied = false;

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
            // Load league from API
            var leagueResponse = await LeagueService.GetLeagueByKeyAsync(LeagueKey);
            if (leagueResponse == null || !leagueResponse.Success || leagueResponse.Data == null)
            {
                Logger.LogWarning("League not found: {LeagueKey}", LeagueKey);
                return;
            }
            league = leagueResponse.Data;

            // Load season from API
            var seasonResponse = await SeasonService.GetSeasonByKeyAsync(league.Id, SeasonKey);
            if (seasonResponse == null || !seasonResponse.Success || seasonResponse.Data == null)
            {
                Logger.LogWarning("Season not found: {SeasonKey}", SeasonKey);
                return;
            }
            season = seasonResponse.Data;

            if (!(league?.IsCurrentUserAdmin == true || AuthService.IsGlobalAdmin))
            {
                accessDenied = true;
                Logger.LogWarning("Unauthorized score entry access attempt. UserEmail={UserEmail}, LeagueKey={LeagueKey}, SeasonKey={SeasonKey}, EventKey={EventKey}",
                    AuthService.UserEmail, LeagueKey, SeasonKey, EventKey);
                return;
            }

            var leagueId = league!.Id;
            var seasonId = season!.Id;

            // Load event details (EventKey currently carries the event ID from season pages)
            var eventResponse = await EventService.GetEventByIdAsync(leagueId, seasonId, EventKey);
            if (eventResponse == null || !eventResponse.Success || eventResponse.Data == null)
            {
                Logger.LogWarning("Event not found: {EventKey}", EventKey);
                return;
            }
            seasonEvent = eventResponse.Data;

            if (!string.Equals(seasonEvent.SeasonId, seasonId, StringComparison.OrdinalIgnoreCase))
            {
                Logger.LogWarning(
                    "Event {EventId} does not belong to requested season {SeasonId}. Actual season: {ActualSeasonId}",
                    EventKey,
                    seasonId,
                    seasonEvent.SeasonId);
                seasonEvent = null;
                return;
            }

            // Load golfers participating in this season
            var golfersResponse = await PlayerService.GetSeasonPlayersAsync(leagueId, seasonId);
            if (golfersResponse?.Success == true && golfersResponse.Data != null)
            {
                golfers = golfersResponse.Data.ToList();
            }
            else
            {
                golfers = new List<PlayerResponse>();
                Logger.LogWarning("Failed to load golfers for season {SeasonId}: {Message}", seasonId, golfersResponse?.Message ?? "Unknown error");
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading score entry data");
        }
        finally
        {
            isLoading = false;
        }
    }

    private void HandleScoreChanged()
    {
        // Trigger re-render when scores change
        StateHasChanged();
    }
}
