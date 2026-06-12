using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Features.Profile;

public partial class ProfileRounds : ComponentBase
{
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private IUserService UserService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<ProfileRounds> Logger { get; set; } = null!;

    private const int PageSize = 10;

    private bool isLoading = true;
    private List<RoundResponse> rounds = new();

    // Filter state
    private string selectedDateRange = "all";
    private string selectedLeagueId = "all";
    private string selectedCourse = "all";

    // Pagination
    private int currentPage = 1;

    // Derived from loaded rounds
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

            return result;
        }
    }

    private List<RoundResponse> PagedRounds =>
        FilteredRounds.Skip((currentPage - 1) * PageSize).Take(PageSize).ToList();

    private int TotalFiltered => FilteredRounds.Count();
    private int TotalPages => Math.Max(1, (int)Math.Ceiling(TotalFiltered / (double)PageSize));
    private int ShowingFrom => TotalFiltered == 0 ? 0 : (currentPage - 1) * PageSize + 1;
    private int ShowingTo => Math.Min(currentPage * PageSize, TotalFiltered);

    protected override async Task OnInitializedAsync()
    {
        if (!AuthService.IsAuthenticated)
        {
            Navigation.NavigateTo("/login");
            return;
        }

        await LoadRounds();
    }

    private async Task LoadRounds()
    {
        isLoading = true;
        try
        {
            var response = await UserService.GetMyRoundsAsync();
            rounds = (response?.Success == true && response.Data != null)
                ? response.Data
                : new List<RoundResponse>();

            BuildFilterOptions();
            Logger.LogInformation("Loaded {Count} rounds from API", rounds.Count);
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
            .Select(g => (g.Key, g.First().LeagueId ?? g.Key))
            .Distinct()
            .ToList();

        courseOptions = rounds
            .Where(r => !string.IsNullOrWhiteSpace(r.CourseName))
            .Select(r => r.CourseName)
            .Distinct()
            .OrderBy(c => c)
            .ToList();
    }

    private void ApplyFilters()
    {
        currentPage = 1;
        StateHasChanged();
    }

    private void PreviousPage()
    {
        if (currentPage > 1)
        {
            currentPage--;
            StateHasChanged();
        }
    }

    private void NextPage()
    {
        if (currentPage < TotalPages)
        {
            currentPage++;
            StateHasChanged();
        }
    }

    private string GetScoreClass(int score)
    {
        if (score < 80) return "text-sm font-semibold text-green-600";
        if (score < 90) return "text-sm font-semibold text-primary-600";
        return "text-sm font-semibold text-gray-900";
    }

    private static RenderFragment DownloadIcon() => _ => { };
}
