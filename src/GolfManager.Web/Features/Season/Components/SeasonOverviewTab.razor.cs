using GolfManager.Shared.DTOs.Event;
using GolfManager.Shared.DTOs.Season;
using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Features.Season.Components;

public partial class SeasonOverviewTab : ComponentBase
{
    [Inject] private ISeasonService SeasonService { get; set; } = null!;
    [Inject] private IEventService EventService { get; set; } = null!;
    [Inject] private ILogger<SeasonOverviewTab> Logger { get; set; } = null!;

    [Parameter] public string LeagueKey { get; set; } = string.Empty;
    [Parameter] public string SeasonKey { get; set; } = string.Empty;
    [CascadingParameter] private SeasonLayoutContext? SeasonCtx { get; set; }

    private string LeagueId => SeasonCtx!.League.Id;
    private string SeasonId => SeasonCtx!.Season.Id;

    private string? _loadedSeasonId;
    private bool isLoading = true;

    private List<PlayerStandingResponse> standings = [];
    private List<EventResponse> events = [];
    private List<SeasonTeamResponse> teams = [];
    private List<EventScoreboardResponse> scoreboards = [];

    private EventResponse? NextEvent =>
        events.Where(e => !e.IsLocked && e.EventDate >= DateTime.Today)
              .OrderBy(e => e.EventDate)
              .FirstOrDefault();

    private EventResponse? LastEvent =>
        events.Where(e => e.IsLocked)
              .OrderByDescending(e => e.EventDate)
              .FirstOrDefault();

    private int CompletedEventCount => events.Count(e => e.IsLocked);

    private IEnumerable<EventPlayerScoreResponse> LastEventTopScores
    {
        get
        {
            if (LastEvent == null) return [];
            var sb = scoreboards.FirstOrDefault(s => s.EventId == LastEvent.Id);
            return sb?.Players
                .Where(p => p.NetScore.HasValue)
                .OrderBy(p => p.NetScore)
                .Take(3) ?? [];
        }
    }

    private sealed record TeamStandingView(SeasonTeamResponse Team, int Wins, int Losses, int Ties, double PointsFor);

    private List<TeamStandingView> RankedTeams =>
        teams.Select(team =>
        {
            var matches = scoreboards
                .SelectMany(sb => sb.Matches)
                .Where(m => string.Equals(m.HomeTeamId, team.Id, StringComparison.OrdinalIgnoreCase)
                         || string.Equals(m.AwayTeamId, team.Id, StringComparison.OrdinalIgnoreCase))
                .ToList();

            int wins = 0, losses = 0, ties = 0;
            double pointsFor = 0;
            foreach (var match in matches)
            {
                var isHome = string.Equals(match.HomeTeamId, team.Id, StringComparison.OrdinalIgnoreCase);
                var scored = isHome ? match.HomePoints ?? 0 : match.AwayPoints ?? 0;
                var allowed = isHome ? match.AwayPoints ?? 0 : match.HomePoints ?? 0;
                pointsFor += scored;
                if (scored > allowed) wins++;
                else if (scored < allowed) losses++;
                else ties++;
            }
            return new TeamStandingView(team, wins, losses, ties, pointsFor);
        })
        .OrderByDescending(t => t.PointsFor)
        .Take(5)
        .ToList();

    private PlayerStandingResponse? PointsLeader =>
        standings.Where(s => s.SeasonPoints.HasValue).MaxBy(s => s.SeasonPoints);

    private PlayerStandingResponse? BestRoundPlayer =>
        standings.Where(s => s.BestRawScore.HasValue).MinBy(s => s.BestRawScore);

    private PlayerStandingResponse? MostRoundsPlayer =>
        standings.Where(s => s.RoundCount > 0).MaxBy(s => s.RoundCount);

    private PlayerStandingResponse? SharpestNetPlayer =>
        standings.Where(s => s.AverageNetScore.HasValue).MinBy(s => s.AverageNetScore);

    private IEnumerable<EventResponse> RecentGameOfDayEvents =>
        events.Where(e => !string.IsNullOrEmpty(e.GameOfDayWinnerDisplayName))
              .OrderByDescending(e => e.EventDate)
              .Take(3);

    private DashboardEventItem ToDashboardItem(EventResponse evt) => new(
        Name: evt.Name ?? evt.EventDate.ToString("MMM d"),
        EventDate: evt.EventDate,
        CourseName: null,
        FormatLabel: evt.ScoringFormat.ToString(),
        Url: $"/league/{LeagueKey}/season/{SeasonKey}/events/{evt.Id}",
        LeagueName: null,
        IsCompleted: evt.IsLocked,
        IsLocked: evt.IsLocked
    );

    protected override async Task OnParametersSetAsync()
    {
        if (SeasonCtx?.Season.Id != null && SeasonCtx.Season.Id != _loadedSeasonId)
        {
            _loadedSeasonId = SeasonCtx.Season.Id;
            await LoadDataAsync();
        }
    }

    private async Task LoadDataAsync()
    {
        isLoading = true;
        try
        {
            var standingsTask = SeasonService.GetSeasonStandingsAsync(LeagueId, SeasonId);
            var eventsTask = EventService.GetSeasonEventsAsync(LeagueId, SeasonId);
            var teamsTask = SeasonService.GetSeasonTeamsAsync(LeagueId, SeasonId);
            await Task.WhenAll(standingsTask, eventsTask, teamsTask);

            standings = standingsTask.Result?.Data ?? [];
            events = (eventsTask.Result?.Data ?? []).OrderBy(e => e.EventDate).ToList();
            teams = teamsTask.Result?.Data ?? [];

            var lockedEvents = events.Where(e => e.IsLocked).ToList();
            if (lockedEvents.Count > 0)
            {
                var sbResults = await Task.WhenAll(
                    lockedEvents.Select(e => EventService.GetEventScoreboardAsync(LeagueId, SeasonId, e.Id)));
                scoreboards = sbResults
                    .Where(r => r?.Success == true && r.Data != null)
                    .Select(r => r!.Data!)
                    .ToList();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading season overview for season {SeasonId}", SeasonId);
        }
        finally
        {
            isLoading = false;
        }
    }
}
