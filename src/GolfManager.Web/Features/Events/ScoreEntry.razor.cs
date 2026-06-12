using Microsoft.AspNetCore.Components;
using GolfManager.Shared.DTOs.Player;
using GolfManager.Shared.DTOs.Event;

namespace GolfManager.Web.Features.Events;

public partial class ScoreEntry : ComponentBase
{
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private IEventService EventService { get; set; } = null!;
    [Inject] private IPlayerService PlayerService { get; set; } = null!;
    [Inject] private IRoundService RoundService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<ScoreEntry> Logger { get; set; } = null!;

    [Parameter] public string LeagueKey { get; set; } = string.Empty;
    [Parameter] public string SeasonKey { get; set; } = string.Empty;
    [Parameter] public string EventKey { get; set; } = string.Empty;

    [CascadingParameter] public EventLayoutContext? EventContext { get; set; }

    private LeagueResponse? league;
    private SeasonResponse? season;
    private EventResponse? seasonEvent;
    private List<PlayerResponse> golfers = new();
    private List<EventMatchupResponse> matchups = new();
    private List<MatchGroup> matchGroups = new();
    private List<PlayerResponse> ungroupedGolfers = new();
    private bool hasMatchLayout;
    private bool isLoading = true;
    private bool accessDenied;

    // Sub editing state — tracks which matchup side is being edited
    private string? subEditMatchupId;
    private bool subEditIsHome;
    private string subEditSelectedSeasonGolferId = string.Empty;
    private bool isSavingSub;

    private record MatchGroup(
        EventMatchupResponse Matchup,
        string? HomeTeamName,
        List<PlayerResponse> HomeMembers,
        PlayerResponse? HomeSub,
        string? AwayTeamName,
        List<PlayerResponse> AwayMembers,
        PlayerResponse? AwaySub);

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
            league = EventContext.League;
            season = EventContext.Season;
            seasonEvent = EventContext.Event;

            if (!EventContext.IsAdmin)
            {
                accessDenied = true;
                Logger.LogWarning("Unauthorized score entry access. User={UserEmail}, League={LeagueKey}", AuthService.UserEmail, LeagueKey);
                return;
            }

            var leagueId = league.Id;
            var seasonId = season.Id;

            var golfersTask = PlayerService.GetSeasonPlayersAsync(leagueId, seasonId);
            var matchupsTask = EventService.GetEventMatchupsAsync(leagueId, seasonId, seasonEvent.Id);
            await Task.WhenAll(golfersTask, matchupsTask);

            var golfersResult = await golfersTask;
            var matchupsResult = await matchupsTask;

            golfers = golfersResult?.Success == true && golfersResult.Data != null
                ? golfersResult.Data.ToList()
                : new();

            matchups = matchupsResult?.Success == true && matchupsResult.Data != null
                ? matchupsResult.Data
                : new();

            BuildMatchGroups();
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

    private void BuildMatchGroups()
    {
        matchGroups = new();

        if (!matchups.Any())
        {
            hasMatchLayout = false;
            ungroupedGolfers = golfers.OrderBy(g => g.DisplayName).ToList();
            return;
        }

        hasMatchLayout = true;

        // All team IDs that appear in at least one matchup — players on these teams are "grouped"
        var matchupTeamIds = matchups
            .SelectMany(m => new[] { m.HomeTeamId, m.AwayTeamId })
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        var subSeasonGolferIds = matchups
            .SelectMany(m => new[] { m.HomeSubSeasonGolferId, m.AwaySubSeasonGolferId })
            .Where(id => !string.IsNullOrWhiteSpace(id))
            .ToHashSet(StringComparer.OrdinalIgnoreCase);

        foreach (var matchup in matchups.OrderBy(m => m.StartingFlight ?? 99).ThenBy(m => m.StartingHole ?? 99))
        {
            var homeMembers = GetPlayersForTeam(matchup.HomeTeamId);
            var awayMembers = GetPlayersForTeam(matchup.AwayTeamId);

            PlayerResponse? homeSub = null;
            if (!string.IsNullOrWhiteSpace(matchup.HomeSubSeasonGolferId))
                homeSub = golfers.FirstOrDefault(g => string.Equals(g.SeasonGolferId, matchup.HomeSubSeasonGolferId, StringComparison.OrdinalIgnoreCase));

            PlayerResponse? awaySub = null;
            if (!string.IsNullOrWhiteSpace(matchup.AwaySubSeasonGolferId))
                awaySub = golfers.FirstOrDefault(g => string.Equals(g.SeasonGolferId, matchup.AwaySubSeasonGolferId, StringComparison.OrdinalIgnoreCase));

            matchGroups.Add(new MatchGroup(
                matchup,
                matchup.HomeTeamName,
                homeMembers,
                homeSub,
                matchup.AwayTeamName,
                awayMembers,
                awaySub));
        }

        // Ungrouped: players whose team is not in any matchup, and who are not a named sub
        ungroupedGolfers = golfers
            .Where(g =>
                (string.IsNullOrWhiteSpace(g.TeamId) || !matchupTeamIds.Contains(g.TeamId)) &&
                (string.IsNullOrWhiteSpace(g.SeasonGolferId) || !subSeasonGolferIds.Contains(g.SeasonGolferId)))
            .OrderBy(g => g.DisplayName)
            .ToList();
    }

    private List<PlayerResponse> GetPlayersForTeam(string? teamId)
    {
        if (string.IsNullOrWhiteSpace(teamId)) return new();
        return golfers
            .Where(g => string.Equals(g.TeamId, teamId, StringComparison.OrdinalIgnoreCase))
            .ToList();
    }

    private void BeginSubEdit(string matchupId, bool isHome)
    {
        subEditMatchupId = matchupId;
        subEditIsHome = isHome;
        var matchup = matchups.FirstOrDefault(m => m.Id == matchupId);
        subEditSelectedSeasonGolferId = (isHome ? matchup?.HomeSubSeasonGolferId : matchup?.AwaySubSeasonGolferId) ?? string.Empty;
    }

    private void CancelSubEdit()
    {
        subEditMatchupId = null;
    }

    private async Task SaveSubAsync()
    {
        if (string.IsNullOrEmpty(subEditMatchupId)) return;

        var matchup = matchups.FirstOrDefault(m => m.Id == subEditMatchupId);
        if (matchup == null) return;

        isSavingSub = true;
        try
        {
            var request = new UpdateEventMatchupRequest
            {
                HomeTeamId = matchup.HomeTeamId,
                AwayTeamId = matchup.AwayTeamId,
                HomeSubSeasonGolferId = subEditIsHome
                    ? (string.IsNullOrWhiteSpace(subEditSelectedSeasonGolferId) ? null : subEditSelectedSeasonGolferId)
                    : matchup.HomeSubSeasonGolferId,
                AwaySubSeasonGolferId = !subEditIsHome
                    ? (string.IsNullOrWhiteSpace(subEditSelectedSeasonGolferId) ? null : subEditSelectedSeasonGolferId)
                    : matchup.AwaySubSeasonGolferId,
                StartingFlight = matchup.StartingFlight,
                StartingHole = matchup.StartingHole
            };

            var result = await EventService.UpdateEventMatchupAsync(league!.Id, season!.Id, seasonEvent!.Id, matchup.Id, request);
            if (result?.Success == true && result.Data != null)
            {
                var idx = matchups.FindIndex(m => m.Id == matchup.Id);
                if (idx >= 0) matchups[idx] = result.Data;
                BuildMatchGroups();
                subEditMatchupId = null;
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving sub for matchup {MatchupId}", subEditMatchupId);
        }
        finally
        {
            isSavingSub = false;
        }
    }

    private IEnumerable<PlayerResponse> GetSubCandidates(string? teamId)
    {
        var candidates = golfers.Where(p => !string.IsNullOrWhiteSpace(p.SeasonGolferId));

        if (!string.IsNullOrWhiteSpace(teamId))
        {
            return candidates
                .Where(p => string.Equals(p.TeamId, teamId, StringComparison.OrdinalIgnoreCase))
                .Concat(candidates.Where(p => string.IsNullOrWhiteSpace(p.TeamId)))
                .GroupBy(p => p.SeasonGolferId, StringComparer.OrdinalIgnoreCase)
                .Select(g => g.First())
                .OrderBy(p => p.DisplayName);
        }

        return candidates.OrderBy(p => p.DisplayName);
    }

    private void HandleScoreChanged()
    {
        StateHasChanged();
    }
}
