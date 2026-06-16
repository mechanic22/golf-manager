using GolfManager.Shared.DTOs.Player;
using GolfManager.Shared.DTOs.Round;
using GolfManager.Shared.DTOs.Season;
using GolfManager.Web.Features.Auth;
using GolfManager.Web.Features.Profile;
using GolfManager.Web.Features.Season.Layout;
using GolfManager.Web.Shared.Charts;
using Microsoft.AspNetCore.Components;
using Microsoft.AspNetCore.Components.Rendering;

namespace GolfManager.Web.Features.Season;

public partial class SeasonPlayerDetail : ComponentBase
{
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private IPlayerService PlayerService { get; set; } = null!;
    [Inject] private IRoundService RoundService { get; set; } = null!;
    [Inject] private ISeasonService SeasonService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<SeasonPlayerDetail> Logger { get; set; } = null!;

    [Parameter] public string LeagueKey { get; set; } = string.Empty;
    [Parameter] public string SeasonKey { get; set; } = string.Empty;
    [Parameter] public string PlayerId { get; set; } = string.Empty;

    [CascadingParameter] private SeasonLayoutContext? SeasonCtx { get; set; }

    private PlayerResponse? player;
    private List<RoundResponse> allRounds = new();
    private List<RoundResponse> currentSeasonRounds = new();
    private List<RoundResponse> prevSeasonRounds = new();
    private string? prevSeasonName;
    private PlayerSeasonHoleStatsResponse? holeStats;
    private PlayerSeasonHoleStatsResponse? careerHoleStats = null; // future: career overlay from all-seasons aggregation
    private bool isLoading = true;

    // Current season stats
    private double? currentPoints;
    private int currentRounds;
    private double? currentAvgGross;
    private double? currentAvgNet;
    private int? currentBest;

    // Previous season stats
    private double? prevPoints;
    private int prevRounds;
    private double? prevAvgGross;
    private double? prevAvgNet;
    private int? prevBest;

    private string? _loadedKey;

    protected override async Task OnParametersSetAsync()
    {
        if (!AuthService.IsAuthenticated)
        {
            Navigation.NavigateTo("/login");
            return;
        }

        var loadKey = $"{PlayerId}|{SeasonCtx?.Season.Id}";
        if (loadKey == _loadedKey || SeasonCtx == null) return;
        _loadedKey = loadKey;

        await LoadData();
    }

    private async Task LoadData()
    {
        isLoading = true;
        try
        {
            var leagueId = SeasonCtx!.League.Id;
            var seasonId = SeasonCtx.Season.Id;

            var playerTask = PlayerService.GetPlayerAsync(leagueId, PlayerId);
            var roundsTask = RoundService.GetGolferRoundsAsync(leagueId, PlayerId);
            var standingsTask = SeasonService.GetSeasonStandingsAsync(leagueId, seasonId);
            var holeStatsTask = SeasonService.GetPlayerHoleStatsAsync(leagueId, seasonId, PlayerId);

            await Task.WhenAll(playerTask, roundsTask, standingsTask, holeStatsTask);

            var playerResult = await playerTask;
            player = playerResult?.Success == true ? playerResult.Data : null;

            allRounds = (await roundsTask).OrderByDescending(r => r.RoundDate).ToList();

            // Partition rounds into current and previous season
            currentSeasonRounds = allRounds
                .Where(r => string.Equals(r.SeasonId, seasonId, StringComparison.OrdinalIgnoreCase))
                .ToList();

            var otherSeasonRounds = allRounds
                .Where(r => !string.Equals(r.SeasonId, seasonId, StringComparison.OrdinalIgnoreCase)
                            && !string.IsNullOrEmpty(r.SeasonId))
                .ToList();

            // Find the most recent prior season by looking at round dates
            var prevSeasonId = otherSeasonRounds
                .GroupBy(r => r.SeasonId!)
                .OrderByDescending(g => g.Max(r => r.RoundDate))
                .FirstOrDefault()?.Key;

            if (prevSeasonId != null)
            {
                prevSeasonRounds = otherSeasonRounds.Where(r => r.SeasonId == prevSeasonId).ToList();
                prevSeasonName = prevSeasonRounds.FirstOrDefault()?.SeasonName;
            }

            // Season points from standings
            var standingsResult = await standingsTask;
            var standings = standingsResult?.Success == true ? standingsResult.Data : null;
            var myStanding = standings?.FirstOrDefault(s =>
                string.Equals(s.SeasonGolferId, player?.SeasonGolferId, StringComparison.OrdinalIgnoreCase));
            currentPoints = myStanding?.SeasonPoints;

            // Hole stats
            var holeResult = await holeStatsTask;
            holeStats = holeResult?.Success == true ? holeResult.Data : null;

            // Compute aggregated stats
            ComputeStats(currentSeasonRounds, prevSeasonRounds);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading season player detail for {PlayerId}", PlayerId);
        }
        finally
        {
            isLoading = false;
        }
    }

    private void ComputeStats(List<RoundResponse> current, List<RoundResponse> prev)
    {
        var curCompleted = current.Where(r => r.TotalScore.HasValue).ToList();
        currentRounds = curCompleted.Count;
        currentAvgGross = curCompleted.Count > 0 ? curCompleted.Average(r => (double)r.TotalScore!.Value) : null;
        currentAvgNet   = curCompleted.Count > 0 && curCompleted.Any(r => r.NetScore.HasValue)
            ? curCompleted.Where(r => r.NetScore.HasValue).Average(r => (double)r.NetScore!.Value) : null;
        currentBest = curCompleted.Count > 0 ? curCompleted.Min(r => r.TotalScore!.Value) : null;

        var prvCompleted = prev.Where(r => r.TotalScore.HasValue).ToList();
        prevRounds    = prvCompleted.Count;
        prevAvgGross  = prvCompleted.Count > 0 ? prvCompleted.Average(r => (double)r.TotalScore!.Value) : null;
        prevAvgNet    = prvCompleted.Count > 0 && prvCompleted.Any(r => r.NetScore.HasValue)
            ? prvCompleted.Where(r => r.NetScore.HasValue).Average(r => (double)r.NetScore!.Value) : null;
        prevBest      = prvCompleted.Count > 0 ? prvCompleted.Min(r => r.TotalScore!.Value) : null;
        prevPoints    = null; // previous season points would need a separate API call — skip for now
    }

    private RenderFragment ComparisonCard(string label, double? current, double? prev, int decimals, bool lowerIsBetter = false) =>
        __builder =>
        {
            var curStr = current.HasValue ? current.Value.ToString($"F{decimals}") : "—";
            var prvStr = prev.HasValue ? prev.Value.ToString($"F{decimals}") : "—";

            string deltaClass = "text-gray-400";
            string deltaText = "";
            if (current.HasValue && prev.HasValue)
            {
                var diff = current.Value - prev.Value;
                bool better = lowerIsBetter ? diff < 0 : diff > 0;
                bool worse  = lowerIsBetter ? diff > 0 : diff < 0;
                deltaClass = better ? "text-green-600" : worse ? "text-red-500" : "text-gray-400";
                deltaText  = diff == 0 ? "=" : (diff > 0 ? $"+{diff.ToString($"F{decimals}")}" : diff.ToString($"F{decimals}"));
            }

            __builder.OpenElement(0, "div");
            __builder.AddAttribute(1, "class", "bg-white border border-gray-200 rounded-lg p-4 text-center");

                __builder.OpenElement(2, "div");
                __builder.AddAttribute(3, "class", "text-xs font-medium text-gray-500 uppercase tracking-wide mb-2");
                __builder.AddContent(4, label);
                __builder.CloseElement();

                __builder.OpenElement(5, "div");
                __builder.AddAttribute(6, "class", "text-2xl font-bold text-gray-900");
                __builder.AddContent(7, curStr);
                __builder.CloseElement();

                __builder.OpenElement(8, "div");
                __builder.AddAttribute(9, "class", $"text-sm mt-1 {deltaClass} font-medium");
                __builder.AddContent(10, string.IsNullOrEmpty(deltaText) ? $"vs {prvStr}" : deltaText);
                __builder.CloseElement();

                __builder.OpenElement(11, "div");
                __builder.AddAttribute(12, "class", "text-xs text-gray-400 mt-0.5");
                __builder.AddContent(13, $"was {prvStr}");
                __builder.CloseElement();

            __builder.CloseElement();
        };

    private List<LineChart.ChartDataSeries> BuildScoreChartSeries(List<RoundResponse> rounds) =>
        BuildChartSeries(
            rounds.OrderBy(r => r.RoundDate).ToList(),
            ("Gross", "#2563eb", r => r.TotalScore.HasValue ? (double?)r.TotalScore.Value : null),
            ("Net",   "#16a34a", r => r.NetScore.HasValue   ? (double?)r.NetScore.Value   : null)
        );

    private List<LineChart.ChartDataSeries> BuildHandicapTrendSeries(List<RoundResponse> rounds) =>
        BuildChartSeries(
            rounds.OrderBy(r => r.RoundDate).ToList(),
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

    private static string GetInitials(string displayName)
    {
        if (string.IsNullOrWhiteSpace(displayName)) return "?";
        var parts = displayName.Split(' ', StringSplitOptions.RemoveEmptyEntries);
        if (parts.Length == 1) return parts[0].Substring(0, Math.Min(2, parts[0].Length)).ToUpper();
        return $"{parts[0][0]}{parts[^1][0]}".ToUpper();
    }
}
