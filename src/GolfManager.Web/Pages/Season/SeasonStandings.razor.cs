using Microsoft.AspNetCore.Components;
using GolfManager.Shared.DTOs.Player;

namespace GolfManager.Web.Pages.Season;

public partial class SeasonStandings : ComponentBase
{
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private IAuthorizationService AuthorizationService { get; set; } = null!;
    [Inject] private ILeagueService LeagueService { get; set; } = null!;
    [Inject] private ISeasonService SeasonService { get; set; } = null!;
    [Inject] private IPlayerService PlayerService { get; set; } = null!;
    [Inject] private IEventService EventService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<SeasonStandings> Logger { get; set; } = null!;

    [Parameter]
    public string LeagueKey { get; set; } = string.Empty;

    [Parameter]
    public string SeasonKey { get; set; } = string.Empty;

    private LeagueResponse? league;
    private SeasonResponse? season;
    private List<PlayerResponse> golfers = new();
    private List<EventResponse> seasonEvents = new();
    private List<EventScoreboardResponse> eventScoreboards = new();
    private bool isLoading = true;

    // Tab navigation
    private string activeTab = "standings";

    protected override Task OnInitializedAsync()
    {
        Navigation.NavigateTo($"/league/{LeagueKey}/season/{SeasonKey}/players", replace: true);
        return Task.CompletedTask;
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

            await LoadStandings();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading data");
        }
        finally
        {
            isLoading = false;
        }
    }

    private bool CanManageSeason => league?.IsCurrentUserAdmin == true || AuthService.IsGlobalAdmin;

    private async Task LoadStandings()
    {
        if (league == null || season == null)
        {
            golfers = new List<PlayerResponse>();
            return;
        }

        try
        {
            var response = await PlayerService.GetSeasonPlayersAsync(league.Id, season.Id);
            golfers = response is { Success: true, Data: not null }
                ? response.Data.ToList()
                : new List<PlayerResponse>();

            var eventsResponse = await EventService.GetSeasonEventsAsync(league.Id, season.Id);
            seasonEvents = eventsResponse is { Success: true, Data: not null }
                ? eventsResponse.Data.OrderBy(e => e.EventDate).ToList()
                : new List<EventResponse>();

            eventScoreboards = new List<EventScoreboardResponse>();
            foreach (var seasonEvent in seasonEvents)
            {
                var scoreboardResponse = await EventService.GetEventScoreboardAsync(league.Id, season.Id, seasonEvent.Id);
                if (scoreboardResponse is { Success: true, Data: not null })
                {
                    eventScoreboards.Add(scoreboardResponse.Data);
                }
            }

            Logger.LogInformation("Loaded {Count} golfers for standings", golfers.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading season standings");
            golfers = new List<PlayerResponse>();
        }
    }

    private List<StandingRow> RankedStandings
    {
        get
        {
            var aggregates = BuildPlayerAggregates();

            var withScores = aggregates
                .Where(g => g.Points.HasValue || g.RoundCount > 0)
                .OrderByDescending(g => g.Points ?? double.MinValue)
                .ThenBy(g => g.AverageNetScore ?? double.MaxValue)
                .ThenBy(g => g.Player.DisplayName)
                .ToList();

            var withoutScores = aggregates
                .Where(g => !g.Points.HasValue && g.RoundCount == 0)
                .OrderBy(g => g.Player.DisplayName)
                .ToList();

            var ranked = new List<StandingRow>(golfers.Count);
            var currentRank = 1;

            foreach (var golfer in withScores)
            {
                ranked.Add(new StandingRow(currentRank, golfer));
                currentRank++;
            }

            foreach (var golfer in withoutScores)
            {
                ranked.Add(new StandingRow(null, golfer));
            }

            return ranked;
        }
    }

    private int TotalRoundsPlayed => BuildPlayerAggregates().Sum(g => g.RoundCount);

    private string? LeaderName => RankedStandings.FirstOrDefault(r => r.Rank.HasValue)?.Aggregate.Player.DisplayName;

    private List<PlayerStandingAggregate> BuildPlayerAggregates()
    {
        var scoreboardPlayers = eventScoreboards.SelectMany(s => s.Players).ToList();

        return golfers.Select(player =>
        {
            var playerScores = scoreboardPlayers
                .Where(sp => string.Equals(sp.SeasonGolferId, player.SeasonGolferId, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var completedRounds = playerScores.Where(sp => sp.RawScore.HasValue).ToList();
            var totalPoints = playerScores.Where(sp => sp.EventPoints.HasValue).Sum(sp => sp.EventPoints ?? 0);
            var hasPoints = playerScores.Any(sp => sp.EventPoints.HasValue);

            return new PlayerStandingAggregate(
                player,
                hasPoints ? totalPoints : null,
                completedRounds.Count,
                completedRounds.Count > 0 ? completedRounds.Average(sp => sp.NetScore ?? sp.RawScore ?? 0) : null,
                completedRounds.Count > 0 ? completedRounds.Min(sp => sp.RawScore ?? int.MaxValue) : null);
        }).ToList();
    }

    private string GetInitials(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
        {
            return "?";
        }

        var parts = displayName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1)
        {
            return parts[0][0].ToString().ToUpperInvariant();
        }

        return (parts[0][0].ToString() + parts[^1][0].ToString()).ToUpperInvariant();
    }

    private sealed record PlayerStandingAggregate(
        PlayerResponse Player,
        double? Points,
        int RoundCount,
        double? AverageNetScore,
        int? BestRawScore);

    private sealed record StandingRow(int? Rank, PlayerStandingAggregate Aggregate);

    private List<MaterialTab> GetNavigationTabs()
    {
        var tabs = new List<MaterialTab>
        {
            new MaterialTab
            {
                Value = "overview",
                Label = "Overview"
            },
            new MaterialTab
            {
                Value = "events",
                Label = "Events"
            },
            new MaterialTab
            {
                Value = "players",
                Label = "Players"
            },
            new MaterialTab
            {
                Value = "teams",
                Label = "Teams"
            }
        };

        // Only show Settings tab to users who can manage this season
        if (CanManageSeason)
        {
            tabs.Add(new MaterialTab
            {
                Value = "settings",
                Label = "Settings"
            });
        }

        return tabs;
    }

    private void HandleTabChange(string tabValue)
    {
        var route = tabValue switch
        {
            "overview" => $"/league/{LeagueKey}/season/{SeasonKey}",
            "events" => $"/league/{LeagueKey}/season/{SeasonKey}/events",
            "players" => $"/league/{LeagueKey}/season/{SeasonKey}/players",
            "teams" => $"/league/{LeagueKey}/season/{SeasonKey}/teams",
            "settings" => $"/league/{LeagueKey}/season/{SeasonKey}/settings",
            _ => $"/league/{LeagueKey}/season/{SeasonKey}"
        };

        Navigation.NavigateTo(route);
    }
}
