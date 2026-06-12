using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.Dashboard;

namespace GolfManager.Web.Features.Dashboard;

public interface IDashboardService
{
    Task<ApiResponse<DashboardResponse>?> GetDashboardAsync();
}
