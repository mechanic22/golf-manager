using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Pages.Profile;

public partial class ProfileHandicap : ComponentBase
{
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<ProfileHandicap> Logger { get; set; } = null!;

    private bool isLoading = true;
    private decimal currentHandicap = 12.5m;
    private DateTime lastUpdated = DateTime.Now.AddDays(-3);
    private List<HandicapRound> handicapRounds = new();

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
            // TODO: Load actual handicap data from API
            handicapRounds = new List<HandicapRound>
            {
                new() { CourseName = "Pebble Beach", Date = DateTime.Now.AddDays(-5), Score = 82, Differential = 10.2m },
                new() { CourseName = "Augusta National", Date = DateTime.Now.AddDays(-12), Score = 85, Differential = 11.5m },
                new() { CourseName = "St. Andrews", Date = DateTime.Now.AddDays(-19), Score = 79, Differential = 9.1m },
                new() { CourseName = "Pinehurst No. 2", Date = DateTime.Now.AddDays(-26), Score = 88, Differential = 12.8m },
                new() { CourseName = "Oakmont", Date = DateTime.Now.AddDays(-33), Score = 81, Differential = 10.5m },
                new() { CourseName = "Shinnecock Hills", Date = DateTime.Now.AddDays(-40), Score = 83, Differential = 11.0m },
                new() { CourseName = "Bethpage Black", Date = DateTime.Now.AddDays(-47), Score = 86, Differential = 12.1m },
                new() { CourseName = "Torrey Pines", Date = DateTime.Now.AddDays(-54), Score = 80, Differential = 9.8m }
            };

            await Task.CompletedTask;
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading handicap history");
        }
        finally
        {
            isLoading = false;
        }
    }

    private class HandicapRound
    {
        public string CourseName { get; set; } = string.Empty;
        public DateTime Date { get; set; }
        public int Score { get; set; }
        public decimal Differential { get; set; }
    }
}
