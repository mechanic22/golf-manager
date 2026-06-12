using GolfManager.Shared.DTOs.Event;
using GolfManager.Shared.DTOs.Season;
using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Features.Events;

public partial class TeamDetail : ComponentBase
{
    [Inject] private ILeagueService LeagueService { get; set; } = null!;
    [Inject] private ISeasonService SeasonService { get; set; } = null!;
    [Inject] private IPlayerService PlayerService { get; set; } = null!;
    [Inject] private IEventService EventService { get; set; } = null!;
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<TeamDetail> Logger { get; set; } = null!;

    [Parameter] public string LeagueKey { get; set; } = string.Empty;
    [Parameter] public string SeasonKey { get; set; } = string.Empty;
    [Parameter] public string TeamId { get; set; } = string.Empty;

    private SeasonTeamResponse? team;
    private List<PlayerStandingResponse> standings = new();
    private Dictionary<string, string> leagueGolferIdBySeasonGolferId = new(StringComparer.OrdinalIgnoreCase);
    private IReadOnlyList<(DateTime EventDate, EventMatchScoreResponse Match)> teamMatches = Array.Empty<(DateTime, EventMatchScoreResponse)>();
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
        isLoading = true;
        try
        {
            var leagueResponse = await LeagueService.GetLeagueByKeyAsync(LeagueKey);
            if (leagueResponse?.Success != true || leagueResponse.Data == null) return;
            var league = leagueResponse.Data;

            var seasonResponse = await SeasonService.GetSeasonByKeyAsync(league.Id, SeasonKey);
            if (seasonResponse?.Success != true || seasonResponse.Data == null) return;
            var seasonId = seasonResponse.Data.Id;

            var teamsTask = SeasonService.GetSeasonTeamsAsync(league.Id, seasonId);
            var standingsTask = SeasonService.GetSeasonStandingsAsync(league.Id, seasonId);
            var playersTask = PlayerService.GetSeasonPlayersAsync(league.Id, seasonId);
            var eventsTask = EventService.GetSeasonEventsAsync(league.Id, seasonId);
            await Task.WhenAll(teamsTask, standingsTask, playersTask, eventsTask);

            var allTeams = teamsTask.Result?.Success == true && teamsTask.Result.Data != null
                ? teamsTask.Result.Data : new();
            team = allTeams.FirstOrDefault(t => t.Id == TeamId);

            standings = standingsTask.Result?.Success == true && standingsTask.Result.Data != null
                ? standingsTask.Result.Data : new();

            if (playersTask.Result?.Success == true && playersTask.Result.Data != null)
            {
                leagueGolferIdBySeasonGolferId = playersTask.Result.Data
                    .Where(p => !string.IsNullOrEmpty(p.SeasonGolferId))
                    .ToDictionary(p => p.SeasonGolferId!, p => p.Id, StringComparer.OrdinalIgnoreCase);
            }

            var events = eventsTask.Result?.Success == true && eventsTask.Result.Data != null
                ? eventsTask.Result.Data : new();

            var scoreboardTasks = events
                .OrderBy(e => e.EventDate)
                .Select(e => EventService.GetEventScoreboardAsync(league.Id, seasonId, e.Id));
            var scoreboardResults = await Task.WhenAll(scoreboardTasks);

            teamMatches = scoreboardResults
                .Where(r => r?.Success == true && r.Data != null)
                .SelectMany(r => r!.Data!.Matches
                    .Where(m => string.Equals(m.HomeTeamId, TeamId, StringComparison.OrdinalIgnoreCase)
                             || string.Equals(m.AwayTeamId, TeamId, StringComparison.OrdinalIgnoreCase))
                    .Select(m => (r.Data!.EventDate, m)))
                .OrderBy(x => x.EventDate)
                .ToList();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading team detail for {TeamId}", TeamId);
        }
        finally
        {
            isLoading = false;
        }
    }

    private PlayerStandingResponse? GetStanding(string seasonGolferId) =>
        standings.FirstOrDefault(s => string.Equals(s.SeasonGolferId, seasonGolferId, StringComparison.OrdinalIgnoreCase));

    private string? GetLeagueGolferId(string seasonGolferId) =>
        leagueGolferIdBySeasonGolferId.TryGetValue(seasonGolferId, out var id) ? id : null;
}
