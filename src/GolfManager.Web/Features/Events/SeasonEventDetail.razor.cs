using GolfManager.Shared.DTOs.Event;
using GolfManager.Shared.DTOs.Season;
using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Features.Events;

public partial class SeasonEventDetail : ComponentBase
{
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private ISeasonService SeasonService { get; set; } = null!;
    [Inject] private IEventService EventService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<SeasonEventDetail> Logger { get; set; } = null!;

    [Parameter] public string LeagueKey { get; set; } = string.Empty;
    [Parameter] public string SeasonKey { get; set; } = string.Empty;
    [Parameter] public string EventKey { get; set; } = string.Empty;

    [CascadingParameter] public EventLayoutContext? EventContext { get; set; }

    private List<EventMatchupResponse> matchups = new();
    private List<SeasonTeamResponse> teams = new();
    private bool isLoading = true;

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
        try
        {
            var matchupsTask = EventService.GetEventMatchupsAsync(
                EventContext.League.Id, EventContext.Season.Id, EventContext.Event.Id);
            var teamsTask = SeasonService.GetSeasonTeamsAsync(
                EventContext.League.Id, EventContext.Season.Id);
            await Task.WhenAll(matchupsTask, teamsTask);

            var matchupsResult = await matchupsTask;
            matchups = matchupsResult?.Success == true && matchupsResult.Data != null
                ? matchupsResult.Data.OrderBy(m => m.StartingHole ?? 99).ThenBy(m => m.StartingFlight ?? 99).ToList()
                : new();

            var teamsResult = await teamsTask;
            teams = teamsResult?.Success == true && teamsResult.Data != null ? teamsResult.Data : new();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading event detail for {EventKey}", EventKey);
        }
        finally
        {
            isLoading = false;
        }
    }

    private string GetTeamName(string? teamId) =>
        string.IsNullOrWhiteSpace(teamId) ? "—" : teams.FirstOrDefault(t => t.Id == teamId)?.Name ?? teamId;

    private static string MatchupLabel(EventMatchupResponse m)
    {
        if (!m.StartingHole.HasValue) return "—";
        var flightLetter = m.StartingFlight is > 0
            ? ((char)('A' + m.StartingFlight.Value - 1)).ToString()
            : string.Empty;
        return string.IsNullOrEmpty(flightLetter)
            ? $"{m.StartingHole}"
            : $"Flight: {m.StartingHole}{flightLetter}";
    }
}
