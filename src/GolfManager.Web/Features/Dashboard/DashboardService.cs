using System.Net.Http.Json;
using GolfManager.Shared.DTOs.Common;
using GolfManager.Shared.DTOs.Dashboard;

namespace GolfManager.Web.Features.Dashboard;

public class DashboardService : IDashboardService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<DashboardService> _logger;

    public DashboardService(HttpClient httpClient, ILogger<DashboardService> logger)
    {
        _httpClient = httpClient;
        _logger = logger;
    }

    public async Task<ApiResponse<DashboardResponse>?> GetDashboardAsync()
    {
        try
        {
            var response = await _httpClient.GetAsync("api/v1/dashboard");

            if (response.IsSuccessStatusCode)
                return await response.Content.ReadFromJsonAsync<ApiResponse<DashboardResponse>>();

            _logger.LogWarning("Failed to get dashboard: {StatusCode}", response.StatusCode);
            return null;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error getting dashboard");
            return null;
        }
    }
}
