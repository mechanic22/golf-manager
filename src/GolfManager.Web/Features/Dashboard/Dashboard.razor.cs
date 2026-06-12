using Microsoft.AspNetCore.Components;
using GolfManager.Shared.DTOs.Dashboard;

namespace GolfManager.Web.Features.Dashboard;

public partial class Dashboard : ComponentBase
{
    [Inject] private IDashboardService DashboardService { get; set; } = null!;
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;

    private DashboardResponse? data;
    private bool isLoading = true;
    private bool showCreateModal = false;

    protected override async Task OnInitializedAsync()
    {
        if (!AuthService.IsAuthenticated)
        {
            Navigation.NavigateTo("/login");
            return;
        }

        try
        {
            var result = await DashboardService.GetDashboardAsync();
            data = result?.Success == true ? result.Data : new DashboardResponse();
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task HandleLeagueCreated(GolfManager.Shared.DTOs.League.LeagueResponse league)
    {
        showCreateModal = false;
        var result = await DashboardService.GetDashboardAsync();
        data = result?.Success == true ? result.Data : data;
    }
}
