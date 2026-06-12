using GolfManager.Shared.DTOs.User;
using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Features.Profile.Components;

public partial class ProfileStatsTab : ComponentBase
{
    [Inject] private IUserService UserService { get; set; } = null!;
    [Inject] private ILogger<ProfileStatsTab> Logger { get; set; } = null!;

    private bool isLoading = true;
    private GolferStatsResponse? stats;

    private static readonly string[] ComingSoonStats =
    [
        "Fairways Hit %",
        "Greens in Regulation %",
        "Average Putts / Round",
        "Best Net Score",
        "Strokes vs. Course Rating",
        "Season-over-Season Trend"
    ];

    protected override async Task OnInitializedAsync()
    {
        await LoadStats();
    }

    private async Task LoadStats()
    {
        isLoading = true;
        try
        {
            var response = await UserService.GetMyStatsAsync();
            stats = response?.Success == true ? response.Data ?? new GolferStatsResponse() : new GolferStatsResponse();
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading profile stats");
            stats = new GolferStatsResponse();
        }
        finally
        {
            isLoading = false;
        }
    }
}
