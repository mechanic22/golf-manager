using GolfManager.Shared.DTOs.Handicap;
using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Features.Profile.Components;

public partial class ProfileHandicapTab : ComponentBase
{
    [Inject] private IUserService UserService { get; set; } = null!;
    [Inject] private IHandicapService HandicapService { get; set; } = null!;
    [Inject] private ILogger<ProfileHandicapTab> Logger { get; set; } = null!;

    private bool isLoading = true;
    private decimal currentHandicap;
    private DateTime? lastUpdated;
    private List<HandicapHistoryResponse> handicapHistory = [];

    protected override async Task OnInitializedAsync()
    {
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
                return;

            var historyResponse = await HandicapService.GetHandicapHistoryAsync(golferId, limit: 20);
            if (historyResponse?.Success == true && historyResponse.Data != null)
            {
                handicapHistory = historyResponse.Data.OrderByDescending(h => h.EffectiveDate).ToList();
                if (handicapHistory.Count > 0)
                {
                    currentHandicap = (decimal)handicapHistory[0].HandicapIndex;
                    lastUpdated = handicapHistory[0].EffectiveDate.ToDateTime(TimeOnly.MinValue);
                }
            }
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
