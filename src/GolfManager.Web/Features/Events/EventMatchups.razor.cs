using GolfManager.Shared.DTOs.Event;
using GolfManager.Shared.DTOs.Season;
using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Features.Events;

public partial class EventMatchups : ComponentBase
{
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private ISeasonService SeasonService { get; set; } = null!;
    [Inject] private IEventService EventService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<EventMatchups> Logger { get; set; } = null!;

    [Parameter] public string LeagueKey { get; set; } = string.Empty;
    [Parameter] public string SeasonKey { get; set; } = string.Empty;
    [Parameter] public string EventKey { get; set; } = string.Empty;

    [CascadingParameter] public EventLayoutContext? EventContext { get; set; }

    private LeagueResponse? league;
    private SeasonResponse? season;
    private EventResponse? seasonEvent;
    private List<EventMatchupResponse> matchups = new();
    private List<SeasonTeamResponse> teams = new();
    private bool isLoading = true;
    private bool accessDenied;
    private string? errorMessage;

    // Auto-setup state
    private bool isAutoSetupInProgress;

    // Edit modal state
    private bool showEditModal;
    private EventMatchupResponse? editingMatchup;
    private string editHomeTeamId = string.Empty;
    private string editAwayTeamId = string.Empty;
    private int? editStartingFlight;
    private int? editStartingHole;
    private bool isSavingEdit;
    private string? editError;

    protected override async Task OnInitializedAsync()
    {
        if (!AuthService.IsAuthenticated)
        {
            Navigation.NavigateTo("/login");
            return;
        }

        await LoadData();
    }

    private async Task LoadData()
    {
        if (EventContext == null) return;

        isLoading = true;
        errorMessage = null;
        try
        {
            league = EventContext.League;
            season = EventContext.Season;
            seasonEvent = EventContext.Event;

            if (!EventContext.IsAdmin)
            {
                accessDenied = true;
                return;
            }

            var matchupsTask = EventService.GetEventMatchupsAsync(league.Id, season.Id, seasonEvent.Id);
            var teamsTask = SeasonService.GetSeasonTeamsAsync(league.Id, season.Id);
            await Task.WhenAll(matchupsTask, teamsTask);

            var matchupsResult = await matchupsTask;
            matchups = matchupsResult?.Success == true && matchupsResult.Data != null
                ? matchupsResult.Data.OrderBy(m => m.StartingFlight ?? 99).ThenBy(m => m.StartingHole ?? 99).ToList()
                : new();

            var teamsResult = await teamsTask;
            teams = teamsResult?.Success == true && teamsResult.Data != null ? teamsResult.Data : new();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading event matchups page");
            errorMessage = "Failed to load matchups.";
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task AutoSetup()
    {
        if (isAutoSetupInProgress || league == null || season == null || seasonEvent == null) return;
        isAutoSetupInProgress = true;
        errorMessage = null;
        try
        {
            var result = await EventService.AutoSetupEventMatchupsAsync(league.Id, season.Id, seasonEvent.Id);
            if (result?.Success == true && result.Data != null)
                matchups = result.Data.OrderBy(m => m.StartingFlight ?? 99).ThenBy(m => m.StartingHole ?? 99).ToList();
            else
                errorMessage = result?.Message ?? "Auto-setup failed.";
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error running auto-setup for event {EventId}", seasonEvent.Id);
            errorMessage = "Auto-setup failed.";
        }
        finally
        {
            isAutoSetupInProgress = false;
        }
    }

    private void OpenEditModal(EventMatchupResponse matchup)
    {
        editingMatchup = matchup;
        editHomeTeamId = matchup.HomeTeamId ?? string.Empty;
        editAwayTeamId = matchup.AwayTeamId ?? string.Empty;
        editStartingFlight = matchup.StartingFlight;
        editStartingHole = matchup.StartingHole;
        editError = null;
        showEditModal = true;
    }

    private void CloseEditModal()
    {
        showEditModal = false;
        editingMatchup = null;
        editError = null;
    }

    private async Task SaveEdit()
    {
        if (editingMatchup == null || league == null || season == null || seasonEvent == null) return;
        isSavingEdit = true;
        editError = null;
        try
        {
            var request = new UpdateEventMatchupRequest
            {
                HomeTeamId = string.IsNullOrWhiteSpace(editHomeTeamId) ? null : editHomeTeamId,
                AwayTeamId = string.IsNullOrWhiteSpace(editAwayTeamId) ? null : editAwayTeamId,
                HomeSubSeasonGolferId = editingMatchup.HomeSubSeasonGolferId,
                AwaySubSeasonGolferId = editingMatchup.AwaySubSeasonGolferId,
                StartingFlight = editStartingFlight,
                StartingHole = editStartingHole
            };

            var result = await EventService.UpdateEventMatchupAsync(league.Id, season.Id, seasonEvent.Id, editingMatchup.Id, request);
            if (result?.Success == true && result.Data != null)
            {
                var idx = matchups.FindIndex(m => m.Id == editingMatchup.Id);
                if (idx >= 0) matchups[idx] = result.Data;
                matchups = matchups.OrderBy(m => m.StartingFlight ?? 99).ThenBy(m => m.StartingHole ?? 99).ToList();
                CloseEditModal();
            }
            else
            {
                editError = result?.Message ?? "Failed to save matchup.";
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving matchup {MatchupId}", editingMatchup.Id);
            editError = "Failed to save matchup.";
        }
        finally
        {
            isSavingEdit = false;
        }
    }

    private string GetTeamName(string? teamId) =>
        string.IsNullOrWhiteSpace(teamId) ? "—" : teams.FirstOrDefault(t => t.Id == teamId)?.Name ?? teamId;

    private static string MatchupLabel(EventMatchupResponse m)
    {
        var parts = new List<string>();
        if (m.StartingFlight.HasValue) parts.Add($"Flight {m.StartingFlight}");
        if (m.StartingHole.HasValue) parts.Add($"Hole {m.StartingHole}");
        return parts.Count > 0 ? string.Join(" · ", parts) : "—";
    }
}
