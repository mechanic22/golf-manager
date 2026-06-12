using GolfManager.Shared.DTOs.League;
using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Features.Admin;

public partial class AdminLeagues : ComponentBase
{
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private IAuthorizationService AuthorizationService { get; set; } = null!;
    [Inject] private ILeagueService LeagueService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<AdminLeagues> Logger { get; set; } = null!;

    private bool isLoading = true;
    private bool isGlobalAdmin;
    private List<LeagueResponse>? leagues;
    private string searchTerm = string.Empty;

    private IEnumerable<LeagueResponse> FilteredLeagues =>
        (leagues ?? []).Where(l =>
            string.IsNullOrWhiteSpace(searchTerm) ||
            l.Name.Contains(searchTerm, StringComparison.OrdinalIgnoreCase) ||
            l.Key.Contains(searchTerm, StringComparison.OrdinalIgnoreCase));

    protected override async Task OnInitializedAsync()
    {
        if (!AuthService.IsAuthenticated)
        {
            Navigation.NavigateTo("/login");
            return;
        }

        await AuthorizationService.InitializeAsync();
        isGlobalAdmin = AuthorizationService.IsGlobalAdmin();

        if (!isGlobalAdmin)
        {
            isLoading = false;
            return;
        }

        await LoadLeagues();
    }

    private async Task LoadLeagues()
    {
        isLoading = true;
        try
        {
            var result = await LeagueService.GetAllLeaguesAsync();
            leagues = result?.Success == true && result.Data != null ? result.Data : [];
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading all leagues");
            leagues = [];
        }
        finally
        {
            isLoading = false;
        }
    }
}
