using GolfManager.Shared.DTOs.Event;
using GolfManager.Shared.DTOs.Season;
using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Features.Events;

public partial class MatchDetail : ComponentBase
{
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private ILeagueService LeagueService { get; set; } = null!;
    [Inject] private ISeasonService SeasonService { get; set; } = null!;
    [Inject] private IEventService EventService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<MatchDetail> Logger { get; set; } = null!;

    [Parameter] public string LeagueKey { get; set; } = string.Empty;
    [Parameter] public string SeasonKey { get; set; } = string.Empty;
    [Parameter] public string EventKey { get; set; } = string.Empty;
    [Parameter] public string MatchupKey { get; set; } = string.Empty;

    private LeagueResponse? league;
    private SeasonResponse? season;
    private EventResponse? seasonEvent;
    private EventMatchupResponse? matchup;
    private EventMatchScoreResponse? matchScore;
    private MatchDetailResponse? matchDetail;
    private bool isLoading = true;

    private string MatchupLabel
    {
        get
        {
            if (matchup?.StartingHole == null) return string.Empty;
            var flightLetter = matchup.StartingFlight is > 1
                ? ((char)('A' + matchup.StartingFlight.Value - 1)).ToString()
                : string.Empty;
            return string.IsNullOrEmpty(flightLetter)
                ? $"Starting Hole: {matchup.StartingHole}"
                : $"Starting Hole: {matchup.StartingHole}{flightLetter}";
        }
    }

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
        isLoading = true;
        try
        {
            var leagueResponse = await LeagueService.GetLeagueByKeyAsync(LeagueKey);
            if (leagueResponse?.Success != true || leagueResponse.Data == null) return;
            league = leagueResponse.Data;

            var seasonResponse = await SeasonService.GetSeasonByKeyAsync(league.Id, SeasonKey);
            if (seasonResponse?.Success != true || seasonResponse.Data == null) return;
            season = seasonResponse.Data;

            var eventResponse = await EventService.GetEventByIdAsync(league.Id, season.Id, EventKey);
            if (eventResponse?.Success != true || eventResponse.Data == null) return;
            seasonEvent = eventResponse.Data;

            var matchupsTask = EventService.GetEventMatchupsAsync(league.Id, season.Id, seasonEvent.Id);
            var scoreboardTask = EventService.GetEventScoreboardAsync(league.Id, season.Id, seasonEvent.Id);
            var detailTask = EventService.GetMatchDetailAsync(league.Id, season.Id, seasonEvent.Id, MatchupKey);
            await Task.WhenAll(matchupsTask, scoreboardTask, detailTask);

            var matchupsResult = await matchupsTask;
            if (matchupsResult?.Success == true && matchupsResult.Data != null)
                matchup = matchupsResult.Data.FirstOrDefault(m => m.Id == MatchupKey);

            var scoreboardResult = await scoreboardTask;
            if (scoreboardResult?.Success == true && scoreboardResult.Data != null)
                matchScore = scoreboardResult.Data.Matches.FirstOrDefault(m => m.MatchupId == MatchupKey);

            var detailResult = await detailTask;
            if (detailResult?.Success == true)
                matchDetail = detailResult.Data;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading match detail for {MatchupKey}", MatchupKey);
        }
        finally
        {
            isLoading = false;
        }
    }

    private static string FormatHandicap(double? handicap)
    {
        if (!handicap.HasValue) return "—";
        return handicap.Value < 0
            ? $"+{Math.Abs(Math.Floor(handicap.Value)):F0}"
            : $"{Math.Floor(handicap.Value):F0}";
    }
}
