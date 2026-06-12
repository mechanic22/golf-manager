using GolfManager.Shared.DTOs.Season;
using GolfManager.Web.Layout;
using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Features.Season.Components;

public partial class SeasonStandingsTab : ComponentBase
{
    [Inject] private ISeasonService SeasonService { get; set; } = null!;
    [Inject] private ILogger<SeasonStandingsTab> Logger { get; set; } = null!;

    [Parameter] public string LeagueKey { get; set; } = string.Empty;
    [Parameter] public string SeasonKey { get; set; } = string.Empty;
    [CascadingParameter] private SeasonLayoutContext? SeasonCtx { get; set; }

    private string LeagueId => SeasonCtx!.League.Id;
    private string SeasonId => SeasonCtx!.Season.Id;

    private string? _loadedSeasonId;

    private List<PlayerStandingResponse> standings = [];
    private bool isLoading = true;

    private sealed record RankedStanding(int DisplayRank, PlayerStandingResponse Standing);

    private IEnumerable<RankedStanding> RankedStandings
    {
        get
        {
            int rank = 1;
            double? prevPoints = null;
            foreach (var s in standings)
            {
                int displayRank = 0;
                if (s.SeasonPoints.HasValue)
                {
                    if (prevPoints != s.SeasonPoints) displayRank = rank;
                    prevPoints = s.SeasonPoints;
                    rank++;
                }
                yield return new RankedStanding(displayRank, s);
            }
        }
    }

    protected override async Task OnParametersSetAsync()
    {
        if (SeasonCtx?.Season.Id != null && SeasonCtx.Season.Id != _loadedSeasonId)
        {
            _loadedSeasonId = SeasonCtx.Season.Id;
            await LoadStandings();
        }
    }

    private async Task LoadStandings()
    {
        isLoading = true;
        try
        {
            var response = await SeasonService.GetSeasonStandingsAsync(LeagueId, SeasonId);
            standings = response?.Success == true && response.Data != null
                ? response.Data
                : [];
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading season standings");
            standings = [];
        }
        finally
        {
            isLoading = false;
        }
    }
}
