using GolfManager.Shared.DTOs.Season;
using GolfManager.Web.Layout;
using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Features.Season.Components;

public partial class SeasonTeamsTab : ComponentBase
{
    [Inject] private ISeasonService SeasonService { get; set; } = null!;
    [Inject] private IEventService EventService { get; set; } = null!;
    [Inject] private ILogger<SeasonTeamsTab> Logger { get; set; } = null!;

    [Parameter] public string LeagueKey { get; set; } = string.Empty;
    [Parameter] public string SeasonKey { get; set; } = string.Empty;
    [CascadingParameter] private SeasonLayoutContext? SeasonCtx { get; set; }

    private string LeagueId => SeasonCtx!.League.Id;
    private string SeasonId => SeasonCtx!.Season.Id;

    private string? _loadedSeasonId;
    private List<SeasonTeamResponse> teams = new();
    private List<EventScoreboardResponse> eventScoreboards = new();
    private bool isLoadingTeams;

    private List<TeamStandingView> RankedTeams =>
        teams
            .Select(team =>
            {
                var matches = eventScoreboards
                    .SelectMany(sb => sb.Matches)
                    .Where(m => string.Equals(m.HomeTeamId, team.Id, StringComparison.OrdinalIgnoreCase)
                             || string.Equals(m.AwayTeamId, team.Id, StringComparison.OrdinalIgnoreCase))
                    .ToList();

                var wins = 0; var losses = 0; var ties = 0;
                var pointsFor = 0d; var pointsAgainst = 0d;

                foreach (var match in matches)
                {
                    var isHome = string.Equals(match.HomeTeamId, team.Id, StringComparison.OrdinalIgnoreCase);
                    var scored  = isHome ? match.HomePoints ?? 0 : match.AwayPoints ?? 0;
                    var allowed = isHome ? match.AwayPoints ?? 0 : match.HomePoints ?? 0;
                    pointsFor += scored; pointsAgainst += allowed;
                    if (scored > allowed) wins++;
                    else if (scored < allowed) losses++;
                    else ties++;
                }

                return new TeamStandingView(team, wins, losses, ties, pointsFor, pointsAgainst, matches.Count);
            })
            .OrderByDescending(t => t.PointsFor)
            .ThenByDescending(t => t.PointDifferential)
            .ThenBy(t => t.Team.Name)
            .ToList();

    protected override async Task OnParametersSetAsync()
    {
        if (SeasonCtx?.Season.Id != null && SeasonCtx.Season.Id != _loadedSeasonId)
        {
            _loadedSeasonId = SeasonCtx.Season.Id;
            await LoadTeams();
        }
    }

    private async Task LoadTeams()
    {
        isLoadingTeams = true;
        try
        {
            var response = await SeasonService.GetSeasonTeamsAsync(LeagueId, SeasonId);
            teams = response?.Success == true && response.Data != null ? response.Data : new();

            var eventsResponse = await EventService.GetSeasonEventsAsync(LeagueId, SeasonId);
            var seasonEvents = eventsResponse?.Success == true && eventsResponse.Data != null
                ? eventsResponse.Data.OrderBy(e => e.EventDate).ToList()
                : new List<EventResponse>();

            eventScoreboards = new();
            foreach (var evt in seasonEvents)
            {
                var sb = await EventService.GetEventScoreboardAsync(LeagueId, SeasonId, evt.Id);
                if (sb is { Success: true, Data: not null })
                    eventScoreboards.Add(sb.Data);
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading teams for season {SeasonId}", SeasonId);
            teams = new();
        }
        finally
        {
            isLoadingTeams = false;
        }
    }

    private sealed record TeamStandingView(SeasonTeamResponse Team, int Wins, int Losses, int Ties, double PointsFor, double PointsAgainst, int MatchCount)
    {
        public double PointDifferential => PointsFor - PointsAgainst;
    }
}
