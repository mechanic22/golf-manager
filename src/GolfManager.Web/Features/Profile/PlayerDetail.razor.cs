using GolfManager.Shared.DTOs.Player;
using GolfManager.Shared.DTOs.Round;
using GolfManager.Web.Shared.Charts;
using Microsoft.AspNetCore.Components;
using UpdatePlayerRequest = GolfManager.Shared.DTOs.Player.UpdatePlayerRequest;

namespace GolfManager.Web.Features.Profile;

public partial class PlayerDetail : ComponentBase
{
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private ILeagueService LeagueService { get; set; } = null!;
    [Inject] private IPlayerService PlayerService { get; set; } = null!;
    [Inject] private IRoundService RoundService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<PlayerDetail> Logger { get; set; } = null!;

    [Parameter]
    public string LeagueKey { get; set; } = string.Empty;

    [Parameter]
    public string PlayerId { get; set; } = string.Empty;

    private LeagueResponse? league;
    private PlayerResponse? player;
    private List<RoundResponse> rounds = new();
    private bool isLoading = true;
    private bool isLoadingRounds = false;

    // Season filter state
    private HashSet<string> selectedSeasonIds = new();
    private bool showAllSeasons = true;

    // Edit modal state
    private bool showEditModal = false;
    private bool isSavingEdit = false;
    private string editDisplayName = string.Empty;
    private string editNickname = string.Empty;
    private double? editHandicap;
    private string editError = string.Empty;

    // Seasons derived from round data (only seasons this player has rounds in)
    private List<(string Id, string Name)> PlayerSeasons => rounds
        .Where(r => !string.IsNullOrEmpty(r.SeasonId) && !string.IsNullOrEmpty(r.SeasonName))
        .GroupBy(r => r.SeasonId!)
        .OrderByDescending(g => g.Max(r => r.RoundDate))
        .Select(g => (g.Key, g.First().SeasonName!))
        .ToList();

    private IEnumerable<RoundResponse> FilteredRounds =>
        showAllSeasons ? rounds : rounds.Where(r => selectedSeasonIds.Contains(r.SeasonId ?? ""));

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
            if (leagueResponse?.Success == true && leagueResponse.Data != null)
            {
                league = leagueResponse.Data;

                var playerResponse = await PlayerService.GetPlayerAsync(league.Id, PlayerId);
                if (playerResponse?.Success == true && playerResponse.Data != null)
                {
                    player = playerResponse.Data;
                    await LoadRounds();
                }
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading player data");
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task LoadRounds()
    {
        if (league == null || player == null) return;

        isLoadingRounds = true;
        try
        {
            rounds = await RoundService.GetGolferRoundsAsync(league.Id, player.Id);
            rounds = rounds.OrderByDescending(r => r.RoundDate).ToList();
            InitSeasonFilter();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading rounds for player {PlayerId}", PlayerId);
            rounds = new();
        }
        finally
        {
            isLoadingRounds = false;
        }
    }

    private void InitSeasonFilter()
    {
        var seasons = PlayerSeasons;
        if (seasons.Any())
        {
            selectedSeasonIds = new HashSet<string> { seasons.First().Id };
            showAllSeasons = false;
        }
        else
        {
            showAllSeasons = true;
            selectedSeasonIds = new();
        }
    }

    private void ToggleSeason(string seasonId)
    {
        if (showAllSeasons)
        {
            showAllSeasons = false;
            selectedSeasonIds = new HashSet<string> { seasonId };
            return;
        }

        if (selectedSeasonIds.Contains(seasonId))
            selectedSeasonIds.Remove(seasonId);
        else
            selectedSeasonIds.Add(seasonId);

        if (!selectedSeasonIds.Any())
            showAllSeasons = true;
    }

    private void SelectAllSeasons()
    {
        showAllSeasons = true;
        selectedSeasonIds.Clear();
    }

    private List<LineChart.ChartDataSeries> BuildScoreChartSeries() =>
        BuildChartSeries(
            FilteredRounds.OrderBy(r => r.RoundDate).ToList(),
            ("Gross", "#2563eb", r => r.TotalScore.HasValue ? (double?)r.TotalScore.Value : null),
            ("Net",   "#16a34a", r => r.NetScore.HasValue   ? (double?)r.NetScore.Value   : null)
        );

    private List<LineChart.ChartDataSeries> BuildHandicapTrendSeries() =>
        BuildChartSeries(
            FilteredRounds.OrderBy(r => r.RoundDate).ToList(),
            ("Handicap", "#9333ea", r => r.HandicapUsed)
        );

    private static List<LineChart.ChartDataSeries> BuildChartSeries(
        List<RoundResponse> orderedRounds,
        params (string Label, string Color, Func<RoundResponse, double?> GetValue)[] specs)
    {
        const int w = 600, h = 300, pad = 50;
        const double innerW = w - 2 * pad;
        const double innerH = h - 2 * pad;

        var allValues = specs
            .SelectMany(s => orderedRounds.Select(r => s.GetValue(r)).Where(v => v.HasValue).Select(v => v!.Value))
            .ToList();
        if (!allValues.Any()) return new();

        var minDate = orderedRounds.Min(r => r.RoundDate);
        var maxDate = orderedRounds.Max(r => r.RoundDate);
        var dateRange = (maxDate - minDate).TotalDays;
        if (dateRange == 0) dateRange = 1;

        var minV = Math.Floor(allValues.Min()) - 1;
        var maxV = Math.Ceiling(allValues.Max()) + 1;
        var vRange = maxV - minV;
        if (vRange == 0) vRange = 1;

        var result = new List<LineChart.ChartDataSeries>();
        foreach (var (label, color, getValue) in specs)
        {
            var points = orderedRounds
                .Select(r => (Round: r, Value: getValue(r)))
                .Where(t => t.Value.HasValue)
                .Select(t =>
                {
                    var xRatio = (t.Round.RoundDate - minDate).TotalDays / dateRange;
                    var x = pad + xRatio * innerW;
                    var y = pad + (1.0 - (t.Value!.Value - minV) / vRange) * innerH;
                    return new LineChart.ChartPoint
                    {
                        X = x, Y = y,
                        Value = t.Value.Value,
                        Date = DateOnly.FromDateTime(t.Round.RoundDate)
                    };
                })
                .ToList();

            if (points.Any())
                result.Add(new LineChart.ChartDataSeries { Label = label, Color = color, Points = points });
        }
        return result;
    }

    private void OpenEditModal()
    {
        if (player == null) return;
        editDisplayName = player.DisplayName;
        editNickname = player.Nickname ?? string.Empty;
        editHandicap = player.LeagueHandicap;
        editError = string.Empty;
        showEditModal = true;
    }

    private void CloseEditModal()
    {
        showEditModal = false;
        editError = string.Empty;
    }

    private async Task SaveEdit()
    {
        if (league == null || player == null || string.IsNullOrWhiteSpace(editDisplayName)) return;

        isSavingEdit = true;
        editError = string.Empty;
        try
        {
            var request = new UpdatePlayerRequest
            {
                DisplayName = editDisplayName.Trim(),
                Nickname = string.IsNullOrWhiteSpace(editNickname) ? null : editNickname.Trim(),
                LeagueHandicap = editHandicap
            };

            var response = await PlayerService.UpdatePlayerAsync(league.Id, player.Id, request);
            if (response?.Success == true && response.Data != null)
            {
                player = response.Data;
                showEditModal = false;
            }
            else
            {
                editError = response?.Message ?? "Failed to save changes.";
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error saving player edit");
            editError = "An unexpected error occurred.";
        }
        finally
        {
            isSavingEdit = false;
        }
    }

    private string GetInitials(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName))
            return "?";

        var parts = displayName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1)
            return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();

        return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
    }
}
