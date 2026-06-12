using GolfManager.Shared.DTOs.User;
using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Features.Profile;

public partial class ProfileStats : ComponentBase
{
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private IUserService UserService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<ProfileStats> Logger { get; set; } = null!;

    private bool isLoading = true;
    private GolferStatsResponse? stats;

    protected override async Task OnInitializedAsync()
    {
        if (!AuthService.IsAuthenticated)
        {
            Navigation.NavigateTo("/login");
            return;
        }

        isLoading = true;
        try
        {
            var response = await UserService.GetMyStatsAsync();
            if (response?.Success == true)
            {
                stats = response.Data ?? new GolferStatsResponse();
            }
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading stats");
            stats = new GolferStatsResponse();
        }
        finally
        {
            isLoading = false;
        }
    }
}
