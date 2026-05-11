using Microsoft.AspNetCore.Components;

namespace GolfManager.Web.Pages.Profile;

public partial class ProfileRounds : ComponentBase
{
    [Inject] private IAuthService AuthService { get; set; } = null!;
    [Inject] private IUserService UserService { get; set; } = null!;
    [Inject] private NavigationManager Navigation { get; set; } = null!;
    [Inject] private ILogger<ProfileRounds> Logger { get; set; } = null!;

    private bool isLoading = true;
    private List<RoundResponse> rounds = new();

    protected override async Task OnInitializedAsync()
    {
        if (!AuthService.IsAuthenticated)
        {
            Navigation.NavigateTo("/login");
            return;
        }

        await LoadRounds();
    }

    private async Task LoadRounds()
    {
        isLoading = true;
        try
        {
            var response = await UserService.GetMyRoundsAsync();
            rounds = (response != null && response.Success && response.Data != null)
                ? response.Data
                : new List<RoundResponse>();

            Logger.LogInformation("Loaded {Count} rounds from API", rounds.Count);
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Error loading rounds");
            rounds = new List<RoundResponse>();
        }
        finally
        {
            isLoading = false;
        }
    }

    private string GetScoreClass(int score)
    {
        if (score < 80) return "text-sm font-semibold text-green-600";
        if (score < 90) return "text-sm font-semibold text-primary-600";
        return "text-sm font-semibold text-gray-900";
    }

    private static RenderFragment DownloadIcon() => _ => { };
}
