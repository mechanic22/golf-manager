using GolfManager.Shared.DTOs.Handicap;
using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Features.Profile;

public partial class ProfileHandicap : ComponentBase
{
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private IUserService UserService { get; set; } = null!;
    [Inject] private IHandicapService HandicapService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<ProfileHandicap> Logger { get; set; } = null!;

    private bool isLoading = true;
    private decimal currentHandicap;
    private DateTime? lastUpdated;
    private List<HandicapHistoryResponse> handicapHistory = [];
    private HandicapCalculationResponse? breakdown;

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
            var profile = await UserService.GetCurrentUserAsync();
            var golferId = profile?.Data?.GolferId;

            if (string.IsNullOrEmpty(golferId))
            {
                isLoading = false;
                return;
            }

            var historyTask = HandicapService.GetHandicapHistoryAsync(golferId, limit: 20);
            var breakdownTask = HandicapService.GetHandicapBreakdownAsync(golferId);
            await Task.WhenAll(historyTask, breakdownTask);

            var historyResponse = await historyTask;
            if (historyResponse?.Success == true && historyResponse.Data != null)
            {
                handicapHistory = historyResponse.Data.OrderByDescending(h => h.EffectiveDate).ToList();
                if (handicapHistory.Count > 0)
                {
                    currentHandicap = (decimal)handicapHistory[0].HandicapIndex;
                    lastUpdated = handicapHistory[0].EffectiveDate.ToDateTime(TimeOnly.MinValue);
                }
            }

            var breakdownResponse = await breakdownTask;
            if (breakdownResponse?.Success == true && breakdownResponse.Data != null)
                breakdown = breakdownResponse.Data;
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
}
