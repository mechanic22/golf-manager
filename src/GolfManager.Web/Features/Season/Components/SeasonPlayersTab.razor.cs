using GolfManager.Shared.DTOs.Player;
using GolfManager.Shared.DTOs.Season;
using GolfManager.Web.Layout;
using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Features.Season.Components;

public partial class SeasonPlayersTab : ComponentBase
{
    [Inject] private ISeasonService SeasonService { get; set; } = null!;
    [Inject] private IPlayerService PlayerService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<SeasonPlayersTab> Logger { get; set; } = null!;

    [Parameter] public string LeagueKey { get; set; } = string.Empty;
    [Parameter] public string SeasonKey { get; set; } = string.Empty;
    [CascadingParameter] private SeasonLayoutContext? SeasonCtx { get; set; }

    private string LeagueId => SeasonCtx!.League.Id;
    private string SeasonId => SeasonCtx!.Season.Id;

    private string? _loadedSeasonId;
    private List<PlayerResponse> golfers = new();
    private List<SeasonTeamResponse> teams = new();
    private Dictionary<string, PlayerStandingResponse> standingsByGolferId = new(StringComparer.OrdinalIgnoreCase);
    private bool isLoadingGolfers;

    protected override async Task OnParametersSetAsync()
    {
        if (SeasonCtx?.Season.Id != null && SeasonCtx.Season.Id != _loadedSeasonId)
        {
            _loadedSeasonId = SeasonCtx.Season.Id;
            await Task.WhenAll(LoadGolfers(), LoadTeams(), LoadStandings());
        }
    }

    private async Task LoadGolfers()
    {
        isLoadingGolfers = true;
        try
        {
            var response = await PlayerService.GetSeasonPlayersAsync(LeagueId, SeasonId);
            golfers = response?.Success == true && response.Data != null ? response.Data.ToList() : new();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading season golfers");
            golfers = new();
        }
        finally
        {
            isLoadingGolfers = false;
        }
    }

    private async Task LoadTeams()
    {
        var response = await SeasonService.GetSeasonTeamsAsync(LeagueId, SeasonId);
        teams = response?.Success == true && response.Data != null ? response.Data : new();
    }

    private async Task LoadStandings()
    {
        try
        {
            var response = await SeasonService.GetSeasonStandingsAsync(LeagueId, SeasonId);
            standingsByGolferId = response?.Success == true && response.Data != null
                ? response.Data
                    .Where(s => !string.IsNullOrEmpty(s.SeasonGolferId))
                    .ToDictionary(s => s.SeasonGolferId, StringComparer.OrdinalIgnoreCase)
                : new(StringComparer.OrdinalIgnoreCase);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading standings");
            standingsByGolferId = new(StringComparer.OrdinalIgnoreCase);
        }
    }

    private IEnumerable<PlayerResponse> RankedGolfers =>
        standingsByGolferId.Count > 0
            ? golfers.OrderByDescending(g => standingsByGolferId.TryGetValue(g.SeasonGolferId ?? "", out var s) ? s.SeasonPoints ?? double.MinValue : double.MinValue)
                     .ThenBy(g => standingsByGolferId.TryGetValue(g.SeasonGolferId ?? "", out var s) ? s.AverageNetScore ?? double.MaxValue : double.MaxValue)
                     .ThenBy(g => g.DisplayName)
            : golfers.OrderBy(g => g.DisplayName);

    private sealed record RankedGolferEntry(int DisplayRank, PlayerResponse Golfer);

    private IEnumerable<RankedGolferEntry> RankedGolfersWithRank
    {
        get
        {
            int rank = 1;
            double? prevPoints = null;
            foreach (var g in RankedGolfers)
            {
                var standing = standingsByGolferId.TryGetValue(g.SeasonGolferId ?? "", out var s) ? s : null;
                int displayRank = 0;
                if (standing?.SeasonPoints.HasValue == true)
                {
                    if (prevPoints != standing.SeasonPoints) displayRank = rank;
                    prevPoints = standing.SeasonPoints;
                    rank++;
                }
                yield return new RankedGolferEntry(displayRank, g);
            }
        }
    }

    private string GetTeamName(string teamId) =>
        teams.FirstOrDefault(t => t.Id == teamId)?.Name ?? teamId;
}
