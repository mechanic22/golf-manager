using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Features.Admin;

public partial class Admin : ComponentBase
{
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private IAuthorizationService AuthorizationService { get; set; } = null!;
    [Inject] private IUserService UserService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<Admin> Logger { get; set; } = null!;

    [Inject] private HttpClient Http { get; set; } = null!;

    private bool isLoading = true;
    private bool isGlobalAdmin = false;
    private GolfManager.Shared.DTOs.Admin.PlatformStatsResponse stats = new();
    private bool isRecalculating = false;
    private string recalcMessage = string.Empty;
    private bool recalcSuccess = false;

    protected override async Task OnInitializedAsync()
    {
        if (!AuthService.IsAuthenticated)
        {
            Navigation.NavigateTo("/login");
            return;
        }

        isGlobalAdmin = AuthorizationService.IsGlobalAdmin();

        if (!isGlobalAdmin)
        {
            isLoading = false;
            return;
        }

        await LoadData();
    }

    private async Task LoadData()
    {
        isLoading = true;
        try
        {
            // Load actual platform stats from API
            var response = await UserService.GetPlatformStatsAsync();
            if (response?.Success == true && response.Data != null)
            {
                stats = response.Data;
                Logger.LogInformation("Loaded platform stats: {Users} users, {Leagues} leagues, {Events} events",
                    stats.TotalUsers, stats.TotalLeagues, stats.TotalEvents);
            }
            else
            {
                Logger.LogWarning("Failed to load platform stats: {Message}", response?.Message);
                stats = new GolfManager.Shared.DTOs.Admin.PlatformStatsResponse();
            }

        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading admin dashboard");
            stats = new GolfManager.Shared.DTOs.Admin.PlatformStatsResponse();
        }
        finally
        {
            isLoading = false;
        }
    }

    private async Task RecalculatePlayerStats()
    {
        isRecalculating = true;
        recalcMessage = string.Empty;
        StateHasChanged();

        try
        {
            var response = await Http.PostAsync("api/v1/admin/recalculate-player-stats", null);
            if (response.IsSuccessStatusCode)
            {
                recalcSuccess = true;
                recalcMessage = "Player stats recalculated successfully!";
            }
            else
            {
                recalcSuccess = false;
                recalcMessage = $"Failed to recalculate: {response.StatusCode}";
            }
        }
        catch (Exception ex)
        {
            recalcSuccess = false;
            recalcMessage = $"Error: {ex.Message}";
            Logger.LogError(ex, "Error recalculating player stats");
        }
        finally
        {
            isRecalculating = false;
            StateHasChanged();
        }
    }

}
