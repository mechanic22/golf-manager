using GolfManager.Shared.DTOs.Round;
using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Features.Profile.Components;

public partial class ProfileRoundsTab : ComponentBase
{
    [Inject] private IUserService UserService { get; set; } = null!;
    [Inject] private AppState AppState { get; set; } = null!;
    [Inject] private ILogger<ProfileRoundsTab> Logger { get; set; } = null!;

    private const int PageSize = 10;

    private bool isLoading = true;
    private List<RoundResponse> rounds = new();
    private readonly HashSet<string> expandedRoundIds = new();

    private string selectedDateRange = "all";
    private string selectedLeagueId = "all";
    private string selectedCourse = "all";
    private string sortColumn = "date";
    private bool sortDescending = true;
    private int currentPage = 1;

    private List<(string Id, string Name)> leagueOptions = new();
    private List<string> courseOptions = new();

    private IEnumerable<RoundResponse> FilteredRounds
    {
        get
        {
            var result = rounds.AsEnumerable();

            if (selectedDateRange != "all")
            {
                var cutoff = selectedDateRange switch
                {
                    "30" => DateTime.UtcNow.AddDays(-30),
                    "90" => DateTime.UtcNow.AddDays(-90),
                    "year" => new DateTime(DateTime.UtcNow.Year, 1, 1),
                    _ => DateTime.MinValue
                };
                result = result.Where(r => r.RoundDate >= cutoff);
            }

            if (selectedLeagueId != "all")
                result = result.Where(r => r.LeagueId == selectedLeagueId);

            if (selectedCourse != "all")
                result = result.Where(r => r.CourseName == selectedCourse);

            result = sortColumn switch
            {
                "score" => sortDescending
                    ? result.OrderByDescending(r => r.TotalScore ?? 0)
                    : result.OrderBy(r => r.TotalScore ?? 0),
                _ => sortDescending
                    ? result.OrderByDescending(r => r.RoundDate)
                    : result.OrderBy(r => r.RoundDate)
            };

            return result;
        }
    }

    private List<RoundResponse> PagedRounds =>
        FilteredRounds.Skip((currentPage - 1) * PageSize).Take(PageSize).ToList();

    private int TotalFiltered => FilteredRounds.Count();
    private int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalFiltered / (double)PageSize));
    private int ShowingFrom => TotalFiltered == 0 ? 0 : (currentPage - 1) * PageSize + 1;
    private int ShowingTo => Math.Min(currentPage * PageSize, TotalFiltered);

    private record RoundGroup(string Label, string? LeagueKey, string? SeasonKey, List<RoundResponse> Rounds);

    private List<RoundGroup> GroupedRounds
    {
        get
        {
            var all = FilteredRounds.ToList();

            // Group: key = (seasonId ?? "none") + "|" + (leagueId ?? "none")
            var grouped = all
                .GroupBy(r => $"{r.SeasonId ?? "__none__"}|{r.LeagueId ?? "__none__"}")
                .Select(g =>
                {
                    var first = g.First();
                    string label;
                    if (!string.IsNullOrEmpty(first.SeasonName) && !string.IsNullOrEmpty(first.LeagueName))
                        label = $"{first.SeasonName} — {first.LeagueName}";
                    else if (!string.IsNullOrEmpty(first.LeagueName))
                        label = $"{first.LeagueName}";
                    else
                        label = "Casual Rounds";

                    return new RoundGroup(label, first.LeagueKey, first.SeasonKey, g.OrderByDescending(r => r.RoundDate).ToList());
                })
                .OrderByDescending(g => g.Rounds.Max(r => r.RoundDate))
                .ToList();

            return grouped;
        }
    }

    protected override async Task OnInitializedAsync()
    {
        await LoadRounds();
    }

    private async Task LoadRounds()
    {
        isLoading = true;
        try
        {
            var response = await UserService.GetMyRoundsAsync();
            rounds = response?.Success == true && response.Data != null
                ? response.Data
                : new List<RoundResponse>();

            BuildFilterOptions();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading rounds");
            rounds = new List<RoundResponse>();
        }
        finally
        {
            isLoading = false;
        }
    }

    private void BuildFilterOptions()
    {
        leagueOptions = rounds
            .Where(r => r.LeagueId != null)
            .GroupBy(r => r.LeagueId!)
            .Select(g => (Id: g.Key, Name: g.First().LeagueName ?? g.Key))
            .OrderBy(l => l.Name)
            .ToList();

        courseOptions = rounds
            .Where(r => !string.IsNullOrWhiteSpace(r.CourseName))
            .Select(r => r.CourseName)
            .Distinct()
            .OrderBy(c => c)
            .ToList();
    }

    private string GetLeagueName(RoundResponse round)
    {
        if (round.LeagueId == null) return "Casual Round";
        var name = AppState.UserLeagues.FirstOrDefault(l => l.LeagueId == round.LeagueId)?.LeagueName;
        return name ?? "League";
    }

    private void ToggleExpanded(string roundId)
    {
        if (!expandedRoundIds.Add(roundId))
            expandedRoundIds.Remove(roundId);
    }

    private void SortBy(string column)
    {
        if (sortColumn == column)
            sortDescending = !sortDescending;
        else
        {
            sortColumn = column;
            sortDescending = true;
        }
        currentPage = 1;
    }

    private string SortIndicator(string column) =>
        sortColumn == column ? (sortDescending ? "↓" : "↑") : "";

    private void ApplyFilters()
    {
        currentPage = 1;
        StateHasChanged();
    }

    private void PreviousPage()
    {
        if (currentPage > 1) currentPage--;
    }

    private void NextPage()
    {
        if (currentPage < TotalPages) currentPage++;
    }

    private static string GetScoreClass(int score)
    {
        if (score < 80) return "text-green-600";
        if (score < 90) return "text-primary-600";
        return "text-gray-900";
    }
}
